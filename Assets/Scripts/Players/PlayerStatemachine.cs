using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerStatemachine : StateMachine
{
    public bool PlayerA;
    public bool isDead;
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
    public float currentHealth;
    public float maxHealth;

    [Header("Dead")]
    public float respawnTimer;
    public float respawnTimerMax;

    [Header("Fuel")]
    public float currentFuel;
    public float maxFuel;
    public float fuelIncrease;
    public float fuelDecrease;

    [Header("Rocket")]
    [HideInInspector] public Vector3 targetPoint;
    public Transform shootPoint;
    public LayerMask hitLayer;
    public GameObject rocketPrefab;
    public GameObject homingRocketPrefab;
    public float recoilForce;
    public float shootForce;
    public bool readyToShoot;
    public float coolDown;
    public bool allowInvoke = false;
    [Header("Backdash")]

    //public GameObject backdashExplosion;
    //public float pushBackForce;
    public GameObject backDashShotPrefab;
    public float backDashForce;
    public float upBackDashForce;
    public float dashcoolDown;
    public float dashcoolDownMax;
    public float dashDuration;
    public float maxDashYSpeed;
    public float dereaseFuel;
    public float jetpackInput;
    public float launchInput;
    [Header("Particles")]
    public ParticleSystem jetPackParticle;
    public ParticleSystem buffParticles; 

    [HideInInspector] public Rigidbody rb;

    [Header("References")]
    public Camera cam;
    public PlayerCam camScript;
    //public List<float> buffs = new List<float>();
    [HideInInspector] public BuffManager buffManager;
    [HideInInspector] public PlayerUI ui;
    public MeshRenderer playerMesh;
    public MeshRenderer armMesh;
    public GameObject face;
    public PlayerInput pi;

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
        pi = GetComponent<PlayerInput>();
        camScript.playerInput = playerInputs;
    }

    protected override void Start()
    {
        base.Start();
        //playerInputs.Disable();
        respawnTimer = respawnTimerMax;
        currentHealth = maxHealth;
        ui.health.maxValue = maxHealth;
        currentFuel = maxFuel;
        ui.fuel.maxValue = maxFuel;

    }
    private void OnEnable()
    {
        playerInputs.Enable();

        //No longer using events because Unity automatically subscribes to On<Action> as seen below
        //playerInputs.Player.Move.performed += ctx => OnMove(ctx);
        //playerInputs.Player.Jump.performed += ctx => OnJump(ctx);
        //playerInputs.Player.Launch.performed += ctx => OnLaunch(ctx);
        //playerInputs.Player.Fire.performed += ctx => OnFireRocket(ctx);
        //playerInputs.Player.Fire2.performed += ctx => OnBackdashShot(ctx);
    }
    private void OnDisable()
    {
        //playerInputs.Player.Move.performed -= SetMove;
        playerInputs.Disable();
    }
    #region Inputs

    void OnMove(InputValue value)
    {
        horizontal = value.Get<Vector2>().x;
        vertical = value.Get<Vector2>().y;
    }

    //public void OnJump(InputAction.CallbackContext ctx)
    public void OnJump(InputValue value)
    {
        var jumpedInput = value.Get<float>();

        if (jumpedInput > 0.1f && currentState is GroundMoveState)
        {
            ChangeState(new JumpState(this));
        }
    }

    void OnLook(InputValue value)
    {
        camScript.lookDir = value.Get<Vector2>();
    }

    //public void OnLaunch(InputValue value)
    //{
    //    launchInput = value.Get<float>();
    //    if (launchInput > 0.1f && currentState is FallState && currentFuel > 0)
    //    {
    //        print("launch state");
    //        ChangeState(new LaunchState(this));
    //    }
    //}
    public void OnJetpack(InputValue value)
    {
        jetpackInput = value.Get<float>();
        if (jetpackInput >= 0.5f && currentFuel > 0 && currentState is FallState)
        {
            //Debug.Log("Jetpack input held");
            ChangeState(new LaunchState(this));
            ChangeState(new JetpackState(this));
        }

        if (jetpackInput <= 0.5f && currentState is JetpackState)
        {
            //Debug.Log("Jetpack input let go");
            ChangeState(new FallState(this));
        }
    }
    public void OnFire(InputValue value)
    {
        var fireInput = value.Get<float>();
        if (fireInput > 0.1f && readyToShoot && GameManager.instance.gameStarted)
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
            if (homing != null)
                homing.target = OppositePerson();    //OppositePerson().transform;
            rb.AddForce(-cam.transform.forward * recoilForce, ForceMode.Impulse);
        }
    }

    public void OnFire2(InputValue value)
    {
        var backDashShotInput = value.Get<float>();
        if (backDashShotInput > 0 && currentFuel > 0 && dashcoolDown <= 0)
        {
            ChangeState(new BackdashState(this));
            //Instantiate(backDashShotPrefab, shootPoint.position, Quaternion.identity);
        }
    }
    private PlayerStatemachine OppositePerson()
    {
        return PlayerA ? GameManager.instance.playerB : GameManager.instance.playerA;
    }

    #endregion
    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        //ProcessInputs();
        ui.health.value = currentHealth;
        ui.fuel.value = currentFuel;
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

        if (currentHealth <= 0)
        {
            Died();
        }

        if (ui.respawningUI.activeSelf) //check if respawning ui is on, if it is start countdown 
        {
            respawnTimer -= 1 * Time.deltaTime;
            pi.DeactivateInput();
            rb.velocity = Vector2.zero;
            playerMesh.enabled = false;
            armMesh.enabled = false;
            face.SetActive(false);
            //Mathf.RoundToInt(gameCountdownTimer);
            ui.respawnTimerText.text = respawnTimer.ToString("0");//countdown ui
            if (respawnTimer <= 0) //countdown over
            {
                respawnTimer = respawnTimerMax;
                ui.respawningUI.gameObject.SetActive(false);
                playerMesh.enabled = true;
                armMesh.enabled = true;
                face.SetActive(true);
                pi.ActivateInput();
                OnSpawn(); //spawn player on spawnpoint 
            }
        }
        

        // OnJetpackHeld();
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
        if (buffManager.HasBuff(AssetDb.instance.invincibleBuff) || ui.respawningUI.activeSelf)
        {
            //print("has invincible buff");
            return;
        }
        currentHealth -= damage;
        Debug.Log("take damage");
        ui.damagedFlash.gameObject.SetActive(true);
        Invoke("ResetFlash", 0.25f);
    }

    public void ResetFlash()
    {
        ui.damagedFlash.gameObject.SetActive(false);
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
        ui.fuel = fuel;
        ui.health = health;
    }

    public void Died()
    {
        if (PlayerA)
        {
            GameManager.instance.playerA_Lives--;
            currentHealth = maxHealth;
            Instantiate(GameManager.instance.playerDeadBody, transform.position, GameManager.instance.playerDeadBody.transform.rotation);
            //call a function to open death ui, start countdown then call on spawn after countdown is done. 
            RespawnUI();
            
        }
        else
        {
            //var spawnPos = GameManager.instance.playerBSpawnpoint.position;
            //var player = GameManager.instance.playerB;
            GameManager.instance.playerB_Lives--;
            currentHealth = maxHealth;
            Instantiate(GameManager.instance.playerDeadBody, transform.position, Quaternion.identity);
            RespawnUI();
            //playerGameObject.GetComponent<PlayerController>().Respawn(fuelSlider, healthSlider);
        }
    }
    public void RespawnUI()
    {
        print("respawn called");
        ui.respawningUI.SetActive(true);
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
