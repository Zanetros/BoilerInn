using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    [SerializeField] private GameObject creditsUI;
    [SerializeReference] private GameObject menuUI;
    [SerializeField] private GameObject optionsUI;

    public AudioClip menuMusic;

    void Start()
    {
        if (SoundManager.instance != null)
        {
            // Pede ao SoundManager para aplicar os volumes salvos
            SoundManager.instance.LoadVolume();
      
            // Toca a música do menu com segurança
            if (menuMusic != null)
            {
                SoundManager.instance.PlayMusic(menuMusic);
            }
        }
    }
  
    // ==============================================================
    // AQUI ESTÁ A MÁGICA: O novo PlayGame que avisa o Cérebro Imortal
    // ==============================================================
    public void PlayGame()
    {
        // Em vez de só carregar a cena às cegas, nós mandamos o DayManager 
        // limpar o histórico de pacientes, encher a urna de novo e começar o Dia 1.
        if (DayManager.instance != null)
        {
            DayManager.instance.StartGameFromMenu();
        }
        else
        {
            Debug.LogError("ERRO: DayManager não encontrado na cena do Menu!");
        }
    }
    // ==============================================================
  
    public void QuitGame()
    {
        Application.Quit();
    }
  
    public void CreditsMenu()
    {
        creditsUI.SetActive(true);
        menuUI.SetActive(false);
    }

    public void BackToMenu()
    {
        creditsUI.SetActive(false);
        menuUI.SetActive(true);
    }
  
    public void OptionsMenu()
    {
        optionsUI.SetActive(true);
        menuUI.SetActive(false);
    }

    public void BackToOptions()
    {
        optionsUI.SetActive(false);
        menuUI.SetActive(true);
    }
}