using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ObjectCollision : MonoBehaviour
{
    public float extraMass = 0; // important for object on player head: rigidbody is temporarily deattached -> save their mass here for correct mass updating
    public List<GameObject> objectsUnderneath = new List<GameObject>();
    public List<GameObject> objectsOnTop = new List<GameObject>();

    public List<DoorTrigger> standingOnPP = new List<DoorTrigger>();
    public List<Scale> standingOnS = new List<Scale>();

    // detects collision with trigger, but saves the "real" object's collider in lists 
    private void OnCollisionEnter2D(Collision2D other)
    {
        // check if colliding with a GO underneath
        List<ContactPoint2D> contacts = new List<ContactPoint2D>();
        other.GetContacts(contacts);
        foreach (ContactPoint2D contact in contacts)
        {
            if (contact.collider == other.collider && contact.otherCollider == other.otherCollider ||
                contact.collider == other.otherCollider && contact.otherCollider == other.collider)
            {
                // see if Vector go.middle to contactpoint is -y
                Vector2 direction = contact.point - new Vector2(this.transform.position.x, this.transform.position.y);
                if (direction.y < 0)
                {
                    if (other.gameObject.tag.Equals("Player") || other.gameObject.tag.Equals("Box"))
                    {
                        if (!objectsUnderneath.Contains(other.collider.gameObject))
                        {
                            objectsUnderneath.Add(other.collider.gameObject);
                        }

                        if (!other.gameObject.GetComponent<ObjectCollision>().objectsOnTop.Contains(other.otherCollider.gameObject))
                        {
                            other.gameObject.GetComponent<ObjectCollision>().objectsOnTop.Add(other.otherCollider.gameObject);
                        }
                        UpdateMassUnderneathPP();
                        UpdateMassUnderneathS();
                    }

                    break;
                }
            }
        }
    }

    public void UpdateMassUnderneathPP()
    {
        List<GameObject> underneath = new List<GameObject>(objectsUnderneath);

        for (int i = 0; i < underneath.Count; i++)
        {
            GameObject objUnderneath = underneath[i];
            foreach (DoorTrigger trigger in objUnderneath.GetComponent<ObjectCollision>().standingOnPP)
            {
                trigger.UpdateMass();
            }

            foreach (GameObject col in objUnderneath.GetComponent<ObjectCollision>().objectsUnderneath)
            {
                if (!underneath.Contains(col)) underneath.Add(col);
            }
        }
    }
    
    public void UpdateMassUnderneathS()
    {
        List<GameObject> underneath = new List<GameObject>(objectsUnderneath);
        underneath.Add(this.gameObject);

        for (int i = 0; i < underneath.Count; i++)
        {
            GameObject objUnderneath = underneath[i];
            foreach (Scale scale in objUnderneath.GetComponent<ObjectCollision>().standingOnS)
            {
                scale.UpdateMass();
            }

            foreach (GameObject col in objUnderneath.GetComponent<ObjectCollision>().objectsUnderneath)
            {
                if (!underneath.Contains(col)) underneath.Add(col);
            }
        }
    }
}
