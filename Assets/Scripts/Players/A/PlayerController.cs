using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public static PlayerController instance;
    public bool PlayerA = false;
    public PlayerCam cam;
    public PlayerStates state;
    public enum PlayerStates
    {
        walking,
        dashing,
        air
    }
    private PlayerStates lastState;

    [Header("Inputs")]
    public KeyCode jump = KeyCode.Space;
    public KeyCode jetPack = KeyCode.Space;

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

    [Header("Health")]
    public int currentHealth;
    public int maxHealth;
    public Slider healthSlider;

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

    private float maxSlopeAngle;
    private RaycastHit slopeHit;
    public bool exitingSlope;

    [Header("Temp")]
    public bool readyToSpawn;
    public ParticleSystem jetPackParticle;


    void StateUpdater()  //changed currentSpeed to desiredMovespeed for dashing
    {
        if (backDashing)
        {
            StateEntry(PlayerStates.dashing);
        }
        if (isGrounded && !backDashing)
        {
            StateEntry(PlayerStates.walking);
        }
        if (!isGrounded && !backDashing)
        {
            StateEntry(PlayerStates.air);
            //desiredMoveSpeed = moveSpeed;
        }


        bool desiredMoveSpeedChanged = desiredMoveSpeed != lastDesiredMoveSpeed;  //keeping momentum condition
        if (lastState == PlayerStates.dashing) keepMomentum = true;
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
    private void StateEntry(PlayerStates nextState) // OnEnter state logic, this only runs once when changing to a state that you currently are not in
    {
        if (state != nextState)
        {
            switch (nextState)
            {
                case PlayerStates.walking:
                    break;
                case PlayerStates.dashing:
                    // play dashing sound
                    break;
                case PlayerStates.air:
                    break;
            }
            state = nextState;
        }
    }
    private void StateHandler()
    {
        switch (state)
        {
            case PlayerStates.air:
                break;

            case PlayerStates.walking:
                desiredMoveSpeed = moveSpeed;
                break;

            case PlayerStates.dashing:
                rb.drag = 0;
                desiredMoveSpeed = dashSpeed;
                speedChangeFactor = dashSpeedChangeFactor;

                break;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        OnSpawn();
        instance = this;
        currentHealth = maxHealth;
        healthSlider.maxValue = maxHealth;
        currentFuel = maxFuel;
        fuelSlider.maxValue = maxFuel;
        rb = GetComponent<Rigidbody>();
        jetPackParticle.gameObject.SetActive(false);

        //StartCoroutine(Dies);
    }

    // Update is called once per frame
    void Update()
    {
        healthSlider.value = currentHealth;
        fuelSlider.value = currentFuel;
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayerMask);
        if (isGrounded)
        {
            if (state == PlayerStates.walking || !lastGrounded)
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
        StateUpdater();
        StateHandler();

        if (currentHealth <= 0)
        {
            Died();
        }
    }
    private void FixedUpdate()
    {
        MovePlayer();
    }

    void ProcessInputs()
    {
        if (PlayerA)
        {
            horizontal = Input.GetAxisRaw("Horizontal");
            vertical = Input.GetAxisRaw("Vertical");
        }
        else
        {
            //controller axis moves mouse axis as well
            //print("set player b to controller movement");
            horizontal = Input.GetAxis("MovementHorizontal");
            vertical = Input.GetAxis("MovementVertical");
        }
      
        if (Input.GetKeyDown(jump) && isGrounded && !hasJumped && !usingJettpack) //jump 
        {
            Jump();
        }

        initialLaunch = (Input.GetKeyDown(jetPack) && !isGrounded && currentFuel > 0); //check for jettpack input

        if (initialLaunch) //jetpack impulse
        {
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            rb.AddForce(Vector2.up * impulseForce, ForceMode.Impulse);
            currentFuel -= impulseDecrease;
            usingJettpack = true;
            jetCooldownTimer = 0;
        }

        if (Input.GetKey(jetPack) && !isGrounded && currentFuel > 0 && usingJettpack) //jetpack force
        {
            //print("jettpack called");
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
        if (state == PlayerStates.dashing) return;

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
        else if (!isGrounded) //when moving and in air
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
        if (rb.velocity.y <= -0.1 && !usingJettpack && state != PlayerStates.dashing)
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

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (PlayerA)
        {
           //GameManager.instance.A_DamageFlash();
        }
        else
        {
            //G//ameManager.instance.B_DamageFlash();
        }
    }

    public void Died()
    {
        if (PlayerA)
        {
            var spawnPos = GameManager.instance.playerASpawnpoint.position;
            var player = GameManager.instance.playerA;
            GameManager.instance.playerA_Lives--;
            Destroy(this.gameObject);
            var playerGameObject = Instantiate(player, spawnPos, Quaternion.identity);
            playerGameObject.GetComponent<PlayerController>().Respawn(fuelSlider, healthSlider);
        }
        else
        {
            var spawnPos = GameManager.instance.playerBSpawnpoint.position;
            var player = GameManager.instance.playerB;
            GameManager.instance.playerB_Lives--;
            Destroy(this.gameObject);
            var playerGameObject = Instantiate(player, spawnPos, Quaternion.identity);
            playerGameObject.GetComponent<PlayerController>().Respawn(fuelSlider, healthSlider);
        }
    }
    public void OnSpawn()
    {
        if (PlayerA)
        {
            var spawnPos = GameManager.instance.playerASpawnpoint;
            transform.position = spawnPos.position;
            cam.xRotation = 0;
            cam.yRotation = spawnPos.localEulerAngles.y;
        }
        else
        {
            var spawnPos = GameManager.instance.playerBSpawnpoint;
            transform.position = spawnPos.position;
            cam.xRotation = 0;
            cam.yRotation = spawnPos.localEulerAngles.y;
        }
    }
    public void Respawn(Slider fuel, Slider health)
    {
        fuelSlider = fuel;
        healthSlider = health;
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

        while (time < difference)
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
    private IEnumerator Dies(float waitTime)
    {
        //ui pops up
        yield return new WaitForSeconds(waitTime);
       
        //Died();
    }

}
