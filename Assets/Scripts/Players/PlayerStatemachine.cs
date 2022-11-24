using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class PlayerStatemachine : StateMachine
{
    public bool PlayerA;
    
    [Header("Inputs")]
    public KeyCode jump = KeyCode.Space;
    public KeyCode jetPack = KeyCode.Space;
    public KeyCode Rocket = KeyCode.Space;
    public KeyCode backDashshot = KeyCode.Space;
    public InputMaster playerInputs;

    [Header("Movement")]
    public Transform orientation;
    public float groundDrag;
    [HideInInspector] public float horizontal;
    [HideInInspector] public float vertical;
    public Vector3 moveDirection;
    public float moveSpeed;
    public float airMultiplier;
    public float maxYSpeed;

    [Header("Jump")]
    [HideInInspector] public float jumpedInput;//temp bool bcuz readvalue only returns float not bool, applies to other input variables too
    public float jumpForce;
    public float maxAirVelocity;
    public float fallJumpGravity;

    [Header("Jetpack")]
    [HideInInspector] public float launchInput; //temp bool bcuz readvalue only returns float not bool, applies to other input variables too
    [HideInInspector] public bool jetpackInput;
    public float impulseForce;
    public float impulseDecrease;
    public float flyForce;
    public float jetCooldownTimer;
    public float jetCooldownMaxTimer;
    public bool usingJetpack = false;

    [Header("GroundCheck")]
    public bool isGrounded = true;
    public Transform groundCheck;
    public float groundCheckRadius;
    public LayerMask groundLayerMask;

    [Header("Slope")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;

    [Header("Health")]
    public Slider healthSlider;
    public float currentHealth;
    public float maxHealth;

    [Header("Fuel")]
    public Slider fuelSlider;
    public float currentFuel;
    public float maxFuel;
    public float fuelIncrease;
    public float fuelDecrease;

    [Header("Rocket")]
    [HideInInspector] public float fireInput;  //temp bool bcuz readvalue only returns float not bool, applies to other input variables too
    public Camera cam;
    [HideInInspector] public Vector3 targetPoint;
    public Transform shootPoint;
    public LayerMask hitLayer;
    public GameObject rocketPrefab;
    [HideInInspector] public GameObject fireRocket;
    public GameObject backDashShotPrefab;
    [HideInInspector] public GameObject backDashShot;
    public float recoilForce;
    public float shootForce;
    public bool readyToShoot;
    public float coolDown;
    public bool allowInvoke = false;

    [Header("Backdash")]
    public GameObject backdashExplosion;
    public float backDashShotInput;
    //public float pushBackForce;
    public float backDashForce;
    public float upBackDashForce;
    public float dashcoolDown;
    public float dashcoolDownMax;
    public float dashDuration;
    public float maxDashYSpeed;
    public float dereaseFuel;

    [Header("Particles")]
    public ParticleSystem jetPackParticle;

    public Rigidbody rb;
   
    public override BaseState DefaultState()
    {
        return new GroundMoveState(this);
    }

    // Start is called before the first frame update
    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody>();
        //var input = GetComponent<PlayerInput>();
        playerInputs = new InputMaster();
    }

    protected override void Start()
    {
        base.Start();
        currentFuel = maxFuel;
        currentHealth = maxHealth;
        fuelSlider.maxValue = maxFuel;
        healthSlider.maxValue = maxHealth;
    }
    private void OnEnable()
    {
        playerInputs.Enable();
        playerInputs.Player.Move.performed += ctx => SetMove(ctx);
        playerInputs.Player.Jump.performed += ctx => OnJump(ctx);
        playerInputs.Player.Launch.performed += ctx => OnLaunch(ctx);
        //playerInputs.Player.Jetpack.performed += OnJetpack;
        //playerInputs.Player.Jetpack.performed += ctx => OnJetpack(ctx);
        playerInputs.Player.Fire.performed += ctx => OnFireRocket(ctx);
        playerInputs.Player.Fire2.performed += ctx => OnBackdashShot(ctx);
        
    }
    private void OnDisable()
    {
        //playerInputs.Player.Move.performed -= SetMove;
        playerInputs.Disable();
    }
    public void SetMove(InputAction.CallbackContext ctx)
    {
        var inputDir = ctx.ReadValue<Vector2>();
        horizontal = inputDir.x;
        vertical = inputDir.y;
        //print("Set input move dir");
    }

    public void OnJump(InputAction.CallbackContext ctx)
    {
        jumpedInput = ctx.ReadValue<float>();
        jumpedInput = 0.1f;
        if (jumpedInput > 0 && currentState is GroundMoveState)
        {
            ChangeState(new JumpState(this));
        }
    }

    public void OnLaunch(InputAction.CallbackContext ctx)
    {
        launchInput = ctx.ReadValue<float>();
        launchInput = 0.1f;
        if (launchInput > 0 && currentState is FallState && currentFuel > 0)
        {
            ChangeState(new LaunchState(this));
        }
    }
    public void OnJetpackHeld()
    {
        float jetpackInput = playerInputs.Player.Jetpack.ReadValue<float>();
        if (jetpackInput != 0f && currentFuel > 0 && currentState is FallState)
        {
            //Debug.Log("Jetpack input held");
            ChangeState(new JetpackState(this));
        }

        if (jetpackInput == 0 && currentState is JetpackState)
        {
            //Debug.Log("Jetpack input let go");
            ChangeState(new FallState(this));
        }
    }
    public void OnFireRocket(InputAction.CallbackContext ctx)
    {
        fireInput = ctx.ReadValue<float>();
        fireInput = 0.1f;
        if (fireInput > 0 && readyToShoot)
        {
            Debug.Log("Fire rocket");
            ChangeState(new RocketState(this));
            fireRocket = Instantiate(rocketPrefab, shootPoint.position, Quaternion.identity);
        }
    }
    public void OnBackdashShot(InputAction.CallbackContext ctx)
    {
        backDashShotInput = ctx.ReadValue<float>();
        backDashShotInput = 0.1f;
        if (backDashShotInput > 0 && currentFuel > 0 && dashcoolDown  <= 0)
        {
            ChangeState(new BackdashState(this));
            backDashShot = Instantiate(backDashShotPrefab, shootPoint.position, Quaternion.identity);
        }
    }
    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        ProcessInputs();
        healthSlider.value = currentHealth;
        fuelSlider.value = currentFuel;
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayerMask);

        if (allowInvoke)
        {
            Invoke("ResetShot", coolDown);
            allowInvoke = false;
        }
        Invoke(nameof(ResetBackDash), dashDuration);

        if (dashcoolDown > 0)
        {
            dashcoolDown -= Time.deltaTime;
        }

        if (!(currentState is JetpackState) && currentFuel < maxFuel) //if not doing anyth and current fuel isnt full
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

        OnJetpackHeld();
        //float jetpackInput = playerInputs.Player.Jetpack.ReadValue<float>();
        //if (jetpackInput != 0f && currentFuel > 0 && currentState is FallState)
        //{
        //    //Debug.Log("Jetpack input held");
        //    ChangeState(new JetpackState(this));
        //}

        //if (jetpackInput == 0 && currentState is JetpackState)
        //{
        //    //Debug.Log("Jetpack input let go");
        //    ChangeState(new FallState(this));
        //}


        //if (jetpackInput == 0f && currentState is JetpackState && !isGrounded)
        //{
        //    ChangeState(new FallState(this));
        //}
        //if (jetpackInput)
        //{
        //    Debug.Log("input held jetpakc");
        //}
        //if (jetpackInput && currentFuel > 0 && currentState is LaunchState)
        //{
        //    ChangeState(new JetpackState(this));
        //}

        //if (!jetpackInput && currentState is JetpackState && !isGrounded)
        //{
        //    ChangeState(new FallState(this));
        //}

    }
    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        moveDirection = orientation.forward * vertical + orientation.right * horizontal;
    }
    private void ProcessInputs()
    {
        //if (PlayerA)
        //{
        //    horizontal = Input.GetAxisRaw("Horizontal");
        //    vertical = Input.GetAxisRaw("Vertical");
        //}
        //else
        //{
        //    //controller axis moves mouse axis as well
        //    //print("set player b to controller movement");
        //    horizontal = Input.GetAxis("MovementHorizontal");
        //    vertical = Input.GetAxis("MovementVertical");
        //}

        //if (Input.GetKeyDown(jump) && currentState is GroundMoveState) //jump 
        //{
        //    var jumpState = new JumpState(this);
        //    ChangeState(jumpState);
        //}

        //if (Input.GetKeyDown(jetPack) &&  currentState is FallState && currentFuel > 0)
        //{
        //    ChangeState(new LaunchState(this));
        //}

        //if (Input.GetKey(jetPack) && currentState is LaunchState && !isGrounded)
        //{
        //    if (currentFuel > 0)
        //    {
        //        print("Set jetpack state");
        //        ChangeState(new JetpackState(this));
        //    }
        //}
        
        //if (!Input.GetKey(jetPack) && currentState is JetpackState && !isGrounded)
        //{
        //    ChangeState(new FallState(this));
        //}

        //if (Input.GetKeyDown(Rocket) && readyToShoot)
        //{
        //    ChangeState(new RocketState(this));
        //    fireRocket = Instantiate(rocketPrefab, shootPoint.position, Quaternion.identity);
        //}

        //if (Input.GetKeyDown(backDashshot) && currentFuel > 0 && dashcoolDown <= 0)
        //{
        //    ChangeState(new BackdashState(this));
        //    backDashShot = Instantiate(backDashShotPrefab, shootPoint.position, Quaternion.identity);
        //}
    }
    private void ResetShot()
    {
        readyToShoot = true;
        allowInvoke = true;
    }

    private void ResetBackDash()
    {
        //backDashing = false;
        maxYSpeed = 0;
        //rb.useGravity = true;
    }
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log("take damage");
        if (PlayerA)
        {
            GameManager.instance.A_DamageFlash();
        }
        else
        {
            GameManager.instance.B_DamageFlash();
        }
    }
    public bool OnSlope()
    {
        //RaycastHit slopeHit;
        if (Physics.Raycast(groundCheck.position, Vector3.down, out slopeHit, 0.3f))
        {
            if (slopeHit.transform.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                Debug.DrawRay(transform.position, Vector3.down, Color.green, 50f); 
                float slopeAngle = Vector3.Angle(Vector3.up, slopeHit.normal);
                return slopeAngle < maxSlopeAngle && slopeAngle != 0;
            }
        }
        return false;
    }
    public Vector3 SlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
    }
   
}
