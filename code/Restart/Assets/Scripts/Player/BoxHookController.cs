using System.Collections.Generic;
using Player;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Movement))]
// handles the box that is currently dragged by the player
public class BoxHookController : MonoBehaviour
{
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private Box_ProgressBar progressBar = null;
    [SerializeField] private Material tongue;
    [SerializeField] private Material tongue_aim;
    private BoxState boxState = BoxState.NONE;
    private GameObject box;
    private float distance;
    private ObjectCollision objectCollision;

    // cooldowns
    [SerializeField] private float onHeadCooldown = 4;
    private float onHeadCooldownCurrent = 0;
    private float reloadCooldown = 3;
    private float reloadCooldownCurrent = 0;


    // variables for dis- end reenabling the box's rigidbody
    private float rigMass;
    private PhysicsMaterial2D rigPhysicsMaterial;

    // timer
    private bool pullTimer;
    private float pullTime;
    
    // for some references:
    private Movement movement;
    
    public BoxState BoxState => boxState;
    public GameObject Box => box;

    private void Start()
    {
        movement = GetComponent<Movement>();
        objectCollision = GetComponent<ObjectCollision>();
    }

    private void FixedUpdate()
    {
        DragObject();
        UpdateLine();
        PullTimerCount();
        
        // onHead cooldown
        if (onHeadCooldownCurrent > 0)
        {
            onHeadCooldownCurrent -= Time.deltaTime;
        }
        if (onHeadCooldownCurrent <= 0 && boxState == BoxState.ONHEAD)
        {
            PutDown(); // object is released from head and hooked "normally"
        }
        // reload cooldown
        if (reloadCooldownCurrent > 0)
        {
            reloadCooldownCurrent -= Time.deltaTime;
        }
        else reloadCooldownCurrent = 0;
    }
    
    public void HookObject(GameObject box, float distance)
    {
        this.box = box;
        this.distance = distance;
        boxState = BoxState.HOOKED;
    }
    
