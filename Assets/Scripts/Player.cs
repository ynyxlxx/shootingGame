using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(PlayerContorller))]
[RequireComponent(typeof(GunController))]
public class Player : LivingEntity {

    public Crosshairs crosshairs;
    public float moveSpeed = 5f;

    Camera viewCamera;
    PlayerContorller contorller;
    GunController gunController;

    void Awake() {
        contorller = GetComponent<PlayerContorller>();
        gunController = GetComponent<GunController>();
        viewCamera = Camera.main;
        FindObjectOfType<Spawner>().OnNewWave += OnNewWave;
    }

    protected override void Start() {
        base.Start();      
    }

    void Update() {
        //移动输入
        Vector3 moveInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        Vector3 moveVelocity = moveInput.normalized * moveSpeed;
        contorller.Move(moveVelocity);
        
        //鼠标输入
        Ray ray = viewCamera.ScreenPointToRay(Input.mousePosition);
        //设置高度使准星高度和枪械高度一致
        float rayDistance;
        Plane groundPlane = new Plane(Vector3.up, Vector3.up * gunController.GunHeight); 

        if (groundPlane.Raycast(ray, out rayDistance)) {
            //point就是摄像机向鼠标位置发射的射线与地面的交点
            Vector3 point = ray.GetPoint(rayDistance);
            //使人物看向鼠标的方向
            contorller.LookAt(point);
            //渲染准星
            crosshairs.transform.position = point;
            crosshairs.DetectTarget(ray);
            //避免准星离人物太近时准星表现怪异
            if ((new Vector2(point.x, point.z) - new Vector2(transform.position.x, transform.position.z)).sqrMagnitude > 1) {
                gunController.Aim(point);
            }
        }
        
        //武器输入
        if (Input.GetMouseButton(0)) {
            gunController.OnTriggerHold();
        }
        if (Input.GetMouseButtonUp(0)) {
            gunController.OnTriggerRelease();
        }
        if (Input.GetKeyDown(KeyCode.R)) {
            gunController.Reload();
        }

        //坠落后死亡
        if (transform.position.y < -10) {
            TakeDamage(health);
        }
    }

    void OnNewWave(int waveNumber) {
        health = startingHealth;
        gunController.EquipGun(waveNumber - 1);
    }

    public override void Die() {
        AudioManager.instance.PlaySound("Player Death", transform.position);
        base.Die();
    }
}
