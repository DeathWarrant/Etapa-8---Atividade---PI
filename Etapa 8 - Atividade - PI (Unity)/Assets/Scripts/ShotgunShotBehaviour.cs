using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotgunShotBehaviour : MonoBehaviour
{
    public int damage = 0;
    public float angleToCauseDamage = 0;
    public float lifeTime = 0.0f;
    public float distanceToCauseDamage = 0.0f;

    private bool readyToShoot = true;
    private float timer = 0.0f;
    private List<GameObject> enemiesList = new List<GameObject>(0);
    private ParticleSystem particles = null;

    private void Start()
    {
        readyToShoot = true;
        enemiesList = new List<GameObject>(0);
        particles = GetComponent<ParticleSystem>();
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
            if (enemiesList[i].activeInHierarchy)
            {
                Vector3 targetDirection = enemiesList[i].transform.position - transform.position;
                float angle = Vector3.Angle(targetDirection, transform.forward);

                if (angle < angleToCauseDamage)
                {
                    Vector3 direction = enemiesList[i].transform.position - transform.localPosition;
                    float sqrLen = direction.sqrMagnitude;

                    if (sqrLen < distanceToCauseDamage * distanceToCauseDamage)
                    {
                        enemiesList[i].GetComponent<EnemyBehaviour>().DoDamage(damage);
                        Debug.Log("Deu dano");
                    }
                }
            }
        }
    }
}