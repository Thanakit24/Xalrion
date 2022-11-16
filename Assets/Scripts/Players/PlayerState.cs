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
        _pc.jetPackParticle.gameObject.SetActive(false);
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
        duration = 0.1f;
    }

    public override void OnEnter()
    {
        base.OnEnter();
        _pc.bufferedState = new FallState(_pc);
        Debug.Log("Jump");
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
    public override void OnEnter()
    {
        base.OnEnter();
        _pc.jetPackParticle.gameObject.SetActive(false);
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
        if (_pc.currentFuel > 0)
        {
            _pc.jetCooldownTimer = 0f;
            _pc.rb.velocity = new Vector3(_pc.rb.velocity.x, 0f, _pc.rb.velocity.z);
            _pc.rb.AddForce(Vector2.up * _pc.impulseForce, ForceMode.Impulse);
            _pc.currentFuel -= _pc.impulseDecrease;
        }
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
        _pc.ChangeState(new FallState(_pc));
    }
}
public class JetpackState : AirMoveState
{
    public JetpackState(PlayerStatemachine pc) : base(pc) { }

    public override void OnFixedUpdate()
    {
        base.OnFixedUpdate();
        Debug.Log("in Jettpack, using");
        _pc.rb.useGravity = false;
        _pc.currentFuel -= _pc.fuelDecrease * Time.deltaTime;
        _pc.jetPackParticle.gameObject.SetActive(true); //temp use
        Vector3 flyDir = _pc.orientation.forward * _pc.vertical + _pc.orientation.right * _pc.horizontal + _pc.orientation.up; //getting the player's direction
        _pc.rb.AddForce(flyDir * _pc.flyForce * Time.deltaTime, ForceMode.Force);
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
        if (_pc.currentFuel <= 0)
        {
            _pc.ChangeState(new FallState(_pc));
        }
    }
}
public class RocketState : BasePlayerState
{
    public RocketState(PlayerStatemachine pc): base(pc) 
    {
        duration = 0.5f;
    }

    public override void OnUpdate()
    {
        _pc.rb.velocity = new Vector3(_pc.rb.velocity.x, 0f, _pc.rb.velocity.z);
        _pc.readyToShoot = false;
        RaycastHit hit;
        Debug.Log("Rocket state");
        if (Physics.Raycast(_pc.cam.transform.position, _pc.cam.transform.forward, out hit, 1000f, _pc.hitLayer))
        {
            _pc.targetPoint = hit.point;
            //print(hit.transform.name);
        }
        Vector3 direction = _pc.targetPoint - _pc.shootPoint.position;
        GameObject currentBullet = _pc.fireRocket;
        currentBullet.transform.forward = direction.normalized;
        Vector3 forceDirection = currentBullet.transform.forward * _pc.shootForce;

        currentBullet.GetComponent<Rigidbody>().AddForce(forceDirection, ForceMode.Impulse);
        _pc.rb.AddForce(-_pc.cam.transform.forward * _pc.recoilForce, ForceMode.Impulse);
        _pc.ChangeState(new FallState(_pc));
    }
}

public class BackdashState : BasePlayerState
{
    public BackdashState(PlayerStatemachine pc): base(pc)
    {
        duration = 0.5f;
    }

    private Vector3 delayedForceToApply;
    public override void OnUpdate()
    {
        base.OnUpdate();
        if (_pc.dashcoolDown > 0 || _pc.currentFuel <= 0) return;
        else _pc.dashcoolDown = _pc.dashcoolDownMax;
        //player.usingJettpack = false;
        //player.backDashing = true;
        _pc.maxYSpeed = _pc.maxDashYSpeed;
        float xRot = _pc.cam.transform.eulerAngles.x;
        float multiplier = 0.1f;
        if (xRot <= 90)
        {
            multiplier = xRot / 90;
        }
        Vector3 backward = new Vector3(-_pc.cam.transform.forward.x, 0, -_pc.cam.transform.forward.z);
        Vector3 forceToApply = (backward * _pc.backDashForce) + (_pc.transform.up * _pc.upBackDashForce * multiplier);
        if (xRot == 90) //print(_pc.cam.transform.up * _pc.upBackDashForce * multiplier);
        _pc.rb.useGravity = false;
        delayedForceToApply = forceToApply;
        BackdashShot();
        float time = 0f;
        float maxTime = 0.025f;
        time = maxTime;
        time -= Time.deltaTime;
        if (time >= 0)
        {
            //Debug.Log("Called delayed dash force");
            DelayedDashForce();
        }
        
        //Invoke(nameof(DelayedDashForce), 0.025f);
    }
    private void DelayedDashForce()
    {
        _pc.currentFuel -= _pc.dereaseFuel;
        _pc.rb.velocity = Vector3.zero;
        _pc.jetCooldownTimer = 0f;
        _pc.rb.AddForce(delayedForceToApply, ForceMode.Impulse);
        
    }

    private void BackdashShot()
    {
        RaycastHit hit;
        if (Physics.Raycast(_pc.cam.transform.position, _pc.cam.transform.forward, out hit, 1000f, _pc.hitLayer))
        {
            _pc.targetPoint = hit.point;
            //print(hit.transform.name);
        }
        Vector3 direction = _pc.targetPoint - _pc.shootPoint.position;
        GameObject currentBullet = _pc.backDashShot;
        _pc.ChangeState(new FallState(_pc));


        //currentBullet.transform.forward = direction.normalized;
        //currentBullet.GetComponent<Rigidbody>().AddForce(direction.normalized * shootForce, ForceMode.Impulse);
        //rb.AddForce(-playerCam.forward * recoilForce, ForceMode.Impulse);
    }
}

public class DeadState : BasePlayerState
{
    public DeadState(PlayerStatemachine pc) : base(pc)
    {

    }
  
}
