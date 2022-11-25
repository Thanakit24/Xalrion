using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffManager : MonoBehaviour
{
    float AGilityTimer;
    public Dictionary<BuffDetails, float> buffs = new Dictionary<BuffDetails, float>();
    private PlayerStatemachine player;
    public BuffDetails test;
    // Start is called before the first frame update
    void Awake()
    {
        player = GetComponent<PlayerStatemachine>();
        //AddBuff(test);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void FixedUpdate()
    {
        List<BuffDetails> buffsToRemove = new List<BuffDetails>();
        foreach (var buffDetails in buffs)
        {
            if (buffs[buffDetails.Key] <= 0) //get value in the key. The water inside the bucket. 
            {
                buffsToRemove.Add(buffDetails.Key);
            }
            else
            {
                buffs[buffDetails.Key] -= Time.fixedDeltaTime; //if got water, then decrease it
            }
        }
        foreach (var buffToRemove in buffsToRemove)
        {
            buffs.Remove(buffToRemove);
            buffToRemove.OnDrop(player);
        }
    }

    public void AddBuff(BuffDetails buff)
    {
        //AGilityTimer += buff.itemduration;
        if (!buffs.ContainsKey(buff))
        {
            buffs.Add(buff, 0);
            buff.OnPickup(player);
        }
        buffs[buff] += buff.duration;
    }

    public bool HasBuff(BuffDetails buff)
    {
        return buffs.ContainsKey(buff);
    }
}
