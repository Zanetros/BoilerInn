using System;
using System.Collections.Generic;
using UnityEngine;

public class ClinicManager : MonoBehaviour
{
    // A gaveta VIP para o arquivo do Tutorial
    [Header("Grafo Fixo do Tutorial")]
    public RuntimeDialogueGraph tutorialGraph;

    [Serializable]
    public struct CharacterStoryData
    {
        public CharacterProfile profile; 
        public RuntimeDialogueGraph stage0Graph; 
        public RuntimeDialogueGraph stage1Graph; 
        public RuntimeDialogueGraph stage2Graph; 
    }

    [Header("Base de Dados de Histórias")]
    public List<CharacterStoryData> characterStories = new List<CharacterStoryData>();

    private void Start()
    {
        if (DayManager.instance == null) return;

        // ============================================
        // 1. CHECAGEM DO TUTORIAL (DIA 1)
        // ============================================
        if (DayManager.instance.currentDay == 1)
        {
            if (tutorialGraph != null && DialogueManager.instance != null)
            {
                Debug.Log("Carregando o Diálogo do Tutorial.");
                DialogueManager.instance.SwitchDialogue(tutorialGraph);
            }
            else
            {
                Debug.LogError("Faltou arrastar o Grafo do Tutorial no ClinicManager!");
            }
            
            return; // IMPORTANTE: Encerra o Start() aqui para não procurar paciente!
        }

        // ============================================
        // 2. SISTEMA NORMAL DE ROLETA (DIA 2 EM DIANTE)
        // ============================================
        CharacterProfile visitor = DayManager.instance.todayVisitor;
        if (visitor == null) return;

        int currentStage = DayManager.instance.characterProgress[visitor];
        RuntimeDialogueGraph graphToLoad = FindGraphForVisitor(visitor, currentStage);

        if (graphToLoad != null && DialogueManager.instance != null)
        {
            DialogueManager.instance.SwitchDialogue(graphToLoad);
        }
        else
        {
            Debug.LogError($"Faltou colocar o Grafo da Fase {currentStage} para o personagem {visitor.characterName}!");
        }
    }

    private RuntimeDialogueGraph FindGraphForVisitor(CharacterProfile profileToFind, int stage)
    {
        foreach (var storyData in characterStories)
        {
            if (storyData.profile == profileToFind)
            {
                if (stage == 0) return storyData.stage0Graph;
                if (stage == 1) return storyData.stage1Graph;
                if (stage == 2) return storyData.stage2Graph;
            }
        }
        return null; 
    }
}