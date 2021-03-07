using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class EnemySpawnerScript : NetworkBehaviour
{
    public GameObject enemy;
    public Transform[] spawnPoints;

    public float incrementTime = 3;
    public float timer;
    public int enemiesSpawned;

    private void Start()
    {
        incrementTime = 3;
    }

    void Update()
    {
        if (NetworkServer.active)
        {
            incrementTime = Mathf.Clamp(incrementTime,1f,999);
            timer += Time.deltaTime;
            if (timer>= incrementTime)
            {
                enemiesSpawned++;
                if (enemiesSpawned%4==0) { incrementTime -= .1f; }
                incrementTime = Mathf.Clamp(incrementTime, 1f, 999);
                GameObject e = Instantiate(enemy, transform);
                NetworkServer.Spawn(e);
                e.transform.position = spawnPoints[Random.Range(0, spawnPoints.Length)].position;
                e.GetComponent<CharacterController>().enabled = true;
                timer = 0;
            }            
        }

    }
}
