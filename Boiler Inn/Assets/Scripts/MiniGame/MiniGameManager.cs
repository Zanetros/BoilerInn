using UnityEngine;

public class MiniGameManager : MonoBehaviour
{
    public GameObject miniGame;
    public GameObject dialogueUI;
    public DialogueManager dialogueManager; // Arraste seu DialogueManager aqui
   
    public void StartMiniGame()
    {
        miniGame.SetActive(true);
        dialogueUI.SetActive(false);
    }

    public void FinishMiniGame()
    {
        // 1. Esconde o minigame e volta a UI de diálogo
        miniGame.SetActive(false);
        dialogueUI.SetActive(true);

        // 2. Comando para o diálogo continuar de onde parou
        if (dialogueManager != null)
        {
            dialogueManager.ResumeDialogueAfterEvent();
        }
    }
}