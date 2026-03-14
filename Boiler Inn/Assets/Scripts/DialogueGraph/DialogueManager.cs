using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

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
    
    private Dictionary<string, RunTimeDialogueNode> nodeLookup = new Dictionary<string, RunTimeDialogueNode>();
    private RunTimeDialogueNode currentNode;

    private void Start()
    {
        foreach (var node in runtimeGraph.AllNodes)
        {
            nodeLookup[node.NodeID] = node;
        }

        if (!string.IsNullOrEmpty(runtimeGraph.EntryNodeID))
        {
            ShowNode(runtimeGraph.EntryNodeID);
        }
        else
        {
            EndDialogue();
        }
    }

    public void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame && currentNode != null && currentNode.Choices.Count == 0)
        {
            if (!string.IsNullOrEmpty(currentNode.NextNodeID))
            {
                ShowNode(currentNode.NextNodeID);
            }
            else
            {
                EndDialogue();
            }
        }
    }

    // ReSharper disable Unity.PerformanceAnalysis
    private void ShowNode(string nodeID)
    {
        
        if (!nodeLookup.ContainsKey(nodeID))
        {
            EndDialogue();
            return;
        }
        
        currentNode = nodeLookup[nodeID];
        
        dialoguePanel.SetActive(true);
        speakerNameText.SetText(currentNode.SpeakerName);
        dialogueText.SetText(currentNode.DialogueText);
        portrait.sprite = currentNode.Sprite;
        characterSprite.sprite = currentNode.Sprite;
        characterSprite.SetNativeSize();
        

        foreach (Transform child in choiceButtonContainer)
        {
            Destroy(child.gameObject);
        }

        if (currentNode.Choices.Count > 0)
        {
            foreach (var choice in currentNode.Choices)
            {
                Button button = Instantiate(choiceButtonPrefab, choiceButtonContainer);

                TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = choice.ChoiceText;
                }

                if (button != null)
                {
                    button.onClick.AddListener(() =>
                    {
                        if (!string.IsNullOrEmpty(choice.DestinationNodeID))
                        {
                            ShowNode(choice.DestinationNodeID);
                        }
                        else
                        {
                            EndDialogue();
                        }
                    });

                }
            }
        }
    }

    private void EndDialogue()
    {
        dialoguePanel.SetActive(false);
        currentNode = null;

        foreach (Transform child in choiceButtonContainer)
        {
            Destroy(child.gameObject);
        }
    }
}
