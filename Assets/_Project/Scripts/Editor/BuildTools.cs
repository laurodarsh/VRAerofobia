using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Aerofobia.EditorTools
{
    /// <summary>
    /// Gera o APK para Quest com as cenas do Build Profile.
    /// Saída em <c>Builds/</c> (fora do Git). Também usado em batchmode para validar o projeto:
    /// <c>-executeMethod Aerofobia.EditorTools.BuildTools.BatchBuildQuest</c>.
    /// </summary>
    public static class BuildTools
    {
        const string OutputFolder = "Builds";

        [MenuItem("Aerofobia/Gerar APK para Quest", priority = 40)]
        static void BuildQuestMenu()
        {
            var report = BuildQuest(development: true);
            if (report.summary.result == BuildResult.Succeeded)
            {
                EditorUtility.RevealInFinder(report.summary.outputPath);
            }
        }

        /// <summary>Ponto de entrada para batchmode. Sai com código 1 se o build falhar.</summary>
        public static void BatchBuildQuest()
        {
            var report = BuildQuest(development: true);
            EditorApplication.Exit(report.summary.result == BuildResult.Succeeded ? 0 : 1);
        }

        /// <summary>Gera <c>Builds/AerofobiaVR.apk</c> para Android (Quest 2/3).</summary>
        public static BuildReport BuildQuest(bool development)
        {
            Directory.CreateDirectory(OutputFolder);
            var options = new BuildPlayerOptions
            {
                scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
                locationPathName = Path.Combine(OutputFolder, "AerofobiaVR.apk"),
                target = BuildTarget.Android,
                options = development ? BuildOptions.Development : BuildOptions.None,
            };

            var report = BuildPipeline.BuildPlayer(options);
            var summary = report.summary;
            Debug.Log($"[Aerofobia.Build] {summary.result}: {summary.outputPath} " +
                      $"({summary.totalSize / (1024f * 1024f):F1} MB, {summary.totalErrors} erro(s), {summary.totalTime:mm\\:ss})");
            return report;
        }
    }
}
