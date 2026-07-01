using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public void StartLevel(int index)
    {
        GameManager.Instance.levelIndex = index;
        SceneManager.LoadScene("Gameplay");
    }

    private void Awake()
    {
        GameEvent.OnPlayBGM?.Invoke(BGMType.MainMenu);
    }
}
