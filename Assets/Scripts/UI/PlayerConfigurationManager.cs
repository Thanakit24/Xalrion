using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
public class PlayerConfigurationManager : MonoBehaviour
{
    public List<PlayerConfiguration> playerConfigs;

    [SerializeField] private int maxPlayers = 2;

    public static PlayerConfigurationManager instance { get; private set; }

    private void Awake()
    { 
        if(instance != null)
        {
            Debug.Log("Creating another instance of singleton");
            //Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(instance);
            playerConfigs = new List<PlayerConfiguration>();
        }
    }

    public void SetPlayerColor(int index, Material color)
    {
        playerConfigs[index].playerMaterial = color;

    }

    public void ReadyPlayer(int index)
    {
        playerConfigs[index].isReady = true; 
        if (playerConfigs.Count == maxPlayers && playerConfigs.All(p => p.isReady == true)) //if all player configs in this collection is ready is true and maxplayer equals to 2
        {
            SceneManager.LoadScene("Level");
        }
    }

    public void HandlePlayerJoin(PlayerInput pi)
    {
        Debug.Log("Player joined" + pi.playerIndex);
        
        if (!playerConfigs.Any(p => p.playerIndex == pi.playerIndex)) //checking the index if didnt already add this player
        {
            pi.transform.SetParent(transform);
            playerConfigs.Add(new PlayerConfiguration(pi));
        }
        //setting players as a child of this object to transition them to the other scene to not lose the input assigned etc.
    }
}
public class PlayerConfiguration
{
    public PlayerConfiguration(PlayerInput pi)
    {
        playerIndex = pi.playerIndex; //setting player index to the first input
        Input = pi; //setting input to the index;
    }

    public Material playerMaterial { get; set; }
    public PlayerInput Input { get; set; }
    public int playerIndex { get; set; }
    public bool isReady { get; set; }

}
