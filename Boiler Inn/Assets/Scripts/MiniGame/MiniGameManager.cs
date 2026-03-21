using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // <-- Obrigatório para manipular os textos da UI

public class MiniGameManager : MonoBehaviour
{
    public static MiniGameManager instance;
    
    [Header("Global UI")]
    public GameObject dialogueUI;
    public GameObject finalScoreUI;
    public DialogueManager dialogueManager;

    [Header("Results UI Texts")]
    public TextMeshProUGUI resultTitleText; // Ex: "Success!" ou "Failed"
    public TextMeshProUGUI cyberCostText;
    public TextMeshProUGUI implantsCostText;
    public TextMeshProUGUI chipsCostText;

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
    public int currentMisses = 0; 

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
        // O DialogueManager já checou a carteira, então podemos só iniciar o jogo!
        if (minigameDictionary.TryGetValue(eventID, out activeMinigameContainer))
        {
            activeMinigameContainer.SetActive(true); 
            dialogueUI.SetActive(false);
            isGameActive = true;
            currentScore = 0; 
            currentMisses = 0; 
            
            Debug.Log($"Starting minigame: {eventID}");
        }
        else
        {
            Debug.LogWarning($"Minigame with EventID '{eventID}' not found in dictionary!");
            if (dialogueManager != null) dialogueManager.ResumeDialogueAfterEvent();
        }
    }

    private bool CheckRequirements(int reqCyber, int reqImplants, int reqChips)
    {
        if (CurrencyManager.instance == null) return false;

        return CurrencyManager.instance.cybercurrency >= reqCyber && 
               CurrencyManager.instance.implants >= reqImplants && 
               CurrencyManager.instance.chips >= reqChips;
    }

    public void CalculateScore(int hits, int misses, int totalGoals)
    {
        currentMisses = misses; 

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
        
        // Faz a matemática e atualiza os textos (incluindo o título novo)
        ProcessPaymentAndUpdateUI();

        // Agora sim mostra o painel para o jogador ver o estrago
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

    private void ProcessPaymentAndUpdateUI()
    {
        if (dialogueManager == null || dialogueManager.currentNode == null) return;
        
        RunTimeDialogueNode nodeData = dialogueManager.currentNode;
        CurrencyManager wallet = CurrencyManager.instance;
        
        if (wallet != null)
        {
            int finalCyber = nodeData.cyberCost;
            int finalImplants = nodeData.implantsCost;
            int finalChips = nodeData.chipsCost;

            bool isPenalized = currentMisses >= 5;
            
            if (resultTitleText != null)
            {
                if (isPenalized)
                {
                    resultTitleText.text = "Failed!"; // Texto de falha
                }
                else
                {
                    resultTitleText.text = "Success!"; // Texto de sucesso
                }
            }

            if (isPenalized)
            {
                if (finalCyber > 0) finalCyber += Mathf.Max(1, Mathf.RoundToInt(finalCyber * 0.2f));
                if (finalImplants > 0) finalImplants += Mathf.Max(1, Mathf.RoundToInt(finalImplants * 0.2f));
                if (finalChips > 0) finalChips += Mathf.Max(1, Mathf.RoundToInt(finalChips * 0.2f));
            }

            wallet.cybercurrency -= finalCyber;
            wallet.implants -= finalImplants;
            wallet.chips -= finalChips;

            if (wallet.cybercurrency < 0) wallet.cybercurrency = 0;
            if (wallet.implants < 0) wallet.implants = 0;
            if (wallet.chips < 0) wallet.chips = 0;

            wallet.AddCybercurrency(0);
            wallet.AddImplants(0);
            wallet.AddChips(0);

            UpdateCurrencyText(cyberCostText, finalCyber, "Cyber");
            UpdateCurrencyText(implantsCostText, finalImplants, "Implants");
            UpdateCurrencyText(chipsCostText, finalChips, "Chips");
        }
    }

    private void UpdateCurrencyText(TextMeshProUGUI uiText, int finalCost, string currencyName)
    {
        if (uiText == null) return;

        if (finalCost > 0)
        {
            uiText.text = $"- {finalCost} {currencyName}";
            uiText.color = Color.red; 
        }
        else
        {
            uiText.text = $"0 {currencyName}";
            uiText.color = Color.green; 
        }
    }
}