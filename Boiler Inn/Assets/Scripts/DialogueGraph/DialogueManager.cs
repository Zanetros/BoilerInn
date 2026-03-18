using System;
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
    public Image portrait;
    public Image characterSprite;
    
    [Header("Choice Button UI")]
    public Button choiceButtonPrefab;
    public Transform choiceButtonContainer;
    
    [Header("Text Settings")]
    [SerializeField] private float typingSpeed = 0.05f;
    
    private Coroutine typingCoroutine; 
    private WaitForSeconds typingDelay; // CACHE: Evita criar lixo na memória toda letra
    
    private Dictionary<string, RunTimeDialogueNode> nodeLookup = new Dictionary<string, RunTimeDialogueNode>();
    public RunTimeDialogueNode currentNode { get; private set; }

    private void Awake()
    {
        // Cacheia a espera para performance extrema
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

        if (!string.IsNullOrEmpty(currentNode.EventID))
        {
            dialoguePanel.SetActive(false);
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);

            if (MiniGameManager.instance != null) MiniGameManager.instance.TriggerMinigame(currentNode.EventID);
            else Debug.LogWarning("MiniGameManager instance not found!");
            
            return; 
        }

        dialoguePanel.SetActive(true);
        speakerNameText.SetText(currentNode.SpeakerName);
    
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeText(currentNode.DialogueText));

        if (currentNode.Sprite != null)
        {
            portrait.sprite = currentNode.Sprite;
            characterSprite.sprite = currentNode.Sprite;
            characterSprite.SetNativeSize();
        }
        
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
                    buttonText.text = choice.ChoiceText;

                    if (currentNode.isHotelNode && choice.ChoiceText == "Accept" && !HotelManager.instance.HasAvailableRoom())
                    {
                        button.interactable = false;
                        buttonText.text += " (Hotel Full)";
                    }
                }

                button.onClick.AddListener(() =>
                {
                    if (currentNode.isHotelNode && choice.ChoiceText == "Accept")
                    {
                        HotelManager.instance.AddGuest(currentNode.guestID);
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
    }
    
    public void ResumeDialogueAfterEvent()
    {
        dialoguePanel.SetActive(true);
        if (currentNode != null && !string.IsNullOrEmpty(currentNode.NextNodeID)) ShowNode(currentNode.NextNodeID);
        else EndDialogue();
    }
    
    private IEnumerator TypeText(string text)
    {
        // OTIMIZAÇÃO MAXIMA: Usa a função nativa do TMPro para evitar alocação de Strings
        dialogueText.text = text;
        dialogueText.maxVisibleCharacters = 0;

        int totalCharacters = text.Length;
        
        for (int i = 0; i <= totalCharacters; i++)
        {
            dialogueText.maxVisibleCharacters = i;
            yield return typingDelay; // Usa a espera cacheada
        }
        
        typingCoroutine = null;
    }
}