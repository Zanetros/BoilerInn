using UnityEngine;
using Random = UnityEngine.Random;

public class ChipsSpawner : MonoBehaviour
{
    [Header("Spawn Points")]
    public Transform[] spawnPoints;

    [Header("Prefabs")]
    public GameObject[] prefabs;

    [Header("Spawn Settings")]
    public float spawnInterval = 1f;

    [Header("Spawn Limit")]
    public int maxSpawns = 20; 

    private float timer;
    private int currentSpawns = 0;

    // Propriedade para o Manager consultar
    public bool IsFinished => currentSpawns >= maxSpawns;

    void Start()
    {
        timer = spawnInterval;
    }

    void Update()
    {
        if (currentSpawns >= maxSpawns)
            return; 

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            SpawnRandomObject();
            timer = spawnInterval;
        }
    }

    void SpawnRandomObject()
    {
        int randomPointIndex = Random.Range(0, spawnPoints.Length);
        Transform spawnPoint = spawnPoints[randomPointIndex];

        int randomPrefabIndex = Random.Range(0, prefabs.Length);
        GameObject prefab = prefabs[randomPrefabIndex];

        Instantiate(prefab, spawnPoint.position, Quaternion.identity);

        currentSpawns++; 
    }
}