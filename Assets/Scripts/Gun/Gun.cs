using UnityEngine;

public class Gun : MonoBehaviour
{
    [SerializeField] GameObject keybind;

    private void Update()
    {
        keybind.transform.rotation = Quaternion.Euler(Vector3.zero);
    }

    public enum GunType
    {
        GUN_SIDE,
        GUN_UP,
        GUN_DOWN
    }

    [SerializeField] GunStats stats;

    public GunStats GetStats()
    {
        return stats;
    }

    [SerializeField] BoxCollider2D boxCol;
    public void PickedUp(Transform player)
    {
        transform.SetParent(player);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

    public void Dropped(Transform player)
    {
        transform.SetParent(null);
        transform.position = player.transform.position;
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
