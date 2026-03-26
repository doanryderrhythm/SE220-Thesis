using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float damage = 5f;
    public bool piercing = false;
    public float lifetime = 4f;

    void Start()
    {
       
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Enemy")) return;

        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy == null) return;

        enemy.TakeDamage(damage, piercing);

        if (!piercing)
            Destroy(gameObject);
    }
}
