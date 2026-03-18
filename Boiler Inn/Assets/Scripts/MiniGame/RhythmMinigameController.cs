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
                        if (hitBar != null)
                        {
                            // ATUALIZADO: Agora enviamos hits, misses e totalNotes
                            MiniGameManager.instance.CalculateScore(hitBar.hits, hitBar.misses, hitBar.totalNotes);
                        }

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