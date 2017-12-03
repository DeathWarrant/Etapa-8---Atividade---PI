using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotgunShotBehaviour2 : MonoBehaviour
{
    public int damage = 0;
    public int angleToCauseDamage = 0;
    public int numberOfRays = 10;
    public float lifeTime = 0.0f;
    public float distanceToCauseDamage = 0.0f;

    private bool readyToShoot = true;
    private float timer = 0.0f;

    private void Start()
    {
        readyToShoot = true;
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
        Vector3 startPos = transform.position; // umm, start position !
        Vector3 targetPosition = Vector3.zero; // variable for calculated end position

        int startAngle = -angleToCauseDamage / 2; // half the angle to the Left of the forward
        int finishAngle = angleToCauseDamage / 2; // half the angle to the Right of the forward

        // the gap between each ray (increment)
        int gapBetweenEachRay = angleToCauseDamage / numberOfRays;

        RaycastHit hit;

        // step through and find each target point
        for (int i = startAngle; i < finishAngle; i += gapBetweenEachRay) // Angle from forward
        {
            targetPosition = (Quaternion.Euler(0, i, 0) * transform.forward).normalized;

            if(Physics.Raycast(transform.position, targetPosition, out hit, distanceToCauseDamage))
            {
                Debug.Log("Hitou: " + hit.collider.name);
            }

            // to show ray just for testing
            Debug.DrawLine(startPos, targetPosition * distanceToCauseDamage, Color.green);
        }
    }
}