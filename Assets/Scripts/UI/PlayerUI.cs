using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerUI : MonoBehaviour
{
    public Slider health;
    public Slider fuel;
    public Image damagedFlash;
    public TextMeshProUGUI checkPlayerReadyText;
    public Image deviceIndicator;
    public GridLayoutGroup buffGrid;
    public Image[] buffIcons;

    private void Start()
    {
        buffIcons = buffGrid.GetComponentsInChildren<Image>();
    }


}
