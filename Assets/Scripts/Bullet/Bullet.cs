using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Bullet : MonoBehaviour
{
    private Rigidbody2D rb;
    public Vector2 direction;
    public float speed;

    public float damage = 1f;
    public bool piercing = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        Destroy(gameObject, 3.5f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(direction.x *4* speed, direction.y *4* speed);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (LayerMask.LayerToName(collision.gameObject.layer) == ValueStorer.groundLM &&
            !collision.CompareTag("Enemy"))
        {
            Destroy(gameObject);
            return;
        }

        if (!collision.CompareTag("Enemy")) return;

        Enemy enemy = collision.GetComponent<Enemy>();
        EliteEnemy eliteEnemy = collision.GetComponent<EliteEnemy>();
        if (enemy == null && eliteEnemy == null) return;

        if (enemy != null)
        {
            enemy.TakeDamage(damage, piercing);
        }
        if (eliteEnemy != null)
        {
            eliteEnemy.TakeDamage(damage, piercing);
        }
        Destroy(gameObject);
    }

    private void OnEnable()
    {
        GameEvent.OnGunPowerUpCollected += CollectGunPowerUp;
    }

    private void OnDisable()
    {
        GameEvent.OnGunPowerUpCollected -= CollectGunPowerUp;
    }
    void CollectGunPowerUp(float value)
    {
        damage += value;
    }
}
