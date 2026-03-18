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

    public int totalNotes = 20; // Ajuste para o mesmo valor do maxSpawns

    private void OnEnable()
    {
        hits = 0; // Resetar hits ao começar
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
        NoteData note = other.GetComponent<NoteData>();
        if (note != null) notesInside.Add(note);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        NoteData note = other.GetComponent<NoteData>();
        if (note != null) notesInside.Remove(note);
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
            Destroy(note.gameObject);
            notesInside.Remove(note);
        }
        else
        {
            if (SoundManager.instance) SoundManager.instance.PlaySFX(missSound);
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

        Debug.Log("Pontuação final: " + score);
    }
}