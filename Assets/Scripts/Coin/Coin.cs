using UnityEngine;

public class Coin : MonoBehaviour
{
    [SerializeField] private float shootSpeedBonus = 0.5f;
    [SerializeField] private float damageBonus = 0.5f;
    [SerializeField] private GameObject coinPrefab;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SpawnCoin(Transform spawnPoint)
    {
        Instantiate(coinPrefab, spawnPoint.position, spawnPoint.rotation);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.root.CompareTag("Player"))
        {
            GameEvent.OnCoinCollected?.Invoke(shootSpeedBonus, damageBonus);
            Destroy(gameObject);
        }
    }
}
