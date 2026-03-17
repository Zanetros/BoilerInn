using UnityEngine;

public class MiniGameManager : MonoBehaviour
{
    public GameObject miniGameUI;
    public GameObject dialogueUI;
    public GameObject finalScoreUI;
    public DialogueManager dialogueManager;

    [Header("Scripts do Minigame")]
    public ChipsSpawner spawner;
    public HitBar hitBar;

    private bool isGameActive = false;

    public void StartMiniGame()
    {
        if (dialogueManager == null || dialogueManager.currentNode == null) return;

        // Obtém os dados de custo do nó de evento atual
        RunTimeDialogueNode nodeData = dialogueManager.currentNode;

        // Verifica se o jogador possui a quantidade estipulada no EventNode
        if (CheckRequirements(nodeData.cyberCost, nodeData.implantsCost, nodeData.chipsCost))
        {
            // Se tiver o suficiente, apenas inicia o jogo sem descontar nada
            miniGameUI.SetActive(true);
            dialogueUI.SetActive(false);
            isGameActive = true;
            
            Debug.Log("Requirements met. Starting minigame without deducting currency.");
        }
        else
        {
            // Caso não tenha o suficiente, o minigame não inicia
            Debug.LogWarning("Insufficient resources to start the minigame.");
            
            // Opcional: Você pode chamar o ResumeDialogue aqui para não travar o jogo
            // dialogueManager.ResumeDialogueAfterEvent();
        }
    }

    // Função interna para validar os saldos no CurrencyManager
    private bool CheckRequirements(int reqCyber, int reqImplants, int reqChips)
    {
        CurrencyManager wallet = CurrencyManager.instance;
        if (wallet == null) return false;

        // Compara o saldo atual com o valor exigido pelo nó
        bool hasCyber = wallet.cybercurrency >= reqCyber;
        bool hasImplants = wallet.implants >= reqImplants;
        bool hasChips = wallet.chips >= reqChips;

        // Retorna verdadeiro apenas se atender a todos os requisitos simultaneamente
        return hasCyber && hasImplants && hasChips;
    }

    void Update()
    {
        if (!isGameActive) return;

        // Se o spawner parou E não há mais nenhum objeto NoteData voando na cena
        if (spawner.IsFinished && Object.FindFirstObjectByType<NoteData>() == null)
        {
            FinalScore();
        }
    }

    public void FinalScore()
    {
        finalScoreUI.SetActive(true);
    }

    public void FinishMiniGame()
    {
        isGameActive = false;
        
        hitBar.CalculateScore();

        miniGameUI.SetActive(false);
        finalScoreUI.SetActive(false);
        dialogueUI.SetActive(true);

        // Avisa o DialogueManager para prosseguir
        if (dialogueManager != null)
        {
            dialogueManager.ResumeDialogueAfterEvent();
        }
    }
}