using UnityEngine;

[CreateAssetMenu(fileName = "New Character Profile", menuName = "Dialogue System/Character Profile")]
public class CharacterProfile : ScriptableObject
{
    public string characterName;
    public Sprite characterSprite;
    
    [Header("Spy Mechanics")]
    public bool isImpostor;
}