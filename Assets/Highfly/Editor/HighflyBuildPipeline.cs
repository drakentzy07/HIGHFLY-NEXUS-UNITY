#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Rendering;

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

                HighflyCombatLabBuilder.BuildOrRefreshWorldLabShell();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                WriteDiagnostic("02-scene-generated.txt", "Combat Lab scene generated: " + HighflyCombatLabBuilder.ScenePath + "\n");

                PlayerSettings.companyName = "HIGHFLY";
                PlayerSettings.productName = "HIGHFLY NEXUS";
                PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.highfly.nexus");
                PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
                PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
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
                    "Custom keystore: " + PlayerSettings.Android.useCustomKeystore + "\n");

                BuildPlayerOptions options = new BuildPlayerOptions
                {
                    scenes = new[] { HighflyCombatLabBuilder.WorldScenePath },
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

        public static void BuildWebGLClaudeWorld()
        {
            Directory.CreateDirectory(DiagnosticsDirectory);
            WriteDiagnostic("10-webgl-pipeline-started.txt",
                "HIGHFLY World Final FREE WebGL pipeline started\n" +
                "Unity: " + Application.unityVersion + "\n" +
                "Initial active target: " + EditorUserBuildSettings.activeBuildTarget + "\n");

            try
            {
                bool switched = EditorUserBuildSettings.SwitchActiveBuildTarget(
                    BuildTargetGroup.WebGL,
                    BuildTarget.WebGL);

                WriteDiagnostic("11-webgl-target-switch.txt",
                    "SwitchActiveBuildTarget returned: " + switched + "\n" +
                    "Active target after switch: " + EditorUserBuildSettings.activeBuildTarget + "\n");

                if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.WebGL)
                    throw new Exception("HIGHFLY could not activate WebGL. Active target is " + EditorUserBuildSettings.activeBuildTarget);

                HighflyCombatLabBuilder.BuildOrRefreshWorldLabShell();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                PlayerSettings.companyName = "HIGHFLY";
                PlayerSettings.productName = "HIGHFLY WORLD FINAL FREE";
                PlayerSettings.runInBackground = true;

                // Project uses Linear color space. Unity 2021 WebGL can only use
                // Linear with WebGL 2.0, so automatic graphics APIs (which include
                // a WebGL 1.0 fallback) make BuildPipeline reject the player.
                PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.WebGL, false);
                PlayerSettings.SetGraphicsAPIs(
                    BuildTarget.WebGL,
                    new[] { GraphicsDeviceType.OpenGLES3 });

                PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
                PlayerSettings.WebGL.decompressionFallback = true;

                WriteDiagnostic("11b-webgl-rendering.txt",
                    "Color space: " + PlayerSettings.colorSpace + "\n" +
                    "Use default graphics APIs: " + PlayerSettings.GetUseDefaultGraphicsAPIs(BuildTarget.WebGL) + "\n" +
                    "Graphics APIs: " + string.Join(", ", PlayerSettings.GetGraphicsAPIs(BuildTarget.WebGL)) + "\n");

                string outputPath = GetArgument("customBuildPath");
                if (string.IsNullOrEmpty(outputPath))
                    outputPath = Path.Combine("build", "WebGL", "HIGHFLY-WORLD-LAB");

                Directory.CreateDirectory(outputPath);

                BuildPlayerOptions options = new BuildPlayerOptions
                {
                    scenes = new[] { HighflyCombatLabBuilder.WorldScenePath },
                    locationPathName = outputPath,
                    target = BuildTarget.WebGL,
                    targetGroup = BuildTargetGroup.WebGL,
                    options = BuildOptions.None
                };

                WriteDiagnostic("12-webgl-build-started.txt",
                    "Output: " + outputPath + "\n" +
                    "World mode: WORLD FINAL FREE / QUATERNIUS CC0\n");

                BuildReport report = BuildPipeline.BuildPlayer(options);
                BuildSummary summary = report.summary;

                WriteDiagnostic("13-webgl-build-result.txt",
                    "Result: " + summary.result + "\n" +
                    "Errors: " + summary.totalErrors + "\n" +
                    "Warnings: " + summary.totalWarnings + "\n" +
                    "Output: " + summary.outputPath + "\n" +
                    "Size: " + summary.totalSize + "\n");

                if (summary.result != BuildResult.Succeeded)
                    throw new Exception("HIGHFLY World Final FREE WebGL build failed: " + summary.result + " / " + summary.totalErrors + " errors");

                WriteDiagnostic("19-webgl-success.txt", "HIGHFLY World Final FREE WebGL built successfully: " + outputPath + "\n");
            }
            catch (Exception exception)
            {
                WriteDiagnostic("WEBGL_ERROR.txt", exception.ToString());
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
