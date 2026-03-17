using UnityEngine;
using Unity.GraphToolkit.Editor;
using System;

[Serializable]
public class StartNode : Node
{
    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        context.AddOutputPort("out").Build();
    }
}

[Serializable]
public class EndNode : Node
{
    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        context.AddInputPort("in").Build();
    }
}

[Serializable]
public class DialogueNode : Node
{
    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        context.AddInputPort("in").Build();
        context.AddOutputPort("out").Build();
        
        context.AddInputPort<string>("Speaker").Build();
        context.AddInputPort<Sprite>("Sprite").WithDisplayName("Character Sprite").WithConnectorUI(PortConnectorUI.Arrowhead).Build();
        context.AddInputPort<string>("Dialogue").Build();
    }
}

[Serializable]
public class EventNode : Node
{
    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        context.AddInputPort("in").Build();
        context.AddOutputPort("out").Build();
        
        context.AddInputPort<string>("EventID").Build(); 
        
        context.AddInputPort<int>("CyberCost").WithDisplayName("Cybercurrency Cost").Build();
        context.AddInputPort<int>("ImplantsCost").WithDisplayName("Implants Cost").Build();
        context.AddInputPort<int>("ChipsCost").WithDisplayName("Chips Cost").Build();
    }
}

[Serializable]
public class ChoiceNode : Node
{
    const string optionID = "portCount";

    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        context.AddInputPort("in").Build();

        context.AddInputPort<string>("Speaker").Build();
        context.AddInputPort<string>("Dialogue").Build();
        context.AddInputPort<Sprite>("Sprite").WithDisplayName("Character Sprite")
            .WithConnectorUI(PortConnectorUI.Arrowhead).Build();

        var option = GetNodeOptionByName(optionID);
        option.TryGetValue(out int portCount);
        for (int i = 0; i < portCount; i++)
        {
            context.AddInputPort<string>($"Choice Text {i}").Build();
            context.AddOutputPort($"Choice {i}").Build();
        }
    }
    protected override void OnDefineOptions(IOptionDefinitionContext context)
    {
        context.AddOption<int>(optionID).WithDefaultValue(2).Delayed();
    }
}

[Serializable]
public class HotelNode : Node
{
    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        // Portas de entrada do fluxo
        context.AddInputPort("in").Build();
        
        // Identificação do hóspede
        context.AddInputPort<string>("GuestID").WithDisplayName("Guest ID").Build();
        
        // Textos e imagens do diálogo de recepção
        context.AddInputPort<string>("Speaker").Build();
        context.AddInputPort<string>("Dialogue").Build();
        context.AddInputPort<Sprite>("Sprite").Build();

        // Portas de saída (Escolhas do jogador)
        context.AddOutputPort("Choice Accept").WithDisplayName("Accept Guest").Build();
        context.AddOutputPort("Choice Refuse").WithDisplayName("Refuse Guest").Build();
    }
}