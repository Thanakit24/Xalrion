using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu( menuName = "Buffs/Agility")]
public class AgilityPickup : BuffDetails
{
    //public float itemDuration;
    public float increaseMoveSpeed;
    public float gainHealthPoints;


    public override void OnPickup(PlayerStatemachine player)
    {
        base.OnPickup(player);
        
        player.moveSpeed += increaseMoveSpeed;
        player.currentHealth = (player.currentHealth + gainHealthPoints<=player.maxHealth) ? player.currentHealth + gainHealthPoints : player.maxHealth;
    }
    public override void OnDrop(PlayerStatemachine player)
    {
        base.OnDrop(player);
        
        player.moveSpeed -= increaseMoveSpeed;
    }

}
