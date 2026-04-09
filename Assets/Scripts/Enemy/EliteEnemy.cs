using UnityEngine;
using System.Collections;


public enum EliteEnemyType
{
   Boss1,
    Boss2,
    Boss3
}

public class EliteEnemy : MonoBehaviour
{
    [SerializeField] private EliteEnemyStats eliteEnemyStats; // Reference to the ScriptableObject for elite enemy stats
    private EliteEnemyType   type;
    private float health;
    private float speed;
    private float reward;
    private float armor;
    private float damage;
    private float attackRate;
    private float attackcooldown;
    private Tower targetedtower; // Reference to the target tower (if needed)
    public Transform[] waypoints;
    private int waypointIndex = 0;
      [Header("Boss 1 Settings")]
    public float disableCooldown = 3f;        
    private float disableTimer = 3f;          
    public float abilityRadius = 10f;         
    public float disableDuration = 2f;
    [Header("Boss 2 Settings")]
    public float explosionRadius = 5f;
    private bool isDead = false;
    [Header("Boss 3 Settings")]
    public float buffCooldown = 5f;      
    private float buffTimer = 0f;        
    public bool isBuffed = false;
    private SpriteRenderer sr;  

     void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        setupEnemyStats();
    }

  void Update()
{if (isDead) return;
    disableTimer -= Time.deltaTime;
    if (type == EliteEnemyType.Boss1 && disableTimer <= 0f)
    {
        DisableNearestTower();
        disableTimer = disableCooldown; 
    }
    if (type == EliteEnemyType.Boss3)
        {
            buffTimer -= Time.deltaTime;
            if (buffTimer <= 0f)
            {
                BuffAllEnemies();
                buffTimer = buffCooldown; 
            }
        }
    if (targetedtower != null)
    {
        attackcooldown -= Time.deltaTime;
        if (attackcooldown <= 0f)
        {
            float actualDamage = (damage > 0) ? damage : 1f; 
            targetedtower.TakeDamage(actualDamage);
            float interval = (attackRate > 0) ? (1f / attackRate) : 1f;
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
        if (type == EliteEnemyType.Boss1 && !pierce && armor > 0f)
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
{if (isDead) return;
  isDead = true;
    if (sr != null)
    {
        sr.color = Color.red;
    }
    speed = 0f;
    Destroy(gameObject, 0.3f);
if (type == EliteEnemyType.Boss2)
    {
        ExplodeOnDeath();
    }
    
}
void ExplodeOnDeath()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (Collider2D hit in hitColliders)
        {
            if (hit.CompareTag("Tower"))
            {
                Tower tower = hit.GetComponent<Tower>();
                if (tower != null)
                {
                    tower.TakeDamage(10f * damage); 
                }
            }
        }
    }

    public void setupEnemyStats()
    {
        if (eliteEnemyStats != null)
        {
            type = eliteEnemyStats.enemyType;
            health = eliteEnemyStats.health;
            speed = eliteEnemyStats.speed;
            damage = eliteEnemyStats.damage;
            attackRate = eliteEnemyStats.attackRate;
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
void DisableNearestTower()
{
    Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, abilityRadius);
    Tower closestTower = null;
    float closestDistance = Mathf.Infinity;
    foreach (Collider2D hit in hitColliders)
    {
        if (hit.CompareTag("Tower"))
        {
            Tower tower = hit.GetComponent<Tower>();
            if (tower != null)
            {
                float distance = Vector2.Distance(transform.position, hit.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestTower = tower;
                }
            }
        }
    }
    if (closestTower != null)
    {
        closestTower.DisableShooting(disableDuration); 
    }
}
void BuffAllEnemies()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        foreach (GameObject enemyObj in enemies)
        {
            EliteEnemy enemyScript = enemyObj.GetComponent<EliteEnemy>();

            if (enemyScript != null)
            {
                if (!enemyScript.isBuffed)
                {
                    enemyScript.ApplyDamageBuff(2f); 
                }
            }
        }
    }
    public void ApplyDamageBuff(float multiplier)
    {
        if (isBuffed) return; 
        damage *= multiplier; 
        isBuffed = true;      
       
    }

}



