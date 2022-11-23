using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRocket_Explosion : MonoBehaviour
{
    public float radius = 5.0f;
    public float power = 10.0f;
    public float upwardForce;
    public GameObject rocketExplosionEffect;
    public LayerMask hitMask;
    public LayerMask blockExplosionLayer;
    public float wallRaycastOffset;
    public int maxDamage;
    public int minDamage;

    private void OnTriggerEnter(Collider other)
    {
        //print(other.gameObject);
        Vector3 explosionPos = transform.position;
        Instantiate(rocketExplosionEffect, transform.position, transform.rotation);
        Collider[] colliders = Physics.OverlapSphere(explosionPos, radius, hitMask);
        
        foreach (Collider hit in colliders)
        {
            //Rigidbody rb = hit.GetComponent<Rigidbody>();
            var rb = hit.attachedRigidbody;
            
            if (rb != null)
            {
                //rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
                float distance = Vector3.Distance(explosionPos, hit.transform.position);
                Vector3 shootDir = (hit.transform.position - explosionPos).normalized;
                Vector3 startPos = explosionPos - shootDir * wallRaycastOffset;
                if (!Physics.Raycast(startPos, shootDir, out RaycastHit hitInfo, distance + wallRaycastOffset, blockExplosionLayer)) //if raycast doesnt hit the wall
                {
                    Debug.DrawLine(explosionPos, explosionPos + shootDir * distance, Color.red, 100f);
                    rb.AddExplosionForce(power, explosionPos, radius, upwardForce, ForceMode.Impulse);

                    //do damage to the hitmask layer (player layer)
                    float damage = RemapRange(Mathf.Clamp(distance, 0f, radius), 0f, radius, maxDamage, minDamage);
                    rb.GetComponent<PlayerStatemachine>().TakeDamage(Mathf.RoundToInt(damage));
                    print(damage);
              
                }
            }
        }
        Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        print(collision.gameObject.name);
    }

    public static float RemapRange(float value, float inputA, float inputB, float outputA, float outputB)
    {
        return (value - inputA) / (inputB - inputA) * (outputB - outputA) + outputA;
    }
    private void OnDrawGizmos()
    {
        print("draw sphere gizmos");
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
