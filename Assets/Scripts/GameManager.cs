using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Level Management")]
    [SerializeField] LevelListener levelListener;
    public int levelIndex;
    public LevelData selectedLevel;
    public bool isLevelFinished = false;

    [SerializeField] GameObject playerPrefab;
    [SerializeField] Transform spawnPoint;
    [SerializeField] Vector2 safePosition;
    [Header("In-game stats")]
    public bool isPaused = false;
    public bool isStarted = false;
    [SerializeField] List<EnemyTimer> timers;
    [SerializeField] List<EnemySpawner> enemySpawners;
    [SerializeField] List<Enemy> enemies;
    [SerializeField] List<EliteEnemy> eliteEnemies;

    [Space(10.0f)]
    public PlayerController player;
    public Tower nexusTower;

    public bool isBuildingTower = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;

        GameEvent.OnEnemyKilled += CheckWaveStatus;
        GameEvent.OnWaveStarted += StartWave;
        GameEvent.OnWaveFinished += FinishWave;

        GameEvent.OnRetry += Retry;
        GameEvent.OnRetire += Retire;

        GameEvent.OnGameLost += CheckLose;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        GameEvent.OnEnemyKilled -= CheckWaveStatus;
        GameEvent.OnWaveStarted -= StartWave;
        GameEvent.OnWaveFinished -= FinishWave;

        GameEvent.OnRetry -= Retry;
        GameEvent.OnRetire -= Retire;

        GameEvent.OnGameLost -= CheckLose;

    }

    #region GAMEPLAY

    void Retry()
    {
        Time.timeScale = 1f;
        isLevelFinished = false;
        isStarted = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void Retire()
    {
        Time.timeScale = 1f;
        isLevelFinished = false;
        isStarted = false;
        SceneManager.LoadScene("Main Menu");
    }

    void CheckWin()
    {
        if (!timers.All(timer => timer.isOutOfSessions)
            || nexusTower == null
            || player == null)
            return;

        isLevelFinished = true;
        Debug.Log("You beat the level!");
    }

    void CheckLose()
    {
        List<Enemy> deleteEnemies = new List<Enemy>();
        List<EliteEnemy> deleteEliteEnemies = new List<EliteEnemy>();

        for (int i = enemies.Count - 1; i >= 0; i--)
        {
            if (enemies[i] == null)
                return;

            deleteEnemies.Add(enemies[i]);
            enemies.RemoveAt(i);
        }

        for (int i = eliteEnemies.Count - 1; i >= 0; i--)
        {
            if (eliteEnemies[i] == null)
                return;

            deleteEliteEnemies.Add(eliteEnemies[i]);
            eliteEnemies.RemoveAt(i);
        }

        foreach (Enemy enemy in deleteEnemies)
            Destroy(enemy.gameObject);

        foreach (EliteEnemy enemy in deleteEliteEnemies)
            Destroy(enemy.gameObject);

        isLevelFinished = true;
    }

    #endregion

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!isStarted &&
            Keyboard.current != null &&
            Keyboard.current.enterKey.wasPressedThisFrame)
        {
            TowerBuilder towerBuilder = FindFirstObjectByType<TowerBuilder>();
            if (towerBuilder != null && !towerBuilder.IsBuildMode)
            {
                GameEvent.OnWaveStarted?.Invoke();
            }
        }
    }

    public void ReadyToPlay()
    {
        selectedLevel = levelListener.GetLevel(levelIndex);
        Instantiate(selectedLevel.environment.gameObject);
        spawnPoint = selectedLevel.environment.GetSpawnPoint();
        GameEvent.OnPlayBGM?.Invoke(BGMType.Normal);

        timers = new List<EnemyTimer>();
        enemySpawners = new List<EnemySpawner>();
        enemies = new List<Enemy>();
        eliteEnemies = new List<EliteEnemy>();

        safePosition = new Vector2(spawnPoint.position.x, spawnPoint.position.y);
        StartCoroutine(RespawnPlayer());
    }

    public IEnumerator RespawnPlayer()
    {
        yield return new WaitForSeconds(ValueStorer.playerRespawnTime);
        GameObject newPlayer = Instantiate(playerPrefab, safePosition, Quaternion.identity);
    }

    public void UpdateSafePosition(Vector2 pos)
    {
        safePosition = pos;
    }

    public void InsertTimer(EnemyTimer timer)
    {
        timers.Add(timer);
    }

    public void InsertEnemy(Enemy enemy)
    {
        enemies.Add(enemy);
    }

    public void DeleteEnemy(Enemy enemy)
    {
        enemies.Remove(enemy);
    }

    public void InsertEliteEnemy(EliteEnemy enemy)
    {
        eliteEnemies.Add(enemy);
    }

    public void DeleteEliteEnemy(EliteEnemy enemy)
    {
        eliteEnemies.Remove(enemy);
    }

    public void InsertEnemySpawner(EnemySpawner spawner)
    {
        enemySpawners.Add(spawner);
    }

    public void StartWave()
    {
        if (isBuildingTower)
            return;

        isStarted = true;
        GameEvent.OnPlayBGM?.Invoke(BGMType.Terrain);
    }

    void CheckWaveStatus()
    {
        bool isTimerFinished = timers.All(timer => !timer.isStarted);
        bool isEnemyFinished = enemies.Count == 0;
        bool isEliteEnemyFinished = eliteEnemies.Count == 0;

        if (!isTimerFinished || !isEnemyFinished || !isEliteEnemyFinished)
            return;

        GameEvent.OnWaveFinished?.Invoke();
        GameEvent.OnPlayBGM?.Invoke(BGMType.Normal);
    }

    void FinishWave()
    {
        isStarted = false;

        timers.Clear();
        enemies.Clear();
        eliteEnemies.Clear();

        if (!enemySpawners.All(spawner => spawner.isFinishedSpawn))
            return;
        CheckWin();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "Gameplay")
            return;

        ReadyToPlay();
    }
}
