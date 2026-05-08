using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class EnemyTimer : MonoBehaviour
{
    [System.Serializable]
    struct Wave
    {
        public float waitTime;
        public GameObject[] enemies;
    }

    [System.Serializable]
    struct WaveSession
    {
        public Wave[] waves;
    }

    public bool isStarted = false;
    private int waveSessionIndex = 0;
    private int waveIndex = 0;

    private float totalWaitTime = 0f;

    [SerializeField] TMP_Text timerText;

    [SerializeField] WaveSession[] waveSessions;

    private void OnEnable()
    {
        GameEvent.OnWaveStarted += StartTimer;
    }

    private void OnDisable()
    {
        GameEvent.OnWaveStarted -= StartTimer;
    }

    void Start()
    {
        for (int i = 0; i < waveSessions.Length; i++)
        {
            for (int j = 0; j < waveSessions[i].waves.Length; j++)
            {
                for (int k = 0; k < waveSessions[i].waves[j].enemies.Length; k++)
                {
                    waveSessions[i].waves[j].enemies[k].SetActive(false);
                }
            }
        }

        GameManager.Instance.InsertTimer(this);
    }

    void Update()
    {
        if (isStarted)
        {
            totalWaitTime += Time.deltaTime;
            timerText.text = (waveSessions[waveSessionIndex].waves[waveIndex].waitTime - totalWaitTime).ToString("0");
            if (totalWaitTime >= waveSessions[waveSessionIndex].waves[waveIndex].waitTime)
            { 
                totalWaitTime -= waveSessions[waveSessionIndex].waves[waveIndex].waitTime;
                EnableEnemies();

                waveIndex += 1;

                if (waveIndex >= waveSessions[waveSessionIndex].waves.Length)
                {
                    waveSessionIndex += 1;
                    waveIndex = 0;

                    isStarted = false;
                    totalWaitTime = 0f;
                }
            }
        }
        else
        {
            if (waveSessionIndex < waveSessions.Length)
                timerText.text = "PAUSED";
            else timerText.text = "OUT OF WAVES";
        }
    }

    void EnableEnemies()
    {
        for (int i = 0; i < waveSessions[waveSessionIndex].waves[waveIndex].enemies.Length; i++)
        {
            waveSessions[waveSessionIndex].waves[waveIndex].enemies[i].SetActive(true);
        }
    }

    void StartTimer()
    {
        if (waveSessionIndex < waveSessions.Length)
            isStarted = true;
    }
}
