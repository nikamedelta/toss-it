using System;
using System.Collections.Generic;
using UnityEngine;

public class DoorTrigger : MonoBehaviour
{
    [SerializeField] private List<Door> doors;
    [SerializeField] private bool stayOpen;

    [Header("Pressure Plate")] 
    [SerializeField] private GameObject pressurePlate;
    [SerializeField] private Transform downPosition;
    [SerializeField] private Transform upPosition;
    [SerializeField] private float requiredMass = 0.5f;
    
    private float currentMass = 0;
    private float currentTime = 0;
    private float downTime = .5f;
    private float upTime = .5f;
    private State state = State.UP;
    
    public List<GameObject> trigger = new List<GameObject>();

    [SerializeField] private AudioSource down;
    [SerializeField] private AudioSource up;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag.Equals("Player") || (other.gameObject.tag.Equals("Box") && !other.isTrigger))
        {
            if (!trigger.Contains(other.gameObject))
            {
                trigger.Add(other.gameObject);
                UpdateMass();
                
                // for boxes: add this doortrigger to its list
                if (!other.gameObject.GetComponent<ObjectCollision>().standingOnPP.Contains(this)) other.gameObject.GetComponent<ObjectCollision>().standingOnPP.Add(this);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if ((other.gameObject.tag.Equals("Box") && !other.isTrigger) || other.gameObject.tag.Equals("Player"))
        {
            if (trigger.Contains(other.gameObject))
            {
                trigger.Remove(other.gameObject);
                UpdateMass();
                
                // for boxes: remove this doortrigger from its list
                if (other.gameObject.GetComponent<ObjectCollision>().standingOnPP.Contains(this)) other.gameObject.GetComponent<ObjectCollision>().standingOnPP.Remove(this);
            }
        }
        
    }

    private void Update()
    {
        currentTime += Time.deltaTime;

        // update PressurePlate position linear to weight
        switch (state)
        {
            case State.MOVING_DOWN:
            {
                if (currentMass < requiredMass)
                {
                    // only move down halfway, then move up again
                    float ratio = currentTime / downTime;
                    if (ratio > 1) ratio = 1; 
                    
                    Vector3 midwayPosition = Vector3.Lerp(upPosition.position, downPosition.position, .5f);
                    pressurePlate.transform.position = Vector3.Lerp(upPosition.position, midwayPosition, ratio);

                    if (currentTime > downTime)
                    {
                        state = State.MOVING_UP;
                        CalculateUpTime();
                    }
                }
                else
                {
                    float ratio = currentTime / downTime;
                    if (ratio > 1) ratio = 1; 
                
                    pressurePlate.transform.position = Vector3.Lerp(upPosition.position, downPosition.position, ratio);

                    if (currentTime > downTime) state = State.DOWN;
                }
                break;
            }
            case State.MOVING_UP:
            {
                float ratio = currentTime / upTime;
                if (ratio > 1) ratio = 1;
                
                pressurePlate.transform.position = Vector3.Lerp(downPosition.position, upPosition.position, ratio);

                if (currentTime > upTime) state = State.UP;
                break;
            }
        }
    }

    private void CalculateUpTime()
    {
        // calculate current time from current position, 0 if up, 1 if down
        Vector3 position = pressurePlate.transform.position;

        float totalDistance = Vector3.Distance(upPosition.position, downPosition.position);
        float currentDistance = Vector3.Distance(downPosition.position, position);

        float ratio = currentDistance / totalDistance;
        currentTime = downTime*ratio;
    }
    
    private void CalculateDownTime()
    {
        // calculate current time from current position, 0 if down, 1 if up
        Vector3 position = pressurePlate.transform.position;

        float totalDistance = Vector3.Distance(upPosition.position, downPosition.position);
        float currentDistance = Vector3.Distance(upPosition.position, position);

        float ratio = currentDistance / totalDistance;
        currentTime = upTime*ratio;
    }

    public void UpdateMass()
    {
        float temp = currentMass;

        GetObjectsFromAbove();

        Debug.Log("Mass: " + currentMass + " previously: " + temp);

        if (currentMass > temp) // always trigger downwards motion if mass increased
        {
            if (state != State.DOWN && state != State.MOVING_DOWN)
            {
                state = State.MOVING_DOWN;
                CalculateDownTime();
                
                // play down sound 
                down.Play();
            }
            
            if (currentMass >= requiredMass)
            {
                foreach (Door door in doors)
                {
                    door.OpenDoor();    
                }
            }
        } else if (currentMass < temp)// mass decreased
        {
            if (currentMass < requiredMass && !stayOpen) 
            {
                // up-motion is only triggered if mass on pressure plate is not high enough anymore
                foreach (Door door in doors)
                {
                    door.CloseDoor();   
                }
                if (state != State.UP && state != State.MOVING_UP) state = State.MOVING_UP;
                else return;
                CalculateUpTime();
                
                // play up sound
                up.Play();
            }
        }
    }

    private void GetObjectsFromAbove()
    {
        List<GameObject> allObjects = new List<GameObject>(trigger);

        currentMass = 0;
        
        for (int i = 0; i < allObjects.Count; i++)
        {
            GameObject collider2D = allObjects[i];
            if (collider2D.gameObject.tag.Equals("Box") || collider2D.gameObject.tag.Equals("Player"))
            {
                // get their ObjectCollision script
                if (collider2D.gameObject.TryGetComponent(out ObjectCollision nextCollider))
                {
                    foreach (GameObject c in nextCollider.objectsOnTop)
                    {
                        if (nextCollider.extraMass != 0)
                        {
                            Debug.Log("extra");
                            currentMass += nextCollider.extraMass;
                        }
                        if (!allObjects.Contains(c)) allObjects.Add(c);    
                    }
                }
            }  
        }

        foreach (GameObject collider2D in allObjects)
        {
            if(collider2D.TryGetComponent(out Rigidbody2D rig))
            {
                currentMass += rig.mass;
            }
            
        }
    }
}
