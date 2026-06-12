using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;

public class TowerBuilder : MonoBehaviour
{
    [Header("Tower Prefabs")]
    [SerializeField] private GameObject normalTowerPrefab;
    [SerializeField] private GameObject piercingTowerPrefab;
    [SerializeField] private GameObject sniperTowerPrefab;

    [Header("Build Settings")]

    [SerializeField] private LevelData levelData;

    [Header("UI ")]
    [SerializeField] private GameObject buildUIPanel;
    [SerializeField] private TMP_Text description;


    private bool isBuildMode = false;
    private List<GameObject> aliveTowers = new List<GameObject>();
    private Rigidbody2D rb;

    private int maxTowers => levelData != null ? levelData.maxTowers : 3;
    [Header("Tower Sprite")]
    [SerializeField] private Image normalTowerSprite;
    [SerializeField] private Image piercingTowerSprite;
    [SerializeField] private Image sniperTowerSprite;

    private GameObject targetTower;
    private PlacementPoint placementPoint;
    private int selectedIndex = 0;
    private int currentIndex = 0;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            Debug.Log("E pressed");

            if (!isBuildMode)
            {
                Debug.Log("Enter Build Mode");
                EnterBuildMode();
            }
            else
            {
                Debug.Log("Exit Build Mode");
                ConfirmTower();
                ExitBuildMode();
            }
        }
        if (isBuildMode)
        {
            if (Keyboard.current.digit1Key.wasPressedThisFrame)
            {
                Debug.Log(1);
                ToggleTowerSelection(1);
            }

            if (Keyboard.current.digit2Key.wasPressedThisFrame)
            {
                Debug.Log(2);
                ToggleTowerSelection(2);
            }

            if (Keyboard.current.digit3Key.wasPressedThisFrame)
            {
                Debug.Log(3);
                ToggleTowerSelection(3);
            }
        }
    }

    int AliveCount()
    {
        aliveTowers.RemoveAll(t => t == null);
        return aliveTowers.Count;
    }

    void EnterBuildMode()
    {
        if (placementPoint == null)
            return;
        if (GameManager.Instance.isStarted)
            return;

        isBuildMode = true;
        GameManager.Instance.isBuildingTower = true;
        if (rb != null) rb.linearVelocityX = 0f;
        if (buildUIPanel != null) buildUIPanel.SetActive(true);
        ResetTowerBorders();
        selectedIndex = 0;
        currentIndex = 0;
        ShowCurrentTower();
    }

    void ExitBuildMode()
    {
        isBuildMode = false;
        GameManager.Instance.isBuildingTower = false;
        if (buildUIPanel != null) buildUIPanel.SetActive(false);
    }

    public void PlaceTower(TowerType type, Transform placement)
    {
        if (AliveCount() >= maxTowers)
        {
            return;
        }

        GameObject prefab = GetPrefabForType(type);
        if (prefab == null)
        {
            return;
        }

        Vector3 spawnPos = new Vector3(placement.position.x, transform.position.y + 0.5f, 0);

        GameObject newTower = Instantiate(prefab, spawnPos, Quaternion.identity);
        aliveTowers.Add(newTower);

        ExitBuildMode();
    }

    GameObject GetPrefabForType(TowerType type)
    {
        if (type == TowerType.Piercing) return piercingTowerPrefab;
        if (type == TowerType.Sniper) return sniperTowerPrefab;
        return normalTowerPrefab;
    }

    public bool IsBuildMode => isBuildMode;

    public void OnTowerDestroyed()
    {
        aliveTowers.RemoveAll(t => t == null);
    }
    void ResetTowerBorders()
    {
        normalTowerSprite.color = Color.gray;
        piercingTowerSprite.color = Color.gray;
        sniperTowerSprite.color = Color.gray;
    }
    void ShowCurrentTower()
    {
        if (placementPoint == null)
            return;

        GameObject tower = placementPoint.CurrentTower;

        if (tower == null)
            return;

        Tower towerScript = tower.GetComponent<Tower>();

        if (towerScript == null)
            return;

        switch (towerScript.towerType)
        {
            case TowerType.Normal:
                currentIndex = 1;
                selectedIndex = 1;
                targetTower = normalTowerPrefab;
                normalTowerSprite.color = Color.white;
                description.text = "Normal Tower";
                break;

            case TowerType.Piercing:
                currentIndex = 2;
                selectedIndex = 2;
                targetTower = piercingTowerPrefab;
                piercingTowerSprite.color = Color.white;
                description.text = "Piercing Tower";
                break;

            case TowerType.Sniper:
                currentIndex = 3;
                selectedIndex = 3;
                targetTower = sniperTowerPrefab;
                sniperTowerSprite.color = Color.white;
                description.text = "Sniper Tower";
                break;
        }
    }
    void ToggleTowerSelection(int index)
    {
        if (selectedIndex == index)
        {
            selectedIndex = 0;
            targetTower = null;

            ResetTowerBorders();

            if (currentIndex != 0)
                HighlightCurrentTower();

            return;
        }

        selectedIndex = index;

        ResetTowerBorders();

        switch (index)
        {
            case 1:
                targetTower = normalTowerPrefab;
                normalTowerSprite.color = Color.white;
                description.text = "Normal Tower";
                break;

            case 2:
                targetTower = piercingTowerPrefab;
                piercingTowerSprite.color = Color.white;
                description.text = "Piercing Tower";
                break;

            case 3:
                targetTower = sniperTowerPrefab;
                sniperTowerSprite.color = Color.white;
                description.text = "Sniper Tower";
                break;
        }
    }
    void HighlightCurrentTower()
    {
        switch (currentIndex)
        {
            case 1:
                normalTowerSprite.color = Color.yellow;
                break;

            case 2:
                piercingTowerSprite.color = Color.yellow;
                break;

            case 3:
                sniperTowerSprite.color = Color.yellow;
                break;
        }
    }
    void ConfirmTower()
    {
        if (placementPoint == null)
            return;

        if (selectedIndex == 0)
            return;

        if (selectedIndex == currentIndex)
            return;

        ReplaceTower();
    }
    void ReplaceTower()
    {

        if (placementPoint.CurrentTower != null)
        {
            aliveTowers.Remove(placementPoint.CurrentTower);
            Destroy(placementPoint.CurrentTower);
        }
        Vector3 spawnPos = placementPoint.transform.position + new Vector3(0, 0.5f, 0);
        GameObject newTower = Instantiate(targetTower, spawnPos, Quaternion.identity);
        aliveTowers.Add(newTower);
        placementPoint.SetTower(newTower);
    }
    public void SetPlacementPoint(PlacementPoint point)
    {
        placementPoint = point;
    }
}