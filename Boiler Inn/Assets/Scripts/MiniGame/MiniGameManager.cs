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

    // OTIMIZAÇÃO: Dicionário faz a busca do jogo ser instantânea na memória
    private Dictionary<string, GameObject> minigameDictionary = new Dictionary<string, GameObject>();

    private GameObject activeMinigameContainer; 
    private bool isGameActive = false;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        // Preenche o dicionário ao iniciar o jogo para pesquisas super rápidas
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
            // OTIMIZAÇÃO: Busca instantânea sem loop 'foreach'
            if (minigameDictionary.TryGetValue(eventID, out activeMinigameContainer))
            {
                activeMinigameContainer.SetActive(true); // Liga APENAS o minigame solicitado
                dialogueUI.SetActive(false);
                isGameActive = true;
                
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

    public void FinalScore()
    {
        if (!isGameActive) return;
        finalScoreUI.SetActive(true);
    }

    public void FinishMiniGame()
    {
        isGameActive = false;

        finalScoreUI.SetActive(false);

        //Desliga o minigame atual específico antes de limpar a referência
        if (activeMinigameContainer != null)
        {
            activeMinigameContainer.SetActive(false);
            activeMinigameContainer = null;
        }
        
        dialogueUI.SetActive(true);

        if (dialogueManager != null) dialogueManager.ResumeDialogueAfterEvent();
    }
}