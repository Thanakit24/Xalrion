using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatemachine : StateMachine
{
    public bool PlayerA;

    [Header("Inputs")]
    public KeyCode jump = KeyCode.Space;
    public KeyCode jetPack = KeyCode.Space;
  
    [Header("Movement")]
    public Transform orientation;
    public float groundDrag;
    public float horizontal;
    public float vertical;
    public Vector3 moveDirection;
    public float moveSpeed;
    public float airMultiplier;
    public float maxYSpeed;

    [Header("Jump")]
    public float jumpForce;
    public float maxAirVelocity;
    public float fallJumpGravity;

    [Header("Jetpack")]
    public float impulseForce;
    public float impulseDecrease;
    public float flyForce;
    public float jetCooldownTimer;
    public float jetCooldownMaxTimer;
    public bool usingJetpack = false;
    
    [Header("GroundCheck")]
    public bool isGrounded;
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
    // Start is called before the first frame update
    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody>();
    }
    public override BaseState DefaultState()
    {
        GroundMoveState groundMoveState = new GroundMoveState(this);
        return groundMoveState;
    }
    protected override void Start()
    {
        base.Start();
        currentHealth = maxHealth;
        healthSlider.maxValue = maxHealth;
        currentFuel = maxFuel;
        fuelSlider.maxValue = maxFuel;
        jetPackParticle.gameObject.SetActive(false);
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
    }
    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        moveDirection = orientation.forward * vertical + orientation.right * horizontal;
    }
    private void ProcessInputs()
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

        if (Input.GetKeyDown(jump) && currentState is GroundMoveState) //jump 
        {
            var jumpState = new JumpState(this);
            ChangeState(jumpState);
        }

        if (Input.GetKeyDown(jetPack) &&  currentState is FallState && currentFuel > 0)
        {
            ChangeState(new LaunchState(this));
        }

        if (Input.GetKey(jetPack) && currentState is LaunchState && !isGrounded)
        {
            if (currentFuel > 0)
            {
                print("Set jetpack state");
                ChangeState(new JetpackState(this));
            }
        }
        
        if (!Input.GetKey(jetPack) && currentState is JetpackState && !isGrounded)
        {
            ChangeState(new FallState(this));
        }

        if (Input.GetKeyDown(KeyCode.Mouse0) && readyToShoot)
        {
            ChangeState(new RocketState(this));
            fireRocket = Instantiate(rocketPrefab, shootPoint.position, Quaternion.identity);
        }

        if (Input.GetKeyDown(KeyCode.Mouse1) && currentFuel > 0 && dashcoolDown <= 0)
        {
            ChangeState(new BackdashState(this));
            backDashShot = Instantiate(backDashShotPrefab, shootPoint.position, Quaternion.identity);
        }
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
