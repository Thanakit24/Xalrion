using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public PlayerStatemachine playerA = null;
    public PlayerStatemachine playerB = null;

    [Header("Player Color")]
    public Material playerAcolor;
    public Material playerArocketColor;
    public Material playerBcolor;
    public Material playerBrocketColor;

    public LayerMask playerAcullingMask;
    public LayerMask playerBcullingMask;

    [Header("UI")]
    public PlayerUI playerAUI;
    public PlayerUI playerBUI;
    public Sprite KeyboardSprite;
    public Sprite ControllerSprite;


    [Header("Player Spawns")]
    public Transform playerASpawnpoint;
    public Transform playerBSpawnpoint;


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

        PlayerInputManager.instance.onPlayerJoined += OnPlayerJoined;

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



    private void OnPlayerJoined(PlayerInput pInput)
    {
        Sprite inputSprite;
        if (pInput.currentControlScheme == "Keyboard and mouse")
            inputSprite = KeyboardSprite;
        else //"Gamepad"
            inputSprite = ControllerSprite;

        var player = pInput.GetComponent<PlayerStatemachine>();
        if (!playerA) //if isnt player A? nani?
        {
            playerA = player;
            player.PlayerA = true;
            player.ui = playerAUI;
            player.playerMesh.material = playerAcolor;
            player.armMesh.material = playerArocketColor;
            player.cam.cullingMask = playerAcullingMask;
            //player.game
            player.cam.rect = new Rect(new Vector2(0, 0), new Vector2(0.5f, 1));
            //Locate player in spawnPoint
        }
        else
        {
            playerB = player;
            player.PlayerA = false;
            player.ui = playerBUI;
            player.playerMesh.material = playerBcolor;
            player.armMesh.material = playerBrocketColor;
            //player.gameObject.layer = 9;
            player.cam.cullingMask = playerBcullingMask;
            player.face.gameObject.layer = 11;
            player.cam.rect = new Rect(new Vector2(0.5f, 0), new Vector2(0.5f, 1));
            //Game has started
            //Start Countdown in UI
        }
        player.ui.gameObject.SetActive(true);
        player.ui.checkPlayerReadyText.gameObject.SetActive(false);
        player.ui.deviceIndicator.sprite = inputSprite;
        player.OnSpawn();
    }

    private void OnDisable()
    {

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


}