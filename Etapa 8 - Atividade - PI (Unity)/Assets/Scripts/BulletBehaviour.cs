using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletBehaviour : MonoBehaviour
{
    [Tooltip("Damage that the bullet causes.")]
    public int bulletDamage = 0;
    [Tooltip("Bullet's speed.")]
    public float bulletSpeed = 0.0f;
    [Tooltip("Bullet's life time.")]
    public float lifeTime = 0.0f;

    private float lifeTimer = 0.0f;

    private Rigidbody rigidBody = null;

    private void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        CountLifeTime();
    }

    private void CountLifeTime()
    {
        rigidBody.velocity = gameObject.transform.forward * bulletSpeed;

        lifeTimer += Time.deltaTime;

        if(lifeTimer > lifeTime)
        {
            rigidBody.velocity = Vector3.zero;
            rigidBody.angularVelocity = Vector3.zero;
            lifeTimer = 0.0f;
            gameObject.SetActive(false);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        rigidBody.velocity = Vector3.zero;
        rigidBody.angularVelocity = Vector3.zero;

        if (collision.gameObject.CompareTag("Enemy"))
        {
            collision.gameObject.GetComponent<EnemyBehaviour>().DoDamage(bulletDamage);
            Debug.Log("Colidiu!");
        }

        lifeTimer = 0.0f;
        gameObject.SetActive(false);
    }
}