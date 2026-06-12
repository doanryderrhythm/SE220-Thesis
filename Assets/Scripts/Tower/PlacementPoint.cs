using UnityEngine;

public class PlacementPoint : MonoBehaviour
{
    [SerializeField] private GameObject currentTower;

    public GameObject CurrentTower => currentTower;

    public void SetTower(GameObject tower)
    {
        currentTower = tower;
    }

    public void ClearTower()
    {
        currentTower = null;
    }
}
