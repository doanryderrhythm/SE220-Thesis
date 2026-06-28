using UnityEngine;

public class BuildManager : MonoBehaviour
{
    private CanvasGroup cg;
    private void OnEnable()
    {
        GameEvent.OnOpenBuildMenu += OpenMenu;
        GameEvent.OnCloseBuildMenu += CloseMenu;
    }

    private void OnDisable()
    {
        GameEvent.OnOpenBuildMenu -= OpenMenu;
        GameEvent.OnCloseBuildMenu -= CloseMenu;
    }

    private void Start()
    {
        cg = GetComponent<CanvasGroup>();
        CloseMenu();
    }

    void OpenMenu(PlacementPoint point)
    {
        cg.alpha = 1f;
        cg.interactable = true;
        cg.blocksRaycasts = true;
    }

    void CloseMenu()
    {
        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;
    }
}
