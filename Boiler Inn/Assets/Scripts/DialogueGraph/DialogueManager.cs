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
        public string eventID;       
        public UnityEvent onTrigger; 
    }

    [Header("Event Settings")]
    public List<DialogueEvent> dialogueEvents = new List<DialogueEvent>();
    
    [Header("Configurações de Texto")]
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
        // Só permite avançar se o painel estiver ativo (evita avançar diálogos invisíveis durante o minigame)
        if (dialoguePanel.activeSelf && Mouse.current.leftButton.wasPressedThisFrame && currentNode != null && currentNode.Choices.Count == 0)
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

        // --- CORREÇÃO AQUI: DISPARO DE EVENTO ---
        if (!string.IsNullOrEmpty(currentNode.EventID))
        {
            // 1. Desliga o painel de diálogo imediatamente
            dialoguePanel.SetActive(false);

            // 2. Para o texto que estava sendo digitado
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);

            // 3. Executa o evento (Minigame)
            ExecuteDialogueEvent(currentNode.EventID);

            return; 
        }

        // Se chegou aqui, não é um evento, então mostramos o diálogo normalmente
        dialoguePanel.SetActive(true);
        speakerNameText.SetText(currentNode.SpeakerName);
    
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        typingCoroutine = StartCoroutine(TypeText(currentNode.DialogueText));

        if (currentNode.Sprite != null)
        {
            portrait.sprite = currentNode.Sprite;
            characterSprite.sprite = currentNode.Sprite;
            characterSprite.SetNativeSize();
        }
        
        RefreshChoices();
    }

    // Método auxiliar para limpar e criar botões
    private void RefreshChoices()
    {
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
                if (buttonText != null) buttonText.text = choice.ChoiceText;

                // --- LÓGICA DE GERENCIAMENTO DE HOTEL ---
                if (currentNode.isHotelNode)
                {
                    if (choice.ChoiceText == "Accept")
                    {
                        // Se não houver quartos, desativa o botão e avisa o jogador visualmente
                        if (!HotelManager.instance.HasAvailableRoom())
                        {
                            button.interactable = false;
                            if (buttonText != null) buttonText.text += " (Hotel Full)";
                        }
                    }
                }
                // ----------------------------------------

                button.onClick.AddListener(() =>
                {
                    // Se o jogador clicou em "Accept" e é um HotelNode, fazemos o check-in
                    if (currentNode.isHotelNode && choice.ChoiceText == "Accept")
                    {
                        HotelManager.instance.AddGuest(currentNode.guestID);
                    }

                    // Avança o diálogo normalmente
                    if (!string.IsNullOrEmpty(choice.DestinationNodeID)) 
                        ShowNode(choice.DestinationNodeID);
                    else 
                        EndDialogue();
                });
            }
        }
    }
    
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
        Debug.LogWarning($"Evento '{id}' não configurado.");
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