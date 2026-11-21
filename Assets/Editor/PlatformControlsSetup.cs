using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class PlatformControlsSetup
{
    [MenuItem("Tools/Rexy 2D/Configurar controles por plataforma")]
    public static void Configure()
    {
        Scene scene = EditorSceneManager.OpenScene("Assets/Scenes/Level1_Bootstrap.unity");
        TrexController player = Object.FindFirstObjectByType<TrexController>();

        Canvas[] canvases = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < canvases.Length; i++)
        {
            if (canvases[i].gameObject.name == "Canvas")
            {
                if (canvases[i].GetComponent<PlatformControls>() == null)
                    canvases[i].gameObject.AddComponent<PlatformControls>();
            }
        }

        ConfigureButton("btn_left", TouchButton.ButtonType.Left, player);
        ConfigureButton("btn_right", TouchButton.ButtonType.Right, player);
        ConfigureButton("btn_jump", TouchButton.ButtonType.Jump, player);
        ConfigureButton("btn_run", TouchButton.ButtonType.Run, player);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Rexy 2D", "Controles Android y PC configurados.", "Aceptar");
    }

    private static void ConfigureButton(string buttonName, TouchButton.ButtonType buttonType, TrexController player)
    {
        GameObject buttonObject = GameObject.Find(buttonName);
        if (buttonObject == null)
        {
            Debug.LogWarning("No se encontró el botón " + buttonName);
            return;
        }

        TouchButton button = buttonObject.GetComponent<TouchButton>();
        if (button == null)
            button = buttonObject.AddComponent<TouchButton>();

        SerializedObject serializedButton = new SerializedObject(button);
        serializedButton.FindProperty("type").enumValueIndex = (int)buttonType;
        serializedButton.FindProperty("player").objectReferenceValue = player;
        serializedButton.ApplyModifiedProperties();
        EditorUtility.SetDirty(button);
    }
}
