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
   private float attackRange; // Prefab for visualizing attack range (optional)
    private float attackcooldown;
    private float maxhealth;
    private float basedamage;
    public bool isBuffed = false; // Flag to track if the enemy is currently buffed
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private LayerMask towerLayer;
    private Tower targetedtower; // Reference to the target tower (if needed)

       public Transform[] waypoints;
    private int waypointIndex = 0;
public int enenmyStatus;
    private SpriteRenderer sr; 
 [SerializeField] private SpriteRenderer Si; // Reference to the arrow prefab for Archer attacks
    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        setupEnemyStats();
    }

  void Update()
{
    Si.gameObject.SetActive(false); // Hide the status indicator by default
    
   SetEnemyStatus(enenmyStatus); // Update the enemy status based on current conditions
        ShowEnemyStatus(enenmyStatus); // Update the visual representation of the enemy status  
    if (targetedtower != null)
    {
        attackcooldown -= Time.deltaTime;
        if (attackcooldown <= 0f)
        {
            PerformAttack();
        }
    }
    else 
    {speed = enemyStats.speed; // Reset speed when not attacking
        if (type == EnemyType.Archer)
        {
            DetectTowerInRange();
        }
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
        attackRange = enemyStats.attackRange;
        maxhealth = health;
        basedamage = damage;
        enenmyStatus =2;
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
private void PerformAttack()
{
    if (targetedtower == null) return;

    speed = 0f;

    if (type == EnemyType.Archer && arrowPrefab != null)
    {
       

        GameObject arrow = Instantiate(arrowPrefab, transform.position, Quaternion.identity);
        Arrow arrowScript = arrow.GetComponent<Arrow>();
        if (arrowScript != null)
        {
            arrowScript.SetTarget(targetedtower.transform);
            arrowScript.SetDamage(damage);
        }
    }
    else
    {

        targetedtower.TakeDamage(damage);
    }

    attackcooldown = (attactrate > 0) ? (1f / attactrate) : 1f;
}
private void DetectTowerInRange()
{
 Collider2D hit = Physics2D.OverlapCircle(transform.position, attackRange, towerLayer);

    if (hit != null)
    {
        Tower tower = hit.GetComponent<Tower>();
        if (tower != null)
        {
            targetedtower = tower;
            attackcooldown = 0f;
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
    public void EnemyharmPlayer(float damage)
    {
        PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
        }
    }
}
