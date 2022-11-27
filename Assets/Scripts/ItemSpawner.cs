using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ItemSpawner : MonoBehaviour
{
    public Transform[] itemsToSpawn;
    public Transform[] spawnLocations;
    public int numberOfItems;
    public float spawnTimer;
    public float spawnMinTimer;
    public float spawnMaxTimer;
    public bool canSpawn = false;

    [Header("WaitTime")]
    public bool waitBeforeSpawn = true;
    public float waitTimer;
    [SerializeField] private float waitTimerSet;
    public float waitTimerMax;
    public float waitTimerMin; 

    public GameObject panel;
    public TMP_Text buffSpawnerText; 
    // Start is called before the first frame update
    void Start()
    {
        spawnTimer = Random.Range(spawnMinTimer, spawnMaxTimer);
        waitTimer = waitTimerSet;
    }

    // Update is called once per frame
    void Update()
    {
        if (!GameManager.instance.gameStarted)
        {
            return;
        }
        
        if (waitBeforeSpawn) //wait before spawning items
        {
            panel.SetActive(false);
            buffSpawnerText.gameObject.SetActive(false);
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0) //wait timer reaches -
            {
                waitTimer = 0f;
                spawnTimer -= Time.deltaTime; //decreases spawn timer; 
                panel.SetActive(true);
                buffSpawnerText.gameObject.SetActive(true);
                buffSpawnerText.text = $"BUFF ITEMS SPAWN IN {spawnTimer.ToString("0")}";
                if (spawnTimer <= 0)
                {
                    numberOfItems = Random.Range(1, 4); //set random number of items to spawn
                    spawnTimer = Random.Range(spawnMinTimer, spawnMaxTimer); //set spawntimer to random 
                    SpawnItems(numberOfItems);
                }
            }
        }
    }
    
    void SpawnItems(int items)
    {
        print(items);
       
        for (int i = 0; i < items; i++)
        {
            var randomItem = Random.Range(0, 4);
            var randomSpawn = Random.Range(0, 3);
            Instantiate(itemsToSpawn[randomItem].gameObject, spawnLocations[randomSpawn].position, Quaternion.identity);
            numberOfItems--;
            print($"spawned {items} items");
            //waitBeforeSpawn = true;
        }
        waitTimer = Random.Range(waitTimerMin, waitTimerMax);
        numberOfItems = 0;
    }
}

