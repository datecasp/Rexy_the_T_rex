using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class CIBuild
{
    private static readonly Dictionary<BuildTarget, BuildTargetGroup> TargetGroups =
        new Dictionary<BuildTarget, BuildTargetGroup>
        {
            { BuildTarget.StandaloneWindows64, BuildTargetGroup.Standalone },
            { BuildTarget.Android, BuildTargetGroup.Android },
            { BuildTarget.WebGL, BuildTargetGroup.WebGL }
        };

    public static void Build()
    {
        string targetArgument = GetArgument("-ciBuildTarget");
        string outputArgument = GetArgument("-ciBuildPath");

        if (string.IsNullOrEmpty(targetArgument))
            throw new ArgumentException("Falta -ciBuildTarget");
        if (string.IsNullOrEmpty(outputArgument))
            throw new ArgumentException("Falta -ciBuildPath");

        BuildTarget target = (BuildTarget)Enum.Parse(typeof(BuildTarget), targetArgument);
        BuildTargetGroup targetGroup = TargetGroups[target];

        EditorUserBuildSettings.SwitchActiveBuildTarget(targetGroup, target);

        ConfigureSigning();

        List<string> scenes = new List<string>();
        EditorBuildSettingsScene[] buildScenes = EditorBuildSettings.scenes;
        for (int i = 0; i < buildScenes.Length; i++)
        {
            if (buildScenes[i].enabled)
                scenes.Add(buildScenes[i].path);
        }

        if (scenes.Count == 0)
            throw new InvalidOperationException("No hay escenas habilitadas en Build Settings");

        BuildOptions options = BuildOptions.None;
        string outputPath = outputArgument;

        if (target == BuildTarget.StandaloneWindows64)
        {
            outputPath = System.IO.Path.Combine(outputArgument, "RexyTheTRex.exe");
        }
        else if (target == BuildTarget.WebGL)
        {
            PlayerSettings.WebGL.decompressionFallback = true;
        }
        else if (target == BuildTarget.Android)
        {
            EditorUserBuildSettings.buildAppBundle = false;
            outputPath = System.IO.Path.Combine(outputArgument, "RexyTheTRex.apk");
        }

        BuildReport report = BuildPipeline.BuildPlayer(
            scenes.ToArray(),
            outputPath,
            target,
            options);

        if (report.summary.result != BuildResult.Succeeded)
            throw new InvalidOperationException(
                "Build fallido: " + report.summary.result + " - " + report.summary.totalErrors + " errores");

        Debug.Log("Build completado: " + outputPath);
    }

    private static void ConfigureSigning()
    {
        string keystore = Environment.GetEnvironmentVariable("ANDROID_KEYSTORE_PATH");
        if (string.IsNullOrEmpty(keystore) || !System.IO.File.Exists(keystore))
        {
            PlayerSettings.Android.useCustomKeystore = false;
            return;
        }

        PlayerSettings.Android.useCustomKeystore = true;
        PlayerSettings.Android.keystoreName = keystore;
        PlayerSettings.Android.keystorePass = Environment.GetEnvironmentVariable("ANDROID_KEYSTORE_PASS") ?? string.Empty;
        PlayerSettings.Android.keyaliasName = Environment.GetEnvironmentVariable("ANDROID_KEYALIAS_NAME") ?? string.Empty;
        PlayerSettings.Android.keyaliasPass = Environment.GetEnvironmentVariable("ANDROID_KEYALIAS_PASS") ?? string.Empty;
    }

    private static string GetArgument(string argumentName)
    {
        string[] args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == argumentName)
                return args[i + 1];
        }

        return null;
    }
}
