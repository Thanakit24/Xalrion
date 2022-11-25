using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomingRocket : MonoBehaviour
{

    public Transform target;
    public float acceleration = 20f;

    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        Vector3 direction = (transform.position - target.position).normalized;
        rb.AddForce(direction * acceleration, ForceMode.Force);
    }
}
