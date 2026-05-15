using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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

    private bool isBuildMode = false;
    private List<GameObject> aliveTowers = new List<GameObject>();
    private Rigidbody2D rb;

    private int maxTowers => levelData != null ? levelData.maxTowers : 3;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (!isBuildMode) EnterBuildMode();
            else ExitBuildMode();
        }

        if (isBuildMode)
        {
            if (Keyboard.current.digit1Key.wasPressedThisFrame) PlaceTower(TowerType.Normal);
            else if (Keyboard.current.digit2Key.wasPressedThisFrame) PlaceTower(TowerType.Piercing);
            else if (Keyboard.current.digit3Key.wasPressedThisFrame) PlaceTower(TowerType.Sniper);
        }
    }

    int AliveCount()
    {
        aliveTowers.RemoveAll(t => t == null);
        return aliveTowers.Count;
    }

    void EnterBuildMode()
    {
        isBuildMode = true;
        if (rb != null) rb.linearVelocityX = 0f;
        if (buildUIPanel != null) buildUIPanel.SetActive(true);
    }

    void ExitBuildMode()
    {
        isBuildMode = false;
        if (buildUIPanel != null) buildUIPanel.SetActive(false);
    }

    void PlaceTower(TowerType type)
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

       
        Vector3 spawnPos = transform.position + new Vector3(0f, 0f, 0f);

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
}