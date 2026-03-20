using UnityEngine;
using UnityEditor.AssetImporters;
using Unity.GraphToolkit.Editor;
using System;
using System.Collections.Generic;
using System.Linq;

[ScriptedImporter(1, DialogueGraph.AssetExtension)]
public class DialogueGraphImporter : ScriptedImporter
{
    public override void OnImportAsset(AssetImportContext ctx)
    {
        DialogueGraph editorGraph = GraphDatabase.LoadGraphForImporter<DialogueGraph>(ctx.assetPath);
        RuntimeDialogueGraph runtimeGraph = ScriptableObject.CreateInstance<RuntimeDialogueGraph>();
        var nodeIDMap = new Dictionary<INode, string>();

        foreach (var node in editorGraph.GetNodes()) nodeIDMap[node] = Guid.NewGuid().ToString();

        var startNode = editorGraph.GetNodes().OfType<StartNode>().FirstOrDefault();
        if (startNode != null)
        {
            var entryPort = startNode.GetOutputPorts().FirstOrDefault()?.firstConnectedPort;
            if (entryPort != null) runtimeGraph.EntryNodeID = nodeIDMap[entryPort.GetNode()];
        }

        foreach (var iNode in editorGraph.GetNodes())
        {
            if (iNode is StartNode || iNode is EndNode) continue;
            
            var runtimeNode = new RunTimeDialogueNode {NodeID =  nodeIDMap[iNode]};
            
            if (iNode is DialogueNode dialogueNode) ProcessDialogueNode(dialogueNode, runtimeNode, nodeIDMap);
            else if (iNode is ChoiceNode choiceNode) ProcessChoiceNode(choiceNode, runtimeNode, nodeIDMap);
            else if (iNode is EventNode eventNode) ProcessEventNode(eventNode, runtimeNode, nodeIDMap);
            else if (iNode is HotelNode hotelNode) ProcessHotelNode(hotelNode, runtimeNode, nodeIDMap);
            else if (iNode is ImpostorNode impostorNode) ProcessImpostorNode(impostorNode, runtimeNode, nodeIDMap);
            else if (iNode is ConditionNode conditionNode) ProcessConditionNode(conditionNode, runtimeNode, nodeIDMap);
            
            runtimeGraph.AllNodes.Add(runtimeNode);
        }
        
        ctx.AddObjectToAsset("RuntimeData", runtimeGraph);
        ctx.SetMainObject(runtimeGraph);
    }

    private void ProcessDialogueNode(DialogueNode node, RunTimeDialogueNode runtimeNode, Dictionary<INode, string> nodeIDMap)
    {
        runtimeNode.speakerProfile = GetPortValue<CharacterProfile>(node.GetInputPortByName("Speaker Profile"));
        runtimeNode.DialogueText = GetPortValue<string>(node.GetInputPortByName("Dialogue"));
        
        var nextNodePort = node.GetOutputPortByName("out")?.firstConnectedPort;
        if (nextNodePort != null) runtimeNode.NextNodeID = nodeIDMap[nextNodePort.GetNode()];
    }
    
    private void ProcessHotelNode(HotelNode node, RunTimeDialogueNode runtimeNode, Dictionary<INode, string> nodeIDMap)
    {
        runtimeNode.isHotelNode = true;
        runtimeNode.guestID = GetPortValue<string>(node.GetInputPortByName("GuestID"));
        runtimeNode.speakerProfile = GetPortValue<CharacterProfile>(node.GetInputPortByName("Speaker Profile"));
        runtimeNode.DialogueText = GetPortValue<string>(node.GetInputPortByName("Dialogue"));

        var nextNodePort = node.GetOutputPortByName("out")?.firstConnectedPort;
        if (nextNodePort != null) runtimeNode.NextNodeID = nodeIDMap[nextNodePort.GetNode()];
    }

    private void ProcessEventNode(EventNode node, RunTimeDialogueNode runtimeNode, Dictionary<INode, string> nodeIDMap)
    {
        runtimeNode.EventID = GetPortValue<string>(node.GetInputPortByName("EventID"));
        runtimeNode.cyberCost = GetPortValue<int>(node.GetInputPortByName("CyberCost"));
        runtimeNode.implantsCost = GetPortValue<int>(node.GetInputPortByName("ImplantsCost"));
        runtimeNode.chipsCost = GetPortValue<int>(node.GetInputPortByName("ChipsCost"));
        
        var nextNodePort = node.GetOutputPortByName("out")?.firstConnectedPort;
        if (nextNodePort != null) runtimeNode.NextNodeID = nodeIDMap[nextNodePort.GetNode()];
    }

    private void ProcessChoiceNode(ChoiceNode node, RunTimeDialogueNode runtimeNode, Dictionary<INode, string> nodeIDMap)
    {
        runtimeNode.speakerProfile = GetPortValue<CharacterProfile>(node.GetInputPortByName("Speaker Profile"));
        runtimeNode.DialogueText = GetPortValue<string>(node.GetInputPortByName("Dialogue"));
    
        var choiceOutputPorts = node.GetOutputPorts().Where(p => p.name.StartsWith("Choice "));
        foreach (var outputPort in choiceOutputPorts)
        {
            string index = outputPort.name.Replace("Choice ", "");
            string customChoiceText = GetPortValue<string>(node.GetInputPortByName($"Choice Text {index}"));
            if (string.IsNullOrEmpty(customChoiceText)) customChoiceText = $"Opção {index}"; 
        
            runtimeNode.Choices.Add(new ChoiceData {
                ChoiceText = customChoiceText,
                DestinationNodeID = outputPort.firstConnectedPort != null ? nodeIDMap[outputPort.firstConnectedPort.GetNode()] : null
            });
        }
    }
    
    private void ProcessImpostorNode(ImpostorNode node, RunTimeDialogueNode runtimeNode, Dictionary<INode, string> nodeIDMap)
    {
        runtimeNode.isImpostorNode = true;
        
        // Puxa o arquivo de perfil conectado à porta
        runtimeNode.speakerProfile = GetPortValue<CharacterProfile>(node.GetInputPortByName("Speaker Profile"));
        
        // Verifica pra onde a linha continua
        var nextNodePort = node.GetOutputPortByName("out")?.firstConnectedPort;
        if (nextNodePort != null)
        {
            runtimeNode.NextNodeID = nodeIDMap[nextNodePort.GetNode()];
        }
    }
    
    private void ProcessConditionNode(ConditionNode node, RunTimeDialogueNode runtimeNode, Dictionary<INode, string> nodeIDMap)
    {
        runtimeNode.isConditionNode = true;
        
        // Lê o texto que o Game Designer digitou no campo Condition ID
        runtimeNode.conditionID = GetPortValue<string>(node.GetInputPortByName("Condition ID"));
        
        var truePort = node.GetOutputPortByName("True")?.firstConnectedPort;
        if (truePort != null) runtimeNode.NextNodeID_True = nodeIDMap[truePort.GetNode()];

        var falsePort = node.GetOutputPortByName("False")?.firstConnectedPort;
        if (falsePort != null) runtimeNode.NextNodeID_False = nodeIDMap[falsePort.GetNode()];
    }
    
    private T GetPortValue<T>(IPort port)
    {
        if (port == null) return default;
        if (port.isConnected && port.firstConnectedPort.GetNode() is IVariableNode variableNode)
        {
            variableNode.variable.TryGetDefaultValue(out T value);
            return value;
        }
        port.TryGetValue(out T fallbackValue);
        return fallbackValue;
    }
}