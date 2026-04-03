using UnityEngine;


[CreateAssetMenu(fileName = "EnemyData", menuName = "Enemy/EnemyData")]

public class EnemyStats : ScriptableObject
{
    public EnemyType enemyType;
    public float health;
    public float speed;
    public float reward;
    public float armor;
    public float damage;
    public float attackRate;
    public float attackcooldown;
    public float attackRange;
    public Tower targetedTower; // Reference to the target tower (if needed)
 
    
}

