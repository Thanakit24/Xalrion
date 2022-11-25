using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu( menuName = "Buffs/Firerate")]
public class FireratePickup : BuffDetails
{
    //public float itemDuration;
    public float newCooldown;
 
    public override void OnPickup(PlayerStatemachine player)
    {
        base.OnPickup(player);
        player.coolDown -= newCooldown;
    }
    public override void OnDrop(PlayerStatemachine player)
    {
        base.OnDrop(player);
        player.coolDown += newCooldown;
    }

}
