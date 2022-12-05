using System.Collections.Generic;
using UnityEngine;

enum State
{
    MOVING_UP, MOVING_DOWN, UP, DOWN, BALANCED
}
public class Door : MonoBehaviour
{
    private State doorState = State.DOWN; // down means closed
    private float currentTime = 0;
    private Vector3 openingDirection;

    [Header("Door")]
    [SerializeField] private GameObject gameObject;
    [SerializeField] private Transform endPosition;
    [SerializeField] private Transform startPosition;
    [SerializeField] private float openingTime;
    [SerializeField] private float closingTime;

    [SerializeField] private AudioSource doorReward;
    [SerializeField] private AudioSource doorOpen;
    [SerializeField] private AudioSource doorClose;
    private bool alreadyOpened = false; // only play doorReward on first open (when this variable is false)

    void Start()
    {
        gameObject.transform.position = startPosition.position;
    }

    public void OpenDoor()
    {
        if (doorState == State.MOVING_UP || doorState == State.UP) return;
        doorState = State.MOVING_UP;
        
        // calculate current time from current position, 0 if down, 1 if up
        Vector3 position = gameObject.transform.position;

        float totalDistance = Vector3.Distance(startPosition.position, endPosition.position);
        float currentDistance = Vector3.Distance(startPosition.position, position);

        float ratio = currentDistance / totalDistance;
        currentTime = openingTime*ratio;
        
        // play sound
        doorOpen.Play();
    }

    public void CloseDoor()
    {
        if (doorState == State.MOVING_DOWN || doorState == State.DOWN) return;
        doorState = State.MOVING_DOWN;
        
        // calculate current time from current position, 0 if up, 1 if down
        Vector3 position = gameObject.transform.position;

        float totalDistance = Vector3.Distance(startPosition.position, endPosition.position);
        float currentDistance = Vector3.Distance(endPosition.position, position);

        float ratio = currentDistance / totalDistance;
        currentTime = closingTime*ratio;
        
        // play sound
        doorClose.Play();
    }
    
    void Update()
    {
        currentTime += Time.deltaTime;

        // update Door position linear to time
        switch (doorState)
        {
            case State.MOVING_UP:
            {
                float ratio = currentTime / openingTime;
                if (ratio > 1) ratio = 1;
                
                gameObject.transform.position = Vector3.Lerp(startPosition.position, endPosition.position, ratio);

                if (currentTime > openingTime)
                {
                    doorState = State.UP;
                    if (!alreadyOpened) doorReward.Play();
                    alreadyOpened = true;
                }
                break;
            }
            case State.MOVING_DOWN:
            {
                float ratio = currentTime / closingTime;
                if (ratio > 1) ratio = 1; 
                
                gameObject.transform.position = Vector3.Lerp(endPosition.position, startPosition.position, ratio);

                if (currentTime > closingTime) doorState = State.DOWN;
                break;
            }
        }
    }
}
