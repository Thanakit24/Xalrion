using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class PlayerStatemachine : StateMachine
{
    public bool PlayerA;

    [Header("Inputs")]
    //public KeyCode jump = KeyCode.Space;
    //public KeyCode jetPack = KeyCode.Space;
    //public KeyCode Rocket = KeyCode.Space;
    //public KeyCode backDashshot = KeyCode.Space;
    InputActionAsset asset;
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
    public float jumpForce;
    public float maxAirVelocity;
    public float fallJumpGravity;

    [Header("Jetpack")]
    //[HideInInspector] public bool jetpackInput;
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
    [HideInInspector] public Vector3 targetPoint;
    public Transform shootPoint;
    public LayerMask hitLayer;
    public PlayerRocket_Explosion rocketPrefab;
    public PlayerRocket_Explosion homingRocketPrefab;
    public GameObject backDashShotPrefab;
    [HideInInspector] public GameObject backDashShot;
    public float recoilForce;
    public float shootForce;
    public bool readyToShoot;
    public float coolDown;
    public bool allowInvoke = false;
    [Header("Backdash")]
    public GameObject backdashExplosion;
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

    [HideInInspector] public Rigidbody rb;

    [Header("References")]
    public Camera cam;
    public PlayerCam camScript;
    public List<float> buffs = new List<float>();

    [HideInInspector]public BuffManager buffManager;

   
    public override BaseState DefaultState()
    {
        return new GroundMoveState(this);
    }

    // Start is called before the first frame update
    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody>();
        buffManager = GetComponent<BuffManager>();
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

        //print("set to player a input");
        playerInputs.Player.Move.performed += ctx => SetMove(ctx);
        playerInputs.Player.Jump.performed += ctx => OnJump(ctx);
        playerInputs.Player.Launch.performed += ctx => OnLaunch(ctx);
        playerInputs.Player.Fire.performed += ctx => OnFireRocket(ctx);
        playerInputs.Player.Fire2.performed += ctx => OnBackdashShot(ctx);
    }
    private void OnDisable()
    {
        //playerInputs.Player.Move.performed -= SetMove;
        playerInputs.Disable();
    }
    #region Inputs
    public void SetMove(InputAction.CallbackContext ctx)
    {
        var inputDir = ctx.ReadValue<Vector2>();
        horizontal = inputDir.x;
        vertical = inputDir.y;
        //print("Set input move dir");
    }

    public void OnJump(InputAction.CallbackContext ctx)
    {
        var jumpedInput = ctx.ReadValue<float>();
        
        if (jumpedInput > 0.1f && currentState is GroundMoveState)
        {
            ChangeState(new JumpState(this));
        }
    }

    public void OnLaunch(InputAction.CallbackContext ctx)
    {
        var launchInput = ctx.ReadValue<float>();
        if (launchInput > 0.1f && currentState is FallState && currentFuel > 0)
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
        var fireInput = ctx.ReadValue<float>();
        if  (fireInput > 0 && readyToShoot)
        {
            //Debug.Log("Fire rocket");
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            readyToShoot = false;
            RaycastHit hit;
            //Debug.Log("Rocket state");
            if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, 1000f, hitLayer))
            {
                targetPoint = hit.point;
                //print(hit.transform.name);
            }
            Vector3 direction = targetPoint - shootPoint.position;
            var prefabToInstance = buffManager.HasBuff(AssetDb.instance.homingRocketBuff) ? homingRocketPrefab : rocketPrefab;

            var currentRocket = Instantiate(prefabToInstance, shootPoint.position, Quaternion.identity);
            currentRocket.transform.forward = direction.normalized;
            var homing = currentRocket.GetComponent<HomingRocket>();
            homing.target = OppositePerson().transform;
            rb.AddForce(-cam.transform.forward * recoilForce, ForceMode.Impulse);

           
        }
    }
    private PlayerStatemachine OppositePerson()
    {
        return PlayerA ? GameManager.instance.playerB : GameManager.instance.playerA;
    }
    public void OnBackdashShot(InputAction.CallbackContext ctx)
    {
        var backDashShotInput = ctx.ReadValue<float>();
        if (backDashShotInput > 0 && currentFuel > 0 && dashcoolDown  <= 0)
        {
            ChangeState(new BackdashState(this));
            backDashShot = Instantiate(backDashShotPrefab, shootPoint.position, Quaternion.identity);
        }
    }
#endregion
    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        //ProcessInputs();
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
    }
    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        moveDirection = orientation.forward * vertical + orientation.right * horizontal;
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
        if (buffManager.HasBuff(AssetDb.instance.invincibleBuff))
        {
            //print("has invincible buff");
            return;
        }
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
    public void OnSpawn()
    {
        if (PlayerA)
        {
            var spawnPos = GameManager.instance.playerASpawnpoint;
            transform.position = spawnPos.position;
            camScript.xRotation = 0;
            camScript.yRotation = spawnPos.localEulerAngles.y;
        }
        else
        {
            var spawnPos = GameManager.instance.playerBSpawnpoint;
            transform.position = spawnPos.position;
            camScript.xRotation = 0;
            camScript.yRotation = spawnPos.localEulerAngles.y;
        }
    }
    public void Respawn(Slider fuel, Slider health)
    {
        fuelSlider = fuel;
        healthSlider = health;
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
