using UnityEngine;
using UnityEngine.SceneManagement;

public class DayManager : MonoBehaviour
{
    public static DayManager instance;

    [Header("Game State")]
    public int currentDay = 1;

    private void Awake()
    {
        // O padrão Singleton IMORTAL
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // O segredo! A Unity não destrói esse objeto ao trocar de cena.
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Chamado quando o jogador clica em "Novo Jogo" no Menu Principal
    public void StartGame()
    {
        currentDay = 1;
        LoadDayScene();
    }

    // Chamado quando o jogador termina as tarefas do Dia e vai para a Cidade
    public void GoToCity()
    {
        SceneManager.LoadScene("City");
    }

    // Chamado quando o jogador termina a exploração da Cidade (ex: vai dormir)
    public void EndCityExploration()
    {
        currentDay++; // Avança para o próximo dia!
        LoadDayScene();
    }

    // Carrega a cena do dia correspondente
    private void LoadDayScene()
    {
        // Se cada dia for uma cena separada (ex: "Day1", "Day2")
        string sceneName = "Day" + currentDay;
        
        // Verifica se a cena existe nas configurações da Unity (Build Settings)
        if (Application.CanStreamedLevelBeLoaded(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.Log("Fim de Jogo! Não há mais dias criados.");
            // SceneManager.LoadScene("Creditos");
        }
    }
}