using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Player Spawns")]
    public Transform playerASpawnpoint;
    public Transform playerBSpawnpoint;

    [Header("Player Prefabs")]
    public GameObject playerA;
    public GameObject playerB;

    [Header("PlayerALives")]
    public TMP_Text playerA_LivesDisplay;
    public bool playerAdied = false;
    public int playerA_Lives;
    public int playerA_MaxLives = 3;

    [Header("PlayerBLives")]
    public TMP_Text playerB_LivesDisplay;
    public bool playerBdied = false;
    public int playerB_Lives;
    public int playerB_MaxLives = 3;

    [Header("Respawn")]
    public GameObject A_respawn;
    public GameObject B_respawn;
    public TMP_Text respawnTimer;
    private float maxRespawnTimer = 3f;
    public float currentRespawnTimer;

    [Header("Damage Flash")] 
    public GameObject A_damageFlash;
    public GameObject B_damageFlash;

   
    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
    }
    void Start()
    {
        A_damageFlash.SetActive(false);
        B_damageFlash.SetActive(false);
        playerA_Lives = playerA_MaxLives;
        playerB_Lives = playerB_MaxLives;

       
    }
    // Update is called once per frame
    void Update()
    {
        playerA_LivesDisplay.text = $"LIVES: {playerA_Lives}";
        playerB_LivesDisplay.text = $"LIVES: {playerB_Lives}";

        if (playerA_Lives == 0 && playerB_Lives < 0)
        {
            //set win ui to active then set who won with string
            //get ref from ping pong game
        }

        if (playerB_Lives == 0 && playerA_Lives < 0)
        {
            //display player A won
        }
        
    }
   
    public void A_PlayRespawn()
    {
        currentRespawnTimer = maxRespawnTimer;
        currentRespawnTimer -= Time.deltaTime;
        A_respawn.SetActive(true);
       
    }



    //cringe shit no cap
    public void A_DamageFlash()
    {
        A_damageFlash.SetActive(true);
        Invoke("A_ResetFlash", 0.25f);
    }
    public void A_ResetFlash()
    {
        A_damageFlash.SetActive(false);
    }
    public void B_DamageFlash()
    {
        B_damageFlash.SetActive(true);
        Invoke("B_ResetFlash", 0.25f);
    }
    public void B_ResetFlash()
    {
        B_damageFlash.SetActive(false);
    }
}
