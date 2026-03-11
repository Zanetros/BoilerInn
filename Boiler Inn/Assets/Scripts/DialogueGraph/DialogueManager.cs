using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class DialogueManager : MonoBehaviour
{
    public RuntimeDialogueGraph runtimeGraph;
    
    [Header("UI Components")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI speakerNameText;
    public TextMeshProUGUI dialogueText;
    
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
        if (Mouse.current.leftButton.wasPressedThisFrame && currentNode != null)
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
    }

    private void EndDialogue()
    {
        dialoguePanel.SetActive(false);
        currentNode = null;
    }
}
