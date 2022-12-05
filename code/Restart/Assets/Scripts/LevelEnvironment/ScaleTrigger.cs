using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class ScaleTrigger : MonoBehaviour
{
    [SerializeField] private Scale scale;
    [SerializeField] private bool partOne;
    
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if ((other.gameObject.tag.Equals("Box") && !other.isTrigger) || other.gameObject.tag.Equals("Player"))
        {
            // for boxes: add this scale to its list
            if (!other.gameObject.GetComponent<ObjectCollision>().standingOnS.Contains(scale)) other.gameObject.GetComponent<ObjectCollision>().standingOnS.Add(scale);
            
            if (partOne)
            {
                if (!scale.oneTrigger.Contains(other.gameObject))
                {
                    scale.oneTrigger.Add(other.gameObject);
                    scale.UpdateMass();
                }    
            }
            else
            {
                if (!scale.twoTrigger.Contains(other.gameObject))
                {
                    scale.twoTrigger.Add(other.gameObject);
                    scale.UpdateMass();
                }   
            }
        }
        
        
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if ((other.gameObject.tag.Equals("Box") && !other.isTrigger) || other.gameObject.tag.Equals("Player"))
        {
            // for boxes: remove this scale from its list
            if (other.gameObject.GetComponent<ObjectCollision>().standingOnS.Contains(scale)) other.gameObject.GetComponent<ObjectCollision>().standingOnS.Remove(scale);
            if (partOne)
            {
                if (scale.oneTrigger.Contains(other.gameObject))
                {
                    scale.oneTrigger.Remove(other.gameObject);
                    scale.UpdateMass();
                }
            }
            else
            {
                if (scale.twoTrigger.Contains(other.gameObject))
                {
                    scale.twoTrigger.Remove(other.gameObject);
                    scale.UpdateMass();
                }
            }
        }
        
    }
}
