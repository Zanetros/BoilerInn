using UnityEngine;

public class RhythmMinigameController : MonoBehaviour
{
    [Header("Rhythm Scripts")]
    public ChipsSpawner spawner;
    public HitBar hitBar;

    private float checkInterval = 0.5f; 
    private float timer;

    private void OnEnable()
    {
        this.enabled = true; 
        timer = checkInterval; 
    }

    void Update()
    {
        if (spawner != null && spawner.IsFinished)
        {
            timer -= Time.deltaTime;

            if (timer <= 0f)
            {
                if (Object.FindFirstObjectByType<NoteData>() == null)
                {
                    if (MiniGameManager.instance != null)
                    {
                        // 1. Manda os dados para o Manager calcular a pontuação
                        if (hitBar != null)
                        {
                            MiniGameManager.instance.CalculateScore(HitBar.hits, hitBar.totalNotes);
                        }

                        // 2. Chama a UI de score final
                        MiniGameManager.instance.FinalScore(); 
                    }

                    this.enabled = false;
                }
                else
                {
                    timer = checkInterval;
                }
            }
        }
    }
}