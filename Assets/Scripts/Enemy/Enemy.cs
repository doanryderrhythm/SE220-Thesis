using UnityEngine;

using System.Diagnostics; // nếu dùng Coroutine

public enum EnemyType
{
    ArmoredInfantry,
    Infantry,
    Archer
}

public class Enemy : MonoBehaviour
{
    [SerializeField] private EnemyStats enemyStats; // Reference to the ScriptableObject for stats

    private EnemyType type;
    private float health;   
    private float speed;
    private float reward;
    private float armor;
    private Transform[] waypoints;
    private int waypointIndex = 0;

    private SpriteRenderer sr; 

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        setupEnemyStats();
      
    }

    void Update()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        Transform target = waypoints[waypointIndex];
        transform.position = Vector2.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

        if (Vector2.Distance(transform.position, target.position) < 0.1f)
        {
            waypointIndex = (waypointIndex + 1) % waypoints.Length;
        }
    }
   
public void TakeDamage(float damage, bool pierce)
{
    UnityEngine.Debug.Log($"Before → HP: {health}, Armor: {armor}");

    if (type == EnemyType.ArmoredInfantry)
    {
        if (pierce)
        {

            health -= damage;
        }
        else
        {
            if (armor > 0)
            {
                float dmgToArmor = Mathf.Min(damage, armor);
                armor -= dmgToArmor;

                float remaining = damage - dmgToArmor;

                if (remaining > 0)
                {
                    health -= remaining;
                }
            }
            else
            {
                health -= damage;
            }
        }
    }
    else
    {
        health -= damage;
    }

    UnityEngine.Debug.Log($"After → HP: {health}, Armor: {armor}");

    if (health <= 0f)
    {
        Die();
    }
}


 void Die()
{
  
    if (sr != null)
    {
        sr.color = Color.red;
    }

    speed = 0f;
    Destroy(gameObject, 0.3f);
}

    public void setupEnemyStats()
    {
        if (enemyStats != null)
        {
            type = enemyStats.enemyType;
            health = enemyStats.health;
            speed = enemyStats.speed;
            armor = enemyStats.armor;
        }
    }
}