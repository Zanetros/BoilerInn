using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class HitBar : MonoBehaviour
{
    [Header("Inputs")]
    public InputActionReference redKey;
    public InputActionReference blueKey;
    public InputActionReference yellowKey;

    private List<NoteData> notesInside = new List<NoteData>();

    public static int hits = 0;
    public static int score = 0;
    
    [Header("Sounds")]
    public AudioClip hitSound;
    public AudioClip missSound;

    [Header("Visual Effects (VFX)")]
    public GameObject hitEffectPrefab; // Arraste seu prefab de partícula aqui
    private List<GameObject> effectPool = new List<GameObject>(); // Piscina das partículas

    public int totalNotes = 20;

    private void OnEnable()
    {
        hits = 0; 
        redKey.action.started += OnRedPressed;
        blueKey.action.started += OnBluePressed;
        yellowKey.action.started += OnYellowPressed;
    }

    private void OnDisable()
    {
        redKey.action.started -= OnRedPressed;
        blueKey.action.started -= OnBluePressed;
        yellowKey.action.started -= OnYellowPressed;
    }

    void OnRedPressed(InputAction.CallbackContext ctx) => TryHit("red");
    void OnBluePressed(InputAction.CallbackContext ctx) => TryHit("blue");
    void OnYellowPressed(InputAction.CallbackContext ctx) => TryHit("yellow");

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out NoteData note))
        {
            notesInside.Add(note);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent(out NoteData note))
        {
            notesInside.Remove(note);
        }
    }

    void TryHit(string inputColor)
    {
        if (notesInside.Count == 0)
        {
            if (SoundManager.instance) SoundManager.instance.PlaySFX(missSound);
            return;
        }

        NoteData note = notesInside[0];

        if (note.color == inputColor)
        {
            hits++;
            if (SoundManager.instance) SoundManager.instance.PlaySFX(hitSound);
            
            // Toca o efeito visual reciclado na posição da nota
            SpawnHitEffect(note.transform.position);
            
            // Remove da lista interna
            notesInside.RemoveAt(0);
            
            // OTIMIZAÇÃO: Apenas desliga a nota, devolvendo-a para a piscina do Spawner!
            note.gameObject.SetActive(false);
        }
        else
        {
            if (SoundManager.instance) SoundManager.instance.PlaySFX(missSound);
        }
    }

    // Função que recicla e toca as partículas
    private void SpawnHitEffect(Vector3 position)
    {
        if (hitEffectPrefab == null) return;

        // Tenta achar uma partícula desligada na piscina
        GameObject effect = null;
        foreach (GameObject obj in effectPool)
        {
            if (!obj.activeInHierarchy)
            {
                effect = obj;
                break;
            }
        }

        // Se a piscina estiver vazia ou todos os efeitos estiverem tocando, cria um novo
        if (effect == null)
        {
            effect = Instantiate(hitEffectPrefab);
            effectPool.Add(effect);
        }

        // Posiciona no local do acerto e liga
        effect.transform.position = position;
        effect.SetActive(true);

        // Manda o sistema de partículas rodar
        if (effect.TryGetComponent(out ParticleSystem ps))
        {
            ps.Play();
        }
    }
    
    public void CalculateScore()
    {
        float accuracy = (float)hits / totalNotes;

        if (accuracy >= 1f) score = 10;
        else if (accuracy >= 0.7f) score = 7;
        else if (accuracy >= 0.5f) score = 5;
        else if (accuracy >= 0.2f) score = 2;
        else score = 0;

        Debug.Log("Final Score: " + score);
    }
}