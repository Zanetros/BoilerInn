using System.Collections.Generic;
using UnityEngine;

public class ChipsSpawner : MonoBehaviour
{
    [Header("Spawn Points")]
    public Transform[] spawnPoints;

    [Header("Prefabs")]
    public GameObject[] prefabs;

    [Header("Spawn Settings")]
    public float spawnInterval = 1f;
    public int maxSpawns = 20; 

    private float timer;
    private int currentSpawns = 0;

    // --- A PISCINA DE OBJETOS ---
    private List<GameObject> objectPool = new List<GameObject>();

    public bool IsFinished => currentSpawns >= maxSpawns;

    private void OnEnable()
    {
        // Reseta os contadores quando o minigame inicia
        currentSpawns = 0;
        timer = spawnInterval;
    }

    void Update()
    {
        if (currentSpawns >= maxSpawns) return; 

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            SpawnRandomObject();
            timer = spawnInterval;
        }
    }

    void SpawnRandomObject()
    {
        // Escolhe o ponto e o tipo de nota aleatoriamente
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        GameObject prefabToSpawn = prefabs[Random.Range(0, prefabs.Length)];

        // TENTA RECICLAR UMA NOTA DESLIGADA
        GameObject note = GetPooledObject(prefabToSpawn.name);

        if (note == null)
        {
            // SE NÃO ACHOU: Cria uma nota nova do zero
            note = Instantiate(prefabToSpawn, spawnPoint.position, Quaternion.identity);
            
            // Remove o "(Clone)" do nome para o sistema reconhecer a cor certa depois
            note.name = prefabToSpawn.name; 
            
            // Adiciona na piscina para reciclagem futura
            objectPool.Add(note);
        }
        else
        {
            // SE ACHOU: Move a nota antiga invisível lá pro topo e liga ela de novo!
            note.transform.position = spawnPoint.position;
            note.SetActive(true);
        }

        currentSpawns++; 
    }

    // Função inteligente que procura notas desligadas e ignora notas destruídas
    private GameObject GetPooledObject(string prefabName)
    {
        // OTIMIZAÇÃO DE SEGURANÇA: Se algum outro script der Destroy sem querer, 
        // essa linha remove o "fantasma" da lista e impede que a Unity trave.
        objectPool.RemoveAll(item => item == null);

        foreach (GameObject obj in objectPool)
        {
            // Retorna o objeto apenas se ele estiver invisível e for do mesmo tipo/cor
            if (!obj.activeInHierarchy && obj.name == prefabName)
            {
                return obj;
            }
        }
        return null; // A piscina está vazia ou todas as notas estão caindo na tela
    }
}