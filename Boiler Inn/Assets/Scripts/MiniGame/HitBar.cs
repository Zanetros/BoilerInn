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

    public int hits = 0; 
    public int misses = 0; // NOVA VARIÁVEL: Conta os erros de timing/cor
    public int totalNotes = 20; 
    
    [Header("Sounds")]
    public AudioClip hitSound;
    public AudioClip missSound;

    private void OnEnable()
    {
        hits = 0; 
        misses = 0; // Reseta os erros ao iniciar
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
        // ERRO DE TIMING: Apertou o botão mas não tinha nota na barra
        if (notesInside.Count == 0)
        {
            misses++; 
            if (SoundManager.instance) SoundManager.instance.PlaySFX(missSound);
            return;
        }

        NoteData note = notesInside[0];

        if (note.color == inputColor)
        {
            // ACERTOU
            hits++;
            if (SoundManager.instance) SoundManager.instance.PlaySFX(hitSound);
            
            notesInside.Remove(note);
            note.gameObject.SetActive(false); 
        }
        else
        {
            // ERRO DE COR: Apertou o botão mas a cor da nota era diferente
            misses++; 
            if (SoundManager.instance) SoundManager.instance.PlaySFX(missSound);
        }
    }
    
    public void RegisterMissedNote(NoteData note)
    {
        misses++; 
        
        if (SoundManager.instance) SoundManager.instance.PlaySFX(missSound);
        
        // Medida de segurança: tira a nota da lista caso o jogador 
        // tenha apertado o botão no exato frame em que ela foi destruída
        if (notesInside.Contains(note))
        {
            notesInside.Remove(note);
        }
        
        Debug.Log("Nota passou direto! Misses: " + misses);
    }
}