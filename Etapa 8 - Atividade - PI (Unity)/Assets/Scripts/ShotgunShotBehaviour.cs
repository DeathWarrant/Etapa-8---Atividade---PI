using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotgunShotBehaviour : MonoBehaviour
{
    public int maxDamage = 0;
    public int minDamage = 0;
    public float lifeTime = 0.0f;
    public float distanceToCauseDamage = 0.0f;

    private bool readyToShoot = true;
    private float timer = 0.0f;
    private List<GameObject> enemiesList = new List<GameObject>(0);
    private ParticleSystem particles = null;
    private Transform yAxisReferece = null;

    private void Start()
    {
        readyToShoot = true;
        enemiesList = new List<GameObject>(0);
        particles = GetComponent<ParticleSystem>();
        yAxisReferece = transform;
    }

    private void Update()
    {
        UpdateBullet();
    }

    private void UpdateBullet()
    {
        timer += Time.deltaTime;

        if (readyToShoot)
        {
            Shoot();
            readyToShoot = false;
        }

        if (timer > lifeTime)
        {
            timer = 0.0f;
            readyToShoot = true;
            gameObject.SetActive(false);
        }
    }

    private void Shoot()
    {
        particles.Play();
        DamageEnemies();
    }

    private void DamageEnemies()
    {
        enemiesList = GameControllerBehaviour.gameControllerInstance.GetEnemies();

        for (int i = 0; i < enemiesList.Count; i++)
        {
            if (enemiesList[i] != null)
            {
                if (enemiesList[i].activeInHierarchy)
                {
                    Vector3 direction = enemiesList[i].transform.position - transform.localPosition;
                    float sqrLen = direction.sqrMagnitude;

                    if (sqrLen < distanceToCauseDamage * distanceToCauseDamage)
                    {
                        float dotProduct = Vector3.Dot(transform.forward, direction.normalized);

                        float distanceToCalculateDotComparison = direction.sqrMagnitude > 16.0f ? direction.magnitude / 10.0f : direction.magnitude / 30.0f;
                        float comparisonLerp = Mathf.Lerp(0.25f, 0.9f, distanceToCalculateDotComparison);

                        if (dotProduct > comparisonLerp)
                        {
                            float distanceToCalculateDamage = direction.magnitude / 13.0f;
                            float damage = Mathf.Lerp(maxDamage, minDamage, distanceToCalculateDamage - 0.15f);
                            enemiesList[i].GetComponent<EnemyBehaviour>().DoDamage((int)damage);
                            Debug.Log("Esta na frente");
                        }
                    }
                }
            }
        }
    }
}