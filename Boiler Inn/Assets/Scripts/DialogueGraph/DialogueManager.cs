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
        // Avança o diálogo com o clique do mouse se não houver escolhas na tela
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
                // Guarda o estado anterior para saber se estamos gastando o chip AGORA
                bool hadUsedChipBefore = ImpostorManager.instance.HasUsedChip;
                
                // Manda o Manager agir (se o chip já foi usado, ele só dá um Debug.LogWarning)
                ImpostorManager.instance.PlantChip(currentNode.speakerProfile);

                // Se o NPC NÃO era o impostor e o jogador de fato gastou o chip agorinha...
                if (!currentNode.speakerProfile.isImpostor && !hadUsedChipBefore)
                {
                    // Encerramos o diálogo na hora para o Game Over brilhar.
                    EndDialogue();
                    return; 
                }
            }

            // O nó invisível pula instantaneamente para a próxima fala!
            if (!string.IsNullOrEmpty(currentNode.NextNodeID)) ShowNode(currentNode.NextNodeID);
            else EndDialogue();
            
            return; // Retorna para impedir que o script tente desenhar a UI abaixo
        }

        // --- NÓ DE EVENTO: Minigame ---
        if (!string.IsNullOrEmpty(currentNode.EventID))
        {
            dialoguePanel.SetActive(false);
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);

            if (MiniGameManager.instance != null) MiniGameManager.instance.TriggerMinigame(currentNode.EventID);
            return; 
        }

        // ==========================================
        // CONFIGURAÇÃO DA UI PARA DIÁLOGOS NORMAIS
        // ==========================================
        dialoguePanel.SetActive(true);
        
        // Puxa as informações do CharacterProfile
        if (currentNode.speakerProfile != null)
        {
            speakerNameText.SetText(currentNode.speakerProfile.characterName);
            if (currentNode.speakerProfile.characterSprite != null)
            {
                portrait.sprite = currentNode.speakerProfile.characterSprite;
                characterSprite.sprite = currentNode.speakerProfile.characterSprite;
                characterSprite.SetNativeSize();
            }
        }
        else
        {
            speakerNameText.SetText("???");
        }
    
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeText(currentNode.DialogueText));
        
        RefreshChoices();
    }

    private void RefreshChoices()
    {
        // Limpa os botões antigos
        foreach (Transform child in choiceButtonContainer) Destroy(child.gameObject);

        // Se houver escolhas (ChoiceNode), cria os botões
        if (currentNode.Choices.Count > 0)
        {
            foreach (var choice in currentNode.Choices)
            {
                Button button = Instantiate(choiceButtonPrefab, choiceButtonContainer);
                if (button.GetComponentInChildren<TextMeshProUGUI>() is TextMeshProUGUI buttonText)
                {
                    buttonText.text = choice.ChoiceText;
                }

                button.onClick.AddListener(() =>
                {
                    // Como as mecânicas complexas foram para Action Nodes, os botões
                    // voltam a fazer apenas o básico: nos levar para o próximo nó!
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
        dialogueText.text = text;
        dialogueText.maxVisibleCharacters = 0;
        int totalCharacters = text.Length;
        
        for (int i = 0; i <= totalCharacters; i++)
        {
            dialogueText.maxVisibleCharacters = i;
            yield return typingDelay; 
        }
        typingCoroutine = null;
    }
}