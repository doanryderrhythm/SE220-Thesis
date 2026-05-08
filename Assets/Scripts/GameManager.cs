using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] GameObject player;
    [SerializeField] Transform spawnPoint;
    [SerializeField] Vector2 safePosition;
    [Header("In-game stats")]
    private bool isStarted = false;
    [SerializeField] List<EnemyTimer> timers;
    [SerializeField] List<Enemy> enemies;
    [SerializeField] List<EliteEnemy> eliteEnemies;

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
        GameEvent.OnEnemyKilled += CheckWaveStatus;
        GameEvent.OnWaveStarted += StartWave;
        GameEvent.OnWaveFinished += FinishWave;
    }

    private void OnDisable()
    {
        GameEvent.OnEnemyKilled -= CheckWaveStatus;
        GameEvent.OnWaveStarted -= StartWave;
        GameEvent.OnWaveFinished -= FinishWave;
    }

    void Start()
    {
        timers = new List<EnemyTimer>();
        enemies = new List<Enemy>();
        eliteEnemies = new List<EliteEnemy>();

        safePosition = new Vector2(spawnPoint.position.x, spawnPoint.position.y);
        StartCoroutine(RespawnPlayer());
    }

    // Update is called once per frame
    void Update()
    {
        if (!isStarted &&
            Keyboard.current != null &&
            Keyboard.current.enterKey.wasPressedThisFrame)
        {
            GameEvent.OnWaveStarted?.Invoke();
        }
    }

    public IEnumerator RespawnPlayer()
    {
        yield return new WaitForSeconds(ValueStorer.playerRespawnTime);
        GameObject newPlayer = Instantiate(player, safePosition, Quaternion.identity);
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

    public void StartWave()
    {
        isStarted = true;
    }

    void CheckWaveStatus()
    {
        bool isTimerFinished = timers.Count == 0;
        bool isEnemyFinished = enemies.Count == 0;
        bool isEliteEnemyFinished = eliteEnemies.Count == 0;

        if (!isTimerFinished || !isEnemyFinished || !isEliteEnemyFinished)
            return;

        GameEvent.OnWaveFinished?.Invoke();
    }

    void FinishWave()
    {
        isStarted = false;

        timers.Clear();
        enemies.Clear();
        eliteEnemies.Clear();
    }
}
