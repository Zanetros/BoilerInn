using System;
using System.Collections.Generic;
using UnityEngine;

public class ClinicManager : MonoBehaviour
{
    [Serializable]
    public struct CharacterStoryData
    {
        public CharacterProfile profile; // O Scriptable Object substitui as duas variáveis antigas!
        public RuntimeDialogueGraph stage0Graph; 
        public RuntimeDialogueGraph stage1Graph; 
        public RuntimeDialogueGraph stage2Graph; 
    }

    [Header("Story Database")]
    public List<CharacterStoryData> characterStories = new List<CharacterStoryData>();

    private void Start()
    {
        if (DayManager.instance == null) return;

        CharacterProfile visitor = DayManager.instance.todayVisitor;
        
        if (visitor == null) return;

        int currentStage = DayManager.instance.characterProgress[visitor];

        RuntimeDialogueGraph graphToLoad = FindGraphForVisitor(visitor, currentStage);

        if (graphToLoad != null && DialogueManager.instance != null)
        {
            DialogueManager.instance.SwitchDialogue(graphToLoad);
        }
    }

    private RuntimeDialogueGraph FindGraphForVisitor(CharacterProfile profileToFind, int stage)
    {
        foreach (var storyData in characterStories)
        {
            // Compara os dois Scriptable Objects diretamente. Muito mais seguro!
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