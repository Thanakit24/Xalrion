using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BuffManager : MonoBehaviour
{
    float AGilityTimer;
    public Dictionary<BuffDetails, float> buffs = new Dictionary<BuffDetails, float>();
    private PlayerStatemachine player;
    //public BuffDetails test;
    // Start is called before the first frame update
    void Awake()
    {
        player = GetComponent<PlayerStatemachine>();
        //AddBuff(test);
    }

    private void Update()
    {
        List<BuffDetails> buffsToRemove = new List<BuffDetails>();
        var buffList = buffs.Keys.ToArray();
        int i = 0;
        
        for (; i<buffList.Length; i++)
        {
            var buffDetails = buffList[i];
            if (buffs[buffDetails] <= 0) //get value in the key. The water inside the bucket. 
            {
                buffsToRemove.Add(buffDetails);
            }
            else
            {
                buffs[buffDetails] -= Time.deltaTime; //if got water, then decrease it
            }
            player.ui.buffIcons[i].gameObject.SetActive(true); //setting active buffs onto ui
            player.ui.buffIcons[i].sprite = buffDetails.icon;
            player.ui.buffIcons[i].color = new Color(1, 1, 1, KongrooUtils.RemapRange(buffs[buffDetails], 0, buffDetails.duration, 0, 1));
        }

        for(; i<player.ui.buffIcons.Length; i++)
            player.ui.buffIcons[i].gameObject.SetActive(false);


        foreach (var buffToRemove in buffsToRemove)
        {
            buffs.Remove(buffToRemove);
            buffToRemove.OnDrop(player);
            player.buffParticles.gameObject.SetActive(false);
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
