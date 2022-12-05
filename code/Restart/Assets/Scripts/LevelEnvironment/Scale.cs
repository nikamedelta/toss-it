using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scale : MonoBehaviour
{
    [SerializeField] private AudioSource audio;
    [Header("Platform 1")] 
    [SerializeField] private GameObject platOne; 
    [SerializeField] private Transform platOneDown; 
    [SerializeField] private Transform platOneUp;
    private float massOne = 0;
    private State state = State.BALANCED; // describes the state of plat1!!
    private float movingTime = 1f;
    public List<GameObject> oneTrigger = new List<GameObject>();
    
    [Header("Platform 2")] 
    [SerializeField] private GameObject platTwo; 
    [SerializeField] private Transform platTwoDown; 
    [SerializeField] private Transform platTwoUp;
    private float massTwo = 0;
    public List<GameObject> twoTrigger = new List<GameObject>();

    private float currentTime = 0;
    private bool balance = false;

    void Update()
    {
        //Debug.Log("1: " + massOne + " 2: " + massTwo);
        
        currentTime += Time.deltaTime;
        
        switch (state)
        {
            case State.MOVING_DOWN:
            {
                float ratio = currentTime / movingTime;

                if (!balance)
                {
                    if (ratio > 1) ratio = 1;
                    
                    // move plat1
                    platOne.transform.position = Vector3.Lerp(platOneUp.position, platOneDown.position, ratio);
                    // move plat2
                    platTwo.transform.position = Vector3.Lerp(platTwoDown.position, platTwoUp.position, ratio);
                    if (!audio.isPlaying) audio.Play();

                    if (currentTime > movingTime)
                    {
                        state = State.DOWN;
                        audio.Stop();
                    }
                }
                else
                {
                    if (ratio > 0.5f) ratio = 0.5f;
                    
                    // move plat1
                    platOne.transform.position = Vector3.Lerp(platOneUp.position, platOneDown.position, ratio);
                    // move plat2
                    platTwo.transform.position = Vector3.Lerp(platTwoDown.position, platTwoUp.position, ratio);
                    
                    if (currentTime > movingTime/2) state = State.BALANCED;
                }
                break;
            }
            case State.MOVING_UP:
            {
                float ratio = currentTime / movingTime;

                if (!balance)
                {
                    if (ratio > 1) ratio = 1;
                    // move all the way down
                    // move plat1
                    platOne.transform.position = Vector3.Lerp(platOneDown.position, platOneUp.position, ratio);
                    // move plat2
                    platTwo.transform.position = Vector3.Lerp(platTwoUp.position, platTwoDown.position, ratio);
                    if (!audio.isPlaying) audio.Play();

                    if (currentTime > movingTime)
                    {
                        state = State.UP;
                        audio.Stop();
                    }
                }
                else
                {
                    if (ratio > 0.5f) ratio = 0.5f;
                    // move all the way down
                    // move plat1
                    platOne.transform.position = Vector3.Lerp(platOneDown.position, platOneUp.position, ratio);
                    // move plat2
                    platTwo.transform.position = Vector3.Lerp(platTwoUp.position, platTwoDown.position, ratio);
    
                    if (currentTime > movingTime) state = State.BALANCED;
                }
                break;
            }
        }
    }

    public void UpdateMass()
    {
        balance = false;
        
        UpdateMassesFromAbove();
        
        // check if ratios have changed
        if (massOne > massTwo && state != State.DOWN && state != State.MOVING_DOWN)
        {
            state = State.MOVING_DOWN;
            CalculateDownTime();
        } 
        else if (massTwo > massOne && state != State.UP && state != State.MOVING_UP)
        {
            state = State.MOVING_UP;
            CalculateUpTime();
        } else if (Math.Abs(massOne - massTwo) < 0.1f)
        {
            balance = true;
            // determine if plat1 has to move up or down
            Vector3 plat1middle = Vector3.Lerp(platOneUp.position, platOneDown.position, .5f);

            Vector3 upDirection = (platOneUp.position - plat1middle).normalized;
            Vector3 currentDirection = (plat1middle - platOne.transform.position).normalized;
            //Debug.Log("1: " + upDirection + " 2: " + currentDirection);

            if (currentDirection == upDirection)
            {
                state = State.MOVING_UP;
                CalculateUpTime();
            }
            else
            {
                state = State.MOVING_DOWN;
                CalculateDownTime();
            }
        }
    }
    
    private void CalculateUpTime()
    {
        // calculate current time from current position, 0 if up, 1 if down
        Vector3 position = platOne.transform.position;

        float totalDistance = Vector3.Distance(platOneUp.position, platOneDown.position);
        float currentDistance = Vector3.Distance(platOneDown.position, position);

        float ratio = currentDistance / totalDistance;
        currentTime = movingTime*ratio;
    }
    
    private void CalculateDownTime()
    {
        // calculate current time from current position, 0 if down, 1 if up
        Vector3 position = platOne.transform.position;

        float totalDistance = Vector3.Distance(platOneUp.position, platOneDown.position);
        float currentDistance = Vector3.Distance(platOneUp.position, position);

        float ratio = currentDistance / totalDistance;
        currentTime = movingTime*ratio;
    }

    private void UpdateMassesFromAbove()
    {
        // platform 1
        List<GameObject> p1 = new List<GameObject>(oneTrigger);

        massOne = 0;
        
        for (int i = 0; i < p1.Count; i++)
        {
            GameObject massObject = p1[i];
            if (massObject.gameObject.tag.Equals("Box") || massObject.gameObject.tag.Equals("Player"))
            {
                // get their ObjectCollision script
                if (massObject.gameObject.TryGetComponent(out ObjectCollision nextCollider))
                {
                    if (nextCollider.extraMass != 0)
                    {
                        Debug.Log("extra");
                        massOne += nextCollider.extraMass;
                    }
                    foreach (GameObject c in nextCollider.objectsOnTop)
                    {
                        if (!p1.Contains(c)) p1.Add(c);
                    }
                }
            }  
        }

        foreach (GameObject go in p1)
        {
            if(go.TryGetComponent(out Rigidbody2D rig))
            {
                massOne += rig.mass;
            }
        }


        // platform 2
        List<GameObject> p2 = new List<GameObject>(twoTrigger);
        
        massTwo = 0;
        
        for (int i = 0; i < p2.Count; i++)
        {
            GameObject massObject = p2[i];
            if (massObject.gameObject.tag.Equals("Box") || massObject.gameObject.tag.Equals("Player"))
            {
                // get their ObjectCollision script
                if (massObject.gameObject.TryGetComponent(out ObjectCollision nextCollider))
                {
                    if (nextCollider.extraMass != 0)
                    {
                        Debug.Log("extra");
                        massTwo += nextCollider.extraMass;
                    }
                    foreach (GameObject c in nextCollider.objectsOnTop)
                    {
                        if (!p2.Contains(c)) p2.Add(c);    
                    }
                }
            }  
        }
        
        foreach (GameObject go in p2)
        {
            if(go.TryGetComponent(out Rigidbody2D rig))
            {
                massTwo += rig.mass;
            }
        }
    }
}
