using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public enum BoxState
    {
        NONE, HOOKED, ONHEAD
    }

    public class Movement : MonoBehaviour
    {
        [SerializeField] private Camera cam;
        [SerializeField] private LayerMask ground;
        public LayerMask Ground => ground;

        [Header("Player")] [SerializeField] private Rigidbody2D rig;
        [SerializeField] private Transform FeetPositionLeft;
        [SerializeField] private Transform FeetPositionRight;
        [SerializeField] private Transform mouthPositionR;
        public Transform MouthPositionR => mouthPositionR;
        [SerializeField] private Transform mouthPositionL;
        public Transform MouthPositionL => mouthPositionL;
        [SerializeField] private float movementSpeed = 400;
        [SerializeField] private float jumpForce = 10;
        [SerializeField] private float coyoteTimer = .5f;

        [Header("Cooldowns")] [SerializeField] private float dashCooldownMax = .5f;
        private float dashCooldownCurrent = 0;

        private bool canInteract = true;
        [SerializeField] private float consciousCooldown = 10;
        private float consciousCooldownCurrent = 0;

        // additional dash variables
        private bool isDashing;
        private bool dashRegained;
        private float dashTimer;
        [SerializeField] private float dashTime = .25f;
        [SerializeField] private float dashSpeed = 40f;


        // additional move variables
        private Vector2 moveInput;
        private float xValue;
        private float yValue;
        private bool moving;

        // additional jump variables
        private bool isGrounded;
        private bool jumpReleased = true;
        private int jumpCounter = 0;
        private float fallMultiplier = 2.5f;
        public float FallMultiplier => fallMultiplier;

        private float lowJumpMultiplier = 2f;
        private float groundSpeedMultiplier = 1;

        private float coyoteTime;

        [Header("Hook")] [SerializeField] private float hookRange = 10f;
        [SerializeField] private LayerMask hookLayer;
        [SerializeField] private LayerMask hookLayerHeavy;
        [SerializeField] private LayerMask hookLayerLight;

        [Header("Scripts Ref:")] public GrappleTongue grappleTongue;

        public SpringJoint2D springJoint2D;

        [SerializeField] private float maxDistance = 20;

        private Vector3 playerScale = new Vector3();

        private enum LaunchType
        {
            Transform_Launch,
            Physics_Launch
        }

        [Header("Launching:")] [SerializeField]
        private bool launchToPoint = true;

        [SerializeField] private LaunchType launchType = LaunchType.Physics_Launch;
        [SerializeField] private float launchSpeed = 1;

        [Header("No Launch To Point")] [SerializeField]
        private bool autoConfigureDistance = false;

        [SerializeField] private float targetDistance = 3;
        [SerializeField] private float targetFrequncy = 1;

        [HideInInspector] public Vector2 grapplePoint;
        [HideInInspector] public Vector2 grappleDistanceVector;

        private Vector2 inputPosition; // position in world space where the input device points to
        public Vector2 InputPosition => inputPosition;

        private bool performingHook;
        public bool PerformingHook => performingHook;

        private bool inHookFlight;
        private bool inHookFall;
        private float hookflightMultiplier = 1.25f;
        private float hookedRotationMultiplyer = .5f;
        private BoxHookController hook;

        public Animator anim;
        public SpriteRenderer sprite;
        private float swingMultiplier = 1;
        private float swingCheck = 0;

        [Header("Sounds")] public GameObject randomJump;
        public AudioSource dash;
        public AudioSource knockout;
        public AudioSource aim;
        public AudioSource slurpWeak;
        public AudioSource slurpStrong;

        private void Start()
        {
            hook = GetComponent<BoxHookController>();
            grappleTongue.enabled = false;
            springJoint2D.enabled = false;
            playerScale = transform.localScale;
        }

        void Update()
        {
            if (rig.velocity.x != 0 && canInteract)
            {
                anim.SetFloat("Speed", Mathf.Abs(rig.velocity.x * moveInput.x));
            }
            else
            {
                anim.SetFloat("Speed", 0);
            }

            // flip sprite in moving direction
            if (Mathf.Approximately(rig.velocity.x, 0)) return;
            if (rig.velocity.x >= 1)
            {
                sprite.flipX = false;
            }
            else if (rig.velocity.x <= -1)
            {
                sprite.flipX = true;
            }
        }

        void FixedUpdate()
        {
            if (Physics2D.OverlapCircle(FeetPositionLeft.position, 0.2f, ground) ||
                Physics2D.OverlapCircle(FeetPositionRight.position, 0.2f, ground))
            {
                groundSpeedMultiplier = 1;
                anim.SetBool("IsAscending", false);
                anim.SetBool("IsFalling", false);
                isGrounded = true;
                coyoteTime = coyoteTimer;
            }
            else
            {
                groundSpeedMultiplier = .7f;
                if (coyoteTime > 0)
                {
                    coyoteTime -= Time.fixedDeltaTime;
                }
                else
                {
                    Debug.Log("not grounded");
                    isGrounded = false;
                }
            }

            if (rig.velocity.y > 0)
            {
                anim.SetBool("IsJumping", false);
                anim.SetBool("IsFalling", false);
                if (canInteract) anim.SetBool("IsAscending", true);
            }

            if (rig.velocity.y < 0 && !isDashing && !isGrounded)
            {
                // falling -> apply gravity

                anim.SetBool("IsAscending", false);
                anim.SetBool("IsJumping", false);
                if (canInteract)
                {
                    anim.SetBool("IsFalling", true);
                }

                rig.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
            }
            else if (rig.velocity.y > 0 && jumpReleased && !isDashing)
            {
                rig.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
            }

            if (isDashing)
            {
                rig.velocity = new Vector2(rig.velocity.x, rig.velocity.y);
                dashTimer += Time.fixedDeltaTime;
                if (dashTimer >= dashTime)
                {
                    isDashing = false;
                }

                if (inHookFlight)
                {
                    inHookFlight = false;
                }
            }

            if (!isDashing)
            {
                if (inHookFlight)
                {
                    if (swingCheck != moveInput.x && moveInput.x != 0)
                    {
                        swingMultiplier += 1;
                    }

                    if (swingCheck == moveInput.x)
                    {
                        swingMultiplier = swingMultiplier * .99f;
                    }

                    if (moveInput.x == 0)
                    {
                        swingMultiplier = swingMultiplier * .1f;
                    }

                    swingCheck = moveInput.x;


                    if (launchType == LaunchType.Transform_Launch)
                    {
                        Vector2 firePointDistance = transform.position - transform.localPosition;
                        Vector2 targetPos = grapplePoint - firePointDistance;
                        transform.position = Vector2.Lerp(transform.position, targetPos, Time.fixedDeltaTime * launchSpeed);
                        rig.AddForce(swingMultiplier * moveInput.x * new Vector2(1.5f, 0), ForceMode2D.Impulse);
                    }
                }

                if (!inHookFlight)
                {
                    rig.velocity =
                        new Vector2(moveInput.x * movementSpeed * Time.fixedDeltaTime * 50 * groundSpeedMultiplier,
                            rig.velocity.y);

                }

                if (hook.BoxState == BoxState.NONE) anim.SetBool("TongueOut", false);
                UpdateCooldowns();
            }
        }

        private void UpdateCooldowns()
        {
            // dash cooldown
            if (dashCooldownCurrent > 0)
            {
                dashCooldownCurrent -= Time.fixedDeltaTime;
            }

            if (dashCooldownCurrent <= 0 && isGrounded)
            {
                dashRegained = true; // dash can be used again
            }

            if (consciousCooldownCurrent > 0)
            {
                consciousCooldownCurrent -= Time.fixedDeltaTime;
            }

            if (consciousCooldownCurrent <= 0)
            {
                canInteract = true;
                anim.SetBool("Knockout", false);
            }
        }

        /// <summary>
        /// Applying the character's movement. 
        /// </summary>
        public void Move(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                if (canInteract)
                    moveInput = new Vector2(context.ReadValue<Vector2>().x, 0);
                else moveInput = new Vector2();
            }
            else if (context.canceled)
            {
                moveInput.x = 0;
            }
        }

        /// <summary>
        /// Applying the jumpforce to the character's movement. 
        /// </summary>
        public void Jump(InputAction.CallbackContext context)
        {
            if (context.performed && canInteract)
            {
                if (isGrounded)
                {
                    anim.SetBool("IsJumping", true);
                    anim.SetBool("IsFalling", false);
                    rig.velocity = new Vector2(rig.velocity.x, jumpForce);
                    
                    // play jump sound -> select random child and play their audio source component
                    PlayRandomSoundOfChildren(randomJump);
                }

                if (inHookFlight)
                {
                    anim.SetBool("IsJumping", true);
                    anim.SetBool("IsFalling", false);
                    inHookFlight = false;
                    grappleTongue.enabled = false;
                    springJoint2D.enabled = false;
                    rig.gravityScale = 1;
                    rig.velocity = new Vector2(rig.velocity.x, jumpForce);
                    
                    // play jump sound -> select random child and play their audio source component
                    dash.Play();
                }
            }

            if (context.canceled)
            {
                jumpReleased = true;
            }
        }

        public void Dash(InputAction.CallbackContext context)
        {
            if (context.performed && canInteract && !grappleTongue.enabled)
            {
                if (dashRegained)
                {
                    if (sprite.flipX)
                    {
                        rig.velocity = new Vector2(-dashSpeed, rig.velocity.y);
                    }
                    else
                    {
                        rig.velocity = new Vector2(dashSpeed, rig.velocity.y);
                    }

                    isDashing = true;
                    dashRegained = false;
                    dashCooldownCurrent = .5f;
                    dashTimer = 0;

                    // play dash sound
                    dash.Play();
                }
            }
            else if (context.canceled && isDashing)
            {
                isDashing = false;
            }
        }

        public void HookAim(InputAction.CallbackContext context)
        {
            Vector2 tempInput = context.ReadValue<Vector2>();
            Vector2 playerPosition = transform.position;
            Plane plane = new Plane(new Vector3(0, 0, 1), playerPosition);
            Ray inputRay = cam.ScreenPointToRay(tempInput);
            if (plane.Raycast(inputRay, out float enter))
            {
                inputPosition = inputRay.GetPoint(enter);
            }

            RaycastHit2D hit = Physics2D.Raycast(playerPosition, inputPosition - playerPosition, hookRange,
                hookLayer);
            if (hit && hit.transform.gameObject.layer != 8)
            {
                inputPosition = hit.point;
            }
        }

        public void Hook(InputAction.CallbackContext context)
        {
            Vector2 playerPosition = transform.position;
            RaycastHit2D hit = Physics2D.Raycast(playerPosition, inputPosition - playerPosition, hookRange,
                hookLayer);


            if (context.started && performingHook == false)
            {
                Time.timeScale = .2f;
                Time.fixedDeltaTime = 0.02F * Time.timeScale;
                performingHook = true;
                inHookFlight = false;
                grappleTongue.enabled = false;
                springJoint2D.enabled = false;
                rig.gravityScale = 1;
                if (hook.BoxState != BoxState.NONE)
                {
                    hook.Drop();
                }

                // play aim sound
                aim.Play();
            }

            if (hook.BoxState == BoxState.NONE)
            {
                if (context.canceled && performingHook)
                {
                    if (hit)
                    {

                        if (hookLayerHeavy == (hookLayerHeavy | (1 << hit.transform.gameObject.layer)))
                        {
                            anim.SetBool("TongueOut", true);
                            anim.SetBool("IsJumping", true);
                            anim.SetBool("IsAscending", true);
                            Debug.Log("Hooked to heavy Layer");
                            if (Vector2.Distance(hit.point, transform.position) <= maxDistance)
                            {
                                grapplePoint = hit.point;
                                grappleDistanceVector = grapplePoint - (Vector2) transform.position;
                                grappleTongue.enabled = true;
                                isDashing = false;
                                inHookFlight = true;
                            }

                            // play weak slurp
                            slurpWeak.Play();
                        }

                        if (hookLayerLight == (hookLayerLight | (1 << hit.transform.gameObject.layer)))
                        {
                            anim.SetBool("TongueOut", true);
                            Debug.Log("Hooked to light Layer");
                            float distance = Vector2.Distance(hit.transform.position, playerPosition);
                            hook.HookObject(hit.collider.gameObject, distance);
                            // play weak slurp
                            slurpWeak.Play();
                        }
                    }
                }

                if (context.canceled)
                {
                    Time.timeScale = 1.5f;
                    Time.fixedDeltaTime = 0.02F;
                    performingHook = false;
                }
            }

            if (!hit) return;
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (inHookFlight)
            {
                rig.velocity = new Vector2();
            }

            if (other.gameObject.CompareTag("Platform"))
            {
                //transform.parent = other.gameObject.transform;

                transform.SetParent(other.gameObject.transform, true);
                transform.localScale.Scale(transform.parent.localScale);
            }

            //Debug.Log(other.relativeVelocity.magnitude);
            // make player unconscious if force is too high
            if (other.relativeVelocity.magnitude >= 35 && other.gameObject.layer != ground)
            {
                rig.velocity = new Vector2();
                // make player fall back a little bit
                Vector2 direction = (rig.transform.position - other.transform.position).normalized;
                if (direction.x < 0) direction = new Vector2(-1, 1).normalized;
                else direction = new Vector2(1, 1).normalized;
                rig.AddForce(direction * 20, ForceMode2D.Impulse);
                if (hook.BoxState != BoxState.NONE) hook.Drop();
                performingHook = false;
                consciousCooldownCurrent = consciousCooldown;
                canInteract = false;
                inHookFlight = false;
                grappleTongue.enabled = false;
                springJoint2D.enabled = false;
                anim.SetBool("IsJumping", false);
                anim.SetBool("IsAscending", false);
                anim.SetBool("IsFalling", false);
                anim.SetBool("Knockout", true);

                // play knockout sound
                knockout.Play();
            }
        }

        private void OnCollisionExit2D(Collision2D other)
        {
            if (other.gameObject.CompareTag("Platform"))
            {
                DetachParent();
            }
        }

        public void Grapple()
        {
            springJoint2D.autoConfigureDistance = false;
            if (!launchToPoint && !autoConfigureDistance)
            {
                springJoint2D.distance = targetDistance;
                springJoint2D.frequency = targetFrequncy;
            }

            if (!launchToPoint)
            {
                if (autoConfigureDistance)
                {
                    springJoint2D.autoConfigureDistance = true;
                    springJoint2D.frequency = 0;
                }

                springJoint2D.connectedAnchor = grapplePoint;
                springJoint2D.enabled = true;
            }
            else
            {
                switch (launchType)
                {
                    case LaunchType.Physics_Launch:
                        springJoint2D.connectedAnchor = grapplePoint;

                        Vector2 distanceVector = transform.position;

                        springJoint2D.distance = distanceVector.magnitude;
                        springJoint2D.frequency = launchSpeed;
                        springJoint2D.enabled = true;
                        break;
                    case LaunchType.Transform_Launch:
                        rig.gravityScale = 0;
                        rig.velocity = Vector2.zero;
                        break;
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, maxDistance);
        }

        public void DetachParent()
        {
            transform.SetParent(null, true);
            transform.localScale = playerScale;
        }
        
        public void PlayRandomSoundOfChildren(GameObject go)
        {
            int i = go.transform.childCount - 1;
            GameObject child = go.transform.GetChild(Random.Range(0, i)).gameObject;
            child.GetComponent<AudioSource>().Play();
        }
    }
}
