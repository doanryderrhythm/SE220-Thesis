using UnityEngine;

public class LevelEnvironment : MonoBehaviour
{
    [SerializeField] Transform spawnPoint;

    public Transform GetSpawnPoint()
    {
        return spawnPoint;
    }
}
