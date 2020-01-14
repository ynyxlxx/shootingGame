using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour {

    //三种射击模式
    public enum FireMode { Auto, Burst, Single};
    public FireMode fireMode;  
    //射击间隔
    public float msBetweenShots = 100;
    //出膛速度
    public float muzzleVelocity = 35;
    //装填弹药
    public int projectilesPerMag;
    int projectilesRemainingInMag;
    bool isReloading;
    public float reloadTime;
    //后座表现
    [Header("Recoil")]
    public Vector2 kickMinMax = new Vector2(0.05f, 0.2f);
    public Vector2 recoilAngleMinMax = new Vector2(3f, 5f);
    public float recoilMoveSettleTime = 0.1f;
    public float recoilRotationSettleTime = 0.1f;

    [Header("Effects")]
    public Transform shell;
    public Transform shellEjectionPoint;
    public Transform[] projectileSpawn;
    public Projectile projectile;
    public AudioClip shootAudio;
    public AudioClip reloadAudio;
    float nextShotTime;
    public int burstCount;
    int ShotsRemainingInBurst;
    
    MuzzleFlash muzzleFlash;

    bool triggerReleasedSinceLastShot;

    Vector3 recoilSmoothDampVelocity;
    float recoilRotationSmoothDampVelocity;
    float recoilAngle;

    private void Start() {
        muzzleFlash = GetComponent<MuzzleFlash>();
        ShotsRemainingInBurst = burstCount;
        projectilesRemainingInMag = projectilesPerMag;
    }

    //LateUpdate保证Update中的指令不被覆盖
    private void LateUpdate() {
        //后坐力效果
        transform.localPosition = Vector3.SmoothDamp(transform.localPosition, Vector3.zero, ref recoilSmoothDampVelocity, recoilMoveSettleTime);
        recoilAngle = Mathf.SmoothDamp(recoilAngle, 0, ref recoilRotationSmoothDampVelocity, recoilRotationSettleTime);
        transform.localEulerAngles = transform.localEulerAngles + Vector3.right * recoilAngle * -1;

        if (!isReloading && projectilesRemainingInMag == 0) {
            Reload();
        }
    }

    void Shoot() {
        
        if (Time.time > nextShotTime && projectilesRemainingInMag > 0 && !isReloading) {

            if (fireMode == FireMode.Burst) {
                if (ShotsRemainingInBurst == 0) {
                    return;
                }
                ShotsRemainingInBurst--;
            }else if (fireMode == FireMode.Single) {
                if (!triggerReleasedSinceLastShot) {
                    return;
                }
            }

            for (int i = 0; i < projectileSpawn.Length; i++) {
                if (projectilesRemainingInMag == 0) {
                    break;
                }
                projectilesRemainingInMag--;
                nextShotTime = Time.time + msBetweenShots / 1000;
                Projectile newProjectile = Instantiate(projectile, projectileSpawn[i].position, projectileSpawn[i].rotation) as Projectile;
                newProjectile.SetSpeed(muzzleVelocity);
            }

            Instantiate(shell, shellEjectionPoint.position, shellEjectionPoint.rotation);
            muzzleFlash.Activate();
            //后坐力表现
            transform.localPosition -= Vector3.forward * Random.Range(kickMinMax.x, kickMinMax.y);
            recoilAngle += Random.Range(recoilAngleMinMax.x, recoilAngleMinMax.y);
            recoilAngle = Mathf.Clamp(recoilAngle, 0, 30);

            AudioManager.instance.PlaySound(shootAudio, transform.position);
        }
    }

    //按下扳机时
    public void OnTriggerHold() {
        Shoot();
        triggerReleasedSinceLastShot = false;
    }

    //松开扳机时
    public void OnTriggerRelease() {
        triggerReleasedSinceLastShot = true;
        ShotsRemainingInBurst = burstCount;
    }

    public void Aim(Vector3 aimPoint) {
        if (!isReloading) {
            transform.LookAt(aimPoint);
        }
    }

    //重装子弹
    public void Reload() {
        if (!isReloading && projectilesRemainingInMag != projectilesPerMag) {
            StartCoroutine(AnimateReload());
            AudioManager.instance.PlaySound(reloadAudio, transform.position);
        }     
    }

    IEnumerator AnimateReload() {
        isReloading = true;
        yield return new WaitForSeconds(.2f);

        float reloadSpeed = 1f / reloadTime;
        float percent = 0;
        Vector3 initialRot = transform.localEulerAngles;
        float maxReloadAngle = 30;
        while (percent < 1) {
            percent += Time.deltaTime * reloadSpeed;
            float interpolation = (-Mathf.Pow(percent, 2) + percent) * 4;
            //抛物线做interpolation参数可以使Mathf.Lerp模拟往复运动
            float reloadAngle = Mathf.Lerp(0, maxReloadAngle, interpolation);
            transform.localEulerAngles = initialRot + Vector3.left * reloadAngle;

            yield return null;
        }

        isReloading = false;
        projectilesRemainingInMag = projectilesPerMag;
    }
}
