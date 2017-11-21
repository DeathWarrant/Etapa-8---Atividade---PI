using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameControllerBehaviour : MonoBehaviour
{
    public static GameControllerBehaviour gameControllerInstance { get; set; }

    public bool AIActive = true;
    public int enemyLimit = 0;
    public float timeToRespawnAmmoCrate = 0.0f;
    public GameObject enemyToSpawn = null;
    public GameObject[] ammoCrates;
    public GameObject joystickCanvas = null;
    public GameObject playingHUD = null;
    public GameObject endScreen = null;
    public Text ammoText = null;
    public Text pointsText = null;
    public Text totalPoints = null;
    public Text healthText = null;

    private bool isEndScreen = false;
    private int enemyCounter = 0;
    private int playerPoints = 0;
    private float[] ammoCrateSpawnTimers;
    private GameObject enemySpawnPoint = null;
    private MobilePlayerBehaviour mobilePlayerBehaviour = null;

    void Start()
    {
        StartComponents();
    }

    void Update()
    {
        UpdateStuff();
    }

    private void StartComponents()
    {
        gameControllerInstance = this;
        endScreen.SetActive(false);
        isEndScreen = false;
        mobilePlayerBehaviour = GameObject.FindGameObjectWithTag("Player").GetComponent<MobilePlayerBehaviour>();
        enemySpawnPoint = GameObject.FindGameObjectWithTag("Enemy Spawn Point");
        enemyCounter = 0;
        playerPoints = 0;
        ammoCrateSpawnTimers = new float[ammoCrates.Length];

        for (int i = 0; i < ammoCrateSpawnTimers.Length; i++)
        {
            ammoCrateSpawnTimers[i] = 0.0f;
        }

        StartCoroutine(RespawnGameObjects());
    }

    private IEnumerator RespawnGameObjects()
    {
        while (true)
        {
            if (enemySpawnPoint == null)
            {
                Debug.LogError("Enemy Spawn Point not defined.");
            }
            else
            {
                if (enemyCounter < enemyLimit)
                {
                    Vector3 positionVec = new Vector3(Random.Range(-30.0f, 30.0f), 1, Random.Range(-30.0f, 30.0f));
                    enemySpawnPoint.transform.position = positionVec;
                    GameObject enemy = Instantiate(enemyToSpawn, enemySpawnPoint.transform.position, enemySpawnPoint.transform.rotation);
                    enemyCounter++;
                }
            }

            for(int i = 0; i < ammoCrates.Length; i++)
            {
                if (!ammoCrates[i].activeInHierarchy)
                {
                    ammoCrateSpawnTimers[i] += 2.0f;

                    if (ammoCrateSpawnTimers[i] > timeToRespawnAmmoCrate)
                    {
                        ammoCrateSpawnTimers[i] = 0.0f;
                        ammoCrates[i].SetActive(true);
                    }
                }
            }

            yield return new WaitForSeconds(2.0f);
        }
    }

    private void UpdateStuff()
    {
        if (!isEndScreen)
        {
            ammoText.text = "Ammo: " + mobilePlayerBehaviour.GetAmmo() + "\nMax Ammo: " + mobilePlayerBehaviour.GetMaxAmmo();
            pointsText.text = "Points: " + playerPoints;
            healthText.text = "Health: " + mobilePlayerBehaviour.GetHealth();
        }
        else
        {
            totalPoints.text = "Total Points: " + playerPoints;
        }
    }

    public void DecreaseEnemyCounter()
    {
        enemyCounter--;
    }

    public void AddPoints(int p_points)
    {
        playerPoints += p_points;
    }

    public void ShowEndScreen()
    {
        isEndScreen = true;
        joystickCanvas.SetActive(false);
        playingHUD.SetActive(false);
        endScreen.SetActive(true);
    }

    public void YesButton()
    {
        SceneManager.LoadScene("Prototype");
    }

    public void NoButton()
    {

    }
}
