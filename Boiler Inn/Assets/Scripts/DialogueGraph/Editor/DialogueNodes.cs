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
public class DialogueNode : Node
{
    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        context.AddInputPort("in").Build();
        context.AddOutputPort("out").Build();
        
        context.AddInputPort<CharacterProfile>("Speaker Profile").Build();
        context.AddInputPort<string>("Dialogue").Build();
    }
}

[Serializable]
public class HotelNode : Node
{
    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        context.AddInputPort("in").Build();
        context.AddOutputPort("out").Build();
        
        context.AddInputPort<string>("GuestID").WithDisplayName("Guest ID").Build();
        context.AddInputPort<CharacterProfile>("Speaker Profile").Build();
        context.AddInputPort<string>("Dialogue").Build();
    }
}

[Serializable]
public class ChoiceNode : Node
{
    const string optionID = "portCount";

    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        context.AddInputPort("in").Build();
        
        context.AddInputPort<CharacterProfile>("Speaker Profile").Build();
        context.AddInputPort<string>("Dialogue").Build();

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
public class ImpostorNode : Node
{
    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        context.AddInputPort("in").Build();
        context.AddOutputPort("out").Build();
        
        // Pede apenas o perfil do personagem para fazer a checagem
        context.AddInputPort<CharacterProfile>("Speaker Profile").Build();
    }
}

[Serializable]
public class ConditionNode : Node
{
    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        context.AddInputPort("in").Build();
        
        // Pede um nome para saber qual condição checar (ex: "ImpostorCaught")
        context.AddInputPort<string>("Condition ID").Build(); 
        
        // As duas saídas possíveis
        context.AddOutputPort("True").WithDisplayName("If True").Build();
        context.AddOutputPort("False").WithDisplayName("If False").Build();
    }
}

[Serializable]
public class ReceiveNode : Node
{
    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        context.AddInputPort("in").Build();
        context.AddOutputPort("out").Build();
        
        context.AddInputPort<int>("CyberReward").WithDisplayName("Cybercurrency Reward").Build();
        context.AddInputPort<int>("ImplantsReward").WithDisplayName("Implants Reward").Build();
        context.AddInputPort<int>("ChipsReward").WithDisplayName("Chips Reward").Build();
    }
}

[Serializable]
public class GoToCityNode : Node
{
    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        context.AddInputPort("in").Build();
    }
}