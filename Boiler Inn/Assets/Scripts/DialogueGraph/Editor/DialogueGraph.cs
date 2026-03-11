using System;
using UnityEngine;
using Unity.GraphToolkit.Editor;
using UnityEditor;

[Serializable]
[Graph(AssetExtension)]
public class DialogueGraph : Graph
{
    public const string AssetExtension = "Dialogue Graph";
    
    [MenuItem("Assets/Create/DialogueGraph", false)]
    private static void CreatAssetFile()
    {
        GraphDatabase.PromptInProjectBrowserToCreateNewAsset<DialogueGraph>("Dialogue Graph");
    }
}
