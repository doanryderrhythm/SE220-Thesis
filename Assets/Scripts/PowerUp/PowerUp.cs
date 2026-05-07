using UnityEngine;

public class PowerUp : MonoBehaviour
{
    [SerializeField] private PowerUpData powerUpData;

    public void SetPowerUp(Transform transform)
    {
        Instantiate(powerUpData, transform.position, transform.rotation);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.root.CompareTag("Player"))
        {
            switch (powerUpData.Type)
            {
                case PowerUpType.PLAYER:
                    GameEvent.OnPlayerPowerUpCollected?.Invoke(powerUpData.value);
                    break;
                case PowerUpType.TOWER:
                    GameEvent.OnTowerPowerUpCollected?.Invoke(powerUpData.value);
                    break;
                case PowerUpType.GUN:
                    GameEvent.OnGunPowerUpCollected?.Invoke(powerUpData.value);
                    break;
            }
            Destroy(gameObject);
        }
    }
}
