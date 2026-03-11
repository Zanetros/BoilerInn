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
    public string NextNodeID;
}
