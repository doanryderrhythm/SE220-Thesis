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
        EliteEnemy eliteEnemy = other.GetComponent<EliteEnemy>();
        if (enemy == null && eliteEnemy == null) return;

        if (enemy != null)
        {
            enemy.TakeDamage(damage, piercing);
        }

        if (eliteEnemy != null)
        {
           if(eliteEnemy.type == EliteEnemyType.Bulletdeflector)
            {
                enemy.TakeDamage(0,true); // Đánh dấu là đạn đã bị deflect để tránh bị deflect lại vô hạn
                Vector2 deflectDirection = (transform.position - eliteEnemy.transform.position).normalized;
            }
            else{
            eliteEnemy.TakeDamage(damage, piercing);
            
            }
        }

        Destroy(gameObject);
    }
}
