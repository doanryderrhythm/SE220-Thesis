using UnityEngine;

[CreateAssetMenu(fileName = "GunData", menuName = "Gun/GunData")]
public class GunStats : ScriptableObject
{
    public Gun.GunType gunType;
    public float shootSpeed;
    public float shootRate;
}
