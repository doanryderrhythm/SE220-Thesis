using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float damage = 5f;
    public bool piercing = false;

    void Start()
    {
       
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Enemy")) return;

        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy == null) return;

        enemy.TakeDamage(damage, piercing);

        
            Destroy(gameObject);
    }
}
