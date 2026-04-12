using UnityEngine;
using UnityEngine.SceneManagement; 

public class ImpostorManager : MonoBehaviour
{
    public static ImpostorManager instance;

    [Header("Chip Tracker")]
    // Consulta o DayManager para saber se o chip foi usado!
    public bool HasUsedChip => DayManager.instance != null && DayManager.instance.chippedCharacter != null;
    
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

        // Salva permanentemente no DayManager!
        if (DayManager.instance != null)
        {
            DayManager.instance.chippedCharacter = targetProfile;
        }

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
        }
    }
}