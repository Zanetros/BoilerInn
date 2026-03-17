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
    
    // A lista de eventos foi removida daqui!
    
    [Header("Text Settings")]
    [SerializeField] private float typingSpeed = 0.05f;
    private Coroutine typingCoroutine; 
    
    private Dictionary<string, RunTimeDialogueNode> nodeLookup = new Dictionary<string, RunTimeDialogueNode>();
    public RunTimeDialogueNode currentNode;

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
        // Avança o diálogo se não houver escolhas
        if (dialoguePanel.activeSelf && Mouse.current.leftButton.wasPressedThisFrame && currentNode != null && currentNode.Choices.Count == 0)
        {
            if (!string.IsNullOrEmpty(currentNode.NextNodeID)) ShowNode(currentNode.NextNodeID);
            else EndDialogue();
        }
    }

    private void ShowNode(string nodeID)
    {
        if (!nodeLookup.ContainsKey(nodeID))
        {
            EndDialogue();
            return;
        }
    
        currentNode = nodeLookup[nodeID];

        // --- DISPARO DE EVENTO PARA O MINIGAME MANAGER ---
        if (!string.IsNullOrEmpty(currentNode.EventID))
        {
            dialoguePanel.SetActive(false);
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);

            // Agora chamamos o MiniGameManager passando o ID do evento diretamente
            if (MiniGameManager.instance != null)
            {
                MiniGameManager.instance.TriggerMinigame(currentNode.EventID);
            }
            else
            {
                Debug.LogWarning("MiniGameManager instance not found!");
            }
            return; 
        }

        // --- EXIBIÇÃO DE DIÁLOGO NORMAL ---
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
                TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null) buttonText.text = choice.ChoiceText;

                // --- LÓGICA DO HOTEL ---
                if (currentNode.isHotelNode)
                {
                    if (choice.ChoiceText == "Accept")
                    {
                        if (!HotelManager.instance.HasAvailableRoom())
                        {
                            button.interactable = false;
                            if (buttonText != null) buttonText.text += " (Hotel Full)";
                        }
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
        dialogueText.SetText(""); 
        foreach (char letter in text.ToCharArray())
        {
            dialogueText.text += letter; 
            yield return new WaitForSeconds(typingSpeed); 
        }
        typingCoroutine = null;
    }
}