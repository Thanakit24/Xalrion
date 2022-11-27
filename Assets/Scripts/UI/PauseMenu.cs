using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    public static PauseMenu Instance;
    public static bool GameIsPaused = false;
    private InputMaster pInput;
    private InputAction action;
    public EventSystem eventSys;
    public GameObject pauseMenuUI;
    public GameObject playerA_UI;
    public GameObject playerB_UI;
    public GameObject resumeButton;
    //public GameObject controlsUI;

    private void Awake()
    {
        Instance = this;
        pauseMenuUI.SetActive(false);
        pInput = new InputMaster();
    }

    private void OnEnable()
    {
        action = pInput.UI.Pause;
        action.Enable();
    }

    private void OnDisable()
    {
        action.Disable();
    }
    public void Init()
    {
        //Resume();
        //GameManager.instance.playerA.playerInputs.UI.Pause.performed += ctx => PauseResumeButton(ctx);
        //GameManager.instance.playerB.playerInputs.UI.Pause.performed += ctx => PauseResumeButton(ctx);
        //GameManager.instance.playerB.playerInputs.Player.Pause.performed += ctx => Pause(ctx);
        //GameManager.instance.playerB.playerInputs.Player.Pause.performed += ctx => Resume(ctx);
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && GameManager.instance.gameStarted) //temp 
        {
            PauseResumeButton();
        }
    }
    //public void Resume(InputAction.CallbackContext ctx) 
    //{
    //    if (pauseMenuUI.activeSelf)
    //    {

    //    }

    //    //resume overrides pause. 
    //}
    public void ResumeOnClick()
    {
        //print("Resume called");
        eventSys.firstSelectedGameObject = null;
        pauseMenuUI.SetActive(false);
        playerA_UI.SetActive(true);
        playerB_UI.SetActive(true);
        GameManager.instance.playerA.playerInputs.Player.Enable();
        Time.timeScale = 1f;
        GameIsPaused = false;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
    public void PauseResumeButton(/*InputAction.CallbackContext ctx*/)
    {
        if (/*ctx.performed && */!pauseMenuUI.activeSelf)
        {
            print("pause on");
            eventSys.firstSelectedGameObject = resumeButton;
            //print("resume button set as first");
            playerA_UI.SetActive(false);
            playerB_UI.SetActive(false);
            pauseMenuUI.SetActive(true);
            GameManager.instance.playerA.playerInputs.Player.Disable();
            Time.timeScale = 0f;
            GameIsPaused = true;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

        }
        else
        {
            //print("Resume called");
            eventSys.firstSelectedGameObject = null;
            pauseMenuUI.SetActive(false);
            playerA_UI.SetActive(true);
            playerB_UI.SetActive(true);
            GameManager.instance.playerA.playerInputs.Player.Enable(); //insert player b below
            Time.timeScale = 1f;
            GameIsPaused = false;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

        }
    }

    public void Close()
    {

    }

    public void Controls()
    {

    }

    public void Menu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}