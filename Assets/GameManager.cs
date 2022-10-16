using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public Transform playerASpawnpoint;
    public Transform playerBSpawnpoint;

    public Transform playerA;
    public Transform playerB;

    public int playerALives = 3;
    public int playerBLives = 3;
    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        playerA = playerASpawnpoint;
        playerB = playerBSpawnpoint;
    }

    // Update is called once per frame
    void Update()
    {
        if (playerALives == 0)
        {
            //display player B won
        }

        if (playerBLives == 0)
        {
            //display player A won
        }
        
    }
}
