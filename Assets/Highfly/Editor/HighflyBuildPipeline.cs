#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Highfly.Editor
{
    public static class HighflyBuildPipeline
    {
        public static void BuildAndroid()
        {
            HighflyCombatLabBuilder.BuildOrRefreshCombatLab();

            PlayerSettings.companyName = "HIGHFLY";
            PlayerSettings.productName = "HIGHFLY NEXUS";
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.highfly.nexus");
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
            EditorUserBuildSettings.buildAppBundle = false;

            string outputPath = ResolveOutputPath();
            string directory = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = new[] { HighflyCombatLabBuilder.ScenePath },
                locationPathName = outputPath,
                target = BuildTarget.Android,
                targetGroup = BuildTargetGroup.Android,
                options = BuildOptions.None
            };

            Debug.Log("HIGHFLY Android build output: " + outputPath);
            BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new Exception("HIGHFLY Android build failed: " + report.summary.result + " / " + report.summary.totalErrors + " errors");

            Debug.Log("HIGHFLY Android APK built successfully: " + outputPath);
        }

        private static string ResolveOutputPath()
        {
            string customPath = GetArgument("customBuildPath");
            string buildName = GetArgument("customBuildName");

            if (string.IsNullOrEmpty(buildName))
                buildName = "HIGHFLY-NEXUS-COMBAT-LAB";

            if (string.IsNullOrEmpty(customPath))
                return Path.Combine("build", "Android", buildName + ".apk");

            if (string.Equals(Path.GetExtension(customPath), ".apk", StringComparison.OrdinalIgnoreCase))
                return customPath;

            return Path.Combine(customPath, buildName + ".apk");
        }

        private static string GetArgument(string name)
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
            {
                string arg = args[i].TrimStart('-');
                if (string.Equals(arg, name, StringComparison.OrdinalIgnoreCase))
                    return args[i + 1];
            }

            return null;
        }
    }
}
#endif
