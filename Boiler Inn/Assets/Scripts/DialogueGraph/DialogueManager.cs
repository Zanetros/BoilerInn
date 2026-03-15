using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Events;

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
    
    [System.Serializable]
    public struct DialogueEvent
    {
        public string eventID;       // ID que você digita no nó do Grafo
        public UnityEvent onTrigger; // Função que será chamada na Unity
    }

    [Header("Event Settings")]
    public List<DialogueEvent> dialogueEvents = new List<DialogueEvent>();
    // --------------------------------
    
    [Header("Configurações de Texto")]
    [SerializeField] private float typingSpeed = 0.05f;
    private Coroutine typingCoroutine; 
    
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
        // Verifica clique para avançar apenas se não houver escolhas (botões) na tela
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

    private void ShowNode(string nodeID)
    {
        if (!nodeLookup.ContainsKey(nodeID))
        {
            EndDialogue();
            return;
        }
    
        currentNode = nodeLookup[nodeID];

        // --- NOVO: DISPARO DE EVENTO ---
        if (!string.IsNullOrEmpty(currentNode.EventID))
        {
            ExecuteDialogueEvent(currentNode.EventID);
        }

        // Lógica para "Nó de Evento Puro": 
        // Se o nó não tiver texto, ele executa o evento e pula para o próximo automaticamente
        if (string.IsNullOrEmpty(currentNode.DialogueText) && currentNode.Choices.Count == 0 && !string.IsNullOrEmpty(currentNode.NextNodeID))
        {
            ShowNode(currentNode.NextNodeID);
            return;
        }
        // -------------------------------
    
        dialoguePanel.SetActive(true);
        speakerNameText.SetText(currentNode.SpeakerName);
    
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        typingCoroutine = StartCoroutine(TypeText(currentNode.DialogueText));

        // Atualiza imagens (se houver sprite configurado)
        if (currentNode.Sprite != null)
        {
            portrait.sprite = currentNode.Sprite;
            characterSprite.sprite = currentNode.Sprite;
            characterSprite.SetNativeSize();
        }
        
        // Limpa botões de escolha anteriores
        foreach (Transform child in choiceButtonContainer)
        {
            Destroy(child.gameObject);
        }

        // Cria novos botões de escolha
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

    // --- NOVO: MÉTODO PARA EXECUTAR O EVENTO ---
    private void ExecuteDialogueEvent(string id)
    {
        foreach (var ev in dialogueEvents)
        {
            if (ev.eventID == id)
            {
                ev.onTrigger.Invoke();
                return;
            }
        }
        Debug.LogWarning($"Evento '{id}' chamado pelo nó, mas não configurado no DialogueManager.");
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
    
    public void ResumeDialogueAfterEvent()
    {
        // Verifica se o nó que disparou o evento tem um próximo nó conectado
        if (currentNode != null && !string.IsNullOrEmpty(currentNode.NextNodeID))
        {
            ShowNode(currentNode.NextNodeID);
        }
        else
        {
            EndDialogue();
        }
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