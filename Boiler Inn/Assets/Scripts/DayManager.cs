using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class DayManager : MonoBehaviour
{
    public static DayManager instance;

    [Header("Game State")]
    public int currentDay = 1;

    [Header("Story Loop System")]
    // A urna atual (que vai esvaziando durante o jogo)
    public List<CharacterProfile> availableCharacters = new List<CharacterProfile>();
    
    // O backup da urna (intacto, para quando começar um Novo Jogo)
    private List<CharacterProfile> originalCharacters = new List<CharacterProfile>();
    
    // O dicionário usa o próprio Perfil como chave para achar o estágio (0, 1, 2)
    public Dictionary<CharacterProfile, int> characterProgress = new Dictionary<CharacterProfile, int>();

    // A variável que guarda quem é o paciente do dia
    public CharacterProfile todayVisitor = null; 

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            
            // FAZ O BACKUP: Copia os personagens do Inspector antes de qualquer um ser apagado!
            originalCharacters = new List<CharacterProfile>(availableCharacters);
            
            InitializeProgress();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void InitializeProgress()
    {
        // Limpa o dicionário antes de preencher para evitar duplicações
        characterProgress.Clear();
        foreach (CharacterProfile profile in availableCharacters)
        {
            characterProgress.Add(profile, 0); 
        }
    }
    
    // ==========================================
    // CHAMADO PELO BOTÃO "PLAY" NO MENU PRINCIPAL
    // ==========================================
    public void StartGameFromMenu()
    {
        // 1. Zera as variáveis globais
        currentDay = 0; 
        todayVisitor = null; 

        // 2. Restaura a urna de sorteio puxando do nosso backup intacto
        availableCharacters = new List<CharacterProfile>(originalCharacters);
        
        // 3. Queima os prontuários antigos e cria novos, todos na Fase 0
        InitializeProgress(); // Reaproveitamos a função aqui para ficar limpo!

        // 4. Agora sim, começa um jogo 100% limpo!
        StartNewDay();  
    }

    public void StartNewDay()
    {
        currentDay++;

        // --- DIA 1: TUTORIAL ---
        if (currentDay == 1)
        {
            Debug.Log("Day 1 started! Loading Tutorial.");
            todayVisitor = null; // Não sorteia ninguém!
            LoadDayScene();      
            return;              
        }

        // --- DIA 2 EM DIANTE: A ROLETA FUNCIONA NORMALMENTE ---
        if (availableCharacters.Count == 0)
        {
            TriggerFinalScene();
            return;
        }

        int randomIndex = Random.Range(0, availableCharacters.Count);
        todayVisitor = availableCharacters[randomIndex];

        Debug.Log($"Day {currentDay} started! Visitor: {todayVisitor.characterName} (Stage {characterProgress[todayVisitor]})");

        LoadDayScene(); 
    }

    public void AdvanceCharacterStory(CharacterProfile profile)
    {
        if (profile != null && characterProgress.ContainsKey(profile))
        {
            characterProgress[profile]++; 

            if (characterProgress[profile] >= 3)
            {
                availableCharacters.Remove(profile);
                Debug.Log($"{profile.characterName} has completed all surgeries and is out of the pool.");
            }
        }
    }

    public void TriggerFinalScene()
    {
        SceneManager.LoadScene("FinalScene");
    }

    public void GoToCity()
    {
        SceneManager.LoadScene("City");
    }

    public void EndCityExploration()
    {
        StartNewDay(); 
    }

    private void LoadDayScene()
    {
        SceneManager.LoadScene("Clinic"); 
    }
}