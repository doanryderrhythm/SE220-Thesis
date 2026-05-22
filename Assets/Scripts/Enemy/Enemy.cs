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
 // Reference to the ScriptableObject for damage to player
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
    [SerializeField] private LayerMask playerLayer;
    private Tower targetedtower; // Reference to the target tower (if needed)
private PlayerController targetedPlayer; // Reference to the target player (if needed)
       public Transform[] waypoints;
    private int waypointIndex = 0;
public int enenmyStatus;
    private SpriteRenderer sr; 
 [SerializeField] private SpriteRenderer Si; // Reference to the arrow prefab for Archer attacks
 private HarmData harmData;
    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        harmData = GetComponent<HarmData>(); 
        setupEnemyStats();

        GameManager.Instance.InsertEnemy(this);
    }

  void Update()
{
  
    Si.gameObject.SetActive(false); // Hide the status indicator by default
    
   SetEnemyStatus(enenmyStatus); // Update the enemy status based on current conditions
        ShowEnemyStatus(enenmyStatus); // Update the visual representation of the enemy status  
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
    speed = enemyStats.speed;

        DetectInRange();
    

    MoveTowardsTarget();
}
}

    public void TakeDamage(float damage, bool pierce)
    {
        if ( !pierce && armor > 0)
        {
            float armorAbsorb = Mathf.Min(armor, damage);
            armor -= armorAbsorb;
            damage -= armorAbsorb;
        }
else
        health -= damage;

        if (health <= 0f)
        {
            Die();
        }
    }

    public bool isDead = false;

 public void Die()
{
  
    if (sr != null)
    {
        sr.color = Color.red;
    }

    isDead = true;
    speed = 0f;
       GameManager.Instance.DeleteEnemy(this);
        GameEvent.OnEnemyKilled?.Invoke();
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
        armor = enemyStats.armor;
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
            Debug.Log("Enemy collided with Player."); // Log collision
            targetedPlayer = player;
            attackcooldown = 0f;
        }

    }
    else
    {
        Debug.Log("Collision detected with: " + collision.gameObject.name);
    }
}
private void OnTriggerExit2D(Collider2D collision)
{
    if (collision.CompareTag("Tower"))
    {
        targetedtower = null;
    }
    else if (collision.CompareTag("Player"))
    {
        targetedPlayer = null;
    }
}
private void PerformAttack()
{
        if (targetedtower == null && targetedPlayer == null) return;

        speed = 0f;

        if (type == EnemyType.Archer && arrowPrefab != null)
        {
            GameObject arrow = Instantiate(arrowPrefab, transform.position, Quaternion.identity);
            Arrow arrowScript = arrow.GetComponent<Arrow>();
            if (arrowScript != null)
            {
                Transform targetTransform = targetedtower != null ? targetedtower.transform : targetedPlayer.transform;
                arrowScript.SetTarget(targetTransform);
                arrowScript.SetDamage(damage);
            }
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

        speed = enemyStats.speed;
    }
else
        {
            targetedtower.TakeDamage(damage);
            if(targetedtower.towerType == TowerType.Nexus)
            {
              Die();
            }
        }
        attackcooldown = (attactrate > 0) ? (1f / attactrate) : 1f;
}
private void DetectInRange()
{

    Collider2D hitTower = Physics2D.OverlapCircle(transform.position, attackRange, towerLayer);
        if (hitTower != null)
        {
            Tower tower = hitTower.GetComponent<Tower>();
            if (tower != null)
            {
                targetedtower = tower;
                attackcooldown = 0f;
                return; // Nếu thấy Tower thì thôi không cần quét Player nữa
            }
        }

        Collider2D hitPlayer = Physics2D.OverlapCircle(transform.position, attackRange, playerLayer);
        if (hitPlayer != null)
        {
            PlayerController player = hitPlayer.GetComponent<PlayerController>();
            if (player != null)
            {
                targetedPlayer = player;
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
   
}
