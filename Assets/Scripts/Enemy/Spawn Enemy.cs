using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Prefabs")]
    public GameObject infantryPrefab;
    public GameObject archerPrefab;
    public GameObject armoredInfantryPrefab;
    public GameObject towerDisablerPrefab;
    public GameObject executionerPrefab;
    public GameObject cheerleaderPrefab;
    public GameObject bulletDeflectorPrefab;

    [Header("Spawn Setup")]
    public Transform spawnPoint;
    public Transform[] routeWaypoints; 
    public float timeBetweenSpawns = 2f;

    [Header("Spawn Pattern")]
    public string[] spawnPattern = { "1", "1", "2", "3", "A" };

    private void Start()
    {
        if (spawnPattern.Length > 0)
        {
            StartCoroutine(SpawnEnemiesRoutine());
        }
    }

    private IEnumerator SpawnEnemiesRoutine()
    {
        foreach (string code in spawnPattern)
        {
        
            SpawnEnemyByCode(code, routeWaypoints);
            yield return new WaitForSeconds(timeBetweenSpawns);
        }
        Debug.Log("Đã spawn xong toàn bộ đợt quái!");
    }

    private void SpawnEnemyByCode(string code, Transform[] pathToFollow)
    {
        GameObject prefabToSpawn = null;

        switch (code.ToUpper())
        {
            case "1": prefabToSpawn = infantryPrefab; break;
            case "2": prefabToSpawn = archerPrefab; break;
            case "3": prefabToSpawn = armoredInfantryPrefab; break;
            case "A": prefabToSpawn = towerDisablerPrefab; break;
            default: return;
        }

        if (prefabToSpawn != null && spawnPoint != null)
        {
            GameObject spawnedEnemy = Instantiate(prefabToSpawn, spawnPoint.position, Quaternion.identity);
            
            Enemy normalEnemyScript = spawnedEnemy.GetComponent<Enemy>();
            if (normalEnemyScript != null)
            {
                normalEnemyScript.waypoints = pathToFollow;
            }
            else
            {
                EliteEnemy eliteEnemyScript = spawnedEnemy.GetComponent<EliteEnemy>();
                if (eliteEnemyScript != null)
                {
                    eliteEnemyScript.waypoints = pathToFollow;
                }
            }
        }
    }
}