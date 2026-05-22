using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Bullet : MonoBehaviour
{
    private Rigidbody2D rb;
    public Vector2 direction;
    public float speed;

    public float damage = 5;
    public bool piercing = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        Destroy(gameObject, 3f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(direction.x * speed, direction.y * speed);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Enemy")) return;

        Enemy enemy = collision.GetComponent<Enemy>();
        if (enemy == null) return;

        enemy.TakeDamage(damage, piercing);
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
