using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

// One-click Windows build: Unity menu  Ostatni Peron > Build Windows (Desktop).
// Creates the folder "OstatniPeron" on your Desktop with OstatniPeron.exe inside.
public static class BuildTools
{
    [MenuItem("Ostatni Peron/Build Windows (Desktop)")]
    public static void BuildWindows()
    {
        PlayerSettings.companyName = "Ostatni Peron Team";
        PlayerSettings.productName = "Ostatni Peron";
        PlayerSettings.defaultScreenWidth = 1280;
        PlayerSettings.defaultScreenHeight = 720;
        PlayerSettings.fullScreenMode = FullScreenMode.FullScreenWindow;
        PlayerSettings.resizableWindow = true;
        PlayerSettings.runInBackground = true;

        string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "OstatniPeron");
        string exe = Path.Combine(folder, "OstatniPeron.exe");
        Directory.CreateDirectory(folder);

        var options = new BuildPlayerOptions
        {
            scenes = new[] { "Assets/Scenes/MainMenu.unity", "Assets/Scenes/Peron.unity" },
            locationPathName = exe,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            EditorUtility.DisplayDialog("Ostatni Peron",
                "Gotowe! Gra jest na pulpicie w folderze OstatniPeron.\nUruchom plik OstatniPeron.exe.", "OK");
            EditorUtility.RevealInFinder(exe);
        }
        else
        {
            EditorUtility.DisplayDialog("Ostatni Peron",
                "Budowanie nie powiodło się (" + summary.result + "). Zobacz Console.", "OK");
        }
    }
}
