using UnityEngine;
using System.Collections; // nếu dùng Coroutine

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
    private float damage;
    private float attactrate;

    private float attackcooldown;
    private Tower targetedtower; // Reference to the target tower (if needed)

       public Transform[] waypoints;
    private int waypointIndex = 0;

    private SpriteRenderer sr; 

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        setupEnemyStats();
    }

  void Update()
{
    if (targetedtower != null)
    {
        attackcooldown -= Time.deltaTime;
        if (attackcooldown <= 0f)
        {
            float actualDamage = (damage > 0) ? damage : 1f; 
            targetedtower.TakeDamage(actualDamage);
            float interval = (attactrate > 0) ? (1f / attactrate) : 1f;
            attackcooldown = interval; 
        }
    }
    else 
    {
        MoveTowardsTarget();
    }
}

    public void TakeDamage(float damage, bool pierce)
    {
        if (type == EnemyType.ArmoredInfantry && !pierce && armor > 0f)
        {
            float armorAbsorb = Mathf.Min(armor, damage);
            armor -= armorAbsorb;
            damage -= armorAbsorb;
        }

        health -= damage;

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
        damage = enemyStats.damage; 
        attactrate = enemyStats.attackRate;
        }
    }
    void MoveTowardsTarget()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        Transform target = waypoints[waypointIndex];
        transform.position = Vector2.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

        if (Vector2.Distance(transform.position, target.position) < 0.1f)
        {
            waypointIndex = (waypointIndex + 1) % waypoints.Length;
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
{
   
    if (collision.CompareTag("Tower"))
    {
        Tower tower = collision.GetComponent<Tower>();
        if (tower != null)
        {
            targetedtower = tower;
            attackcooldown = 0f; 
        }
    }
}
}
