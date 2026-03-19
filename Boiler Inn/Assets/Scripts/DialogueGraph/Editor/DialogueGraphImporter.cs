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
            else if (iNode is SpyNode spyNode)
            {
                ProcessSpyNode(spyNode, runtimeNode, nodeIDMap);
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
            // 1. Descobre qual é o número desta escolha (ex: tira "Choice " de "Choice 0" = "0")
            string index = outputPort.name.Replace("Choice ", "");
            
            // 2. Lê o texto que o Game Designer digitou no campo "Choice Text X" correspondente
            string customChoiceText = GetPortValue<string>(node.GetInputPortByName($"Choice Text {index}"));

            // Se o campo estiver vazio (o designer esqueceu de preencher), usa um texto padrão
            if (string.IsNullOrEmpty(customChoiceText)) 
            {
                customChoiceText = $"Opção {index}"; 
            }
        
            var choiceData = new ChoiceData
            {
                ChoiceText = customChoiceText, // <-- Agora usa o texto customizado!
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
    
        // Lê os dados
        runtimeNode.guestID = GetPortValue<string>(node.GetInputPortByName("GuestID"));
        runtimeNode.SpeakerName = GetPortValue<string>(node.GetInputPortByName("Speaker"));
        runtimeNode.DialogueText = GetPortValue<string>(node.GetInputPortByName("Dialogue"));
        runtimeNode.Sprite = GetPortValue<Sprite>(node.GetInputPortByName("Sprite"));

        // Agora o nó de Hotel age como um DialogueNode normal, com apenas um caminho a seguir
        var nextNodePort = node.GetOutputPortByName("out")?.firstConnectedPort;
        if (nextNodePort != null)
        {
            runtimeNode.NextNodeID = nodeIDMap[nextNodePort.GetNode()];
        }
    }
    
    private void ProcessSpyNode(SpyNode node, RunTimeDialogueNode runtimeNode, Dictionary<INode, string> nodeIDMap)
    {
        runtimeNode.isSpyNode = true;
        runtimeNode.isImpostor = GetPortValue<bool>(node.GetInputPortByName("Impostor"));

        runtimeNode.SpeakerName = GetPortValue<string>(node.GetInputPortByName("Speaker"));
        runtimeNode.DialogueText = GetPortValue<string>(node.GetInputPortByName("Dialogue"));
        runtimeNode.Sprite = GetPortValue<Sprite>(node.GetInputPortByName("Sprite"));
    
        var choiceOutputPorts = node.GetOutputPorts().Where(p => p.name.StartsWith("Choice "));
    
        foreach (var outputPort in choiceOutputPorts)
        {
            // Faz exatamente a mesma lógica para ler o texto customizado do SpyNode
            string index = outputPort.name.Replace("Choice ", "");
            string customChoiceText = GetPortValue<string>(node.GetInputPortByName($"Choice Text {index}"));

            if (string.IsNullOrEmpty(customChoiceText)) 
            {
                customChoiceText = $"Opção {index}"; 
            }
        
            var choiceData = new ChoiceData
            {
                ChoiceText = customChoiceText, // <-- Agora usa o texto customizado!
                DestinationNodeID = outputPort.firstConnectedPort != null
                    ? nodeIDMap[outputPort.firstConnectedPort.GetNode()]
                    : null
            };
        
            runtimeNode.Choices.Add(choiceData);
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