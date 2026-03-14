using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class ChipsSpawner : MonoBehaviour
{
      [Header("Spawn Points")]
    public Transform[] spawnPoints; // 3 pontos de spawn (arraste os objetos na cena)

    [Header("Prefabs")]
    public GameObject[] prefabs;    // Array de prefabs para sortear

    [Header("Spawn Settings")]
    public float spawnInterval = 1f; // Intervalo entre spawns
    public float moveSpeed = 5f;     // Velocidade de movimento dos objetos spawnados
    public Vector3 moveDirection = Vector3.down; // Direção do movimento (ex: para baixo)

    private float timer;

    void Start()
    {
        timer = spawnInterval; // Começa já pronto para spawnar
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            SpawnRandomObject();
            timer = spawnInterval; // Reinicia o timer
        }
    }

    void SpawnRandomObject()
    {
        // Escolhe um ponto de spawn aleatório
        int randomPointIndex = Random.Range(0, spawnPoints.Length);
        Transform spawnPoint = spawnPoints[randomPointIndex];

        // Escolhe um prefab aleatório
        int randomPrefabIndex = Random.Range(0, prefabs.Length);
        GameObject prefab = prefabs[randomPrefabIndex];

        // Instancia o objeto
        GameObject spawnedObject = Instantiate(prefab, spawnPoint.position, Quaternion.identity);
    }
}
