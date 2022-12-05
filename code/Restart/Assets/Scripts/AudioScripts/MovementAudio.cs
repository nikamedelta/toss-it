using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// For moving objects (player, boxes) to play sounds while colliding with different grounds. 
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class MovementAudio : MonoBehaviour
{
    [SerializeField] private GameObject mud;
    [SerializeField] private GameObject leaves;
    [SerializeField] private GameObject water;
    [SerializeField] private GameObject wood;

    // wait times for repetition of sounds per type 
    private float leavesWaitTime = 0;
    private float mudWaitTime = 0;
    private float waterWaitTime = 0;
    private float woodWaitTime = 0;

    private Vector2 previousPosition = new Vector2(); // position has to change for a new sound to play. otherwise it is very unnatural.

    private float wait = 10; // wait a few seconds (for all objects to land on ground at first collision). prevents loud sound at scene load. 

    private void Start()
    {
        previousPosition = transform.position;
    }

    private void Update()
    {
        if (wait > 0) wait -= Time.unscaledDeltaTime;
        //update wait times
        if (leavesWaitTime > 0)
        {
            leavesWaitTime -= Time.deltaTime;
        }
        if (mudWaitTime > 0)
        {
            mudWaitTime -= Time.deltaTime;
        }
        if (waterWaitTime > 0)
        {
            waterWaitTime -= Time.deltaTime;
        }
        if (woodWaitTime > 0)
        {
            woodWaitTime -= Time.deltaTime;
        }
    }

    private void OnCollisionStay2D(Collision2D other)
    {
        if (wait > 0 && !gameObject.tag.Equals("Player")) return;
        if (Vector2.Distance(previousPosition,transform.position)<0.2f) return;
        // see if velocity of this object is high engogh for "movement"
        if (TryGetComponent(out Rigidbody2D rig))
        {
            if (rig.velocity.magnitude < 0.1) return;
        }
        
        if (other.gameObject.TryGetComponent(out AudioType type))
        {
            PlaySound(type.soundType);
            // if this object is a box: always play an additional wood sound
            if (type.soundType != AudioType.SoundType.WOOD && tag.Equals("Box")) 
            { 
                PlaySound(AudioType.SoundType.WOOD);
            }
        }
        
    }
    
    public void PlayRandomSoundOfChildren(GameObject go)
    {
        int i = go.transform.childCount - 1;
        GameObject child = go.transform.GetChild(Random.Range(0, i)).gameObject;
        child.GetComponent<AudioSource>().Play();
    }

    private void PlaySound(AudioType.SoundType type)
    {
        previousPosition = transform.position;
        {
            switch (type)
            {
                case AudioType.SoundType.MUD:
                {
                    if (mudWaitTime > 0) return;
                    PlayRandomSoundOfChildren(mud);
                    mudWaitTime = Random.Range(0.5f, 0.8f);
                    break;
                }
                case AudioType.SoundType.WOOD:
                {
                    if (woodWaitTime > 0) return;
                    PlayRandomSoundOfChildren(wood);
                    woodWaitTime = Random.Range(0.5f, 0.8f);
                    break;
                }
                case AudioType.SoundType.WATER:
                {
                    if (waterWaitTime > 0) return;
                    PlayRandomSoundOfChildren(water);
                    waterWaitTime = Random.Range(0.5f, 0.8f);
                    break;
                }
                case AudioType.SoundType.LEAVES:
                {
                    if (leavesWaitTime > 0) return;
                    PlayRandomSoundOfChildren(leaves);
                    leavesWaitTime = Random.Range(0.5f, 0.8f);
                    break;
                }
            }
        }
    }
}
