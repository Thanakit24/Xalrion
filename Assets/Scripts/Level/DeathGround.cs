using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathGround : MonoBehaviour
{
    public float upwardForce = 15f;
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<PlayerStatemachine>().TakeDamage(20);
            collision.gameObject.GetComponent<Rigidbody>().AddForce(Vector3.up * 9f, ForceMode.Impulse);
        }
    }
}
