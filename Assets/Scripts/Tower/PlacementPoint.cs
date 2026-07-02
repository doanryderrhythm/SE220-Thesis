using UnityEngine;

public class PlacementPoint : MonoBehaviour
{
    [SerializeField] private GameObject currentTower;
    [SerializeField] private GameObject keybind;

    public GameObject CurrentTower => currentTower;

    public void SetTower(GameObject tower)
    {
        currentTower = tower;
    }

    public void ClearTower()
    {
        currentTower = null;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (LayerMask.LayerToName(collision.gameObject.layer) == "TransparentFX")
        {
            keybind.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (LayerMask.LayerToName(collision.gameObject.layer) == "TransparentFX")
        {
            keybind.SetActive(false);
        }
    }
}
