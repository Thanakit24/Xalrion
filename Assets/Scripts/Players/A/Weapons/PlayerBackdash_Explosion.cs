using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBackdash_Explosion : MonoBehaviour
{
    public float radius = 5.0f;
    public float power = 10.0f;
    public float upwardForce;
    public GameObject backDashExplosionEffect;
    public LayerMask hitMask;
    public LayerMask blockExplosionLayer;
    public int damage = 70;

    private void Start()
    {
        Vector3 explosionPos = transform.position;
        Instantiate(backDashExplosionEffect, transform.position, transform.rotation);
        Collider[] colliders = Physics.OverlapSphere(explosionPos, radius, hitMask);

        foreach (Collider hit in colliders)
        {
            //Rigidbody rb = hit.GetComponent<Rigidbody>();
            var rb = hit.attachedRigidbody;

            if (rb != null)
            {
                float distance = Vector3.Distance(explosionPos, hit.transform.position);
                if (!Physics.Raycast(explosionPos, (hit.transform.position - explosionPos).normalized, distance, blockExplosionLayer)) //if raycast doesnt hit the wall
                                                                                                                                             
                rb.AddExplosionForce(power, explosionPos, radius, upwardForce, ForceMode.Impulse);
                rb.GetComponent<PlayerController>().TakeDamage(damage);
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
