using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Collider2D))]
public class ObjectTrigger : MonoBehaviour
{
    [SerializeField] private ObjectCollision objectCollision;

    private void OnTriggerExit2D(Collider2D other)
    {
        if (objectCollision.objectsOnTop.Contains(other.gameObject))
        {
            objectCollision.objectsOnTop.Remove(other.gameObject);
        }

        if (objectCollision.objectsUnderneath.Contains(other.gameObject))
        {
            objectCollision.objectsUnderneath.Remove(other.gameObject);
        }
        objectCollision.UpdateMassUnderneathPP();
        objectCollision.UpdateMassUnderneathS();
        
        // also remove the components at the other object's script
        if (other.tag.Equals("Box") && !other.isTrigger || other.tag.Equals("Player"))
        {
            ObjectCollision otherCollision = other.GetComponent<ObjectCollision>();
            Collider2D parentCollider = transform.parent.GetComponent<Collider2D>();
            if (otherCollision.objectsOnTop.Contains(parentCollider.gameObject))
            {
                otherCollision.objectsOnTop.Remove(parentCollider.gameObject);
            }

            if (otherCollision.objectsUnderneath.Contains(parentCollider.gameObject))
            {
                otherCollision.objectsUnderneath.Remove(parentCollider.gameObject);
            }
            otherCollision.UpdateMassUnderneathPP();
            otherCollision.UpdateMassUnderneathS();
        }
        
        
    }
}
