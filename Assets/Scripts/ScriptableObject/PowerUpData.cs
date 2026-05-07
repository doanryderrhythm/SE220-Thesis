using UnityEngine;

[CreateAssetMenu(fileName = "PowerUpData", menuName = "PowerUp/PowerUpData")]
public class PowerUpData : ScriptableObject
{
    public PowerUpType Type;
    public float value;
}
