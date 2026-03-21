using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class DayManager : MonoBehaviour
{
    public static DayManager instance;

    [Header("Game State")]
    public int currentDay = 1;

    [Header("Story Loop System")]
    // Agora arrastamos os Scriptable Objects direto no Inspector!
    public List<CharacterProfile> availableCharacters = new List<CharacterProfile>();
    
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
            InitializeProgress();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void InitializeProgress()
    {
        foreach (CharacterProfile profile in availableCharacters)
        {
            characterProgress.Add(profile, 0); 
        }
    }
    
    public void StartGameFromMenu()
    {
        currentDay = 0; // Zera o contador para o StartNewDay somar +1 e virar Dia 1
        StartNewDay();  // Sorteia o primeiro paciente e abre a clínica!
    }

    public void StartNewDay()
    {
        currentDay++;

        // --- DIA 1: TUTORIAL ---
        if (currentDay == 1)
        {
            Debug.Log("Day 1 started! Loading Tutorial.");
            todayVisitor = null; // Não sorteia ninguém!
            LoadDayScene();      // Chama a sua função com o nome correto!
            return;              // Interrompe a função aqui para não rodar a roleta
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

        LoadDayScene(); // Chama a sua função com o nome correto!
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