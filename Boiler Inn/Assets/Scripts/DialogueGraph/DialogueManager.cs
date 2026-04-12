using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager instance;
    
    public RuntimeDialogueGraph runtimeGraph;
    
    [Header("UI Components")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI speakerNameText;
    public TextMeshProUGUI dialogueText;
    public Image characterSprite;
    
    [Header("Choice Button UI")]
    public Button choiceButtonPrefab; 
    public Transform choiceButtonContainer;
    
    [Header("Text Settings")]
    [SerializeField] private float typingSpeed = 0.05f;
    
    [Header("Audio Settings")]
    public AudioClip shortTypingSound;   
    public AudioClip mediumTypingSound;  
    public AudioClip longTypingSound;    
   
    [Header("Text Length Thresholds")]
    public int shortTextLimit = 20;  
    public int mediumTextLimit = 60;
    
    private Coroutine typingCoroutine; 
    private WaitForSeconds typingDelay; 
    
    private Dictionary<string, RunTimeDialogueNode> nodeLookup = new Dictionary<string, RunTimeDialogueNode>();
    public RunTimeDialogueNode currentNode { get; private set; }
    
    private CharacterProfile activeSpeaker; 
    
    private int lastReceivedCyber = 0;
    private int lastReceivedImplants = 0;
    private int lastReceivedChips = 0;

    private void Awake()
    {
        typingDelay = new WaitForSeconds(typingSpeed);
        
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (runtimeGraph == null)
        {
            Debug.LogError("[ERRO DIALOGUE MANAGER] O campo 'Runtime Graph' está vazio no Inspector!");
            return; 
        }

        foreach (var node in runtimeGraph.AllNodes)
        {
            nodeLookup[node.NodeID] = node;
        }

        if (SceneManager.GetActiveScene().name == "City") EndDialogue();
        else GoToNextNode(runtimeGraph.EntryNodeID);
    }

    public void Update()
    {
        if (PauseMenu.IsGamePaused) return;
        
        if (dialoguePanel.activeSelf && Mouse.current.leftButton.wasPressedThisFrame && currentNode != null && currentNode.Choices.Count == 0)
        {
            GoToNextNode(currentNode.NextNodeID);
        }
    }
    
    private void ShowNode(string nodeID)
    {
        if (!nodeLookup.TryGetValue(nodeID, out RunTimeDialogueNode node))
        {
            EndDialogue();
            return;
        }
    
        currentNode = node;

        // Se o nó tiver um personagem arrastado nele, o Manager atualiza a memória.
        // Se for um nó de lógica vazio, ele continua lembrando de quem falou por último!
        if (currentNode.speakerProfile != null)
        {
            activeSpeaker = currentNode.speakerProfile;
        }

        if (currentNode.isImpostorNode) HandleImpostorNode();
        else if (currentNode.isConditionNode) HandleConditionNode();
        else if (currentNode.isReceiveNode) HandleReceiveNode();
        else if (currentNode.isAdvanceStoryNode) HandleAdvanceStoryNode();
        else if (currentNode.isGoToCityNode) HandleGoToCityNode();
        else if (!string.IsNullOrEmpty(currentNode.EventID)) HandleEventNode();
        else if (currentNode.isCreditsNode) HandleCreditsNode();
        else ProcessStandardDialogue(); 
    }

    private void HandleImpostorNode()
    {
        // Agora usa o activeSpeaker! Mais seguro caso você esqueça de preencher no Grafo.
        if (ImpostorManager.instance != null && activeSpeaker != null)
        {
            ImpostorManager.instance.PlantChip(activeSpeaker);
        }
        GoToNextNode(currentNode.NextNodeID);
    }

    private void HandleConditionNode()
    {
        bool conditionMet = false;
        if (currentNode.conditionID == "ImpostorCaught")
        {
            conditionMet = ImpostorManager.isImpostorCaught;
        }

        string nextNode = conditionMet ? currentNode.NextNodeID_True : currentNode.NextNodeID_False;
        GoToNextNode(nextNode);
    }

    private void HandleReceiveNode()
    {
        if (DayManager.instance != null && activeSpeaker != null)
        {
            if (DayManager.instance.charactersPaidToday.Contains(activeSpeaker))
            {
                GoToNextNode(currentNode.NextNodeID);
                return;
            }
        }

        lastReceivedCyber = currentNode.cyberCost;
        lastReceivedImplants = currentNode.implantsCost;
        lastReceivedChips = currentNode.chipsCost;

        if (CurrencyManager.instance != null)
        {
            CurrencyManager.instance.cybercurrency += currentNode.cyberCost;
            CurrencyManager.instance.implants += currentNode.implantsCost;
            CurrencyManager.instance.chips += currentNode.chipsCost;

            CurrencyManager.instance.AddCybercurrency(0);
            CurrencyManager.instance.AddImplants(0);
            CurrencyManager.instance.AddChips(0);
        }
        
        if (DayManager.instance != null && activeSpeaker != null)
        {
            if (!DayManager.instance.charactersPaidToday.Contains(activeSpeaker))
            {
                DayManager.instance.charactersPaidToday.Add(activeSpeaker);
            }
        }

        GoToNextNode(currentNode.NextNodeID);
    }

    private void HandleAdvanceStoryNode()
    {
        if (DayManager.instance != null)
        {
            DayManager.instance.AdvanceCharacterStory(currentNode.advanceCharacterProfile);
        }
        GoToNextNode(currentNode.NextNodeID);
    }

    private void HandleGoToCityNode()
    {
        if (DayManager.instance != null) DayManager.instance.GoToCity();
        else Debug.LogWarning("DayManager não encontrado na cena!");
        
        EndDialogue();
    }

    private void HandleEventNode()
    {
        bool hasEnoughCurrency = false;

        if (CurrencyManager.instance != null)
        {
            hasEnoughCurrency = (CurrencyManager.instance.cybercurrency >= currentNode.cyberCost) &&
                                (CurrencyManager.instance.implants >= currentNode.implantsCost) &&
                                (CurrencyManager.instance.chips >= currentNode.chipsCost);
        }

        if (hasEnoughCurrency)
        {
            currentNode.NextNodeID = currentNode.NextNodeID_True;
            dialoguePanel.SetActive(false);
            
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            if (SoundManager.instance != null) SoundManager.instance.FadeOutSFX(0.2f);
            if (MiniGameManager.instance != null) MiniGameManager.instance.TriggerMinigame(currentNode.EventID);
        }
        else
        {
            GoToNextNode(currentNode.NextNodeID_False);
        }
    }

    private void HandleCreditsNode()
    {
        EndDialogue(); 
        
        if (CreditsManager.instance != null) CreditsManager.instance.StartCredits();
        else SceneManager.LoadScene("FinalScene");
    }

    private void ProcessStandardDialogue()
    {
        if (currentNode.isHotelNode && HotelManager.instance != null)
        {
            HotelManager.instance.AddGuest(currentNode.guestID);
        }

        dialoguePanel.SetActive(true);
        
        if (currentNode.speakerProfile != null)
        {
            speakerNameText.SetText(currentNode.speakerProfile.characterName);
            if (currentNode.speakerProfile.characterSprite != null)
            {
                characterSprite.gameObject.SetActive(true);
                characterSprite.sprite = currentNode.speakerProfile.characterSprite;
                characterSprite.SetNativeSize();
            }
            else characterSprite.gameObject.SetActive(false);
        }
        else
        {
            speakerNameText.SetText("???");
            characterSprite.gameObject.SetActive(false);
        }
    
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        
        string processedText = FormatDialogueText(currentNode.DialogueText);
        
        if (SoundManager.instance != null && !string.IsNullOrEmpty(processedText))
        {
            int textLength = processedText.Length;
            AudioClip soundToPlay = (textLength <= shortTextLimit) ? shortTypingSound : 
                                    (textLength <= mediumTextLimit) ? mediumTypingSound : longTypingSound;

            if (soundToPlay != null) SoundManager.instance.PlaySFX(soundToPlay);
        }
        
        typingCoroutine = StartCoroutine(TypeText(processedText));
        RefreshChoices();
    }

    private void RefreshChoices()
    {
        foreach (Transform child in choiceButtonContainer) Destroy(child.gameObject);

        if (currentNode.Choices.Count > 0)
        {
            foreach (var choice in currentNode.Choices)
            {
                Button button = Instantiate(choiceButtonPrefab, choiceButtonContainer);
                
                if (button.GetComponentInChildren<TextMeshProUGUI>() is TextMeshProUGUI buttonText)
                {
                    buttonText.text = FormatDialogueText(choice.ChoiceText);
                    
                    string cleanChoiceText = choice.ChoiceText.Trim().ToLower();

                    if (cleanChoiceText.Contains("spy chip") && ImpostorManager.instance != null && ImpostorManager.instance.HasUsedChip) 
                    {
                        button.interactable = false; 
                    }

                    bool isCollectButton = cleanChoiceText.Contains("collect tribute");

                    // Avalia se o activeSpeaker está na lista de quem já pagou
                    if (isCollectButton && DayManager.instance != null && activeSpeaker != null)
                    {
                        if (DayManager.instance.charactersPaidToday.Contains(activeSpeaker))
                        {
                            button.interactable = false;
                        }
                    }
                }

                button.onClick.AddListener(() =>
                {
                    if (PauseMenu.IsGamePaused) return;
                    
                    string clickedCleanText = choice.ChoiceText.Trim().ToLower();

                    if (currentNode != null && currentNode.isHotelNode && clickedCleanText.Contains("accept"))
                    {
                        if (HotelManager.instance != null) HotelManager.instance.AddGuest(currentNode.guestID);
                    }

                    GoToNextNode(choice.DestinationNodeID);
                });
            }
        }
    }
    
    private void GoToNextNode(string nextNodeID)
    {
        if (!string.IsNullOrEmpty(nextNodeID)) ShowNode(nextNodeID);
        else EndDialogue();
    }

    private void EndDialogue()
    {
        dialoguePanel.SetActive(false);
        currentNode = null;
        activeSpeaker = null;
        
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        
        foreach (Transform child in choiceButtonContainer) Destroy(child.gameObject);
        
        if (SoundManager.instance != null) SoundManager.instance.FadeOutSFX(0.2f);
    }
    
    public void ResumeDialogueAfterEvent()
    {
        dialoguePanel.SetActive(true);
        GoToNextNode(currentNode?.NextNodeID);
    }
    
    public void SwitchDialogue(RuntimeDialogueGraph newGraph)
    {
        if (newGraph == null) return;

        runtimeGraph = newGraph;
        nodeLookup.Clear();

        foreach (var node in runtimeGraph.AllNodes)
        {
            nodeLookup[node.NodeID] = node;
        }

        GoToNextNode(runtimeGraph.EntryNodeID);
    }
    
    private string FormatDialogueText(string rawText)
    {
        if (string.IsNullOrEmpty(rawText)) return rawText;

        string formattedText = rawText
            .Replace("{Cyber}", lastReceivedCyber.ToString())
            .Replace("{Implants}", lastReceivedImplants.ToString())
            .Replace("{Chips}", lastReceivedChips.ToString());

        if (CurrencyManager.instance != null)
        {
            formattedText = formattedText.Replace("{TotalCyber}", CurrencyManager.instance.cybercurrency.ToString());
        }

        return formattedText;
    }

    private IEnumerator TypeText(string text)
    {
        dialogueText.text = text;
        dialogueText.maxVisibleCharacters = 0;
        int totalCharacters = text.Length;
        
        for (int i = 0; i <= totalCharacters; i++)
        {
            dialogueText.maxVisibleCharacters = i;
            yield return typingDelay; 
        }
        
        if (SoundManager.instance != null) SoundManager.instance.FadeOutSFX(0.2f);
        typingCoroutine = null;
    }
}