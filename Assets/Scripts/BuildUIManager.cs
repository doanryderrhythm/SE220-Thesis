using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[System.Serializable]
public class TowerInfo
{
    public TowerType type;
    public string description;
    public Sprite icon;
}

public class BuildUIManager : MonoBehaviour
{
    public static BuildUIManager Instance { get; private set; }
    private TowerBuilder builder;
    private PlacementPoint currentPlacement;
    private bool isBuildMode = false;
    [SerializeField] private List<TowerInfo> towers;
    private int currentIndex;
    private TowerType? currentTower;
    public TowerType? selectedTower;

    [SerializeField] private Transform leftSlot;
    [SerializeField] private Transform centerSlot;
    [SerializeField] private Transform rightSlot;

    [SerializeField] private Image leftImage;
    [SerializeField] private Image centerImage;
    [SerializeField] private Image rightImage;

    [SerializeField] public TMP_Text description;
    [SerializeField] private TMP_Text countTower;
    private int countTowerBuilt;
    private int maxTowerBuilt;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    private void OnEnable()
    {
        GameEvent.OnOpenBuildMenu += OpenBuildMode;
    }
    private void Update()
    {
        if (Keyboard.current == null)
            return;

        if (isBuildMode)
        {
            if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
            {
                Next();
            }
            else if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
            {
                Previous();
            }
        }
    }
    public void Next()
    {
        currentIndex++;
        if (currentIndex >= towers.Count) currentIndex = 0;
        selectedTower = towers[currentIndex].type;
        RefreshUI();
    }

    public void Previous()
    {
        currentIndex--;
        if (currentIndex < 0) currentIndex = towers.Count - 1;
        selectedTower = towers[currentIndex].type;
        RefreshUI();
    }

    void RefreshUI()
    {
        int left = (currentIndex - 1 + towers.Count) % towers.Count;
        int right = (currentIndex + 1) % towers.Count;

        leftImage.sprite = towers[left].icon;
        centerImage.sprite = towers[currentIndex].icon;
        rightImage.sprite = towers[right].icon;

        description.text = towers[currentIndex].description;

        leftImage.transform.localScale = Vector3.one;
        rightImage.transform.localScale = Vector3.one;
        centerImage.transform.localScale = Vector3.one * 1.4f;
    }

    public void OpenBuildMode(PlacementPoint point)
    {
        currentPlacement = point;
        Debug.Log("Current Tower = " + point.CurrentTower);
        if (point.CurrentTower != null)
        {
            Tower tower = point.CurrentTower.GetComponent<Tower>();
            Debug.Log("Tower Name = " + point.CurrentTower.name);
            Debug.Log("Tower Type = " + tower.towerType);
            currentIndex = towers.FindIndex(t => t.type == tower.towerType);
            Debug.Log("FindIndex = " + currentIndex);
            if (currentIndex < 0)
                currentIndex = 0;
            currentTower = tower.towerType;
            selectedTower = currentTower;
        }
        else
        {
            currentTower = null;
            selectedTower = null;
            currentIndex = 0;
        }
        countTower.text = $"Number of towers built: {countTowerBuilt}/{maxTowerBuilt}";

        RefreshUI();
    }

    public void CannotClose()
    {
        description.text = "A new tower needs to be selected.";
    }

    public TowerType SelectedTowerType
    {
        get { return towers[currentIndex].type; }
    }

    public void SetCountBuild(int count, int max)
    {
        countTowerBuilt = count;
        maxTowerBuilt = max;
    }

    public void SetBuildMode(bool buildMode)
    {
        isBuildMode = buildMode;
    }
}
