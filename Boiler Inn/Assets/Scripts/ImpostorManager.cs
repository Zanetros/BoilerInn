using UnityEngine;
using UnityEngine.SceneManagement; // Obrigatório para trocar de cena

public class ImpostorManager : MonoBehaviour
{
    public static ImpostorManager instance;

    [Header("Chip Tracker")]
    public CharacterProfile chippedCharacter; 
    public bool HasUsedChip => chippedCharacter != null;
    
    public static bool isImpostorCaught = false; 

    [Header("UI Panels")]
    public GameObject gameOverPanel; 

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
        
        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            isImpostorCaught = false;
        }
    }

    public void PlantChip(CharacterProfile targetProfile)
    {
        if (HasUsedChip) return;

        chippedCharacter = targetProfile;
        Debug.Log($"Chip plantado em: {targetProfile.characterName}");

        if (targetProfile.isImpostor)
        {
            Debug.Log("SUCESSO ABSOLUTO: Impostor pego!");
            
            isImpostorCaught = true; 
        }
        else
        {
            Debug.LogWarning("ERRO CRÍTICO: Inocente grampeado.");
            isImpostorCaught = false;
            TriggerGameOver();
        }
    }
    
    private void TriggerGameOver()
    {
        if (gameOverPanel != null) 
        {
            gameOverPanel.SetActive(true);
            isImpostorCaught = false;
        }
        else 
        {
            Debug.LogError("Painel de Game Over não foi referenciado no ImpostorManager!");
        }
        
        Time.timeScale = 0f; 
    }
    
    public void ReturnToMenu()
    {
        Time.timeScale = 1f; 
        SceneManager.LoadScene(0); 
    }
}