using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using System.IO;
using System.Linq;

public class TrexAnimationSetup : AssetPostprocessor
{
    void OnPreprocessTexture()
    {
        if (!assetPath.Contains("Assets/Trex/Frames/")) return;
        var ti = (TextureImporter)assetImporter;
        ti.textureType = TextureImporterType.Sprite;
        ti.spriteImportMode = SpriteImportMode.Single;
        ti.spritePixelsPerUnit = 100;
        ti.alphaIsTransparency = true;
        ti.mipmapEnabled = false;
        ti.filterMode = FilterMode.Bilinear;
        ti.textureCompression = TextureImporterCompression.Uncompressed;
    }

    [MenuItem("Tools/Trex/Build Animator Controller")]
    public static void Build()
    {
        AssetDatabase.Refresh();
        const string generated = "Assets/Trex/Generated";
        if (!AssetDatabase.IsValidFolder(generated)) AssetDatabase.CreateFolder("Assets/Trex", "Generated");
        var controller = AnimatorController.CreateAnimatorControllerAtPath(generated + "/Trex.controller");
        foreach (var dir in AssetDatabase.GetSubFolders("Assets/Trex/Frames"))
        {
            string name = Path.GetFileName(dir);
            var sprites = AssetDatabase.FindAssets("t:Sprite", new[] { dir })
                .Select(g => AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(g)))
                .Where(s => s != null).OrderBy(s => s.name).ToArray();
            if (sprites.Length == 0) continue;
            float fps = (name == "Walk" || name == "Run" || name == "Jump") ? 10f : 8f;
            var clip = new AnimationClip { frameRate = fps };
            var binding = new EditorCurveBinding { type = typeof(SpriteRenderer), path = "", propertyName = "m_Sprite" };
            var keys = new ObjectReferenceKeyframe[sprites.Length];
            for (int i = 0; i < sprites.Length; i++) keys[i] = new ObjectReferenceKeyframe { time = i / fps, value = sprites[i] };
            AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = name == "Idle" || name == "Walk" || name == "Run";
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            AssetDatabase.CreateAsset(clip, generated + "/Trex_" + name + ".anim");
            var state = controller.layers[0].stateMachine.AddState(name);
            state.motion = clip;
            if (name == "Idle") controller.layers[0].stateMachine.defaultState = state;
        }
        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("T-Rex", "Animator clips and controller created in Assets/Trex/Generated. Add SpriteRenderer and Animator to the character GameObject.", "OK");
    }
}
