using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupItems : MonoBehaviour
{ 
    public float itemDuration; //{ get; protected set; }
    //protected PlayerStatemachine player;
    public float buffTimer;
    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerStatemachine player))
        {
            print("disable game object collider");
            //ApplyItemEffects(player);
        }
    }

    protected virtual void Start()
    {
        
    }
    protected virtual void Update()
    {
        //if (this != null)
        //{
        //    itemDuration -= Time.deltaTime;
        //}

        //if (itemDuration <= 0)
        //{
        //    //DisableItemEffects();
        //    Destroy(gameObject);
        //}
    }
    //public virtual void ApplyItemEffects(PlayerStatemachine player)
    //{
    //    player.buffs.Add(this);
    //}

    //public virtual void DisableItemEffects(PlayerStatemachine player)
    //{

    //}
}
