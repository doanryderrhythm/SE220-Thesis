using UnityEngine;

public class PauseManager : MonoBehaviour
{
    private CanvasGroup cg;

    private void OnEnable()
    {
        GameEvent.OnPaused += Pause;
    }

    private void OnDisable()
    {
        GameEvent.OnPaused -= Pause;
    }

    private void Start()
    {
        cg = GetComponent<CanvasGroup>();

        Resume();
    }

    void Pause()
    {
        cg.alpha = 1f;
        cg.blocksRaycasts = true;

        Time.timeScale = 0f;
    }

    public void Resume()
    {
        cg.alpha = 0f;
        cg.blocksRaycasts = false;

        Time.timeScale = 1f;
    }

    public void Retry()
    {
        GameEvent.OnRetry.Invoke();
    }

    public void Retire()
    {
        GameEvent.OnRetire.Invoke();
    }
}
