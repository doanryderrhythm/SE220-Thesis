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
}
