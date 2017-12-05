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
    public float walkCooldownTime = 0.0f;
    public GameObject damageParticle = null;
    public GameObject deathParticle = null;
    public AudioClip attackSound = null;
    public AudioClip[] walkSounds = null;
    public AudioClip[] randomSounds = null;

    private bool isOnAFKTime = true;
    private bool isAttackOnCooldown = false;
    private bool isWalkOnCooldown = false;
    private bool isRightStep = false;
    private bool randomTimeDecided = false;
    private int health = 0;
    private int randomSoundNumber = 0;
    private int randomSoundTempNumber = 0;
    private float afkTimer = 0.0f;
    private float attackCooldownTimer = 0.0f;
    private float walkCooldownTimer = 0.0f;
    private float randomSoundTimer = 0.0f;
    private float randomSoundTime = 0.0f;
    private Animator animator = null;
    private AudioSource audioSource = null;
    private GameObject player = null;
    private MobilePlayerBehaviour mobilePlayerBehaviour = null;
    private NavMeshAgent navMeshAgent = null;

    private void Awake()
    {
        StartComponents();
    }

    private void Update()
    {
        UpdateStuff();
    }

    private void StartComponents()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        animator = GetComponent<Animator>();
        animator.SetBool("Walk", false);
        animator.SetBool("Idle", true);
        afkTimer = 0.0f;
        isOnAFKTime = true;
        randomSoundNumber = 0;
        randomSoundTempNumber = -1;
        randomTimeDecided = false;
        health = baseHealth;

        if (GameControllerBehaviour.gameControllerInstance.GetGameState() != 2)
        {
            mobilePlayerBehaviour = player.GetComponent<MobilePlayerBehaviour>();
        }

        audioSource = GetComponent<AudioSource>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.speed = baseEnemySpeed;

        StartCoroutine(SetDestination());
    }

    private void RestartComponents()
    {
        health = baseHealth;
        afkTimer = 0.0f;
        isOnAFKTime = true;
        randomSoundNumber = 0;
        randomSoundTempNumber = -1;
        randomTimeDecided = false;

        StartCoroutine(SetDestination());
    }

    private void UpdateStuff()
    {
        if (afkTimer > 1.0f)
        {
            if(isOnAFKTime)
            {
                isOnAFKTime = false;
                navMeshAgent.speed = baseEnemySpeed;
                animator.SetBool("Walk", true);
                animator.SetBool("Idle", false);
            }

            if(!randomTimeDecided)
            {
                if(randomSoundTempNumber < 0)
                {
                    randomSoundNumber = Random.Range(0, randomSounds.Length);
                }
                else
                {
                    do
                    {
                        randomSoundNumber = Random.Range(0, randomSounds.Length);
                    }
                    while (randomSoundNumber == randomSoundTempNumber);
                }
                randomSoundTime = Random.Range(2.5f, 5.0f);
                randomTimeDecided = true;
            }
            else
            {
                randomSoundTimer += Time.deltaTime;

                if(randomSoundTimer > randomSoundTime)
                {
                    randomSoundTimer = 0.0f;
                    audioSource.PlayOneShot(randomSounds[randomSoundNumber], 0.3f);
                    randomSoundTempNumber = randomSoundNumber;
                    //Debug.Log(randomSoundNumber);
                    randomTimeDecided = false;
                }
            }

            if (!isAttackOnCooldown)
            {
                if (GameControllerBehaviour.gameControllerInstance.GetGameState() != 2)
                {
                    Vector3 distance = player.transform.position - transform.position;
                    float sqrLen = distance.sqrMagnitude;
                    //Debug.Log(sqrLen);

                    if (sqrLen < distanceToCauseDamage * distanceToCauseDamage)
                    {
                        audioSource.PlayOneShot(attackSound, 0.1f);
                        animator.SetBool("Walk", false);
                        animator.SetBool("Idle", false);
                        mobilePlayerBehaviour.DoDamage(baseDamage);
                        navMeshAgent.speed = 0.0f;
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
                    navMeshAgent.speed = baseEnemySpeed;
                    animator.SetBool("Walk", true);
                    animator.SetBool("Idle", false);
                    isAttackOnCooldown = false;
                }
            }
        }
        else
        {
            Vector3 directionVector = player.transform.position - transform.position;
            Vector3 newDirectionVector = Vector3.RotateTowards(transform.forward, directionVector, 10, 0);
            transform.rotation = Quaternion.LookRotation(newDirectionVector);
            afkTimer += Time.deltaTime;
            navMeshAgent.speed = 0.0f;            
        }

        if(navMeshAgent.velocity != Vector3.zero)
        {
            if (!isWalkOnCooldown)
            {
                if (isRightStep)
                {
                    audioSource.PlayOneShot(walkSounds[0], 1.0f);
                    isRightStep = false;
                }
                else
                {
                    audioSource.PlayOneShot(walkSounds[1], 1.0f);
                    isRightStep = true;
                }

                isWalkOnCooldown = true;
            }
            else
            {
                walkCooldownTimer += Time.deltaTime;

                if(walkCooldownTimer > walkCooldownTime)
                {
                    walkCooldownTimer = 0.0f;
                    isWalkOnCooldown = false;
                }
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
        health -= p_damage;
        Vector3 damageVector = new Vector3(transform.position.x, transform.position.y + 2.0f, transform.position.z);
        Instantiate(damageParticle, damageVector, transform.rotation);

        if (health <= 0)
        {
            health = 0;
            Instantiate(deathParticle, damageVector, transform.rotation);
            GameControllerBehaviour.gameControllerInstance.AddPoints(pointsForKilling);
            GameControllerBehaviour.gameControllerInstance.DecreaseEnemyCounter();
            Destroy(gameObject);
            //gameObject.SetActive(false);
        }
    }

    public void IncreasePower(int p_health, int p_damage, float p_speed, int p_wave)
    {
        p_health *= p_wave;
        p_damage *= p_wave;
        p_speed *= p_wave;

        health += p_health;
        baseDamage  += p_damage;
        baseEnemySpeed += p_speed;
    }

    public void RestartAI()
    {
        RestartComponents();
    }
}
