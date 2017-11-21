using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyBehaviour : MonoBehaviour
{
    public int health = 0;
    public int pointsForKilling = 0;
    public int damage = 0;
    public float enemySpeed = 0.0f;
    public float attackCooldown = 0.0f;
    public float distanceToCauseDamage = 0.0f;

    private bool isAttackOnCooldown = false;
    private float attackCooldownTimer = 0.0f;
    private GameObject player = null;
    private MobilePlayerBehaviour mobilePlayerBehaviour = null;
    private NavMeshAgent navMeshAgent = null;

    private void Start()
    {
        StartComponents();
    }

    private void Update()
    {
        CalculateDistance();
    }

    private void StartComponents()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        mobilePlayerBehaviour = player.GetComponent<MobilePlayerBehaviour>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.speed = enemySpeed;

        StartCoroutine(SetDestination());
    }

    private void CalculateDistance()
    {
        if(!isAttackOnCooldown && player.activeInHierarchy)
        {
            Vector3 distance = player.transform.position - transform.position;
            float sqrLen = distance.sqrMagnitude;
            //Debug.Log(sqrLen);

            if (sqrLen < distanceToCauseDamage * distanceToCauseDamage)
            {
                mobilePlayerBehaviour.DoDamage(damage);
                isAttackOnCooldown = true;
            }
        }
        else if(isAttackOnCooldown)
        {
            attackCooldownTimer += Time.deltaTime;

            if(attackCooldownTimer > attackCooldown)
            {
                attackCooldownTimer = 0.0f;
                isAttackOnCooldown = false;
            }
        }
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
                if (GameControllerBehaviour.gameControllerInstance.AIActive)
                    navMeshAgent.destination = player.transform.position;
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    public void DoDamage(int p_damage)
    {
        health -= p_damage;

        if(health <= 0)
        {
            health = 0;
            GameControllerBehaviour.gameControllerInstance.AddPoints(pointsForKilling);
            GameControllerBehaviour.gameControllerInstance.DecreaseEnemyCounter();
            Destroy(gameObject);
        }
    }
}
