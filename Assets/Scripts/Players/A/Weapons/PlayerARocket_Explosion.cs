using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerARocket_Explosion : MonoBehaviour
{
    public float radius = 5.0f;
    public float power = 10.0f;
    public float upwardForce;
    public GameObject rocketExplosionEffect;
    public LayerMask blockExplosionLayer;

    public int maxDamage;
    public int minDamage;
   
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
                //rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
                float distance = Vector3.Distance(explosionPos, hit.transform.position);
                if (!Physics.Raycast(explosionPos, (hit.transform.position - explosionPos).normalized, distance, blockExplosionLayer.value))
                {
                    rb.AddExplosionForce(power, explosionPos, radius, upwardForce, ForceMode.Impulse);

                    if (rb.gameObject.CompareTag("Player"))
                    {
                        float damage = RemapRange(Mathf.Clamp(distance, 0f, radius), 0f, radius, maxDamage, minDamage);
                        PlayerAController.instance.TakeDamage(Mathf.RoundToInt(damage));
                        print(damage);
                    }
                }
            }
        }
        Destroy(gameObject);
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
