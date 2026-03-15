using System;
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
    public int maxSpawns = 20; // quantidade máxima de objetos

    private float timer;
    private int currentSpawns = 0;

    void Start()
    {
        timer = spawnInterval;
    }

    void Update()
    {
        if (currentSpawns >= maxSpawns)
            return; // para de spawnar

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

        currentSpawns++; // conta o spawn
    }
}
