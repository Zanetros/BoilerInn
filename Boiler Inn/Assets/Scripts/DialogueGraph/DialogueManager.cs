using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections;

public class DialogueManager : MonoBehaviour
{
    public RuntimeDialogueGraph runtimeGraph;
    
    [Header("UI Components")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI speakerNameText;
    public TextMeshProUGUI dialogueText;
    public Image characterSprite;
    
    [Header("Choice Button UI")]
    public Button choiceButtonPrefab; // Voltamos para o Button padrão da Unity!
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

    private void Awake()
    {
        typingDelay = new WaitForSeconds(typingSpeed);
    }

    private void Start()
    {
        foreach (var node in runtimeGraph.AllNodes)
        {
            nodeLookup[node.NodeID] = node;
        }

        if (!string.IsNullOrEmpty(runtimeGraph.EntryNodeID)) ShowNode(runtimeGraph.EntryNodeID);
        else EndDialogue();
    }

    public void Update()
    {
        // Trava do Pause no clique geral do mouse
        if (PauseMenu.IsGamePaused) return;
        
        if (dialoguePanel.activeSelf && Mouse.current.leftButton.wasPressedThisFrame && currentNode != null && currentNode.Choices.Count == 0)
        {
            if (!string.IsNullOrEmpty(currentNode.NextNodeID)) ShowNode(currentNode.NextNodeID);
            else EndDialogue();
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

        // --- NÓ DE HOTEL: Check-in Automático e Invisível ---
        if (currentNode.isHotelNode && HotelManager.instance != null)
        {
            HotelManager.instance.AddGuest(currentNode.guestID);
        }

        // --- NÓ DO IMPOSTOR: Checagem Automática e Invisível ---
        if (currentNode.isImpostorNode)
        {
            if (ImpostorManager.instance != null && currentNode.speakerProfile != null)
            {
                bool hadUsedChipBefore = ImpostorManager.instance.HasUsedChip;
                ImpostorManager.instance.PlantChip(currentNode.speakerProfile);

                if (!currentNode.speakerProfile.isImpostor && !hadUsedChipBefore)
                {
                    EndDialogue();
                    return; 
                }
            }

            if (!string.IsNullOrEmpty(currentNode.NextNodeID)) ShowNode(currentNode.NextNodeID);
            else EndDialogue();
            
            return; 
        }

        // --- NÓ DE CONDIÇÃO (Bifurcação Invisível) ---
        if (currentNode.isConditionNode)
        {
            bool conditionMet = false;

            if (currentNode.conditionID == "ImpostorCaught")
            {
                conditionMet = ImpostorManager.isImpostorCaught;
            }

            string nextNode = conditionMet ? currentNode.NextNodeID_True : currentNode.NextNodeID_False;

            if (!string.IsNullOrEmpty(nextNode)) ShowNode(nextNode);
            else EndDialogue();
            
            return; 
        }

        // --- NÓ DE EVENTO: Minigame ---
        if (!string.IsNullOrEmpty(currentNode.EventID))
        {
            dialoguePanel.SetActive(false);
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);

            if (MiniGameManager.instance != null) MiniGameManager.instance.TriggerMinigame(currentNode.EventID);
            return; 
        }

        dialoguePanel.SetActive(true);
        
        if (currentNode.speakerProfile != null)
        {
            speakerNameText.SetText(currentNode.speakerProfile.characterName);
            if (currentNode.speakerProfile.characterSprite != null)
            {
                characterSprite.sprite = currentNode.speakerProfile.characterSprite;
                characterSprite.SetNativeSize();
            }
        }
        else
        {
            speakerNameText.SetText("???");
        }
    
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        
        if (SoundManager.instance != null && !string.IsNullOrEmpty(currentNode.DialogueText))
        {
            int textLength = currentNode.DialogueText.Length;
            AudioClip soundToPlay = null;

            if (textLength <= shortTextLimit) soundToPlay = shortTypingSound;
            else if (textLength <= mediumTextLimit) soundToPlay = mediumTypingSound;
            else soundToPlay = longTypingSound;

            if (soundToPlay != null) SoundManager.instance.PlaySFX(soundToPlay);
        }
        
        typingCoroutine = StartCoroutine(TypeText(currentNode.DialogueText));
        
        RefreshChoices();
    }

    private void RefreshChoices()
    {
        foreach (Transform child in choiceButtonContainer) Destroy(child.gameObject);

        if (currentNode.Choices.Count > 0)
        {
            foreach (var choice in currentNode.Choices)
            {
                // Voltamos para a sua lógica original do GetComponent!
                Button button = Instantiate(choiceButtonPrefab, choiceButtonContainer);
                
                if (button.GetComponentInChildren<TextMeshProUGUI>() is TextMeshProUGUI buttonText)
                {
                    buttonText.text = choice.ChoiceText;

                    if (choice.ChoiceText == "Yes") 
                    {
                        if (ImpostorManager.instance != null && ImpostorManager.instance.HasUsedChip)
                        {
                            button.interactable = false; 
                        }
                    }
                }

                button.onClick.AddListener(() =>
                {
                    if (PauseMenu.IsGamePaused) return;

                    if (currentNode != null && currentNode.isHotelNode && choice.ChoiceText == "Accept")
                    {
                        if (HotelManager.instance != null) HotelManager.instance.AddGuest(currentNode.guestID);
                    }

                    if (!string.IsNullOrEmpty(choice.DestinationNodeID)) ShowNode(choice.DestinationNodeID);
                    else EndDialogue();
                });
            }
        }
    }

    private void EndDialogue()
    {
        dialoguePanel.SetActive(false);
        currentNode = null;
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        
        // Mantive a limpeza de botões aqui para a memória não estourar!
        foreach (Transform child in choiceButtonContainer) 
        {
            Destroy(child.gameObject);
        }
        
        if (SoundManager.instance != null)
        {
            SoundManager.instance.FadeOutSFX(0.2f);
        }
    }
    
    public void ResumeDialogueAfterEvent()
    {
        dialoguePanel.SetActive(true);
        if (currentNode != null && !string.IsNullOrEmpty(currentNode.NextNodeID)) ShowNode(currentNode.NextNodeID);
        else EndDialogue();
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
        
        if (SoundManager.instance != null)
        {
            SoundManager.instance.FadeOutSFX(0.2f);
        }
        
        typingCoroutine = null;
    }
}