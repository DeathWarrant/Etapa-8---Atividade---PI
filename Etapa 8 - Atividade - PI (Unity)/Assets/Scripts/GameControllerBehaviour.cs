using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameControllerBehaviour : MonoBehaviour
{
    public GameObject enemyToSpawn = null;

    private int enemyCounter = 0;
    private GameObject enemySpawnPoint = null;

    void Start()
    {
        StartComponents();
    }

    void Update()
    {

    }

    private void StartComponents()
    {
        enemySpawnPoint = GameObject.FindGameObjectWithTag("Enemy Spawn Point");
        StartCoroutine(SetEnemySpawnPosition());
    }

    private IEnumerator SetEnemySpawnPosition()
    {
        while(true)
        {
            if(enemySpawnPoint == null)
            {
                Debug.LogError("Enemy Spawn Point not defined.");
            }
            else
            {
                if (enemyCounter < 5)
                {
                    Vector3 positionVec = new Vector3(Random.Range(-30.0f, 30.0f), 1, Random.Range(-30.0f, 30.0f));
                    enemySpawnPoint.transform.position = positionVec;
                    Instantiate(enemyToSpawn, enemySpawnPoint.transform.position, enemySpawnPoint.transform.rotation);
                    enemyCounter++;
                }
            }

            yield return new WaitForSeconds(1.0f);
        }
    }
}
