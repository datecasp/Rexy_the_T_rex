using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class TrexEnemyAssetSetup
{
    private const string Root = "Assets/TrexEnemies";
    private static readonly string[] Species = { "Velociraptor", "Stegosaurus", "Carnotaurus", "Dilophosaurus" };

    [MenuItem("Tools/Trex Enemies/Generate All Animations + Animators")]
    public static void GenerateAll()
    {
        try
        {
            EnsureFolder(Root + "/Animations"); EnsureFolder(Root + "/Animators");
            foreach (string species in Species)
            {
                AnimationClip idle = BuildClip(species, "Idle", 6f);
                AnimationClip walk = BuildClip(species, "Walk", 10f);
                SaveOrReplace(idle, $"{Root}/Animations/{species}/{species}_Idle.anim");
                SaveOrReplace(walk, $"{Root}/Animations/{species}/{species}_Walk.anim");
                BuildController(species, idle, walk);
            }
            AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Trex Enemies", "Listo: 8 clips (6 Idle + 10 Walk por especie) y 4 Animator Controllers.", "OK");
        }
        catch (Exception ex) { Debug.LogException(ex); EditorUtility.DisplayDialog("Trex Enemies - Error", ex.Message, "OK"); }
    }

    private static AnimationClip BuildClip(string species, string action, float fps)
    {
        string folder = $"{Root}/Frames/{species}/{action}";
        string[] paths = AssetDatabase.FindAssets("t:Texture2D", new[] { folder })
            .Select(AssetDatabase.GUIDToAssetPath).OrderBy(p => p, StringComparer.OrdinalIgnoreCase).ToArray();
        int expected = action == "Idle" ? 6 : 10;
        if (paths.Length != expected) throw new Exception($"{species}/{action}: se esperaban {expected} PNG y hay {paths.Length} en {folder}");
        var keys = new ObjectReferenceKeyframe[paths.Length];
        for (int i = 0; i < paths.Length; i++)
        {
            var importer = AssetImporter.GetAtPath(paths[i]) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                importer.filterMode = FilterMode.Bilinear;
                importer.textureCompression = TextureImporterCompression.Compressed;
                importer.SaveAndReimport();
            }
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(paths[i]);
            if (sprite == null) throw new Exception("No se pudo importar como Sprite: " + paths[i]);
            keys[i] = new ObjectReferenceKeyframe { time = i / fps, value = sprite };
        }
        var clip = new AnimationClip { frameRate = fps, name = $"{species}_{action}" };
        // SpriteRenderer sprite curve.
        var binding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
        AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);
        clip.wrapMode = WrapMode.Loop;
        return clip;
    }

    private static void BuildController(string species, AnimationClip idle, AnimationClip walk)
    {
        string path = $"{Root}/Animators/{species}.controller";
        if (AssetDatabase.LoadAssetAtPath<AnimatorController>(path) != null) AssetDatabase.DeleteAsset(path);
        AnimatorController c = AnimatorController.CreateAnimatorControllerAtPath(path);
        c.AddParameter("Speed", AnimatorControllerParameterType.Float);
        var sm = c.layers[0].stateMachine;
        var idleState = sm.AddState("Idle"); var walkState = sm.AddState("Walk");
        idleState.motion = idle; walkState.motion = walk; sm.defaultState = idleState;
        var a = idleState.AddTransition(walkState); a.hasExitTime = false; a.duration = .08f; a.AddCondition(AnimatorConditionMode.Greater, .05f, "Speed");
        var b = walkState.AddTransition(idleState); b.hasExitTime = false; b.duration = .08f; b.AddCondition(AnimatorConditionMode.Less, .05f, "Speed");
    }

    private static void SaveOrReplace(UnityEngine.Object asset, string path)
    {
        EnsureFolder(Path.GetDirectoryName(path).Replace('\\', '/'));
        if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path) != null) AssetDatabase.DeleteAsset(path);
        AssetDatabase.CreateAsset(asset, path);
    }
    private static void EnsureFolder(string path)
    {
        string[] parts = path.Split('/'); string current = parts[0];
        for (int i = 1; i < parts.Length; i++) { string next = current + "/" + parts[i]; if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(current, parts[i]); current = next; }
    }
}
