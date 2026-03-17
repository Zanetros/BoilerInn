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
    public string SpeakerName;
    public string DialogueText;
    
    public List<ChoiceData> Choices = new List<ChoiceData>();
    
    public Sprite Sprite;
    
    public string NextNodeID;
    
    public string EventID;
    
    public int cyberCost;
    public int implantsCost;
    public int chipsCost;
    
    public bool isHotelNode;
    public string guestID;
}

[Serializable]
public class ChoiceData
{
    public string ChoiceText;
    public string DestinationNodeID;
}
