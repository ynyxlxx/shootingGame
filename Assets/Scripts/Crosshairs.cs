using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crosshairs : MonoBehaviour {

    public LayerMask targetMask;
    public SpriteRenderer dot;
    public Color dotHighlightColor;
    Color originalDotColor;

    void Start() {
        Cursor.visible = false;
        originalDotColor = dot.color;
    }

    void Update() {
        //准星旋转效果
        transform.Rotate(Vector3.forward * -40 * Time.deltaTime);
    }

    //当准星对准目标时变色
    public void DetectTarget(Ray ray) {
        if (Physics.Raycast(ray, 100, targetMask)) {
            dot.color = dotHighlightColor;
        } else {
            dot.color = originalDotColor;
        }
    }

}