    public void PickUp(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            switch (boxState)
            {
                case BoxState.HOOKED:
                {
                    PutOnHead();
                    break;
                }
                case BoxState.ONHEAD:
                {
                    PutDown();
                    progressBar.InterruptBar();
                    break;
                }
            }
        }
    }

    public void PutOnHead()
    {
        if (reloadCooldownCurrent > 0) return;
        
        // make box rigidbody kinematic
        Rigidbody2D rig = box.GetComponent<Rigidbody2D>(); 
        rigMass = rig.mass;
        rig.bodyType = RigidbodyType2D.Kinematic;

        box.layer = 11; // layer 11 == object on player head. no collisions with player layer
        box.transform.parent = transform;
        Vector3 playerPosition = transform.position;
        box.transform.position =
            new Vector3(playerPosition.x, playerPosition.y + 1, playerPosition.z);
        boxState = BoxState.ONHEAD;
        
        // add mass of box virtually to the players object collision component
        objectCollision.extraMass = rigMass;
        // update the box's object collision script
        if (box.TryGetComponent(out ObjectCollision oc))
        {
            List<Scale> scale = new List<Scale>();
            List<DoorTrigger> trigger = new List<DoorTrigger>();
            oc.objectsUnderneath = new List<GameObject>();
            oc.objectsOnTop = new List<GameObject>();
            oc.standingOnS = new List<Scale>();
            oc.standingOnPP = new List<DoorTrigger>();
            foreach (Scale s in scale)
            {
                s.UpdateMass();
            }

            foreach (DoorTrigger t in trigger)
            {
                t.UpdateMass();
            }
        }

        onHeadCooldownCurrent = onHeadCooldown;
        
        // play sound
        movement.slurpStrong.Play();
        
        // show bar
        progressBar.StartProgressBar(Color.white, onHeadCooldown, Color.gray, reloadCooldown);
    }

    public void PutDown()
    {
        // reset rigidbody to kinematic
        Rigidbody2D rig = box.GetComponent<Rigidbody2D>();
        rig.bodyType = RigidbodyType2D.Dynamic;
        
        box.transform.parent = null;
        box.layer = 10;
        rig.freezeRotation = false;
        rig.isKinematic = false;
        boxState = BoxState.HOOKED;
        
        // subtract mass from object collision
        objectCollision.extraMass = 0;

        onHeadCooldownCurrent = 0;
        reloadCooldownCurrent = reloadCooldown;
        
        // play sound
        movement.slurpWeak.Play();
    }

    public void Pull(InputAction.CallbackContext context)
    {
        Debug.Log("pull");
        if (boxState != BoxState.NONE && context.started)
        {
            pullTimer = true;
            // play aim sound
            movement.aim.Play();
        }

        if (boxState != BoxState.NONE && context.canceled)
        {
            float multiplier = pullTime < 1 ? pullTime / 1 : 1;
            Vector2 direction = box.transform.position - transform.position;

            if (boxState == BoxState.ONHEAD) PutDown();
            box.GetComponent<Rigidbody2D>().AddForce(-direction * 5f *  multiplier, ForceMode2D.Impulse);
            box = null;
            boxState = BoxState.NONE; 
                
            pullTimer = false;
            pullTime = 0;
            
            // play sound
            movement.slurpWeak.Play();
        }
    }

    public void Drop()
    {
        if (boxState != BoxState.NONE)
        {
            if (boxState == BoxState.ONHEAD) PutDown();
            box = null;
            boxState = BoxState.NONE;
            
            // play sound
            movement.slurpWeak.Play();
        }
    }
    
    public void Drop(InputAction.CallbackContext context)
    {
        if (context.canceled)
        {
            Drop();
        }
    }

    private void DragObject()
    {
        if (boxState == BoxState.HOOKED)
        {
            Rigidbody2D objectRig = box.GetComponent<Rigidbody2D>();

            // if distance between player and object is bigger than at the start of the hook, drag object towards player
            float currentDistance = Vector2.Distance(box.transform.position, transform.position);
            if (currentDistance > distance)
            {
                float distanceMultiplier = 0.5f;
                if (currentDistance > distance * 2) distanceMultiplier = 1;
                // move object towards player
                Vector2 move = (Vector2) (transform.position - box.transform.position);
                //objectRig.velocity = move * 1f;
                objectRig.AddForce(move.normalized * (Mathf.Log(move.magnitude) * distanceMultiplier),
                    ForceMode2D.Impulse);

                if (objectRig.velocity.magnitude <= 0.5f || currentDistance>distance*1.5f)
                {
                    // move player if object can't be moved
                    this.GetComponent<Rigidbody2D>().AddForce(-move.normalized * (Mathf.Log(move.magnitude) * distanceMultiplier),ForceMode2D.Impulse);
                }


                // if that object is not on ground, apply gravity
                float distanceToGround = box.GetComponent<Collider2D>().bounds.extents.y;
                Vector2 floorPosition = new Vector2(box.transform.position.x,
                    box.transform.position.y - distanceToGround);
                if (!Physics2D.OverlapCircle(floorPosition, 0.1f, movement.Ground))
                {
                    // apply gravity (if there is no upwards pull?)
                    objectRig.velocity += Vector2.up * (Physics2D.gravity.y * (movement.FallMultiplier - 1) * objectRig.gravityScale * objectRig.mass * Time.deltaTime);
                }
            }
        }
    }

    private void PullTimerCount()
    {
        if (pullTimer)
        {
            pullTime += Time.fixedDeltaTime;
        }
    }
    
    private void UpdateLine()
    {
        Vector3 mouthPosition;
        if (movement.sprite.flipX)
        {
            mouthPosition = movement.MouthPositionL.position;
        }
        else mouthPosition = movement.MouthPositionR.position;
        if (boxState != BoxState.NONE) // has Box
        {
            lineRenderer.material = tongue;
            lineRenderer.positionCount = 3;
            
            lineRenderer.SetPosition(0, mouthPosition);
            Vector3 pullPosition = Vector3.Lerp(mouthPosition, box.transform.position,
                (pullTime < 1 ? pullTime : 1));
            lineRenderer.SetPosition(1, pullPosition);
            lineRenderer.SetPosition(2, box.transform.position);
                
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(pullTime != 0? Color.red : Color.white, 0.0f), new GradientColorKey(Color.white, (pullTime < 1 ? pullTime : 1)) },
                new GradientAlphaKey[] { new GradientAlphaKey(1, 0.0f), new GradientAlphaKey(1, (pullTime < 1 ? pullTime : 1)) }
            );
            gradient.mode = GradientMode.Fixed;
            lineRenderer.colorGradient = gradient;
        }
        else if (movement.PerformingHook) // aiming
        {
            lineRenderer.colorGradient = new Gradient();
            lineRenderer.material = tongue_aim;
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, mouthPosition);
            lineRenderer.SetPosition(1, movement.InputPosition);
        }
        else // show no lineRenderer
        {
            lineRenderer.positionCount = 0;
        }
    }
}
