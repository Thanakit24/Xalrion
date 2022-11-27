using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDeadBody : MonoBehaviour
{
    // Start is called before the first frame update
    private Rigidbody rb;
    [SerializeField] private float upwardForce;
    [SerializeField] private float upwardMaxForce;
    [SerializeField] private float upwardMinForce;
    [SerializeField] private float minRotateAngle;
    [SerializeField] private float maxRotateAngle;
    [SerializeField] private float rotateAngle;


    void Start()
    {
        rotateAngle = Random.Range(minRotateAngle, maxRotateAngle);
        upwardForce = Random.Range(upwardMinForce, upwardMaxForce);
        transform.Rotate(new Vector3(0f, 0f, rotateAngle) * Time.deltaTime);
        rb = GetComponent<Rigidbody>();
        rb.AddForce(transform.up * upwardForce, ForceMode.Impulse);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
