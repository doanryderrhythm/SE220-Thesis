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
        cg.interactable = true;
        cg.blocksRaycasts = true;

        GameManager.Instance.isPaused = true;

        Time.timeScale = 0f;
    }

    public void Resume()
    {
        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;

        GameManager.Instance.isPaused = false;

        Time.timeScale = 1f;
    }

    public void Retry()
    {
        GameManager.Instance.isPaused = false;
        GameEvent.OnRetry.Invoke();
    }

    public void Retire()
    {
        GameManager.Instance.isPaused = false;
        GameEvent.OnRetire.Invoke();
    }
}
