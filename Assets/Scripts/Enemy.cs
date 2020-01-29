using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Enemy : LivingEntity {

    public enum State { Idle, Chasing, Attacking };
    State currentState;

    public ParticleSystem deathEffect;

    NavMeshAgent pathFinder;
    Transform target;
    LivingEntity targetEntity;
    Material skinMaterial;

    Color originalColor;

    float attackDitanceThreshold = .5f;
    float timeBetweenAttacks = 1;
    float nextAttackTime;
    float myCollisionRadius;
    float targetCollisionRadius;
    float damage = 1;
    bool hasTarget;

    public static event System.Action OnDeathStatic;

    private void Awake() {
        pathFinder = GetComponent<NavMeshAgent>();

        if (GameObject.FindGameObjectWithTag("Player") != null) {
            hasTarget = true;
            target = GameObject.FindGameObjectWithTag("Player").transform;
            targetEntity = target.GetComponent<LivingEntity>();

            myCollisionRadius = GetComponent<CapsuleCollider>().radius;
            targetCollisionRadius = GetComponent<CapsuleCollider>().radius;

        }
    }

    protected override void Start() {
        base.Start();
        
        if (hasTarget) {
            currentState = State.Chasing;
            targetEntity.OnDeath += OnTargetDeath;

            StartCoroutine(UpdatePath());
        }
        
    }

    void Update() {

        //判断目标是否存在
        if (hasTarget) {
            //进行判断 是否满足攻击条件 =》 1.攻击冷却时间已过 2.进入攻击范围 
            if (Time.time > nextAttackTime) {
                //使用距离的平方来进行比较可以避免开根号的计算开销
                float sqrDstToTarget = (target.position - transform.position).sqrMagnitude;
                if (sqrDstToTarget < Mathf.Pow(attackDitanceThreshold + myCollisionRadius + targetCollisionRadius, 2)) {
                    nextAttackTime = Time.time + timeBetweenAttacks;
                    AudioManager.instance.PlaySound("Enemy Attack", transform.position);
                    StartCoroutine(Attack());
                }
            }
        }
    }

    IEnumerator UpdatePath() {
        float refreshRate = .25f;

        while (hasTarget) {
            if (currentState == State.Chasing) {
                Vector3 dirToTarget = (target.position - transform.position).normalized;
                Vector3 targetPosition = target.position - dirToTarget * (myCollisionRadius + targetCollisionRadius + attackDitanceThreshold / 2f);
                if (dead == false) {
                    pathFinder.SetDestination(targetPosition);
                }
            }
            yield return new WaitForSeconds(refreshRate);
        }
    }

    IEnumerator Attack() {
        //切换至攻击状态，暂停pathFinder的功能防止关闭寻路AI导致错误
        currentState = State.Attacking;
        //防止寻路干扰动作，攻击前先关闭寻路AI
        pathFinder.enabled = false;

        Vector3 originalPos = transform.position;
        //改变attackPos的位置可以调整攻击动画的冲力
        Vector3 dirToTarget = (target.position - transform.position).normalized;
        Vector3 attackPos = target.position - dirToTarget * (myCollisionRadius);

        float percent = 0;
        float attackSpeed = 3;

        //攻击前改变颜色
        skinMaterial.color = Color.red;

        bool hasAppliedDamage = false;

        while (percent <= 1) {

            if (percent >= 0.5f && !hasAppliedDamage) {
                hasAppliedDamage = true;
                targetEntity.TakeDamage(damage);
            }

            percent += Time.deltaTime * attackSpeed;
            // y = 4(-x^2+x) 抛物线控制移动，percent 到达0.5时冲撞到目标，攻击后自动回到原点
            float interpolation = (-Mathf.Pow(percent, 2) + percent) * 4;
            transform.position = Vector3.Lerp(originalPos, attackPos, interpolation);

            yield return null;
        }

        //攻击动作完成，重新启动寻路，恢复原来材质的颜色
        skinMaterial.color = originalColor;
        currentState = State.Chasing;
        pathFinder.enabled = true;
    }

    public override void TakeHit(float damage, Vector3 hitPoint, Vector3 hitDirection) {

        AudioManager.instance.PlaySound("Impact", transform.position);
        if (damage >= health) {
            if (OnDeathStatic != null) {
                OnDeathStatic();
            }
            AudioManager.instance.PlaySound("Enemy Death", transform.position);
            Destroy(Instantiate(deathEffect.gameObject, hitPoint, Quaternion.FromToRotation(Vector3.forward, hitDirection)) as GameObject, deathEffect.main.startLifetime.constant);
        }
        base.TakeHit(damage, hitPoint, hitDirection);
    }

    public void SetCharacteristics(float moveSpeed, int hitsToKillPlayer, float enemyHealth, Color skinColor) {
        pathFinder.speed = moveSpeed;

        if (hasTarget) {
            damage = Mathf.Ceil(targetEntity.startingHealth / hitsToKillPlayer);
        }
        startingHealth = enemyHealth;

        deathEffect.startColor = new Color(skinColor.r, skinColor.g, skinColor.b, 1);
        skinMaterial = GetComponent<Renderer>().material;
        skinMaterial.color = skinColor;
        originalColor = skinMaterial.color;

    }

    //委托给OnDeath方法
    void OnTargetDeath() {
        hasTarget = false;
        currentState = State.Idle;
    }
}
