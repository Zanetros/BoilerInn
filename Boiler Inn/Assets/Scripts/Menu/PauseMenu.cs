using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    [Header("Input Action")]
    public InputActionReference pauseAction; // Arraste sua ação de "ESC" ou "Start" aqui

    [Header("Painéis de UI")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameObject optionsMenuPanel;

    public static bool IsGamePaused { get; private set; }

    private void Awake()
    {
        IsGamePaused = false; 
    }

    private void OnEnable()
    {
        // Subscreve ao evento de clique da tecla de pausa
        pauseAction.action.started += OnPauseTriggered;
    }

    private void OnDisable()
    {
        // Desinscreve para evitar erros de memória
        pauseAction.action.started -= OnPauseTriggered;
    }

    // Função chamada pelo Input System
    private void OnPauseTriggered(InputAction.CallbackContext ctx)
    {
        TogglePause();
    }

    public void TogglePause()
    {
        if (IsGamePaused)
            ResumeGame();
        else
            PauseGame();
    }

    public void PauseGame()
    {
        pauseMenuPanel.SetActive(true);
        optionsMenuPanel.SetActive(false);
        Time.timeScale = 0f;
        IsGamePaused = true;
        
        // PAUSA TODOS OS SONS DA CENA
        AudioListener.pause = true; 
    }
    
    public void ResumeGame()
    {
        pauseMenuPanel.SetActive(false);
        optionsMenuPanel.SetActive(false);
        Time.timeScale = 1f;
        IsGamePaused = false;
        
        // RETOMA TODOS OS SONS
        AudioListener.pause = false;
    }

    // Transição entre Menu de Pausa e Opções
    public void OpenOptions()
    {
        pauseMenuPanel.SetActive(false);
        optionsMenuPanel.SetActive(true);
    }

    public void CloseOptions()
    {
        optionsMenuPanel.SetActive(false);
        pauseMenuPanel.SetActive(true);
    }

    public void LoadMainMenu(int sceneName)
    {
        Time.timeScale = 1f;
        IsGamePaused = false;
        
        // 1. Tira o pause do ouvinte para o Menu Principal não começar mudo!
        AudioListener.pause = false;
        
        // 2. Manda o SoundManager cortar o efeito sonoro atual IMEDIATAMENTE (0 segundos)
        if (SoundManager.instance != null)
        {
            SoundManager.instance.FadeOutSFX(0f); 
        }

        SceneManager.LoadScene(sceneName);
    }
}