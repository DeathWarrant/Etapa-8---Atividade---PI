using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyBehaviour : MonoBehaviour
{
    [Tooltip("Enemy's base hit points (HP).")]
    public int baseHealth = 0;
    [Tooltip("Points that the player will win for killing this enemy.")]
    public int pointsForKilling = 0;
    [Tooltip("Enemy's base damage.")]
    public int baseDamage = 0;
    [Tooltip("Enemy's base movement speed.")]
    public float baseEnemySpeed = 0.0f;
    [Tooltip("Time between one attack and another.")]
    public float attackCooldown = 0.0f;
    [Tooltip("Enemy's attack range.")]
    public float distanceToCauseDamage = 0.0f;
    public AudioClip attackSound = null;

    private bool isAttackOnCooldown = false;
    private float attackCooldownTimer = 0.0f;
    private AudioSource audioSource = null;
    private GameObject player = null;
    private MobilePlayerBehaviour mobilePlayerBehaviour = null;
    private NavMeshAgent navMeshAgent = null;

    private void Start()
    {
        StartComponents();
    }

    private void Update()
    {
        Attack();
    }

    private void StartComponents()
    {
        player = GameObject.FindGameObjectWithTag("Player");

        if (GameControllerBehaviour.gameControllerInstance.GetGameState() != 2)
        {
            mobilePlayerBehaviour = player.GetComponent<MobilePlayerBehaviour>();
        }

        audioSource = GetComponent<AudioSource>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.speed = baseEnemySpeed;

        StartCoroutine(SetDestination());
    }

    private void Attack()
    {
        if (!isAttackOnCooldown)
        {
            if (GameControllerBehaviour.gameControllerInstance.GetGameState() != 2)
            {
                Vector3 distance = player.transform.position - transform.position;
                float sqrLen = distance.sqrMagnitude;
                //Debug.Log(sqrLen);

                if (sqrLen < distanceToCauseDamage * distanceToCauseDamage)
                {
                    audioSource.PlayOneShot(attackSound, 0.5f);
                    mobilePlayerBehaviour.DoDamage(baseDamage);
                    isAttackOnCooldown = true;
                }
            }
        }
        else if (isAttackOnCooldown)
        {
            attackCooldownTimer += Time.deltaTime;

            if (attackCooldownTimer > attackCooldown)
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
            if (GameControllerBehaviour.gameControllerInstance.GetGameState() != 2 &&
                GameControllerBehaviour.gameControllerInstance.AIActive)
            {
                if (player == null)
                {
                    Debug.LogError("Player not defined.");
                }
                else
                {
                    navMeshAgent.destination = player.transform.position;
                }
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    public void DoDamage(int p_damage)
    {
        baseHealth -= p_damage;

        if (baseHealth <= 0)
        {
            baseHealth = 0;
            GameControllerBehaviour.gameControllerInstance.AddPoints(pointsForKilling);
            GameControllerBehaviour.gameControllerInstance.DecreaseEnemyCounter();
            Destroy(gameObject);
        }
    }

    public void IncreasePower(int p_health, int p_damage, float p_speed)
    {
        baseHealth += p_health;
        baseDamage += p_damage;
        baseEnemySpeed += p_speed;
    }
}
