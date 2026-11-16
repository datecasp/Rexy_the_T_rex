using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using TrexSprite;

[InitializeOnLoad]
public static class TrexSpriteInstaller
{
    private const string Root = "Assets/TrexSprite";
    private static readonly string[] Moves = { "Idle", "Walk", "Run", "Jump", "Attack" };

    static TrexSpriteInstaller()
    {
        EditorApplication.delayCall += TryAutoInstall;
    }

    private static void TryAutoInstall()
    {
        if (!AssetDatabase.IsValidFolder(Root)) return;
        if (AssetDatabase.LoadAssetAtPath<AnimatorController>(Root + "/Animator/Trex.controller") != null) return;
        string[] sprites = AssetDatabase.FindAssets("t:Sprite", new[] { Root + "/Frames" });
        if (sprites.Length >= 50) CreateAll();
    }

    [MenuItem("Tools/Trex Sprite/Create or Rebuild Unity Assets")]
    public static void CreateAll()
    {
        EnsureFolder(Root, "Animations");
        EnsureFolder(Root, "Animator");
        EnsureFolder(Root, "Prefabs");

        var clips = new System.Collections.Generic.Dictionary<string, AnimationClip>();
        foreach (string move in Moves)
        {
            string folder = Root + "/Frames/" + move;
            string[] guids = AssetDatabase.FindAssets("t:Sprite", new[] { folder });
            var frames = guids.Select(g => AssetDatabase.GUIDToAssetPath(g))
                .OrderBy(p => p, StringComparer.Ordinal)
                .Select(p => AssetDatabase.LoadAssetAtPath<Sprite>(p))
                .Where(s => s != null).ToArray();

            if (frames.Length == 0)
            {
                Debug.LogError("TrexSprite: no sprites found in " + folder);
                return;
            }

            int fps = move == "Idle" ? 6 : move == "Walk" ? 10 : move == "Run" ? 14 : move == "Jump" ? 8 : 12;
            var clip = new AnimationClip { frameRate = fps };
            clip.name = move;
            var keys = new ObjectReferenceKeyframe[frames.Length + 1];
            for (int i = 0; i < frames.Length; i++)
                keys[i] = new ObjectReferenceKeyframe { time = i / (float)fps, value = frames[i] };
            keys[frames.Length] = new ObjectReferenceKeyframe {
                time = frames.Length / (float)fps, value = frames[frames.Length - 1] };

            var binding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
            AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = move == "Idle" || move == "Walk" || move == "Run";
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            string path = Root + "/Animations/" + move + ".anim";
            AssetDatabase.DeleteAsset(path);
            AssetDatabase.CreateAsset(clip, path);
            clips[move] = clip;
        }

        string controllerPath = Root + "/Animator/Trex.controller";
        AssetDatabase.DeleteAsset(controllerPath);
        var controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
        controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
        controller.AddParameter("Running", AnimatorControllerParameterType.Bool);
        controller.AddParameter("Jump", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Attack", AnimatorControllerParameterType.Trigger);

        var sm = controller.layers[0].stateMachine;
        sm.states = new ChildAnimatorState[0];
        var idle = sm.AddState("Idle");
        var walk = sm.AddState("Walk");
        var run = sm.AddState("Run");
        var jump = sm.AddState("Jump");
        var attack = sm.AddState("Attack");
        idle.motion = clips["Idle"];
        walk.motion = clips["Walk"];
        run.motion = clips["Run"];
        jump.motion = clips["Jump"];
        attack.motion = clips["Attack"];
        sm.defaultState = idle;

        AddConditionTransition(idle, walk, "Speed", AnimatorConditionMode.Greater, 0.1f);
        AddConditionTransition(walk, idle, "Speed", AnimatorConditionMode.Less, 0.1f);
        AddConditionTransition(walk, run, "Running", AnimatorConditionMode.If, 0f);
        AddConditionTransition(run, walk, "Running", AnimatorConditionMode.IfNot, 0f);
        AddAnyTrigger(sm, jump, "Jump");
        AddAnyTrigger(sm, attack, "Attack");
        AddExitTransition(jump, idle);
        AddExitTransition(attack, idle);

        AssetDatabase.SaveAssets();
        CreatePrefab(clips["Idle"], controller);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("TrexSprite: animations, Animator Controller and prefab created.");
    }

    private static void AddConditionTransition(AnimatorState from, AnimatorState to, string parameter,
        AnimatorConditionMode mode, float threshold)
    {
        var t = from.AddTransition(to);
        t.hasExitTime = false;
        t.duration = 0.08f;
        t.AddCondition(mode, threshold, parameter);
    }

    private static void AddAnyTrigger(AnimatorStateMachine sm, AnimatorState to, string parameter)
    {
        var t = sm.AddAnyStateTransition(to);
        t.hasExitTime = false;
        t.duration = 0.05f;
        t.canTransitionToSelf = false;
        t.AddCondition(AnimatorConditionMode.If, 0f, parameter);
    }

    private static void AddExitTransition(AnimatorState from, AnimatorState to)
    {
        var t = from.AddTransition(to);
        t.hasExitTime = true;
        t.exitTime = 0.95f;
        t.duration = 0.08f;
    }

    private static void CreatePrefab(AnimationClip idleClip, AnimatorController controller)
    {
        var go = new GameObject("Trex");
        var sr = go.AddComponent<SpriteRenderer>();
        string firstFramePath = Root + "/Frames/Idle/Idle_01.png";
        sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(firstFramePath);
        sr.sortingOrder = 0;
        var animator = go.AddComponent<Animator>();
        animator.runtimeAnimatorController = controller;
        animator.applyRootMotion = false;
        go.AddComponent<TrexAnimationDriver>();
        string prefabPath = Root + "/Prefabs/Trex.prefab";
        PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
        UnityEngine.Object.DestroyImmediate(go);
    }

    private static void EnsureFolder(string parent, string child)
    {
        string path = parent + "/" + child;
        if (!AssetDatabase.IsValidFolder(path)) AssetDatabase.CreateFolder(parent, child);
    }
}
