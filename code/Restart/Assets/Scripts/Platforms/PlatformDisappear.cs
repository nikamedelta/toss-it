using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformDisappear : MonoBehaviour
{
    public float standOnTime = 2;
    private float standOnTimer;

    public float respawnTime = 3;
    private float respawnTimer;
    private bool enabled;

    private void Start()
    {
        enabled = true;
        standOnTimer = standOnTime;
        respawnTimer = respawnTime;
    }

    private void Update()
    {
        if (enabled)
        {
            if (transform.GetChild(0).Find("Player"))
            {
                standOnTimer -= Time.deltaTime;
            }
            else if (standOnTimer < standOnTime && enabled)
            {
                standOnTimer += Time.deltaTime;
            }

            if (standOnTimer <= 0)
            {
                enabled = false;
                transform.GetChild(0).Find("Player").parent = null;
                foreach (Transform child in gameObject.transform)
                {
                    child.gameObject.SetActive(enabled);
                }

            }
        }
        
        else
        {
            respawnTimer -= Time.deltaTime;
            if (respawnTimer <= 0)
            {
                enabled = true;
                foreach (Transform child in gameObject.transform)
                {
                    child.gameObject.SetActive(true);
                }
                standOnTimer = standOnTime;
                respawnTimer = respawnTime;
            }
        }
    }
}
