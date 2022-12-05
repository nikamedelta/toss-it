using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class MoveAlongPathController : MonoBehaviour
{
    public int currentPathIndex = 0;
    public Transform[] setPaths;
    public float speed;
    
    void Update()
    {
        transform.position = Vector2.MoveTowards(transform.position, setPaths[currentPathIndex].position, speed * Time.deltaTime);

        if (transform.position.x == setPaths[currentPathIndex].position.x &&
            transform.position.y == setPaths[currentPathIndex].position.y)
        {
            currentPathIndex++;
            if (currentPathIndex >= setPaths.Length)
            {
                currentPathIndex = 0;
            }
        }
    }
}
