using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="Buff.asset", menuName ="Buffs/Standard")]
public class BuffDetails : ScriptableObject
{
    public float duration;
    public Sprite icon;
    public string description;
    /// <summary>
    /// Treat this like a static function to perform side effects on the the player
    /// </summary>
    /// <param name="player"></param>
    public virtual void OnPickup(PlayerStatemachine player)
    {
        Debug.Log($"{player.name}: Pickup up {name}");
        player.buffParticles.gameObject.SetActive(true);
    }
    public virtual void OnDrop(PlayerStatemachine player)
    {
        Debug.Log($"{player.name}: Finished up {name}");
        player.buffParticles.gameObject.SetActive(false);
    }

}
