using UnityEngine;


[CreateAssetMenu(fileName = "TowerData", menuName = "Tower/TowerData")]

public class TowerStats : ScriptableObject
{
    public TowerType towerType;
    public float range ;
    public float fireRate ;
    public float projectileDamage ;
    public bool projectilePierces ;
    public float cost;
    public float upgradeCost;
}
