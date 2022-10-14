using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerAController : MonoBehaviour
{
    public static PlayerAController instance;
    public MovementState state;
    public enum MovementState
    {
        walking,
        dashing,
        air
    }
    private MovementState lastState;

    [Header("Movement")]
    public Transform orientation;
    private Rigidbody rb;
    private Vector3 moveDirection;
    private float horizontal;
    private float vertical;
    //public float currentSpeed;
    public float moveSpeed;
    public float dashSpeed;
    public float dashSpeedChangeFactor;
    public bool backDashing = false;
    public float desiredMoveSpeed;
    private float lastDesiredMoveSpeed;
    private bool keepMomentum = false;
    private float speedChangeFactor;
    public float maxYSpeed;

    [Header("Jump")]
    public float jumpForce;
    public float airMultiplier;
    public float fallJumpGravity;
    public float maxAirVelocity;
    public bool hasJumped = false;

    [Header("Jetpack")]
    public Transform jetPackTransform;
    public bool initialLaunch;
    public bool usingJettpack = false;
    public float impulseForce;
    public float flyForce;
    public float jetCooldownTimer = 0f;
    public float jetCooldownMaxTimer;
    
    [Header("Fuel")]
    public float maxFuel;
    public float currentFuel;
    public float impulseDecrease;
    public float fuelDecrease;
    public float fuelIncrease;
    public Slider fuelSlider; 

    [Header("Ground Check")]
    public Transform groundCheck;
    public LayerMask groundLayerMask;
    public float groundCheckRadius;
    public bool isGrounded;
    private bool lastGrounded;
    public float groundDrag;

    [Header("SlopeHandling")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    public bool exitingSlope;

    public ParticleSystem jetPackParticle;
    void StateHandler()  //changed currentSpeed to desiredMovespeed for dashing
    {
        if (backDashing)
        {
            //rb.drag = 0;
            state = MovementState.dashing;
            rb.drag = 0;
            desiredMoveSpeed = dashSpeed;
            speedChangeFactor = dashSpeedChangeFactor;
        }

        if (isGrounded && !backDashing)
        {
            state = MovementState.walking;
            desiredMoveSpeed = moveSpeed;
        }

       if (!isGrounded && !backDashing)
        {
            state = MovementState.air;
            //desiredMoveSpeed = moveSpeed;
        }

        bool desiredMoveSpeedChanged = desiredMoveSpeed != lastDesiredMoveSpeed;  //keeping momentum condition
        if (lastState == MovementState.dashing) keepMomentum = true;
        if (desiredMoveSpeedChanged)
        {
            if (keepMomentum)
            {
                StopAllCoroutines();
                StartCoroutine(LerpSpeed());
            }
            else
            {
                //moveSpeed = desiredMoveSpeed;  this behaves weirdly, dk y commenting it out fixes though
            }
        }
            
        lastDesiredMoveSpeed = desiredMoveSpeed;
        lastState = state;
    }
    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        currentFuel = maxFuel;
        fuelSlider.maxValue = maxFuel;
        rb = GetComponent<Rigidbody>();
        jetPackParticle.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        //print(exitingSlope);
        fuelSlider.value = currentFuel;
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayerMask);
        if (isGrounded)
        {
            if (state == MovementState.walking || !lastGrounded)
            {
                hasJumped = false;
                rb.drag = groundDrag;
                exitingSlope = false;
            }
        }
        else
        {
            rb.drag = 0;
            JumpFall();
        }
        lastGrounded = isGrounded;

        if (!usingJettpack && currentFuel < maxFuel) //if not doing anyth and current fuel isnt full
        {
            jetCooldownTimer += Time.deltaTime; //start countodown

            if (jetCooldownTimer >= jetCooldownMaxTimer) //when countdown reaches, do behavior below
            {
                jetCooldownTimer = jetCooldownMaxTimer;
                currentFuel += fuelIncrease * Time.deltaTime;

                if (currentFuel >= maxFuel)
                {
                    jetCooldownTimer = 0;
                }
            }
        }
        ProcessInputs();
        SpeedControl();
        StateHandler();
    }
    private void FixedUpdate()
    {
        MovePlayer();
    }

    void ProcessInputs()
    {
        horizontal = Input.GetAxisRaw("MovementHorizontal");
        vertical = Input.GetAxisRaw("MovementVertical");

        //string test = "horiz";
        //print($"HORIZ: {horizontal}");
        //print($"VERT: { vertical}");
        if (Input.GetKeyDown(KeyCode.Joystick1Button1) && isGrounded && !hasJumped && !usingJettpack) //jump 
        {
            Jump();
        }

        initialLaunch = (Input.GetKeyDown(KeyCode.Joystick1Button4) && !isGrounded && currentFuel > 0); //check for jettpack input

        if (initialLaunch) //jetpack impulse
        {
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            rb.AddForce(Vector2.up * impulseForce, ForceMode.Impulse);
            currentFuel -= impulseDecrease;
            usingJettpack = true;
            jetCooldownTimer = 0;
        }

        if (Input.GetKey(KeyCode.Joystick1Button4) && !isGrounded && currentFuel > 0 && usingJettpack) //jetpack force
        {
            print("jettpack called");
            Jetpack();
        }
        else
        {
            usingJettpack = false;
            jetPackParticle.gameObject.SetActive(false);
            rb.useGravity = true;
        }
    }

    //public bool isMoving()
    //{
    //    return (vertical != 0 && horizontal != 0);
    //}
    void MovePlayer()
    {
        if (state == MovementState.dashing) return;
        moveDirection = orientation.forward * vertical + orientation.right * horizontal; //use this for jump dir
        if (OnSlope() && !exitingSlope) //when moving on slope and not exiting slope
        {
            print("currently on slope");
            rb.AddForce(SlopeMoveDirection() * moveSpeed * 15f, ForceMode.Force);
            if (rb.velocity.y > 0)
            {
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
            }
        }
        else if (isGrounded) //when moving and on ground
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        }
        else if(!isGrounded) //when moving and in air
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 15f * airMultiplier, ForceMode.Force);  
        }
        rb.useGravity = !OnSlope();   
    }
    private void Jump()
    {
        hasJumped = true;
        exitingSlope = true;
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        Vector3 jumpDir = orientation.forward * vertical + orientation.right * horizontal + orientation.up; //getting the player's direction
        rb.AddForce(jumpDir * jumpForce, ForceMode.Impulse);
    }
    private void JumpFall()
    {
        if (rb.velocity.y <= -0.1 && !usingJettpack && state != MovementState.dashing)
        {
            //print("Jump fall used");
            rb.AddForce(Vector3.down * fallJumpGravity, ForceMode.Impulse);
        }
    }
    private void Jetpack()
    {
        rb.useGravity = false;
        currentFuel -= fuelDecrease * Time.deltaTime;
        jetPackParticle.gameObject.SetActive(true); //temp use
        Vector3 flyDir = orientation.forward * vertical + orientation.right * horizontal + orientation.up; //getting the player's direction
        rb.AddForce(flyDir * flyForce * Time.deltaTime, ForceMode.Force);
    }
    private void SpeedControl()
    {
        if (OnSlope() && !exitingSlope)
        {
            if (rb.velocity.magnitude > moveSpeed)
            {
                rb.velocity = rb.velocity.normalized * moveSpeed;
            }
        }
        else
        {
            Vector3 velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            float maxSpeed = moveSpeed;

            if (!isGrounded)
            {
                maxSpeed = maxAirVelocity; //variable maxJumpSpeed is used to change values in the inspector
            }
            if (velocity.magnitude > maxSpeed)
            {
                Vector3 limitVelocity = velocity.normalized * maxSpeed;
                rb.velocity = new Vector3(limitVelocity.x, rb.velocity.y, limitVelocity.z);
            }
        }

        if (maxYSpeed != 0 && rb.velocity.y > maxYSpeed)  //for dash stuff
        {
            rb.velocity = new Vector3(rb.velocity.x, maxYSpeed, rb.velocity.z);
        }
    }
    private IEnumerator LerpSpeed() //dash lerp or momentum behavior
    {
        float time = 0;
        float difference = Mathf.Abs(desiredMoveSpeed - moveSpeed); 
        float startValue = moveSpeed;

        float boostFactor = speedChangeFactor;

        while(time < difference)
        {
            moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time / difference);
            time += Time.deltaTime * boostFactor;
            yield return null;
        }
        moveSpeed = desiredMoveSpeed;
        speedChangeFactor = 1f;
        keepMomentum = false;
    }
    private bool OnSlope()
    {
        if (Physics.Raycast(groundCheck.position, Vector3.down, out slopeHit, 0.2f))
        {
            if (slopeHit.transform.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                //Debug.DrawRay(transform.position, Vector3.down, Color.green, 50f); //print("Hit");
                float slopeAngle = Vector3.Angle(Vector3.up, slopeHit.normal);
                return slopeAngle < maxSlopeAngle && slopeAngle != 0;
            }
        }
        return false;
    }
    private Vector3 SlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
    }
}
