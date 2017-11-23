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
    public GameObject player = null;
    public GameObject playerSpawnPoint = null;
    public GameObject enemyToSpawn = null;
    public GameObject[] ammoCrates;
    public GameObject joystickCanvas = null;
    public GameObject menu = null;
    public GameObject playingHUD = null;
    public GameObject endScreen = null;
    public List<Text> leaderboardNames = new List<Text>();
    public List<Text> leaderboardPoints = new List<Text>();

    private string p_name = "PUT";
    private int p_points = 12345;

    private enum States { Menu, Gaming, Dead }
    private States gameState = States.Menu;
    private bool isFirstStart = false;
    private int enemyCounter = 0;
    private int playerPoints = 0;
    private float[] ammoCrateSpawnTimers;
    private APIClient apiClient = null;
    private GameObject enemySpawnPoint = null;
    private MobilePlayerBehaviour mobilePlayerBehaviour = null;
    private Text ammoText = null;
    private Text pointsText = null;
    private Text totalPoints = null;
    private Text healthText = null;

    private void Start()
    {
        StartComponents();
    }

    private void Update()
    {
        UpdateStuff();
    }

    private void StartComponents()
    {
        if (gameControllerInstance == null)
        {
            gameControllerInstance = this;
        }
        else
        {
            Debug.Log("GameController já apresenta uma instância.");
        }
        apiClient = GameObject.FindObjectOfType<APIClient>().GetComponent<APIClient>();
        apiClient.GetLeaderboard();
        StartCoroutine(UpdateMenu());
        gameState = States.Menu;

        isFirstStart = true;
        menu.SetActive(true);
        joystickCanvas.SetActive(false);
        playingHUD.SetActive(false);
        endScreen.SetActive(false);
        player.SetActive(false);
        enemySpawnPoint = GameObject.FindGameObjectWithTag("Enemy Spawn Point");
    }

    private void RestartMenu()
    {
        gameState = States.Menu;
        apiClient.GetLeaderboard();

        GameObject[] enemies = new GameObject[enemyLimit];
        enemies = GameObject.FindGameObjectsWithTag("Enemy");

        for (int i = 0; i < enemies.Length; i++)
        {
            Destroy(enemies[i]);
        }

        if (!player.activeInHierarchy)
        {
            player.SetActive(false);
        }
        else
        {
            Debug.Log("Player já ativo.");
        }

        isFirstStart = false;
        menu.SetActive(true);
        joystickCanvas.SetActive(false);
        playingHUD.SetActive(false);
        endScreen.SetActive(false);
        player.SetActive(false);
    }

    private void StartGameComponents()
    {
        if (!player.activeInHierarchy)
        {
            player.SetActive(true);
        }
        else
        {
            Debug.Log("Player já ativo.");
        }

        gameState = States.Gaming;
        enemyCounter = 0;
        playerPoints = 0;
        menu.SetActive(false);
        joystickCanvas.SetActive(true);
        playingHUD.SetActive(true);
        endScreen.SetActive(false);
        mobilePlayerBehaviour = player.GetComponent<MobilePlayerBehaviour>();
        ammoCrateSpawnTimers = new float[ammoCrates.Length];
        ammoText = playingHUD.transform.Find("Ammo").GetComponent<Text>();
        pointsText = playingHUD.transform.Find("Points").GetComponent<Text>();
        healthText = playingHUD.transform.Find("Health").GetComponent<Text>();
        totalPoints = endScreen.transform.Find("Background").Find("Background").Find("Total Points").GetComponent<Text>();

        for (int i = 0; i < ammoCrateSpawnTimers.Length; i++)
        {
            ammoCrateSpawnTimers[i] = 0.0f;
        }

        StartCoroutine(RespawnGameObjects());
    }

    private void RestartGameComponents()
    {
        gameState = States.Gaming;
        GameObject[] enemies = new GameObject[enemyLimit];
        enemies = GameObject.FindGameObjectsWithTag("Enemy");

        for (int i = 0; i < enemies.Length; i++)
        {
            Destroy(enemies[i]);
        }

        if (!player.activeInHierarchy)
        {
            player.SetActive(true);
        }
        else
        {
            Debug.Log("Player já ativo.");
        }

        enemyCounter = 0;

        player.transform.position = playerSpawnPoint.transform.position;
        player.SetActive(true);
        menu.SetActive(false);
        joystickCanvas.SetActive(true);
        playingHUD.SetActive(true);
        endScreen.SetActive(false);
        mobilePlayerBehaviour.RestartVariables();
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

            for (int i = 0; i < ammoCrates.Length; i++)
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

    private IEnumerator UpdateMenu()
    {
        while (true)
        {
            if (gameState == States.Menu)
            {
                if (apiClient.players.Length != 0)
                {
                    for (int i = 0; i < apiClient.players.Length; i++)
                    {
                        leaderboardNames[i].text = apiClient.players[i].Name;
                        leaderboardPoints[i].text = apiClient.players[i].Points.ToString();

                        Debug.Log(apiClient.players[i].Name);
                    }
                }
            }

            yield return new WaitForSeconds(5.0f);
        }
    }

    private void UpdateStuff()
    {
        if (gameState == States.Gaming)
        {
            ammoText.text = "Ammo: " + mobilePlayerBehaviour.GetAmmo() + "\nMax Ammo: " + mobilePlayerBehaviour.GetMaxAmmo();
            pointsText.text = "Points: " + playerPoints;
            healthText.text = "Health: " + mobilePlayerBehaviour.GetHealth();
        }
        else if (gameState == States.Dead)
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
        gameState = States.Dead;
        menu.SetActive(false);
        joystickCanvas.SetActive(false);
        playingHUD.SetActive(false);
        endScreen.SetActive(true);
    }

    public void YesButton()
    {
        if (isFirstStart)
        {
            if (gameState == States.Menu)
            {
                StartGameComponents();
            }
            else if (gameState == States.Dead)
            {
                apiClient.PostOnLeaderBoard(p_name, p_points);
                RestartMenu();
            }
        }
        else
        {
            if (gameState == States.Menu)
            {
                RestartGameComponents();
            }
            else if (gameState == States.Dead)
            {
                apiClient.PostOnLeaderBoard(p_name, p_points);
                RestartMenu();
            }
        }
    }
}
