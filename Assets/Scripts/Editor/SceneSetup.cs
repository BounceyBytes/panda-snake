using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;

public class SceneSetup
{
    [MenuItem("Tools/Setup Scenes In Build")]
    public static void SetupScenesInBuild()
    {
        // Get the scenes
        string menuScene = "Assets/Scenes/MenuScene.unity";
        string gameScene = "Assets/Scenes/GameScene.unity";

        // Add scenes to build settings
        EditorBuildSettings.scenes = new EditorBuildSettingsScene[]
        {
            new EditorBuildSettingsScene(menuScene, true),
            new EditorBuildSettingsScene(gameScene, true)
        };

        Debug.Log("Scenes have been set up in build settings!");
    }
} 