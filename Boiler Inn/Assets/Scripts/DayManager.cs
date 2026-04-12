using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class DayManager : MonoBehaviour
{
    public static DayManager instance;

    [Header("Game State")]
    public int currentDay = 1;

    [Header("Impostor State")]
    public CharacterProfile chippedCharacter = null;

    [Header("Daily Limits")]
    public List<CharacterProfile> charactersPaidToday = new List<CharacterProfile>();

    [Header("Story Loop System")]
    public List<CharacterProfile> availableCharacters = new List<CharacterProfile>();
    private List<CharacterProfile> originalCharacters = new List<CharacterProfile>();
    public Dictionary<CharacterProfile, int> characterProgress = new Dictionary<CharacterProfile, int>();
    public CharacterProfile todayVisitor = null; 

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            
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
        characterProgress.Clear();
        foreach (CharacterProfile profile in availableCharacters)
        {
            characterProgress.Add(profile, 0); 
        }
    }
    
    public void StartGameFromMenu()
    {
        currentDay = 0; 
        todayVisitor = null; 
        chippedCharacter = null; 
        charactersPaidToday.Clear(); // LIMPA A LISTA NO NOVO JOGO
        availableCharacters = new List<CharacterProfile>(originalCharacters);
        InitializeProgress(); 
        StartNewDay();  
    }

    public void StartNewDay()
    {
        currentDay++;

        charactersPaidToday.Clear(); // LIMPA A LISTA SEMPRE QUE O DIA VIRA!

        if (currentDay == 1)
        {
            Debug.Log("Day 1 started! Loading Tutorial.");
            todayVisitor = null; 
            LoadDayScene();      
            return;              
        }

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
        CharacterProfile targetProfile = profile;
        if (targetProfile == null)
        {
            targetProfile = todayVisitor;
            Debug.LogWarning($"[DayManager] O nó de AdvanceStory veio sem perfil! Usando o paciente atual ({targetProfile.characterName}) por segurança.");
        }

        if (targetProfile != null && characterProgress.ContainsKey(targetProfile))
        {
            characterProgress[targetProfile]++; 
            Debug.Log($"[DayManager] História de {targetProfile.characterName} avançou para a Fase {characterProgress[targetProfile]}");

            if (characterProgress[targetProfile] >= 3)
            {
                availableCharacters.Remove(targetProfile);
                Debug.Log($"[DayManager] {targetProfile.characterName} completou todas as cirurgias e saiu da roleta!");
                
                if (availableCharacters.Count == 0)
                {
                    Debug.Log("Todos os personagens foram atendidos! O próximo dia será o final.");
                }
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