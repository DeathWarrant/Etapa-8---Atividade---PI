using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeathCrateBehaviour : MonoBehaviour
{
    public int healthAmmount = 0;

    private int playerHealth = 0;
    private int lessHealthAmmount = 0;

    private void OnTriggerEnter(Collider collider)
    {
        if(collider.CompareTag("Player"))
        {
            playerHealth = collider.gameObject.GetComponent<MobilePlayerBehaviour>().GetHealth();

            if (playerHealth < 100)
            {
                if (playerHealth + healthAmmount <= 100)
                {
                    collider.gameObject.GetComponent<MobilePlayerBehaviour>().AddHealth(healthAmmount);
                    GameControllerBehaviour.gameControllerInstance.PlayCrateSound(0);
                }
                else
                {
                    lessHealthAmmount = 100 - playerHealth;
                    collider.gameObject.GetComponent<MobilePlayerBehaviour>().AddHealth(lessHealthAmmount);
                    GameControllerBehaviour.gameControllerInstance.PlayCrateSound(0);
                }

                gameObject.SetActive(false);
            }
        }
    }
}
