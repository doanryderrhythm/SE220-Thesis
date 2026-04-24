using UnityEngine;
using System.Collections;


public enum EliteEnemyType
{
   TowerDisabler,
    Excutioner,
    Cheerleader,
    Bulletdeflector,
    OmniSlasher
}

public class EliteEnemy : MonoBehaviour
{
    [SerializeField] private EliteEnemyStats eliteEnemyStats; // Reference to the ScriptableObject for elite enemy stats
    public EliteEnemyType   type;
    private float health;
    private float speed;
    private float reward;
    private float armor;
    private float damage;
    private float attackRate;
    private float attackcooldown;
    private float maxhealth;
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
        [Header("Boss 4 Settings")]
        public float detectRadius = 1f;
        public float deflectDuration = 2f;
 // Prefab for the deflected projectile
    private SpriteRenderer sr;  
 [SerializeField] private SpriteRenderer Si;
  [SerializeField] private GameObject arrowPrefab;
    public int enenmyStatus;
    private PlayerController targetedPlayer; // Reference to the target player (if needed)
    
private HarmData harmData;
     void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        Si.gameObject.SetActive(false); // Hide the status indicator at the start
        harmData = GetComponent<HarmData>(); // Get the HarmData component attached to this enemy
        setupEnemyStats();
    }

  void Update()
{if (isDead) return;
SetEnemyStatus(enenmyStatus);
        ShowEnemyStatus(enenmyStatus);

    disableTimer -= Time.deltaTime;
    if (type == EliteEnemyType.TowerDisabler && disableTimer <= 0f)
    {
        DisableNearestTower();
        disableTimer = disableCooldown; 
    }
    if (type == EliteEnemyType.Cheerleader)
        {
            buffTimer -= Time.deltaTime;
            if (buffTimer <= 0f)
            {
                BuffAllEnemies();
                buffTimer = buffCooldown; 
            }
        }

    if (targetedtower != null || targetedPlayer != null)
    {
        attackcooldown -= Time.deltaTime;
        if (attackcooldown <= 0f)
        {
            PerformAttack();
  
        }
    }
    else 
    {
        MoveTowardsTarget();
    }
}

    public void TakeDamage(float damage, bool pierce)
    {
  
         if (type == EliteEnemyType.TowerDisabler && !pierce && armor > 0f)
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
if (type == EliteEnemyType.Excutioner)
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
            else if (hit.CompareTag("Player"))
            {
                PlayerController player = hit.GetComponent<PlayerController>();
                if (player != null)
                {
                  
                    player.HurtPlayer(1);
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
            maxhealth = health;
            enenmyStatus = 2;
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
    else if (collision.CompareTag("Player"))
    {
        PlayerController player = collision.GetComponent<PlayerController>();
        if (player != null)
        {
            targetedPlayer = player;
            attackcooldown = 0f;
            speed = eliteEnemyStats.speed / 2f; // Slow down the enemy when it starts attacking the player
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
            Enemy normalenemyScript = enemyObj.GetComponent<Enemy>();
            if(normalenemyScript != null)
             {
                if (!normalenemyScript.isBuffed)
                {
                    normalenemyScript.ApplyDamageBuff(2f); 
                }
             }
        }
    }
       public void SetEnemyStatus(int status)
    {
        enenmyStatus = status;
        if(health <= maxhealth * 0.3f)
        {
            enenmyStatus = 1; // NearlyDeath
        }
        else if(isBuffed)
        {
            enenmyStatus = 0; // AttackBuff
        }
        else
        {
            enenmyStatus = 2; // Normal
        }
    }
    public void ShowEnemyStatus(int status)
    {

        switch (status)
        {
            case 0: // AttackBuff
             if(Si != null){
                Si.color = Color.yellow;
                Si.gameObject.SetActive(true); }
                break;
            case 1: // NearlyDeath
               if(Si != null){
                Si.color = Color.red;
                Si.gameObject.SetActive(true); }
                break;
            case 2: // Normal
               if(Si != null){

                Si.gameObject.SetActive(false); }
                break;
        }
    }
    public void ApplyDamageBuff(float multiplier)
    {
        if (isBuffed) return; 
        damage *= multiplier; 
        isBuffed = true;      
    }

    private void PerformAttack()
    {
         if (targetedtower == null && targetedPlayer == null) return;

    if (targetedtower != null)
    {
        targetedtower.TakeDamage(damage);
    }
    else if (targetedPlayer != null)
    {
        HarmStats stats = harmData != null ? harmData.GetHarmStats() : null;

        if (stats != null)
        {
            targetedPlayer.HurtPlayer(stats.damage);
        }
        else
        {
            targetedPlayer.HurtPlayer(damage); 
        }

        speed = eliteEnemyStats.speed;
    }

    attackcooldown = (attackRate > 0) ? (1f / attackRate) : 1f;
    }

 
    // Thêm hàm này vào trong class EliteEnemy
    private void OnDrawGizmosSelected()
    {
        if (type == EliteEnemyType.Bulletdeflector)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, detectRadius);
        }


        if (type == EliteEnemyType.TowerDisabler)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, abilityRadius);
        }
    }


}



