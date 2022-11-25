using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssetDb : MonoBehaviour
{
    public static AssetDb instance;
    [Header("Buffs")]
    public BuffDetails invincibleBuff;
    public BuffDetails unlimitedFuelBuff;
    public BuffDetails homingRocketBuff;

    void Awake()
    {
        instance = this;
    }

}
