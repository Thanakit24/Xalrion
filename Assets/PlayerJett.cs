using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJett : MonoBehaviour
{
    public float pushBackForce;
    public bool canUse = true;
    public float currentFuel;
    public float maxFuel;
    private Rigidbody rb;

    public Transform direction;

    // Start is called before the first frame update
    void Start()
    {
        currentFuel = maxFuel;
        //rb = GetComponent<Rigidbody>();

    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKey(KeyCode.Mouse1) && canUse)
        {
            rb.AddForce(-direction.forward * pushBackForce, ForceMode.Force);
            currentFuel -= Time.deltaTime;
        }
    }
}
