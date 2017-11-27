using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoCrateBehaviour : MonoBehaviour
{
    public int ammoAmmount = 0;

    private void OnTriggerEnter(Collider collider)
    {
        if(collider.CompareTag("Player"))
        {
            collider.gameObject.GetComponent<MobilePlayerBehaviour>().AddAmmo(ammoAmmount);
            gameObject.SetActive(false);
        }
    }
}
