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
        private const string DiagnosticsDirectory = "build/diagnostics";

        public static void BuildAndroid()
        {
            Directory.CreateDirectory(DiagnosticsDirectory);
            WriteDiagnostic("00-pipeline-started.txt",
                "HIGHFLY Android pipeline started\n" +
                "Unity: " + Application.unityVersion + "\n" +
                "Initial active target: " + EditorUserBuildSettings.activeBuildTarget + "\n");

            try
            {
                Debug.Log("HIGHFLY: forcing Android as the active build target before scene generation.");
                bool switched = EditorUserBuildSettings.SwitchActiveBuildTarget(
                    BuildTargetGroup.Android,
                    BuildTarget.Android);

                WriteDiagnostic("01-target-switch.txt",
                    "SwitchActiveBuildTarget returned: " + switched + "\n" +
                    "Active target after switch: " + EditorUserBuildSettings.activeBuildTarget + "\n");

                if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
                    throw new Exception("HIGHFLY could not activate the Android build target. Active target is " + EditorUserBuildSettings.activeBuildTarget);

                HighflySupremeAssetInventory.Report();
                HighflyCombatLabBuilder.BuildOrRefreshCombatLab();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                WriteDiagnostic("02-scene-generated.txt", "Combat Lab scene generated: " + HighflyCombatLabBuilder.ScenePath + "\n");

                PlayerSettings.companyName = "HIGHFLY";
                PlayerSettings.productName = "HIGHFLY NEXUS SUPREME";
                PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.highfly.nexus");
                PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
                PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
                PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
                PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
                PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;

                int versionCode = ResolveVersionCode();
                PlayerSettings.Android.bundleVersionCode = versionCode;
                PlayerSettings.bundleVersion = "0.2." + versionCode;
                EditorUserBuildSettings.buildAppBundle = false;

                // The source project points at a Windows-only custom keystore and does
                // not persist its passwords. GitHub Actions therefore cannot sign with
                // it. CI builds are test APKs, so force Unity's debug keystore here.
                // Release/store signing remains a separate concern and is not changed
                // in the committed ProjectSettings.
                PlayerSettings.Android.useCustomKeystore = false;
                WriteDiagnostic("02b-signing.txt",
                    "Custom Android keystore disabled for CI. Unity debug keystore signing enabled.\n");

                string outputPath = ResolveOutputPath();
                string directory = Path.GetDirectoryName(outputPath);
                if (!string.IsNullOrEmpty(directory))
                    Directory.CreateDirectory(directory);

                WriteDiagnostic("03-build-started.txt",
                    "Output: " + outputPath + "\n" +
                    "Active target: " + EditorUserBuildSettings.activeBuildTarget + "\n" +
                    "Custom keystore: " + PlayerSettings.Android.useCustomKeystore + "\n" +
                    "Version: " + PlayerSettings.bundleVersion + "\n" +
                    "Version code: " + PlayerSettings.Android.bundleVersionCode + "\n" +
                    "Architectures: " + PlayerSettings.Android.targetArchitectures + "\n");

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
                BuildSummary summary = report.summary;

                WriteDiagnostic("04-build-result.txt",
                    "Result: " + summary.result + "\n" +
                    "Errors: " + summary.totalErrors + "\n" +
                    "Warnings: " + summary.totalWarnings + "\n" +
                    "Output: " + summary.outputPath + "\n" +
                    "Size: " + summary.totalSize + "\n");

                if (summary.result != BuildResult.Succeeded)
                    throw new Exception("HIGHFLY Android build failed: " + summary.result + " / " + summary.totalErrors + " errors");

                Debug.Log("HIGHFLY Android APK built successfully: " + outputPath);
                WriteDiagnostic("99-success.txt", "APK built successfully: " + outputPath + "\n");
            }
            catch (Exception exception)
            {
                WriteDiagnostic("ERROR.txt", exception.ToString());
                Debug.LogException(exception);
                throw;
            }
        }

        private static void WriteDiagnostic(string fileName, string contents)
        {
            try
            {
                Directory.CreateDirectory(DiagnosticsDirectory);
                File.WriteAllText(Path.Combine(DiagnosticsDirectory, fileName), contents ?? string.Empty);
            }
            catch (Exception diagnosticException)
            {
                Debug.LogWarning("HIGHFLY could not write diagnostic file " + fileName + ": " + diagnosticException.Message);
            }
        }

        private static int ResolveVersionCode()
        {
            string runNumber = Environment.GetEnvironmentVariable("GITHUB_RUN_NUMBER");
            if (int.TryParse(runNumber, out int parsed) && parsed > 0)
                return parsed;

            return Math.Max(1, PlayerSettings.Android.bundleVersionCode);
        }

        private static string ResolveOutputPath()
        {
            string customPath = GetArgument("customBuildPath");
            string buildName = GetArgument("customBuildName");

            if (string.IsNullOrEmpty(buildName))
                buildName = "HIGHFLY-NEXUS-SUPREME";

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
