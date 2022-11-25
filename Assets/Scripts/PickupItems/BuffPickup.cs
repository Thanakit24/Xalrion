using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffPickup : MonoBehaviour
{
    public BuffDetails buffDetails;
    public void OnTriggerEnter(Collider other)
    {

        if (other.transform.parent.TryGetComponent(out BuffManager player))
        {
            print("disable game object collider");
            //ApplyItemEffects(player);
            player.AddBuff(buffDetails);
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
