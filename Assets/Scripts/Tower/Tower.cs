using System.Dynamic;
using UnityEngine;

public enum TowerType
{
    Normal,
    Piercing,
    Sniper
}

public class Tower : MonoBehaviour
{
    [Header("Tower config")]
    public TowerType towerType = TowerType.Normal;
    public Transform firePoint;
    public GameObject projectilePrefab;

    [Header("Targeting")]
    public string enemyTag = "Enemy";
    [SerializeField] private TowerStats towerStats; // Reference to the ScriptableObject for stats
    [Header("Manual config (nếu không dùng TowerStats)")]
    public float range = 5f;
    public float fireRate = 1f;
    public float projectileDamage = 5f;
    public bool projectilePierces = false;
    private bool isMouseOver = false;
    private float fireCooldown;
    private float cost;
    private float upgradeCost;

    void Awake()
    {
        SetupTowerStats();
    }

    void SetupTowerStats()
    {
        if (towerStats != null)
        {
            towerType = towerStats.towerType;
            range = towerStats.range;
            fireRate = towerStats.fireRate;
            projectileDamage = towerStats.projectileDamage;
            projectilePierces = towerStats.projectilePierces;
        }
     
      
    }

    void Update()
    {
        if (firePoint == null || projectilePrefab == null) return;

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
        Projectile p = spawned.GetComponent<Projectile>();
        if (p != null)
        {
            p.damage = projectileDamage;
            p.piercing = projectilePierces;
        }

        Rigidbody2D rb = spawned.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
          rb.linearVelocity = firePoint.up * 14f;
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
        // Implement build logic here (e.g., check player resources, deduct cost, etc.)
    }

    void OnMouseEnter()
    {
        isMouseOver = true;
    }

    void OnMouseExit()
    {
        isMouseOver = false;
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
}

