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
    public bool isReceiveNode;
    
    public bool isHotelNode;
    public string guestID;

    public bool isImpostorNode; 
    
    public bool isConditionNode;
    public string conditionID;
    public string NextNodeID_True; 
    public string NextNodeID_False;
}

[Serializable]
public class ChoiceData
{
    public string ChoiceText;
    public string DestinationNodeID;
}