using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletBehaviour : MonoBehaviour
{
    //public int bulletDamage = 0;

    private void OnCollisionEnter(Collision collision)
    {
        Destroy(gameObject);
    }
}
