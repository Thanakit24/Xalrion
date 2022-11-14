using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasePlayerState : BaseState
{
    public PlayerStatemachine _pc;
    public BasePlayerState(PlayerStatemachine pc): base(pc)
    {
        _pc = pc;
    }
    //public override void OnEnter()
    //{
    //    base.OnEnter();
    //    _pc.jumpForce *= 20;
    //}

    //public override void OnExit()
    //{
    //    base.OnExit();
    //    _pc.jumpForce /= 20;
    //}

    //public override void OnUpdate()
    //{

    //}
    
}
public class GroundMoveState : BasePlayerState
{
    public GroundMoveState(PlayerStatemachine pc) : base(pc) { }
    public float originalDrag;
    public override void OnEnter()
    {
        base.OnEnter();
        originalDrag = _pc.rb.drag;
        _pc.rb.drag = _pc.groundDrag;
    }
    public override void OnUpdate()
    {
        base.OnUpdate();
        if (!_pc.isGrounded)
        {
            _pc.ChangeState(new FallState(_pc));
        }
    }
  
    public override void OnFixedUpdate()
    {
        base.OnFixedUpdate();
        if (_pc.OnSlope()) //when moving on slope and not exiting slope
        {
            Debug.Log("On Slope");
            _pc.rb.useGravity = false;
            _pc.rb.AddForce(_pc.SlopeMoveDirection() * _pc.moveSpeed * 15f, ForceMode.Force);
            //While moving up keeps player grounded/attatched to slope
            if (_pc.rb.velocity.y > 0)
            {
                _pc.rb.AddForce(Vector3.down * 80f, ForceMode.Force);
            }
            //Slope clamping
            if (_pc.rb.velocity.magnitude > _pc.moveSpeed)
            {
                _pc.rb.velocity = _pc.rb.velocity.normalized * _pc.moveSpeed;
            }
        }
        else
        {
            _pc.rb.useGravity = true;
            //The magic number is arbirtrary to scale acceleration with maxVel
            _pc.rb.AddForce(_pc.moveDirection.normalized * _pc.moveSpeed * 10f, ForceMode.Force);
            //Clamping
            Vector3 velocity = new Vector3(_pc.rb.velocity.x, 0f, _pc.rb.velocity.z);
            if (velocity.magnitude > _pc.moveSpeed)
            {
                Vector3 limitVelocity = velocity.normalized * _pc.moveSpeed;
                _pc.rb.velocity = new Vector3(limitVelocity.x, _pc.rb.velocity.y, limitVelocity.z);
            }
        }
       
    }

    public override void OnExit()
    {
        base.OnExit();
        _pc.rb.drag = originalDrag;
    }
}

public class SlopeMoveState : BasePlayerState
{
    public SlopeMoveState(PlayerStatemachine pc) : base(pc)
    {
    }
    public override void OnFixedUpdate()
    {
        base.OnFixedUpdate();
    }
}

public class AirMoveState : BasePlayerState
{

    public AirMoveState(PlayerStatemachine pc): base(pc)
    {
    }

    public override void OnEnter()
    {
        base.OnEnter();
        _pc.rb.useGravity = true;
        
    }
    public override void OnFixedUpdate()
    {
        base.OnFixedUpdate();

        _pc.rb.AddForce(_pc.moveDirection.normalized * _pc.moveSpeed * 15f * _pc.airMultiplier , ForceMode.Force);

        //This clamps the air vel
        var maxSpeed = _pc.maxAirVelocity;
        Vector3 horizVel = new Vector3(_pc.rb.velocity.x, 0f, _pc.rb.velocity.z);
        if (horizVel.magnitude > maxSpeed)
        {
            Vector3 limitVelocity = horizVel.normalized * maxSpeed;
            _pc.rb.velocity = new Vector3(limitVelocity.x, _pc.rb.velocity.y, limitVelocity.z);
        }

        if (_pc.rb.velocity.y <= -0.1)
        {
            //print("Jump fall used");
            _pc.rb.AddForce(Vector3.down * _pc.fallJumpGravity, ForceMode.Impulse);
        }
    }
}
public class JumpState : AirMoveState
{
    public JumpState(PlayerStatemachine pc) : base(pc) 
    {
        duration = 0.2f;
    }

    public override void OnEnter()
    {
        base.OnEnter();
        _pc.bufferedState = new FallState(_pc);

        //_pc.exitingSlope = true;
        _pc.rb.drag = 0f;
        _pc.rb.velocity = new Vector3(_pc.rb.velocity.x, 0f, _pc.rb.velocity.z);
        Vector3 jumpDir = _pc.orientation.forward * _pc.vertical + _pc.orientation.right * _pc.horizontal + _pc.orientation.up; //getting the player's direction
        _pc.rb.AddForce(jumpDir * _pc.jumpForce, ForceMode.Impulse);
        _pc.isGrounded = false;
    }

}
public class FallState : AirMoveState
{
    public FallState(PlayerStatemachine pc) : base(pc) 
    {

    }
    public override void OnUpdate()
    {
        base.OnUpdate();
        if (_pc.isGrounded)
        {
            var groundMoveState = new GroundMoveState(_pc);
            _pc.ChangeState(groundMoveState);
        }
    }

}
public class LaunchState : AirMoveState
{
    public LaunchState(PlayerStatemachine pc) : base(pc)
    {

    }
    public override void OnEnter()
    {
        base.OnEnter();
        Debug.Log("Launch Jetpack");
        _pc.rb.velocity = new Vector3(_pc.rb.velocity.x, 0f, _pc.rb.velocity.z);
        _pc.rb.AddForce(Vector2.up * _pc.impulseForce, ForceMode.Impulse);
        _pc.currentFuel -= _pc.impulseDecrease;
    }
    public override void OnUpdate()
    {
        base.OnUpdate();
        if (_pc.isGrounded)
        {
            var groundMoveState = new GroundMoveState(_pc);
            _pc.ChangeState(groundMoveState);
        }

        //usingJettpack = true;
        //jetCooldownTimer = 0;
    }
}
public class JetpackState : AirMoveState
{
    public JetpackState(PlayerStatemachine pc) : base(pc) { }

    public override void OnFixedUpdate()
    {
        base.OnFixedUpdate();
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
        Debug.Log("in Jettpack, using");
        _pc.rb.useGravity = false;
        //currentFuel -= fuelDecrease * Time.deltaTime;
        _pc.jetPackParticle.gameObject.SetActive(true); //temp use
        Vector3 flyDir = _pc.orientation.forward * _pc.vertical + _pc.orientation.right * _pc.horizontal + _pc.orientation.up; //getting the player's direction
        _pc.rb.AddForce(flyDir * _pc.flyForce * Time.deltaTime, ForceMode.Force);
    }
}

