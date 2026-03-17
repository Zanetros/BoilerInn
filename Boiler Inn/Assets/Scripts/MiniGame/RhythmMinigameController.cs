using UnityEngine;

public class RhythmMinigameController : MonoBehaviour
{
    [Header("Rhythm Scripts")]
    public ChipsSpawner spawner;
    public HitBar hitBar;

    // Reseta o script toda vez que o GameObject do minigame é ativado
    private void OnEnable()
    {
        this.enabled = true; 
    }

    void Update()
    {
        // Se o spawner parou E não há mais notas voando na cena
        if (spawner != null && spawner.IsFinished && Object.FindFirstObjectByType<NoteData>() == null)
        {
            // Calcula o score
            if (hitBar != null) hitBar.CalculateScore();

            // Avisa o Manager Principal que este minigame específico acabou
            MiniGameManager.instance.FinishMiniGame();

            // Desativa este script para não chamar a finalização múltiplas vezes
            this.enabled = false;
        }
    }
}