using UnityEngine;

public class MiniGameManager : MonoBehaviour
{
    public GameObject miniGameUI;
    public GameObject dialogueUI;
    public DialogueManager dialogueManager;

    [Header("Scripts do Minigame")]
    public ChipsSpawner spawner;
    public HitBar hitBar;

    private bool isGameActive = false;

    public void StartMiniGame()
    {
        miniGameUI.SetActive(true);
        dialogueUI.SetActive(false);
        isGameActive = true;
    }

    void Update()
    {
        if (!isGameActive) return;

        // Se o spawner parou E não há mais nenhum objeto NoteData voando na cena
        if (spawner.IsFinished && Object.FindFirstObjectByType<NoteData>() == null)
        {
            FinishMiniGame();
        }
    }

    public void FinishMiniGame()
    {
        isGameActive = false;
      
        // Calcula o score final antes de fechar
        hitBar.CalculateScore();

        miniGameUI.SetActive(false);
        dialogueUI.SetActive(true);

        // Avisa o DialogueManager para prosseguir
        if (dialogueManager != null)
        {
            dialogueManager.ResumeDialogueAfterEvent();
        }
    }
}