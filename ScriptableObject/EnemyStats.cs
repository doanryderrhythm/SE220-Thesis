using UnityEngine;


[CreateAssetMenu(fileName = "EnemyData", menuName = "Enemy/EnemyData")]

public class EnemyStats : ScriptableObject
{
    public EnemyType enemyType;
    public float health;
    public float speed;
    public float reward;
    public float armor;
    public Transform[] waypoints;
    
}


