using UnityEngine;
using UnityEngine.SceneManagement; // Obrigatório para trocar de cena

public class ImpostorManager : MonoBehaviour
{
    public static ImpostorManager instance;

    [Header("Chip Tracker")]
    public CharacterProfile chippedCharacter; 
    public bool HasUsedChip => chippedCharacter != null;

    [Header("UI Panels")]
    public GameObject gameOverPanel; // Arraste a sua tela de Game Over inteira (desativada) aqui

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    // Função que tenta colocar o chip em alguém
    public void PlantChip(CharacterProfile targetProfile)
    {
        if (HasUsedChip)
        {
            Debug.LogWarning("Você já usou o seu único chip espião!");
            return;
        }

        chippedCharacter = targetProfile;
        Debug.Log($"Chip plantado em: {targetProfile.characterName}");

        if (targetProfile.isImpostor)
        {
            Debug.Log("SUCESSO ABSOLUTO: O chip está no impostor real. Continue a investigação!");
        }
        else
        {
            Debug.LogWarning($"ERRO CRÍTICO: Você plantou o chip em {targetProfile.characterName}, que é inocente.");
            TriggerGameOver();
        }
    }

    // Função que ativa a tela de derrota
    private void TriggerGameOver()
    {
        if (gameOverPanel != null) 
        {
            gameOverPanel.SetActive(true);
        }
        else 
        {
            Debug.LogError("Painel de Game Over não foi referenciado no ImpostorManager!");
        }

        // Congela o tempo do jogo para parar animações, física e cliques acidentais no fundo
        Time.timeScale = 0f; 
    }
    
    public void ReturnToMenu()
    {
        // REGRA DE OURO: Sempre volte o tempo para o normal antes de carregar outra cena!
        // Se você esquecer isso, o Menu Principal vai carregar totalmente congelado.
        Time.timeScale = 1f; 
        
        // Carrega a cena de index 0 (certifique-se de que o Menu está no Build Settings)
        SceneManager.LoadScene(0); 
    }
    
    // Deixei essa de brinde caso você queira colocar um botão "Tentar Novamente" no futuro
    public void RestartCurrentLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}