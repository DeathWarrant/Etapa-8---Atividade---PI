using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialWeaponCrateBehaviour : MonoBehaviour
{
    public int weaponID = 0;
    public int ammoAmmount = 0;

    private void OnTriggerEnter(Collider collider)
    {
        if(collider.CompareTag("Player"))
        {
            if(weaponID == 0)
            {
                Debug.LogError("Weapon ID 0? Muda isso aí, fera.");
            }
            else if(ammoAmmount == 0)
            {
                Debug.LogError("Ammo Ammount 0? Muda isso aí, fera.");
            }
            else if(weaponID != 0 && ammoAmmount != 0)
            {
                collider.gameObject.GetComponent<MobilePlayerBehaviour>().ChangeWeapon(weaponID, ammoAmmount);
                gameObject.SetActive(false);
            }
        }
    }
}
