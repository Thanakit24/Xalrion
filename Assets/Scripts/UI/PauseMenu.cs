using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public static bool GameIsPaused = false;

    public GameObject pauseMenuUI;
    public GameObject playerA_UI;
    public GameObject playerB_UI;
    //public GameObject controlsUI;

    void Start()
    {
        pauseMenuUI.SetActive(false);
        Resume();
    }
    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Escape))
        //{
        //    if (GameIsPaused)
        //    {
        //        Resume();
        //    }
        //    else
        //    {
        //        Pause();
        //    }
        //}
    }
    public void Resume()
    {
        //print("Resume called");
        pauseMenuUI.SetActive(false);
        playerA_UI.SetActive(true);
        playerB_UI.SetActive(true);
        Time.timeScale = 1f;
        GameIsPaused = false;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
    public void Pause()
    {
        playerA_UI.SetActive(false);
        playerB_UI.SetActive(false);
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        GameIsPaused = true;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
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