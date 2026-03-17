using System;
using System.Collections.Generic;
using UnityEngine;

public class MiniGameManager : MonoBehaviour
{
    public static MiniGameManager instance;
    
    [Header("Global UI")]
    public GameObject miniGameUI;
    public GameObject dialogueUI;
    public GameObject finalScoreUI;
    public DialogueManager dialogueManager;

    // Estrutura que mapeia o ID do evento para o objeto do minigame
    [Serializable]
    public struct MinigameEntry
    {
        public string eventID;
        public GameObject minigameContainer; // O objeto pai que contém o minigame específico
    }

    [Header("Minigames Database")]
    public List<MinigameEntry> minigamesList = new List<MinigameEntry>();

    private GameObject activeMinigameContainer; // Guarda qual minigame está rodando agora
    private bool isGameActive = false;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }
    
    // Chamado pelo DialogueManager passando o ID do nó
    public void TriggerMinigame(string eventID)
    {
        if (dialogueManager == null || dialogueManager.currentNode == null) return;
        RunTimeDialogueNode nodeData = dialogueManager.currentNode;

        if (CheckRequirements(nodeData.cyberCost, nodeData.implantsCost, nodeData.chipsCost))
        {
            // Busca o minigame correto na lista
            foreach (var minigame in minigamesList)
            {
                if (minigame.eventID == eventID)
                {
                    activeMinigameContainer = minigame.minigameContainer;
                    break;
                }
            }

            // Se achou o minigame, liga tudo
            if (activeMinigameContainer != null)
            {
                miniGameUI.SetActive(true);
                activeMinigameContainer.SetActive(true); // Liga APENAS o minigame solicitado
                dialogueUI.SetActive(false);
                isGameActive = true;
                
                Debug.Log($"Requirements met. Starting minigame: {eventID}");
            }
            else
            {
                Debug.LogWarning($"Minigame with EventID '{eventID}' not found in MiniGameManager list!");
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
        CurrencyManager wallet = CurrencyManager.instance;
        if (wallet == null) return false;

        return wallet.cybercurrency >= reqCyber && 
               wallet.implants >= reqImplants && 
               wallet.chips >= reqChips;
    }

    // Chamado pelo script controlador de cada minigame individual quando eles terminam
    public void FinalScore()
    {
        if (!isGameActive) return;
        finalScoreUI.SetActive(true);
        // Desligamos o minigame atual para não continuar rodando atrás do painel de score
        if (activeMinigameContainer != null) activeMinigameContainer.SetActive(false); 
    }

    // Geralmente chamado por um botão na tela de "FinalScoreUI"
    public void FinishMiniGame()
    {
        isGameActive = false;
        
        miniGameUI.SetActive(false);
        finalScoreUI.SetActive(false);
        activeMinigameContainer = null;
        dialogueUI.SetActive(true);

        if (dialogueManager != null)
        {
            dialogueManager.ResumeDialogueAfterEvent();
        }
    }
}