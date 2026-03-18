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
    
    public int currentScore = 0; 
    public int currentMisses = 0; // NOVA VARIÁVEL: Guarda os erros para a cobrança final

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

        // Verifica se tem o dinheiro base (sem multa) para poder entrar
        if (CheckRequirements(nodeData.cyberCost, nodeData.implantsCost, nodeData.chipsCost))
        {
            if (minigameDictionary.TryGetValue(eventID, out activeMinigameContainer))
            {
                activeMinigameContainer.SetActive(true); 
                dialogueUI.SetActive(false);
                isGameActive = true;
                currentScore = 0; 
                currentMisses = 0; // Reseta erros ao iniciar
                
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

    // ATUALIZADO: Agora recebe também a quantidade de erros (misses)
    public void CalculateScore(int hits, int misses, int totalGoals)
    {
        currentMisses = misses; // Guarda os erros para usar no pagamento

        float accuracy = (float)hits / totalGoals;

        if (accuracy >= 1f) currentScore = 10;
        else if (accuracy >= 0.7f) currentScore = 7;
        else if (accuracy >= 0.5f) currentScore = 5;
        else if (accuracy >= 0.2f) currentScore = 2;
        else currentScore = 0;

        Debug.Log($"Score: {currentScore} | Hits: {hits} | Misses: {misses}");
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

        // Processa o pagamento e possíveis multas antes de fechar o jogo
        ProcessPayment();

        if (activeMinigameContainer != null)
        {
            activeMinigameContainer.SetActive(false);
            activeMinigameContainer = null;
        }
        
        dialogueUI.SetActive(true);

        if (dialogueManager != null) dialogueManager.ResumeDialogueAfterEvent();
    }

    // NOVO MÉTODO: Faz a cobrança real baseado no desempenho
   private void ProcessPayment()
    {
        if (dialogueManager == null || dialogueManager.currentNode == null) return;
        
        RunTimeDialogueNode nodeData = dialogueManager.currentNode;
        CurrencyManager wallet = CurrencyManager.instance;
        
        if (wallet != null)
        {
            // Pega os custos base
            int finalCyber = nodeData.cyberCost;
            int finalImplants = nodeData.implantsCost;
            int finalChips = nodeData.chipsCost;

            bool isPenalized = currentMisses >= 5;

            // MULTA DE 20%: Garante que a multa seja de no mínimo +1 (se o custo não for zero)
            if (isPenalized)
            {
                if (finalCyber > 0) finalCyber += Mathf.Max(1, Mathf.RoundToInt(finalCyber * 0.2f));
                if (finalImplants > 0) finalImplants += Mathf.Max(1, Mathf.RoundToInt(finalImplants * 0.2f));
                if (finalChips > 0) finalChips += Mathf.Max(1, Mathf.RoundToInt(finalChips * 0.2f));
            }

            // Subtrai do CurrencyManager
            wallet.cybercurrency -= finalCyber;
            wallet.implants -= finalImplants;
            wallet.chips -= finalChips;

            // --- TRAVA DE SEGURANÇA (SALDO NEGATIVO) ---
            if (wallet.cybercurrency < 0) wallet.cybercurrency = 0;
            if (wallet.implants < 0) wallet.implants = 0;
            if (wallet.chips < 0) wallet.chips = 0;
            // -------------------------------------------

            // Força a UI a atualizar com os valores já corrigidos
            wallet.AddCybercurrency(0);
            wallet.AddImplants(0);
            wallet.AddChips(0);

            if (isPenalized)
            {
                Debug.LogWarning($"PENA APLICADA! Erros: {currentMisses}. Custo final (+20% ou min +1): Cyber({finalCyber}) Implants({finalImplants}) Chips({finalChips})");
            }
            else
            {
                Debug.Log($"Pagamento normal. Custo final: Cyber({finalCyber}) Implants({finalImplants}) Chips({finalChips})");
            }
        }
    }
}