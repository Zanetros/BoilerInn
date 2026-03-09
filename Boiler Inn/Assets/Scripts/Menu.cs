using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
  [SerializeField] private GameObject creditsUI;
  [SerializeReference] private GameObject menuUI;
  [SerializeField] private GameObject optionsUI;

  public void PlayGame(int sceneIndex)
  {
    SceneManager.LoadScene(sceneIndex);
  }
  
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
