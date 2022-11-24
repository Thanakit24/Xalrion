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

    //[Header("Respawn A Player")]
    //public GameObject A_respawn;
    //public TMP_Text A_respawnTimer;
    //private float A_maxRespawnTimer = 3;
    //public float A_currentRespawnTimer;
    //public bool A_canSpawn = false;

    //[Header("Respawn B Player")]
    //public GameObject B_respawn;
    //public TMP_Text B_respawnTimer;
    //private float B_maxRespawnTimer = 3;
    //public float B_currentRespawnTimer;

    [Header("Damage Flash")]
    public GameObject A_damageFlash;
    public GameObject B_damageFlash;

    [Header("GameFinish UI")]
    public GameObject gameWonUI;
    public TMP_Text playerText;

    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
    }
    void Start()
    {
        Time.timeScale = 1f;
        gameWonUI.SetActive(false);
        A_damageFlash.SetActive(false);
        B_damageFlash.SetActive(false);
        playerA_Lives = playerA_MaxLives;
        playerB_Lives = playerB_MaxLives;


    }
    // Update is called once per frame
    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.R))
        //{
        //    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        //}

        playerA_LivesDisplay.text = $"LIVES: {playerA_Lives}";
        playerB_LivesDisplay.text = $"LIVES: {playerB_Lives}";

        if (playerA_Lives == 0 || playerB_Lives == 0)
        {
            PlayerWon();

            //get ref from ping pong game
        }
    }
    public void PlayerWon()
    {
        if (playerA_Lives == 0 && playerB_Lives != 0)
        {
            playerText.color = new Color32(255, 76, 76, 255);
            playerText.text = $"RED PLAYER WON";
            //textTest.text = "<color=#E0E300>This is golden!</color>";
            //$"{playerAScore} - {playerBScore}";
        }
        else
        {
            playerText.color = new Color32(0, 149, 255, 255);
            playerText.text = $"BLUE PLAYER WON";

        }
        Time.timeScale = 0f;
        gameWonUI.SetActive(true);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
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
