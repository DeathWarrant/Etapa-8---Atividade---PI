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
    [Tooltip("Time that will take for health packs to respawn after being picked up.")]
    public float timeToRespawnHealthPack = 0.0f;
    [Tooltip("Time that will take for special weapon crates to respawn after being picked up.")]
    public float timeToRespawnSpecialWeaponCrate = 0.0f;
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
    public AudioClip[] crateSounds = null;

    private enum States { Menu, Gaming, Dead }
    private States gameState = States.Menu;
    private bool isFirstStart = false;
    private int enemyCounter = 0;
    private int playerPoints = 0;
    private int arrayCapacity = 0;
    private int wave = 0;
    private int waveCounter = 0;
    private int enemySpawnerController = 0;
    private float[] ammoCrateSpawnTimers = null;
    private float[] healthPackSpawnTimers = null;
    private float[] specialWeaponCrateSpawnTimers = null;
    private APIClient apiClient = null;
    private AudioSource audioSource = null;
    private GameObject enemySpawnPoint = null;
    private GameObject[] ammoCrates;
    private GameObject[] healthPacks;
    private GameObject[] specialWeaponCrates;
    private List<GameObject> enemiesList = new List<GameObject>(0);
    private MobilePlayerBehaviour mobilePlayerBehaviour = null;
    private DualJoystickTouchContoller joystickTouchController = null;
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
            Destroy(this);
            Debug.Log("GameController já apresenta uma instância.");
        }

        apiClient = GameObject.FindObjectOfType<APIClient>().GetComponent<APIClient>();
        StartCoroutine(UpdateMenu());
        gameState = States.Menu;

        audioSource = GetComponent<AudioSource>();
        mobilePlayerBehaviour = player.GetComponent<MobilePlayerBehaviour>();
        ammoText = playingHUD.transform.Find("Ammo").GetComponent<Text>();
        pointsText = playingHUD.transform.Find("Points").GetComponent<Text>();
        healthText = playingHUD.transform.Find("Health").GetComponent<Text>();
        totalPoints = endScreen.transform.Find("Background").Find("Background").Find("Total Points").GetComponent<Text>();
        inputField = endScreen.transform.Find("Background").Find("Background").Find("InputField Background").Find("InputField Text").GetComponent<InputField>();
        joystickTouchController = joystickCanvas.transform.Find("Touch Controller").GetComponent<DualJoystickTouchContoller>();
        player.transform.position = playerSpawnPoint[0].transform.position;

        isFirstStart = true;
        joystickTouchController.SetIsAbleToTouch(false);
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
        StartCoroutine(UpdateMenu());
        GameObject[] enemies = new GameObject[enemyLimit];
        enemies = GameObject.FindGameObjectsWithTag("Enemy");

        for (int i = 0; i < enemies.Length; i++)
        {
            Destroy(enemies[i]);
        }

        enemiesList.Clear();

        isFirstStart = false;
        menu.SetActive(true);
        joystickCanvas.SetActive(false);
        playingHUD.SetActive(false);
        endScreen.SetActive(false);
        player.transform.position = playerSpawnPoint[0].transform.position;
        apiClient.GetLeaderboard();
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
        enemySpawnerController = 0;
        gameState = States.Gaming;
        enemyCounter = 0;
        playerPoints = 0;
        menu.SetActive(false);
        joystickCanvas.SetActive(true);
        playingHUD.SetActive(true);
        endScreen.SetActive(false);
        joystickTouchController.SetIsAbleToTouch(true);
        ammoCrates = GameObject.FindGameObjectsWithTag("Ammo Crate");
        healthPacks = GameObject.FindGameObjectsWithTag("Health Pack");
        specialWeaponCrates = GameObject.FindGameObjectsWithTag("Special Weapon Crate");
        ammoCrateSpawnTimers = new float[ammoCrates.Length];
        healthPackSpawnTimers = new float[healthPacks.Length];
        specialWeaponCrateSpawnTimers = new float[specialWeaponCrates.Length];
        player.transform.position = playerSpawnPoint[1].transform.position;

        for (int i = 0; i < ammoCrateSpawnTimers.Length; i++)
        {
            ammoCrateSpawnTimers[i] = 0.0f;
        }

        for(int i = 0; i < healthPacks.Length; i++)
        {
            healthPackSpawnTimers[i] = 0.0f;
        }

        for(int i = 0; i < specialWeaponCrates.Length; i++)
        {
            specialWeaponCrateSpawnTimers[i] = 0.0f;
        }

        StartCoroutine(RespawnGameObjects());
    }

    private void RestartGameComponents()
    {
        gameState = States.Gaming;

        if (!player.activeInHierarchy)
        {
            player.SetActive(true);
        }
        else
        {
            Debug.Log("Player já ativo.");
        }

        enemyCounter = 0;
        mobilePlayerBehaviour.RestartVariables();

        wave = 0;
        waveCounter = 0;
        enemySpawnerController = 0;
        player.transform.position = playerSpawnPoint[1].transform.position;
        menu.SetActive(false);
        joystickCanvas.SetActive(true);
        playingHUD.SetActive(true);
        endScreen.SetActive(false);
        joystickTouchController.SetIsAbleToTouch(true);
        playerPoints = 0;

        for (int i = 0; i < ammoCrateSpawnTimers.Length; i++)
        {
            ammoCrateSpawnTimers[i] = 0.0f;
        }

        for (int i = 0; i < healthPacks.Length; i++)
        {
            healthPackSpawnTimers[i] = 0.0f;
        }

        for (int i = 0; i < specialWeaponCrates.Length; i++)
        {
            specialWeaponCrateSpawnTimers[i] = 0.0f;
        }

        StartCoroutine(RespawnGameObjects());
    }

    private IEnumerator RespawnGameObjects()
    {
        while (gameState == States.Gaming)
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

            for(int i = 0; i < specialWeaponCrates.Length; i++)
            {
                if(!specialWeaponCrates[i].activeInHierarchy)
                {
                    specialWeaponCrateSpawnTimers[i] += 2.0f;

                    if(specialWeaponCrateSpawnTimers[i] > timeToRespawnSpecialWeaponCrate)
                    {
                        specialWeaponCrateSpawnTimers[i] = 0.0f;
                        specialWeaponCrates[i].SetActive(true);
                    }
                }
            }

            yield return new WaitForSeconds(2.0f);
        }
    }

    private IEnumerator SpawnEnemyWithDelay()
    {
        yield return new WaitForSeconds(1.0f);

        if (enemiesList.Count < 5)
        {
            GameObject enemy = Instantiate(enemyToSpawn, enemySpawnPoint.transform.position, enemySpawnPoint.transform.rotation);
            enemiesList.Add(enemy);

            if (wave > 0)
            {
                enemy.GetComponent<EnemyBehaviour>().IncreasePower(enemyHealthToAdd, enemyDamageToAdd, enemySpeedToAdd, wave);
            }
        }
        else
        {
            for (int i = 0; i < enemiesList.Count; i++)
            {
                if (enemiesList[i] == null)
                {
                    GameObject enemy = Instantiate(enemyToSpawn, enemySpawnPoint.transform.position, enemySpawnPoint.transform.rotation);
                    enemiesList.Insert(i, enemy);

                    if (wave > 0)
                    {
                        enemy.GetComponent<EnemyBehaviour>().IncreasePower(enemyHealthToAdd, enemyDamageToAdd, enemySpeedToAdd, wave);
                    }

                    break;
                }
            }
        }

        /*if (enemiesList.Count < 5 && enemySpawnerController < 5)
        {
            GameObject enemy = Instantiate(enemyToSpawn, enemySpawnPoint.transform.position, enemySpawnPoint.transform.rotation);
            enemiesList.Add(enemy);

            if (wave > 0)
            {
                enemy.GetComponent<EnemyBehaviour>().IncreasePower(enemyHealthToAdd * wave, enemyDamageToAdd * wave, enemySpeedToAdd * wave);
            }

            enemySpawnerController++;
        }
        else
        {
            for (int i = 0; i < enemiesList.Count; i++)
            {
                if(!enemiesList[i].activeInHierarchy)
                {
                    enemiesList[i].transform.position = enemySpawnPoint.transform.position;
                    enemiesList[i].transform.rotation = enemySpawnPoint.transform.rotation;

                    if (wave > 0)
                    {
                        Debug.Log("Wave: " + wave);
                        enemiesList[i].GetComponent<EnemyBehaviour>().IncreasePower(enemyHealthToAdd * wave, enemyDamageToAdd * wave, enemySpeedToAdd * wave);
                    }

                    enemiesList[i].SetActive(true);
                    enemiesList[i].GetComponent<EnemyBehaviour>().RestartAI();
                    break;
                }
            }
        }*/
    }

    private IEnumerator UpdateMenu()
    {
        while (gameState == States.Menu)
        {
            if (apiClient.players.Length != 0)
            {
                arrayCapacity = apiClient.players.Length < leaderboardNames.Count ? apiClient.players.Length : leaderboardNames.Count;

                for (int i = 0; i < arrayCapacity; i++)
                {
                    leaderboardNames[i].text = apiClient.players[i].Name;
                    leaderboardPoints[i].text = apiClient.players[i].Points.ToString();

                    Debug.Log(apiClient.players[i].Name);
                }
            }

            yield return new WaitForSeconds(2.0f);
        }
    }

    public int GetGameState()
    {
        return (int)gameState;
    }

    public List<GameObject> GetEnemies()
    {
        return enemiesList;
    }

    public void PlayCrateSound(int p_crateID)
    {
        audioSource.PlayOneShot(crateSounds[p_crateID], 0.7f);
    }

    private void UpdateStuff()
    {
        if (gameState == States.Gaming)
        {
            ammoText.text = mobilePlayerBehaviour.GetAmmo() + "\n\n\n\n" + mobilePlayerBehaviour.GetMaxAmmo();
            pointsText.text = "Total Points\n" + playerPoints;
            healthText.text = mobilePlayerBehaviour.GetHealth().ToString();
        }
        else if (gameState == States.Dead)
        {
            totalPoints.text = "Total Points\n" + playerPoints;
        }
    }

    public void DecreaseEnemyCounter()
    {
        enemyCounter--;

        if (waveCounter < enemyWaveSize)
        {
            waveCounter++;
        }

        if(waveCounter >= enemyWaveSize)
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
        joystickTouchController.SetIsAbleToTouch(false);
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
