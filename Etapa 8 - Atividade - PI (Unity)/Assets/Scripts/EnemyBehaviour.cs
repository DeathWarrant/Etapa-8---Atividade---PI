using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyBehaviour : MonoBehaviour
{
    public float enemySpeed = 0.0f;

    private int enemyCounter = 0;
    private GameObject player = null;
    private NavMeshAgent navMeshAgent = null;

    void Start()
    {
        StartComponents();
    }

    private void StartComponents()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.speed = enemySpeed;
        StartCoroutine(SetDestination());
    }

    private IEnumerator SetDestination()
    {
        while (true)
        {
            if (player == null)
            {
                Debug.LogError("Player not defined.");
            }
            else
            {
                navMeshAgent.destination = player.transform.position;
            }

            yield return new WaitForSeconds(0.1f);
        }
    }
}
