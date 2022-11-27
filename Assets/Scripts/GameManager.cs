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
    
    public bool gameStarted = false;

    [SerializeField] private float gameCountdownTimer = 0f;
    [SerializeField] private float gameCountdownMaxTimer; 

    [Header("Player Color")]
    public Material playerAcolor;
    public Material playerArocketColor;
    public Material playerBcolor;
    public Material playerBrocketColor;
    public LayerMask playerAcullingMask;
    public LayerMask playerBcullingMask;

    [Header("Player Rocket")]
    public GameObject playerARocket;
    public GameObject playerBRocket;
    public GameObject playerABackdash;
    public GameObject playerBBackdash;

    [Header("UI")]
    public PlayerUI playerAUI;
    public PlayerUI playerBUI;
    public Sprite KeyboardSprite;
    public Sprite ControllerSprite;
    public TMP_Text gameStartTimer; 


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
        gameStarted = false;
        gameCountdownTimer = gameCountdownMaxTimer;

        PlayerInputManager.instance.onPlayerJoined += OnPlayerJoined;

    }
    // Update is called once per frame
    void Update()
    {
        if (gameStartTimer.gameObject.activeSelf)
        {
            
            gameCountdownTimer -= 1 * Time.deltaTime;
            //Mathf.RoundToInt(gameCountdownTimer);
            gameStartTimer.text = gameCountdownTimer.ToString("0");
            if (gameCountdownTimer <= 0)
            {
                gameCountdownTimer = gameCountdownMaxTimer;
                gameStartTimer.gameObject.SetActive(false);
                SettingPlayers();
            }
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

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
        //if (!canJoin)
        //{
        //    return; 
        //}
        Sprite inputSprite;
        var numberOfActivePlayers = PlayerInput.all.Count;
        print("there are" + numberOfActivePlayers + "players");
       
        //PlayerInputManager.instance.JoinPlayer(1, 0, pInput.currentControlScheme);
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
            player.rocketPrefab = playerARocket;
            player.backDashShotPrefab = playerABackdash;
            player.cam.rect = new Rect(new Vector2(0, 0), new Vector2(0.5f, 1)); //split cam 
            //player.enabled = false; 
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
            SetLayerAllChildren(player.transform, 9);
            player.face.gameObject.layer = 11;
            player.rocketPrefab = playerBRocket;
            player.backDashShotPrefab = playerBBackdash;
            player.cam.rect = new Rect(new Vector2(0.5f, 0), new Vector2(0.5f, 1)); //split cam
            //player.enabled = false;
            //Game has started
            //Start Countdown in UI
        }
        player.OnSpawn();

        if (numberOfActivePlayers == 1) //if 1 player joins
        {
            //wait for other player
            //change ui text to playerX is ready 
            playerA.ui.checkPlayerReadyText.text = $"Ready";
        }
        if (numberOfActivePlayers == 2)
        {
            playerB.ui.checkPlayerReadyText.text = $"Ready";
            //display playerBready
            //invoke a function to start game countdown before start playing.
            Invoke("GameStarting", 1.3f);
        }
        player.ui.deviceIndicator.sprite = inputSprite;
      
    }

    void GameStarting()
    {
        //decrement time counter; 
        //set both players and cam to enabled true when done. 
        print("starting game timer");
        gameStartTimer.gameObject.SetActive(true);
        playerA.ui.checkPlayerReadyText.gameObject.SetActive(false);
        playerA.ui.playerName.gameObject.SetActive(false);
        playerB.ui.checkPlayerReadyText.gameObject.SetActive(false);
        playerB.ui.playerName.gameObject.SetActive(false);
        //set object on
        //set timer object to true;
        //show timer on canvas, reach 0 then disable and enable below 
    }

    void SettingPlayers()
    {
        playerA.ui.playerReadyPanel.gameObject.SetActive(false);
        playerB.ui.playerReadyPanel.gameObject.SetActive(false);
        playerA.ui.gameObject.SetActive(true);
        playerB.ui.gameObject.SetActive(true);

        playerA.enabled = true;
        playerB.enabled = true;
        gameStarted = true;
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

    public void SetLayerAllChildren(Transform root, int layer)
    {
        var children = root.GetComponentsInChildren<Transform>(includeInactive: true);
        foreach (var child in children)
        {
            child.gameObject.layer = layer;
        }
    }
}