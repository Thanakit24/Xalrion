using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    public BuffPickup[] itemsToSpawn;
    public Transform[] spawnLocations;
    public float spawnTimer;
    public float spawnMaxTimer;
    public bool itemSpawned = false;
    // Start is called before the first frame update
    void Start()
    {
        spawnTimer = spawnMaxTimer;
    }

    // Update is called once per frame
    void Update()
    {
        if (!itemSpawned)
        {
            spawnTimer -= Time.deltaTime;
            //print(spawnTimer);

            if (spawnTimer <= 0)
            {
                //spawn shit randomly, random items, random spawn locations 
                print("run func");
                itemSpawned = true;
                var randomItem = Random.Range(0, 1);
                var randomSpawn = Random.Range(0, 1);
                //Debug.Log(randomItem);
                //Debug.Log(randomSpawn);
                Instantiate(itemsToSpawn[randomItem].gameObject, spawnLocations[randomSpawn].position, Quaternion.identity);
                //Debug.Log($"item {randomItem} spawned at {randomSpawn} location");
                
            }
        }
    }
}
