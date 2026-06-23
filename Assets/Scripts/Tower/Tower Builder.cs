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

    private bool isBuildMode = false;
    public List<GameObject> aliveTowers = new List<GameObject>();
    private Rigidbody2D rb;

    public int maxTowers => levelData != null ? levelData.maxTowers : 3;

    [SerializeField] private PlacementPoint placementPoint;

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
                EnterBuildMode();
            }
            else
            {
                ConfirmBuild();
            }
        }
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            if (isBuildMode)
            {
                ExitBuildMode();
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
        BuildUIManager.Instance.SetCountBuild(AliveCount(), maxTowers);
        GameEvent.OnOpenBuildMenu?.Invoke(placementPoint);
        BuildUIManager.Instance.SetBuildMode(isBuildMode);
    }

    void ExitBuildMode()
    {
        isBuildMode = false;
        GameEvent.OnCloseBuildMenu.Invoke();
        BuildUIManager.Instance.SetBuildMode(isBuildMode);
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

        Vector3 spawnPos = placementPoint.transform.position;

        GameObject newTower = Instantiate(prefab, spawnPos, Quaternion.identity);
        aliveTowers.Add(newTower);
        placementPoint.SetTower(newTower);

        ConfirmBuild();
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
    void ReplaceTower(TowerType type)
    {
        if (placementPoint.CurrentTower != null)
        {
            aliveTowers.Remove(placementPoint.CurrentTower);
            Destroy(placementPoint.CurrentTower);
        }

        GameObject prefab = GetPrefabForType(type);

        if (prefab == null) return;

        Vector3 spawnPos = placementPoint.transform.position;

        GameObject newTower = Instantiate(prefab, spawnPos, Quaternion.identity);

        aliveTowers.Add(newTower);
        placementPoint.SetTower(newTower);
    }
    public void SetPlacementPoint(PlacementPoint point)
    {
        placementPoint = point;
    }

    private void ConfirmBuild()
    {
        TowerType selectedType = BuildUIManager.Instance.SelectedTowerType;
        TowerType? currentType = null;

        if (placementPoint.CurrentTower != null)
        {
            currentType = placementPoint.CurrentTower.GetComponent<Tower>().towerType;
        }

        if (selectedType == currentType)
        {
            BuildUIManager.Instance.CannotClose();
            return;
        }

        if (placementPoint.CurrentTower == null)
        {
            PlaceTower(selectedType, placementPoint.transform);
        }
        else
        {
            ReplaceTower(selectedType);
        }

        ExitBuildMode();
    }
}