using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameControllerBehaviour : MonoBehaviour
{
    public static GameControllerBehaviour gameControllerInstance { get; set; }

    [Tooltip("If deactivated, enemies will no longer walk towards the player. (They will still cause damage if the player gets next to them)")]
    public bool AIActive = true;
    [Tooltip("Max number of enemies that will be spawned. (Example: If this number is 5, no more enemies will spawn if there are already 5 enemies alive.)")]
    public int enemyLimit = 0;
    [Tooltip("Max number of enemies to be killed for the wave to change.")]
    public int enemyWaveSize = 0;
    [Tooltip("Number of Health that is added to the enemy's Base Health when the wave grows stronger.")]
    public int enemyHealthToAdd = 0;
    [Tooltip("Number of Damage that is added to the enemy's Base Damage when the wave grows stronger.")]
    public int enemyDamageToAdd = 0;
    [Tooltip("Number of Speed that is added to the enemy's Base Speed when the wave grows stronger.")]
    public float enemySpeedToAdd = 0.0f;
    [Tooltip("Time that will take for ammo crates to respawn after being picked up.")]
    public float timeToRespawnAmmoCrate = 0.0f;
    public float timeToRespawnHealthPack = 0.0f;
    [Tooltip("Não mexe aqui seu poura, nem nas variáveis de baixo.")]
    public GameObject player = null;
    [Tooltip("Poura, disse para não mexer, por que desceu aqui?")]
    public GameObject[] playerSpawnPoint = null;
    [Tooltip("Ainda descendo né...")]
    public GameObject enemyToSpawn = null;
    [Tooltip("Descendo ainda -.-'")]
    public GameObject joystickCanvas = null;
    [Tooltip("Poura, ta vacilando em...")]
    public GameObject menu = null;
    [Tooltip("Vacilaum morre cedo em...")]
    public GameObject playingHUD = null;
    [Tooltip("Já disse para não mexer aqui em baixo.")]
    public GameObject endScreen = null;
    [Tooltip("O que ta fazendo aqui afinal de contas?")]
    public List<Text> leaderboardNames = new List<Text>();
    [Tooltip("Fico imaginando por que está tão curioso em vir aqui? -.-' Sai daqui meu.")]
    public List<Text> leaderboardPoints = new List<Text>();

    private enum States { Menu, Gaming, Dead }
    private States gameState = States.Menu;
    private bool isFirstStart = false;
    private int enemyCounter = 0;
    private int playerPoints = 0;
    private int arrayCapacity = 0;
    private int wave = 0;
    private int waveCounter = 0;
    private float[] ammoCrateSpawnTimers;
    private float[] healthPackSpawnTimers;
    private APIClient apiClient = null;
    private GameObject enemySpawnPoint = null;
    private GameObject[] ammoCrates;
    private GameObject[] healthPacks;
    private MobilePlayerBehaviour mobilePlayerBehaviour = null;
    private ParticleSystem enemySpawnParticleSystem = null;
    private Text ammoText = null;
    private Text pointsText = null;
    private Text totalPoints = null;
    private Text healthText = null;
    private InputField inputField = null;

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
        StartCoroutine(UpdateMenu());
        gameState = States.Menu;

        ammoText = playingHUD.transform.Find("Ammo").GetComponent<Text>();
        pointsText = playingHUD.transform.Find("Points").GetComponent<Text>();
        healthText = playingHUD.transform.Find("Health").GetComponent<Text>();
        totalPoints = endScreen.transform.Find("Background").Find("Background").Find("Total Points").GetComponent<Text>();
        inputField = endScreen.transform.Find("Background").Find("Background").Find("InputField Background").Find("InputField Text").GetComponent<InputField>();
        player.transform.position = playerSpawnPoint[0].transform.position;

        isFirstStart = true;
        menu.SetActive(true);
        joystickCanvas.SetActive(false);
        playingHUD.SetActive(false);
        endScreen.SetActive(false);
        player.SetActive(true);
        enemySpawnPoint = GameObject.FindGameObjectWithTag("Enemy Spawn Point");
        enemySpawnParticleSystem = enemySpawnPoint.GetComponent<ParticleSystem>();
        apiClient.GetLeaderboard();
    }

    private void RestartMenu()
    {
        gameState = States.Menu;

        GameObject[] enemies = new GameObject[enemyLimit];
        enemies = GameObject.FindGameObjectsWithTag("Enemy");

        for (int i = 0; i < enemies.Length; i++)
        {
            Destroy(enemies[i]);
        }

        isFirstStart = false;
        menu.SetActive(true);
        joystickCanvas.SetActive(false);
        playingHUD.SetActive(false);
        endScreen.SetActive(false);
        apiClient.GetLeaderboard();
        player.transform.position = playerSpawnPoint[0].transform.position;
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

        wave = 0;
        waveCounter = 0;
        gameState = States.Gaming;
        enemyCounter = 0;
        playerPoints = 0;
        menu.SetActive(false);
        joystickCanvas.SetActive(true);
        playingHUD.SetActive(true);
        endScreen.SetActive(false);
        mobilePlayerBehaviour = player.GetComponent<MobilePlayerBehaviour>();
        ammoCrates = GameObject.FindGameObjectsWithTag("Ammo Crate");
        healthPacks = GameObject.FindGameObjectsWithTag("Health Pack");
        ammoCrateSpawnTimers = new float[ammoCrates.Length];
        healthPackSpawnTimers = new float[healthPacks.Length];
        player.transform.position = playerSpawnPoint[1].transform.position;

        for (int i = 0; i < ammoCrateSpawnTimers.Length; i++)
        {
            ammoCrateSpawnTimers[i] = 0.0f;
        }

        for(int i = 0; i < healthPacks.Length; i++)
        {
            healthPackSpawnTimers[i] = 0.0f;
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

        wave = 0;
        waveCounter = 0;
        player.transform.position = playerSpawnPoint[1].transform.position;
        menu.SetActive(false);
        joystickCanvas.SetActive(true);
        playingHUD.SetActive(true);
        endScreen.SetActive(false);
        mobilePlayerBehaviour.RestartVariables();
        playerPoints = 0;

        for (int i = 0; i < ammoCrateSpawnTimers.Length; i++)
        {
            ammoCrateSpawnTimers[i] = 0.0f;
        }

        for (int i = 0; i < healthPacks.Length; i++)
        {
            healthPackSpawnTimers[i] = 0.0f;
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
                    Vector3 positionVec = new Vector3(Random.Range(-30.0f, 30.0f), 0, Random.Range(-30.0f, 30.0f));
                    enemySpawnPoint.transform.position = positionVec;
                    enemySpawnParticleSystem.Play();
                    StartCoroutine(SpawnEnemyWithDelay());
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

            for(int i = 0; i < healthPacks.Length; i++)
            {
                if(!healthPacks[i].activeInHierarchy)
                {
                    healthPackSpawnTimers[i] += 2.0f;

                    if(healthPackSpawnTimers[i] > timeToRespawnHealthPack)
                    {
                        healthPackSpawnTimers[i] = 0.0f;
                        healthPacks[i].SetActive(true);
                    }
                }
            }

            yield return new WaitForSeconds(2.0f);
        }
    }

    private IEnumerator SpawnEnemyWithDelay()
    {
        yield return new WaitForSeconds(1.0f);

        GameObject enemy = Instantiate(enemyToSpawn, enemySpawnPoint.transform.position, enemySpawnPoint.transform.rotation);

        if (wave > 0)
        {
            enemy.GetComponent<EnemyBehaviour>().IncreasePower(enemyHealthToAdd * wave, enemyDamageToAdd * wave, enemySpeedToAdd * wave);
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
                    arrayCapacity = apiClient.players.Length < leaderboardNames.Capacity ? apiClient.players.Length : leaderboardNames.Capacity;

                    for (int i = 0; i < arrayCapacity; i++)
                    {
                        leaderboardNames[i].text = apiClient.players[i].Name;
                        leaderboardPoints[i].text = apiClient.players[i].Points.ToString();

                        Debug.Log(apiClient.players[i].Name);
                    }
                }
            }

            yield return new WaitForSeconds(2.0f);
        }
    }

    public int GetGameState()
    {
        return (int)gameState;
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

        if (waveCounter < enemyWaveSize)
        {
            waveCounter++;
        }
        else
        {
            wave++;
            waveCounter = 0;
        }
    }

    public void AddPoints(int p_points)
    {
        playerPoints += p_points;
    }

    public void ShowEndScreen()
    {
        gameState = States.Dead;
        player.transform.position = playerSpawnPoint[0].transform.position;
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
                string playerName = inputField.text;
                apiClient.PostOnLeaderBoard(playerName, playerPoints);
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
                string playerName = inputField.text;
                apiClient.PostOnLeaderBoard(playerName, playerPoints);
                RestartMenu();
            }
        }
    }
}
