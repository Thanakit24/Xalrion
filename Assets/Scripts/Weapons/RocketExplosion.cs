using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketExplosion : MonoBehaviour
{
    public float radius = 5.0f;
    public float power = 10.0f;
    public float upwardForce;
    public GameObject rocketExplosionEffect; 
   
    private void OnCollisionEnter(Collision collision)
    {
        Vector3 explosionPos = transform.position;
        Instantiate(rocketExplosionEffect, transform.position, transform.rotation);
        Collider[] colliders = Physics.OverlapSphere(explosionPos, radius);
        
        foreach (Collider hit in colliders)
        {
            //Rigidbody rb = hit.GetComponent<Rigidbody>();
            var rb = hit.attachedRigidbody;
            
            if (rb != null)
            {
                //print("Explode");
                rb.AddExplosionForce(power, explosionPos, radius, upwardForce, ForceMode.Impulse);
            }
        }
        Destroy(gameObject);

    }
    private void OnDrawGizmos()
    {
        print("draw sphere gizmos");
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
