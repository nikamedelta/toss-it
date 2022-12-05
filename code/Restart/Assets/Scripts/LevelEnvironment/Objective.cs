using System;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Objective : MonoBehaviour
{
    private LevelController levelController;
    [SerializeField] private GameObject objective;
    [SerializeField] private int index = 0;
    [SerializeField] private AudioSource pickupAudio;
    [SerializeField] private AudioSource hummingAudio;

    public bool reached = false;
    
    private void Start()
    {
        levelController = GameObject.Find("LevelController").GetComponent<LevelController>();
        if (levelController == null) throw new Exception("no LevelController found");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.tag.Equals("Player")) return;
        levelController.UpdateObjectives(index);
        
        GetComponent<Collider2D>().enabled = false;
        if (objective != null) Destroy(objective);
        pickupAudio.Play();
        hummingAudio.Stop();

        reached = true;
    }
}
