using UnityEngine;

public class Gun : MonoBehaviour
{
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
}
