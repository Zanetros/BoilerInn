using UnityEngine;
using System;
using System.Collections.Generic;

public class RuntimeDialogueGraph : ScriptableObject
{
    public string EntryNodeID;
    public List<RunTimeDialogueNode> AllNodes = new List<RunTimeDialogueNode>();
}

[Serializable]
public class RunTimeDialogueNode
{
    public string NodeID;
    
    // NOVO: Referência direta ao perfil do personagem
    public CharacterProfile speakerProfile; 
    
    public string DialogueText;
    public List<ChoiceData> Choices = new List<ChoiceData>();
    public string NextNodeID;
    
    public string EventID;
    public int cyberCost;
    public int implantsCost;
    public int chipsCost;
    
    public bool isHotelNode;
    public string guestID;

    public bool isImpostorNode; 
}

[Serializable]
public class ChoiceData
{
    public string ChoiceText;
    public string DestinationNodeID;
}