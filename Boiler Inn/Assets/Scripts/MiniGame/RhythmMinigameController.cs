using UnityEngine;

public class RhythmMinigameController : MonoBehaviour
{
    [Header("Rhythm Scripts")]
    public ChipsSpawner spawner;
    public HitBar hitBar;

    // OTIMIZAÇÃO: Define um intervalo para checar a cena (0.5 segundos)
    private float checkInterval = 0.5f; 
    private float timer;

    private void OnEnable()
    {
        this.enabled = true; 
        timer = checkInterval; // Reseta o timer toda vez que o jogo iniciar
    }

    void Update()
    {
        // Passo 1: Só gasta energia processando SE o spawner já terminou de criar as notas
        if (spawner != null && spawner.IsFinished)
        {
            timer -= Time.deltaTime;

            // Passo 2: Em vez de checar todo frame, checa apenas quando o timer zera
            if (timer <= 0f)
            {
                // Agora o FindFirstObjectByType roda apenas 2 vezes por segundo, eliminando o lag!
                if (Object.FindFirstObjectByType<NoteData>() == null)
                {
                    // Calcula o score
                    if (hitBar != null) hitBar.CalculateScore();

                    // Avisa o Manager Principal para mostrar a tela de Score
                    if (MiniGameManager.instance != null)
                    {
                        MiniGameManager.instance.FinalScore(); 
                    }

                    // Desativa este script para finalizar
                    this.enabled = false;
                }
                else
                {
                    // Se ainda existem notas caindo, reseta o timer para checar novamente em 0.5s
                    timer = checkInterval;
                }
            }
        }
    }
}