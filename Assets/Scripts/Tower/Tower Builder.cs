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

        Vector3 spawnPos = new Vector3(placement.position.x, transform.position.y, 0);

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