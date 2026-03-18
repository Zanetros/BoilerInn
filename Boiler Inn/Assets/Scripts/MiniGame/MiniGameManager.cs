using System;
using System.Collections.Generic;
using UnityEngine;

public class MiniGameManager : MonoBehaviour
{
    public static MiniGameManager instance;
    
    [Header("Global UI")]
    public GameObject dialogueUI;
    public GameObject finalScoreUI;
    public DialogueManager dialogueManager;

    [Serializable]
    public struct MinigameEntry
    {
        public string eventID;
        public GameObject minigameContainer;
    }

    [Header("Minigames Database")]
    public List<MinigameEntry> minigamesList = new List<MinigameEntry>();

    private Dictionary<string, GameObject> minigameDictionary = new Dictionary<string, GameObject>();

    private GameObject activeMinigameContainer; 
    private bool isGameActive = false;
    
    // NOVA VARIÁVEL: Guarda a pontuação calculada
    public int currentScore = 0; 

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        foreach (var entry in minigamesList)
        {
            if (!minigameDictionary.ContainsKey(entry.eventID))
            {
                minigameDictionary.Add(entry.eventID, entry.minigameContainer);
            }
        }
    }
    
    public void TriggerMinigame(string eventID)
    {
        if (dialogueManager == null || dialogueManager.currentNode == null) return;
        RunTimeDialogueNode nodeData = dialogueManager.currentNode;

        if (CheckRequirements(nodeData.cyberCost, nodeData.implantsCost, nodeData.chipsCost))
        {
            if (minigameDictionary.TryGetValue(eventID, out activeMinigameContainer))
            {
                activeMinigameContainer.SetActive(true); 
                dialogueUI.SetActive(false);
                isGameActive = true;
                currentScore = 0; // Reseta a pontuação ao iniciar um novo jogo
                
                Debug.Log($"Requirements met. Starting minigame: {eventID}");
            }
            else
            {
                Debug.LogWarning($"Minigame with EventID '{eventID}' not found in dictionary!");
                dialogueManager.ResumeDialogueAfterEvent();
            }
        }
        else
        {
            Debug.LogWarning("Insufficient resources to start the minigame.");
            dialogueManager.ResumeDialogueAfterEvent(); 
        }
    }

    private bool CheckRequirements(int reqCyber, int reqImplants, int reqChips)
    {
        if (CurrencyManager.instance == null) return false;

        return CurrencyManager.instance.cybercurrency >= reqCyber && 
               CurrencyManager.instance.implants >= reqImplants && 
               CurrencyManager.instance.chips >= reqChips;
    }

    // NOVO MÉTODO: Calcula a pontuação baseada em acertos e total
    public void CalculateScore(int hits, int totalGoals)
    {
        float accuracy = (float)hits / totalGoals;

        if (accuracy >= 1f) currentScore = 10;
        else if (accuracy >= 0.7f) currentScore = 7;
        else if (accuracy >= 0.5f) currentScore = 5;
        else if (accuracy >= 0.2f) currentScore = 2;
        else currentScore = 0;

        Debug.Log("Final Score Calculated in Manager: " + currentScore);
    }

    public void FinalScore()
    {
        if (!isGameActive) return;
        finalScoreUI.SetActive(true);
    }

    public void FinishMiniGame()
    {
        isGameActive = false;
        finalScoreUI.SetActive(false);

        if (activeMinigameContainer != null)
        {
            activeMinigameContainer.SetActive(false);
            activeMinigameContainer = null;
        }
        
        dialogueUI.SetActive(true);

        if (dialogueManager != null) dialogueManager.ResumeDialogueAfterEvent();
    }
}