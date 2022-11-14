using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    [Header("Jump")]
    public float jumpForce;
    public float maxAirVelocity;
    public float fallJumpGravity;

    [Header("Jetpack")]
    public float currentFuel;
    public float impulseForce;
    public float impulseDecrease;
    public float flyForce;
    
    [Header("GroundCheck")]
    public bool isGrounded;
    public Transform groundCheck;
    public float groundCheckRadius;
    public LayerMask groundLayerMask;

    [Header("Slope")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;

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
    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        ProcessInputs();
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayerMask);
        
        //if (isGrounded)
        //{
        //         //do if state = moving later
        //    {
        //        //hasJumped = false;
              
        //        //exitingSlope = false;
        //    }
        //}
        //else
        //{
        //    rb.drag = 0;

        //    //if (rb.velocity.y <= -0.1 && //!usingJettpack && state != PlayerStates.dashing)
        //    //{
        //    //    //print("Jump fall used");
        //    //    //rb.AddForce(Vector3.down * fallJumpGravity, ForceMode.Impulse);
        //    //}
        //}
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

        if (Input.GetKeyDown(jetPack) &&  currentState is FallState)
        {
            ChangeState(new LaunchState(this));
        }

        if (Input.GetKey(jetPack) && currentState is FallState)
        {
            ChangeState(new JetpackState(this));
        }
        else
        {
            ChangeState(new FallState(this));
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
