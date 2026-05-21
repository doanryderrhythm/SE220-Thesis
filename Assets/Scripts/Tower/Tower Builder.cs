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

    private Transform currentPlacementPoint = null;

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
            if (!isBuildMode)
            {
                if (currentPlacementPoint != null)
                    EnterBuildMode();
                else
                    Debug.Log("Not on Placement point!");
            }
            else
            {
                ExitBuildMode();
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PlacementPoint"))
        {
            currentPlacementPoint = other.transform;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("PlacementPoint"))
        {
            if (currentPlacementPoint == other.transform)
            {
                currentPlacementPoint = null;
                if (isBuildMode) ExitBuildMode();
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
        isBuildMode = true;
        if (rb != null) rb.linearVelocityX = 0f;
        if (buildUIPanel != null) buildUIPanel.SetActive(true);
    }

    void ExitBuildMode()
    {
        isBuildMode = false;
        if (buildUIPanel != null) buildUIPanel.SetActive(false);
    }

    public void PlaceTower(TowerType type)
    {
        if (currentPlacementPoint == null)
        {
            Debug.Log("Khong co Placement Point nao!");
            return;
        }

        if (AliveCount() >= maxTowers)
        {
            return;
        }

        GameObject prefab = GetPrefabForType(type);
        if (prefab == null)
        {
            return;
        }

        Vector3 spawnPos = new Vector3(currentPlacementPoint.position.x, transform.position.y, 0);

        GameObject newTower = Instantiate(prefab, spawnPos, Quaternion.identity);
        aliveTowers.Add(newTower);

        ExitBuildMode();
    }

    public void PlaceTower(TowerType type, Transform placement)
    {
        currentPlacementPoint = placement;
        PlaceTower(type);
    }

    GameObject GetPrefabForType(TowerType type)
    {
        if (type == TowerType.Piercing) return piercingTowerPrefab;
        if (type == TowerType.Sniper) return sniperTowerPrefab;
        return normalTowerPrefab;
    }

    public bool IsBuildMode => isBuildMode;
    public Transform CurrentPlacementPoint => currentPlacementPoint;

    public void OnTowerDestroyed()
    {
        aliveTowers.RemoveAll(t => t == null);
    }
}