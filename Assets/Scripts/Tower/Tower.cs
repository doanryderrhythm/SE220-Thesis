using System.Dynamic;
using UnityEngine;
using System.Collections;
public enum TowerType
{
    Normal,
    Piercing,
    Sniper,
    Nexus
}

public class Tower : MonoBehaviour
{
    [Header("Tower config")]
    public TowerType towerType = TowerType.Normal;
    public Transform firePoint;
    public GameObject projectilePrefab;

    public bool islosing = false;
    [Header("Targeting")]
    public string enemyTag = "Enemy";
    [SerializeField] private TowerStats towerStats; // Reference to the ScriptableObject for stats
    public float range = 5f;
    public float fireRate = 1f;
    public float projectileDamage = 5f;
    public bool projectilePierces = false;
    private bool isMouseOver = false;
    private float fireCooldown;
    private float towerHP;
    private float cost;
    private float upgradeCost;
    private bool canShoot = true;
    private SpriteRenderer sr;
    private Color originalColor;

    [Header("HP Bar Settings")]
    [SerializeField] private Transform hpBarFill;
    public Transform hpBarPivot;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr != null)
    {
        originalColor = sr.color; 
    }
        SetupTowerStats();
        
    }
    float towermaxHP;
    bool isUpgraded = false;

    void SetupTowerStats()
    {
        if (towerStats != null)
        {
            towerType = towerStats.towerType;
            range = towerStats.range;
            fireRate = towerStats.fireRate;
            projectileDamage = towerStats.projectileDamage;
            projectilePierces = towerStats.projectilePierces;
            towerHP = towerStats.towerHP;
            towermaxHP = towerHP;
        }
     
      
    }

    void Start()
    {
        if (towerType == TowerType.Nexus)
            GameManager.Instance.nexusTower = this;
    }

    void Update()
    {
     if (firePoint == null || projectilePrefab == null) return;
    if (!canShoot||towerType == TowerType.Nexus) return;
        fireCooldown -= Time.deltaTime;
        if (fireCooldown <= 0f)
        {
            Transform target = FindNearestEnemy();
            if (target != null)
            {  
                AimAt(target);
                Fire();
                fireCooldown = 1f / fireRate;
            }
 
        }
        
    }

Transform FindNearestEnemy(){
    GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
    Transform nearest = null;
    float minDist = Mathf.Infinity;
    Vector3 pos = transform.position;

    foreach (GameObject enemy in enemies)
    {
        float dist = Vector3.Distance(pos, enemy.transform.position);
        if (dist < minDist && dist <= range)
        {
            minDist = dist;
            nearest = enemy.transform;
        }
    }

    return nearest;
}

  void AimAt(Transform target)
{


    Vector3 direction = (target.position - firePoint.position).normalized;
    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
    firePoint.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
    
}

    void Fire()
    {
        GameObject spawned = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);

        // Ensure the scale of the projectile remains unchanged
        spawned.transform.localScale = projectilePrefab.transform.localScale;

        Projectile p = spawned.GetComponent<Projectile>();
        if (p != null)
        {  
            p.damage = projectileDamage;
            p.piercing = projectilePierces;
        }

        Rigidbody2D rb = spawned.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = firePoint.up * 10f;
            Debug.DrawRay(firePoint.position, firePoint.up * 2, Color.red, 1f);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, range);
    }
    void BuildTower()
    {
       
    }

    void OnMouseEnter()
    {
        isMouseOver = true;
    }

    void OnMouseExit()
    {
        isMouseOver = false;
    }


public void DisableShooting(float duration)
    {
        StopCoroutine("DisableRoutine"); 
        StartCoroutine(DisableRoutine(duration));
    }

    private IEnumerator DisableRoutine(float duration)
    {
        canShoot = false;
        if (sr != null) sr.color = Color.gray; 

        yield return new WaitForSeconds(duration);
        canShoot = true;
        if (sr != null) sr.color = originalColor;
    }
     public void TakeDamage(float damage)
    {
        towerHP -= damage;
        if (towerHP <= 0f)
        {
            if (towerType == TowerType.Nexus)
            {
               Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, 1000.0f);
        foreach (Collider2D hit in hitColliders)
        {
            if (hit.CompareTag("Enemy"))
            {
                Enemy enemy = hit.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.Die(); 
                }
            }
            
        }
                GameEvent.OnGameLost.Invoke();
                Debug.Log("Game Over! Nexus destroyed.");
            }
            SpawnExplosion();
            Destroy(gameObject);
        }
    }
    
//show tower range
    void OnGUI()
    {
        if (isMouseOver )
        {
            Vector3 worldPos = transform.position;
            Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
            float screenRadius = Camera.main.WorldToScreenPoint(worldPos + Vector3.right * range).x - screenPos.x;
            screenPos.y = Screen.height - screenPos.y; 

            Color oldColor = GUI.color;
            GUI.color = new Color(0, 1, 1, 0.15f);
            DrawCircle(screenPos, screenRadius, 64);
            GUI.color = oldColor;
        }
    }

    [SerializeField] ParticleSystem explosionPrefab;


    void SpawnExplosion()
    {
        AudioManager.Instance.InstantiateSFX(AudioManager.Instance.destroyTowerSound);
        Instantiate(explosionPrefab.gameObject, transform.position, Quaternion.identity);
    }

    void DrawCircle(Vector3 center, float radius, int segments)
    {
        Vector3[] points = new Vector3[segments + 1];
        for (int i = 0; i <= segments; i++)
        {
            float angle = i * Mathf.PI * 2f / segments;
            points[i] = center + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius;
        }
        for (int i = 0; i < segments; i++)
        {
            DrawLine(points[i], points[i + 1], 2f);
        }
    }


    void DrawLine(Vector3 p1, Vector3 p2, float width)
    {
        Matrix4x4 matrix = GUI.matrix;
        float angle = Mathf.Atan2(p2.y - p1.y, p2.x - p1.x) * Mathf.Rad2Deg;
        float length = (p2 - p1).magnitude;
        GUIUtility.RotateAroundPivot(angle, p1);
        GUI.DrawTexture(new Rect(p1.x, p1.y - width / 2, length, width), Texture2D.whiteTexture);
        GUI.matrix = matrix;
    }
    public void upgradeTower()
    {
        if (isUpgraded) return; // Prevent multiple upgrades
        // Implement upgrade logic here (e.g., check player resources, apply stat increases, etc.)
        towerHP = towermaxHP;
        projectileDamage = towerStats.projectileDamage * 1.5f; // Example: Increase damage by 50%
        isUpgraded = true;
    }

   void UpdateHPBar()
{
    if (hpBarPivot != null && towermaxHP > 0f)
    {
        float hpPercentage = Mathf.Clamp01(towerHP / towermaxHP);
        hpBarPivot.localScale = new Vector3(hpPercentage, 1f, 1f);
    }
}
    void LateUpdate()
    {
        UpdateHPBar();
    }
}
