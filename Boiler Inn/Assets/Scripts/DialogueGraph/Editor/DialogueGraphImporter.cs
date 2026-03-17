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

        foreach (var node in editorGraph.GetNodes())
        {
            nodeIDMap[node] = Guid.NewGuid().ToString();
        }

        var startNode = editorGraph.GetNodes().OfType<StartNode>().FirstOrDefault();
        if (startNode != null)
        {
            var entryPort = startNode.GetOutputPorts().FirstOrDefault()?.firstConnectedPort;
            if (entryPort != null)
            {
                runtimeGraph.EntryNodeID = nodeIDMap[entryPort.GetNode()];
            }
        }

        foreach (var iNode in editorGraph.GetNodes())
        {
            if (iNode is StartNode || iNode is EndNode) continue;
            
            var runtimeNode = new RunTimeDialogueNode {NodeID =  nodeIDMap[iNode]};
            if (iNode is DialogueNode dialogueNode)
            {
                ProcessDialogueNode(dialogueNode, runtimeNode, nodeIDMap);
            }
            else if (iNode is ChoiceNode choiceNode)
            {
                ProcessChoiceNode(choiceNode, runtimeNode, nodeIDMap);
            }
            // Verifica se o nó atual é do tipo EventNode
            else if (iNode is EventNode eventNode)
            {
                // Chama o novo método para processar os dados do evento
                ProcessEventNode(eventNode, runtimeNode, nodeIDMap);
            }
            else if (iNode is HotelNode hotelNode)
            {
                ProcessHotelNode(hotelNode, runtimeNode, nodeIDMap);
            }
            
            runtimeGraph.AllNodes.Add(runtimeNode);
        }
        
        ctx.AddObjectToAsset("RuntimeData", runtimeGraph);
        ctx.SetMainObject(runtimeGraph);
    }

    private void ProcessDialogueNode(DialogueNode node, RunTimeDialogueNode runtimeNode, Dictionary<INode, string> nodeIDMap)
    {
        runtimeNode.SpeakerName = GetPortValue<string>(node.GetInputPortByName("Speaker"));
        runtimeNode.DialogueText = GetPortValue<string>(node.GetInputPortByName("Dialogue"));
        runtimeNode.Sprite = GetPortValue<Sprite>(node.GetInputPortByName("Sprite"));
        
        var nextNodePort = node.GetOutputPortByName("out")?.firstConnectedPort;
        if (nextNodePort != null)
        {
            runtimeNode.NextNodeID = nodeIDMap[nextNodePort.GetNode()];
        }
    }
    
    private void ProcessEventNode(EventNode node, RunTimeDialogueNode runtimeNode, Dictionary<INode, string> nodeIDMap)
    {
        runtimeNode.EventID = GetPortValue<string>(node.GetInputPortByName("EventID"));
        runtimeNode.cyberCost = GetPortValue<int>(node.GetInputPortByName("CyberCost"));
        runtimeNode.implantsCost = GetPortValue<int>(node.GetInputPortByName("ImplantsCost"));
        runtimeNode.chipsCost = GetPortValue<int>(node.GetInputPortByName("ChipsCost"));
        
        var nextNodePort = node.GetOutputPortByName("out")?.firstConnectedPort;
        if (nextNodePort != null)
        {
            runtimeNode.NextNodeID = nodeIDMap[nextNodePort.GetNode()];
        }
    }

    private void ProcessChoiceNode(ChoiceNode node, RunTimeDialogueNode runtimeNode, Dictionary<INode, string> nodeIDMap)
    {
        runtimeNode.SpeakerName = GetPortValue<string>(node.GetInputPortByName("Speaker"));
        runtimeNode.DialogueText = GetPortValue<string>(node.GetInputPortByName("Dialogue"));
        runtimeNode.Sprite = GetPortValue<Sprite>(node.GetInputPortByName("Sprite"));
    
        var choiceOutputPorts = node.GetOutputPorts().Where(p => p.name.StartsWith("Choice "));
    
        foreach (var outputPort in choiceOutputPorts)
        {
            string choiceText = outputPort.name; 
        
            var choiceData = new ChoiceData
            {
                ChoiceText = choiceText,
                DestinationNodeID = outputPort.firstConnectedPort != null
                    ? nodeIDMap[outputPort.firstConnectedPort.GetNode()]
                    : null
            };
        
            runtimeNode.Choices.Add(choiceData);
        }
    }
    
    private void ProcessHotelNode(HotelNode node, RunTimeDialogueNode runtimeNode, Dictionary<INode, string> nodeIDMap)
    {
        // Define que este nó tem comportamento especial de hotel
        runtimeNode.isHotelNode = true;
    
        // Lê os dados do hóspede e do diálogo
        runtimeNode.guestID = GetPortValue<string>(node.GetInputPortByName("GuestID"));
        runtimeNode.SpeakerName = GetPortValue<string>(node.GetInputPortByName("Speaker"));
        runtimeNode.DialogueText = GetPortValue<string>(node.GetInputPortByName("Dialogue"));
        runtimeNode.Sprite = GetPortValue<Sprite>(node.GetInputPortByName("Sprite"));

        // Mapeia a porta de "Aceitar"
        var acceptPort = node.GetOutputPortByName("Choice Accept")?.firstConnectedPort;
        if (acceptPort != null)
        {
            runtimeNode.Choices.Add(new ChoiceData { 
                ChoiceText = "Accept", 
                DestinationNodeID = nodeIDMap[acceptPort.GetNode()] 
            });
        }

        // Mapeia a porta de "Recusar"
        var refusePort = node.GetOutputPortByName("Choice Refuse")?.firstConnectedPort;
        if (refusePort != null)
        {
            runtimeNode.Choices.Add(new ChoiceData { 
                ChoiceText = "Refuse", 
                DestinationNodeID = nodeIDMap[refusePort.GetNode()] 
            });
        }
    }
    
    private T GetPortValue<T>(IPort port)
    {
        if (port == null) return default;

        if (port.isConnected)
        {
            if (port.firstConnectedPort.GetNode() is IVariableNode variableNode)
            {
                variableNode.variable.TryGetDefaultValue(out T value);
                return value;
            }
        }
        
        port.TryGetValue(out T fallbackValue);
        return fallbackValue;
    }
}