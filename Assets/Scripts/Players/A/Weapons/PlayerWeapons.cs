using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerWeapons : MonoBehaviour
{
    [Header("Inputs")]
    public KeyCode fire01 = KeyCode.Mouse0;
    public KeyCode fire02 = KeyCode.Mouse1;

    [Header("Rocket")]
    Vector3 targetPoint;
    public GameObject rocket;
    public float recoilForce;
    public float shootForce;
    public bool readyToShoot;
    public float coolDown;
    public bool allowInvoke = false;
    public Camera cam;
    public Transform shootPoint;
    public LayerMask hitLayer;

    [Header("Backdash")]
    public GameObject backdashExplosion;
    public Transform orientation;
    private Rigidbody rb;
    //public float pushBackForce;
    public float backDashForce;
    public float upBackDashForce;
    public float dashcoolDown;
    public float dashcoolDownMax;
    public float dashDuration;
    public float maxDashYSpeed;
    public float dereaseFuel;

    [Header("Backdash Settings")]
    public bool useCameraForward = true;
    public bool allowAllDir = true;
    public bool disableGravity = false;
    public bool resetVel = true;
 

    //Ref
    private PlayerController player;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        player = GetComponent<PlayerController>();
        readyToShoot = true;
    }

    // Update is called once per frame
    void Update()
    {
        FiringCheck();
        // print(player.dashing);
        if (dashcoolDown > 0)
        {
            dashcoolDown -= Time.deltaTime;
        }
    }

    private void FiringCheck()
    {
        if (Input.GetKeyDown(fire01) && readyToShoot)
        {
            Shoot();
        }

        if (Input.GetKeyDown(fire02))
        {
            Backdash();
        }
    }
    private void Shoot()
    {
        readyToShoot = false;
        //Vector3 mousePos = Input.mousePosition;
        //Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, 1000f, hitLayer))
        {
            targetPoint = hit.point;
            //print(hit.transform.name);
        }
        Vector3 direction = targetPoint - shootPoint.position;
     
        GameObject currentBullet = Instantiate(rocket, shootPoint.position, Quaternion.identity);
        currentBullet.transform.forward = direction.normalized;
        Vector3 forceDirection = currentBullet.transform.forward * shootForce;
        //print(currentBullet.transform.forward);

        currentBullet.GetComponent<Rigidbody>().AddForce(forceDirection, ForceMode.Impulse);
        rb.AddForce(-cam.transform.forward * recoilForce, ForceMode.Impulse);

        if (allowInvoke)
        {
            Invoke("ResetShot", coolDown);
            allowInvoke = false;
        }
    }
    private void ResetShot()
    {
        readyToShoot = true;
        allowInvoke = true;
    }

    private void BackdashShot()
    {
        RaycastHit hit;

        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, 1000f, hitLayer))
        {
            targetPoint = hit.point;
            //print(hit.transform.name);
        }
        Vector3 direction = targetPoint - shootPoint.position;

        GameObject currentBullet = Instantiate(backdashExplosion, shootPoint.position, Quaternion.LookRotation(direction)); 
        //currentBullet.transform.forward = direction.normalized;
        //currentBullet.GetComponent<Rigidbody>().AddForce(direction.normalized * shootForce, ForceMode.Impulse);
        //rb.AddForce(-playerCam.forward * recoilForce, ForceMode.Impulse);
    }
    private void Backdash()
    {
        if (dashcoolDown > 0 || player.currentFuel <= 0) return;
        else dashcoolDown = dashcoolDownMax;
        player.usingJettpack = false;
        player.backDashing = true;
        player.maxYSpeed = maxDashYSpeed;

        float xRot = cam.transform.eulerAngles.x;
        float multiplier = 0.1f;
        if (xRot <= 90)
        {
            multiplier = xRot / 90;
        }  
        Vector3 backward = new Vector3(-cam.transform.forward.x, 0, -cam.transform.forward.z);
        Vector3 forceToApply = (backward * backDashForce) + (transform.up * upBackDashForce * multiplier);
        if (xRot == 90) print(cam.transform.up * upBackDashForce * multiplier);

        if (disableGravity)
            rb.useGravity = false; 
        delayedForceToApply = forceToApply;
        BackdashShot();
        Invoke(nameof(DelayedDashForce), 0.025f);
        Invoke(nameof(ResetBackDash), dashDuration);
    }

    private Vector3 delayedForceToApply;

    private void DelayedDashForce()
    {
        player.currentFuel -= dereaseFuel;
        if (resetVel)
            rb.velocity = Vector3.zero;
        player.jetCooldownTimer = 0f;
        rb.AddForce(delayedForceToApply, ForceMode.Impulse);
    }
    private void ResetBackDash()
    {
        player.backDashing = false;
        player.maxYSpeed = 0; 
        if (disableGravity)
            rb.useGravity = true;
    }

    //private Vector3 GetDashDirection(Transform forwardT) //need to get dash dir from cam in viewport
    //{
    //    //float horizontal = Input.GetAxisRaw("Horizontal");
    //    //float vertical= Input.GetAxisRaw("Vertical");

    //    //Vector3 dashDirection = new Vector3();

    //    //if (allowAllDir)
    //    //{
    //    //    dashDirection = forwardT.forward * vertical + forwardT.right * horizontal;
    //    //}
    //    //else
    //    //{
    //    //    dashDirection = forwardT.forward;
    //    //}

    //    //if (vertical == 0 && horizontal == 0)
    //    //{
    //    //    dashDirection = forwardT.forward;
    //    //}
    //    //return dashDirection.normalized;
    //}

    //private void Jet()
    //{
    //    Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
    //    RaycastHit hit;

    //    Vector3 targetPoint;
    //    if (Physics.Raycast(ray, out hit))
    //    {
    //        targetPoint = hit.point;
    //    }
    //    else
    //    {
    //        targetPoint = ray.GetPoint(75);
    //    }
    //    Vector3 flyDirection = targetPoint - shootPoint.position;
    //    rb.AddForce(-targetPoint * pushBackForce, ForceMode.Impulse);

    //}

}
