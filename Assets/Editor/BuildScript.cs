#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// Build u jedan klik. U Unityju: izbornik  Build > Napravi Windows build.
/// Rezultat ide u mapu "Build/RunSlavkoRun" pokraj mape Assets.
///
/// Ova datoteka MORA biti u mapi Assets/Editor/ — inace se ukljuci u build
/// i srusi ga, jer UnityEditor ne postoji u gotovoj igri.
/// </summary>
public static class BuildScript
{
    private const string OutputDir = "Build/RunSlavkoRun";
    private const string ExeName = "RunSlavkoRun.exe";

    [MenuItem("Build/Napravi Windows build %#b")]
    public static void BuildWindows()
    {
        string[] scenes = GetEnabledScenes();
        if (scenes.Length == 0)
        {
            EditorUtility.DisplayDialog("Build",
                "U Build Settings nema nijedne ukljucene scene.", "U redu");
            return;
        }

        string root = Path.GetDirectoryName(Application.dataPath);
        string dir = Path.Combine(root, OutputDir);
        Directory.CreateDirectory(dir);

        var options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = Path.Combine(dir, ExeName),
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        };

        Debug.Log("Build krece. Scene: " + string.Join(", ", scenes));
        BuildReport report = BuildPipeline.BuildPlayer(options);
        BuildSummary s = report.summary;

        if (s.result == BuildResult.Succeeded)
        {
            double mb = s.totalSize / (1024.0 * 1024.0);
            Debug.Log($"Build gotov: {s.outputPath}  ({mb:F1} MB, {s.totalTime.TotalSeconds:F0} s)");
            EditorUtility.RevealInFinder(s.outputPath);
        }
        else
        {
            Debug.LogError($"Build nije uspio: {s.result}, gresaka: {s.totalErrors}");
            EditorUtility.DisplayDialog("Build",
                "Build nije uspio. Pogledaj Console za greske.", "U redu");
        }
    }

    /// <summary>Isti build, ali odmah pokrene igru kad zavrsi.</summary>
    [MenuItem("Build/Napravi build i pokreni")]
    public static void BuildAndRun()
    {
        string root = Path.GetDirectoryName(Application.dataPath);
        string dir = Path.Combine(root, OutputDir);
        Directory.CreateDirectory(dir);

        BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = GetEnabledScenes(),
            locationPathName = Path.Combine(dir, ExeName),
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.AutoRunPlayer
        });
    }

    private static string[] GetEnabledScenes()
    {
        var list = new System.Collections.Generic.List<string>();
        foreach (var s in EditorBuildSettings.scenes)
        {
            if (s.enabled) list.Add(s.path);
        }
        return list.ToArray();
    }
}
#endif
