using UnityEngine;

public class Arrow : MonoBehaviour
{
    private Transform target;
    private float damage;
    public float speed = 10f;

    public void SetTarget(Transform target)
    {
        this.target = target;
    }

    public void SetDamage(float damage)
    {
        this.damage = damage;
    }

    void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        // Move the arrow towards the target
        Vector3 direction = (target.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;

        // Check if the arrow has reached the target
        if (Vector3.Distance(transform.position, target.position) < 0.2f)
        {
            HitTarget();
        }
    }

    private void HitTarget()
    {
        Tower tower = target.GetComponent<Tower>();
        if (tower != null)
        {
            tower.TakeDamage(damage);
        }

        Destroy(gameObject);
    }
}