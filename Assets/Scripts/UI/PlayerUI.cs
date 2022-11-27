using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerUI : MonoBehaviour
{
    [Header("Gameplay")]
    public Slider health;
    public GameObject respawningUI;
    public TMP_Text respawnTimerText;
    public Slider fuel;
    public GridLayoutGroup buffGrid;
    public Image[] buffIcons;
    public Image damagedFlash;

    [Header("JoinGame")]
   
    public TMP_Text checkPlayerReadyText;
    public TMP_Text playerName;
    public Image playerReadyPanel;
    public Image deviceIndicator;
   

    private void Start()
    {
        buffIcons = buffGrid.GetComponentsInChildren<Image>();
    }


}
