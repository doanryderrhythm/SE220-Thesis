using System.Reflection;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float damage = 5f;
    public bool piercing = false;
    public float lifetime = 5f;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Default"))
        {
            Destroy(gameObject);
            return; 
        }

        if (!other.CompareTag("Enemy")) return;

        Enemy enemy = other.GetComponent<Enemy>();
        EliteEnemy eliteEnemy = other.GetComponent<EliteEnemy>();
        
        if (enemy == null && eliteEnemy == null) return;

        if (enemy != null)
        {
            enemy.TakeDamage(damage, piercing);
            Destroy(gameObject);
        }

        if (eliteEnemy != null)
        {
            if (eliteEnemy.type == EliteEnemyType.Bulletdeflector)
            {
                if (enemy != null) enemy.TakeDamage(0, false); 
                
                Vector2 deflectDirection = (transform.position - eliteEnemy.transform.position).normalized;
                Destroy(gameObject, lifetime); 
            }
            else
            {
                eliteEnemy.TakeDamage(damage, piercing);
                Destroy(gameObject);
            }
        }
    }
}