using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuadraticDrag : MonoBehaviour
{
    public float quadraticDrag;
    private Rigidbody2D rig;

    void Start()
    {
        rig = gameObject.GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        Vector2 force = rig.velocity.normalized * -quadraticDrag * rig.velocity.sqrMagnitude;
        rig.AddForce(force);
    }
}
