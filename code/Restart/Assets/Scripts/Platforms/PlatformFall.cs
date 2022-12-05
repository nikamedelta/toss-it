using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformFall : MonoBehaviour
{
    public Rigidbody2D rig;
    public float fallTime = 3f;
    public float respawnTime = 10f;
    Vector2 startPos;

    public bool respawns;

    void Start()
    {
        rig = GetComponent<Rigidbody2D>();
        startPos = transform.position;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.transform.position.y < transform.position.y) return;    // only "collide" with objects that are "above" the platform
        if (rig.isKinematic) StartCoroutine(WaitForFall());         // if already falling -> no coroutine
    }

    IEnumerator WaitForFall()
    {
        yield return new WaitForSeconds(fallTime);
        rig.isKinematic = false;
        
        if (respawns)
        {
            StartCoroutine(Respawn());
        }
    }

    IEnumerator Respawn()
    {
        yield return new WaitForSeconds(respawnTime);
        rig.isKinematic = true;
        rig.velocity = new Vector2();
        transform.position = startPos;
    }
}
