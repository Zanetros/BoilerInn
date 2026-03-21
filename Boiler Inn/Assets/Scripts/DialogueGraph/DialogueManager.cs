using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class DialogueManager : MonoBehaviour
{
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

    // Variáveis de memória para as tags de diálogo
    private int lastReceivedCyber = 0;
    private int lastReceivedImplants = 0;
    private int lastReceivedChips = 0;

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

        if (SceneManager.GetActiveScene().name == "City")
        {
            EndDialogue();
        }
        else
        {
            if (!string.IsNullOrEmpty(runtimeGraph.EntryNodeID)) ShowNode(runtimeGraph.EntryNodeID);
            else EndDialogue();
        }
    }

    public void Update()
    {
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

        if (currentNode.isHotelNode && HotelManager.instance != null)
        {
            HotelManager.instance.AddGuest(currentNode.guestID);
        }

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

        // --- NÓ DE RECEBER RECOMPENSA (Invisível) ---
        if (currentNode.isReceiveNode)
        {
            // Salva os valores na memória para o tradutor de texto usar depois
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

            if (!string.IsNullOrEmpty(currentNode.NextNodeID)) ShowNode(currentNode.NextNodeID);
            else EndDialogue();
            
            return; 
        }
        
        if (currentNode.isGoToCityNode)
        {
            if (DayManager.instance != null)
            {
                // Chama o script imortal para trocar a cena
                DayManager.instance.GoToCity();
            }
            else
            {
                Debug.LogWarning("DayManager não encontrado na cena!");
            }

            // Desliga a interface de diálogo atual
            EndDialogue();
            return; 
        }

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
                characterSprite.gameObject.SetActive(true);
                characterSprite.sprite = currentNode.speakerProfile.characterSprite;
                characterSprite.SetNativeSize();
            }
            else
            {
                characterSprite.gameObject.SetActive(false);
            }
        }
        else
        {
            speakerNameText.SetText("???");
            characterSprite.gameObject.SetActive(false);
        }
    
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        
        // Passa o texto do editor pelo tradutor de Tags
        string processedText = FormatDialogueText(currentNode.DialogueText);
        
        if (SoundManager.instance != null && !string.IsNullOrEmpty(processedText))
        {
            int textLength = processedText.Length;
            AudioClip soundToPlay = null;

            if (textLength <= shortTextLimit) soundToPlay = shortTypingSound;
            else if (textLength <= mediumTextLimit) soundToPlay = mediumTypingSound;
            else soundToPlay = longTypingSound;

            if (soundToPlay != null) SoundManager.instance.PlaySFX(soundToPlay);
        }
        
        // Digita o texto formatado com os números reais
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
                    // Também formata os textos dos botões de escolha, caso você queira usar as tags lá!
                    buttonText.text = FormatDialogueText(choice.ChoiceText);

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
    
    public void SwitchDialogue(RuntimeDialogueGraph newGraph)
    {
        if (newGraph == null) return;

        runtimeGraph = newGraph;
        nodeLookup.Clear();

        foreach (var node in runtimeGraph.AllNodes)
        {
            nodeLookup[node.NodeID] = node;
        }

        if (!string.IsNullOrEmpty(runtimeGraph.EntryNodeID)) ShowNode(runtimeGraph.EntryNodeID);
        else EndDialogue();
    }
    
    private string FormatDialogueText(string rawText)
    {
        if (string.IsNullOrEmpty(rawText)) return rawText;

        string formattedText = rawText;
        
        formattedText = formattedText.Replace("{Cyber}", lastReceivedCyber.ToString());
        formattedText = formattedText.Replace("{Implants}", lastReceivedImplants.ToString());
        formattedText = formattedText.Replace("{Chips}", lastReceivedChips.ToString());

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
        
        if (SoundManager.instance != null)
        {
            SoundManager.instance.FadeOutSFX(0.2f);
        }
        
        typingCoroutine = null;
    }
}