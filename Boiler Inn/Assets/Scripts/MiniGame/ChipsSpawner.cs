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
    public int maxSpawns = 20; 
    
    [Header("Difficulty Settings")]
    public float noteSpeed = 5f; // <-- A velocidade de queda configurada direto no Spawner!

    private float timer;
    private int currentSpawns = 0;

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

        // 1. Cria a nota e guarda ela numa variável
        GameObject spawnedNote = Instantiate(prefab, spawnPoint.position, Quaternion.identity);

        // 2. Pega o script da nota recém-criada e injeta a velocidade nela!
        if (spawnedNote.TryGetComponent(out NoteData noteData))
        {
            noteData.currentSpeed = this.noteSpeed;
        }

        currentSpawns++; 
    }
}