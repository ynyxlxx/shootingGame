using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour {

    float damage = 1;
    float speed = 10;
    public LayerMask collisionMask;
    public Color trailColor;

    float lifeTime = 3f;
    float skinWidth = .1f;

    public void SetSpeed(float newSpeed) {
        speed = newSpeed;
    }

    void Start() {
        Destroy(gameObject, lifeTime);

        //检测投射物创建时是否在敌人碰撞体内部
        Collider[] initialCollisions = Physics.OverlapSphere(transform.position, 0.1f, collisionMask);
        if (initialCollisions.Length > 0) {
            OnHitObject(initialCollisions[0], transform.position);
        }

        GetComponent<TrailRenderer>().material.SetColor("_TintColor", trailColor);
    }

    void Update() {
        float moveDistancePerFrame = speed * Time.deltaTime;
        //只检测投射物在一帧内移动的距离范围内的碰撞节省计算开销
        CheckCollisions(moveDistancePerFrame);
        transform.Translate(Vector3.forward * moveDistancePerFrame);
    }

    void CheckCollisions(float moveDistance) {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        //检测给定最大距离范围内的碰撞, 适当增大最大距离来避免目标移动过快导致碰撞失效的问题(skinWidth)
        if (Physics.Raycast(ray, out hit, moveDistance + skinWidth, collisionMask, QueryTriggerInteraction.Collide)) {
            OnHitObject(hit.collider, hit.point);
        }
    }

    void OnHitObject(Collider c, Vector3 hitPoint) {
        IDamageable damageableObject = c.GetComponent<IDamageable>();
        if (damageableObject != null) {
            damageableObject.TakeHit(damage, hitPoint, transform.forward);
        }

        GameObject.Destroy(gameObject);
    }
}
