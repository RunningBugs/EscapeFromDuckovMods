using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

//
// tools/BuildMyBundles.cs
//
// Small Editor helpers to mark selected assets as AssetBundle assets
// and to build AssetBundles for the project. Place this file under
// Assets/Editor/ or any Editor folder in the project to enable menu items.
//
// Usage:
// - Select a prefab(s) in Project window and choose:
//     Tools/AssetBundles/Mark Selected As Bundle
//   This assigns an assetBundleName equal to the selection name (sanitized).
//
// - To clear assignment:
//     Tools/AssetBundles/Clear Selected Bundle Name
//
// - To build all bundles:
//     Tools/AssetBundles/Build AssetBundles
//
// - To build only the bundle(s) assigned to current selection:
//     Tools/AssetBundles/Build Selected Bundle(s)
//

public static class BuildMyBundles
{
    private const string OutputFolderName = "AssetBundles";

    [MenuItem("Tools/AssetBundles/Mark Selected As Bundle")]
    public static void MarkSelectedAsBundle()
    {
        var selection = Selection.objects;
        if (selection == null || selection.Length == 0)
        {
            Debug.LogWarning("No asset selected.");
            return;
        }

        foreach (var obj in selection)
        {
            var path = AssetDatabase.GetAssetPath(obj);
            if (string.IsNullOrEmpty(path))
                continue;

            var importer = AssetImporter.GetAtPath(path);
            if (importer == null)
                continue;

            // Create a safe bundle name from the asset file name (without extension)
            var fileName = Path.GetFileNameWithoutExtension(path);
            var bundleName = SanitizeBundleName(fileName) + ".bundle";

            importer.assetBundleName = bundleName;
            Debug.Log($"Assigned bundle name '{bundleName}' to '{path}'");
        }

        AssetDatabase.RemoveUnusedAssetBundleNames();
        AssetDatabase.Refresh();
    }

    [MenuItem("Tools/AssetBundles/Clear Selected Bundle Name")]
    public static void ClearSelectedBundleName()
    {
        var selection = Selection.objects;
        if (selection == null || selection.Length == 0)
        {
            Debug.LogWarning("No asset selected.");
            return;
        }

        foreach (var obj in selection)
        {
            var path = AssetDatabase.GetAssetPath(obj);
            if (string.IsNullOrEmpty(path))
                continue;

            var importer = AssetImporter.GetAtPath(path);
            if (importer == null)
                continue;

            if (!string.IsNullOrEmpty(importer.assetBundleName))
            {
                Debug.Log($"Clearing bundle name for '{path}' (was '{importer.assetBundleName}')");
                importer.assetBundleName = string.Empty;
            }
        }

        AssetDatabase.RemoveUnusedAssetBundleNames();
        AssetDatabase.Refresh();
    }

    [MenuItem("Tools/AssetBundles/Build AssetBundles")]
    public static void BuildAllAssetBundles()
    {
        string outDir = Path.Combine(Directory.GetCurrentDirectory(), OutputFolderName);
        if (!Directory.Exists(outDir))
            Directory.CreateDirectory(outDir);

        var target = EditorUserBuildSettings.activeBuildTarget;
        Debug.Log($"Building all AssetBundles -> {outDir} (target: {target})");

        var options = BuildAssetBundleOptions.None;
        BuildPipeline.BuildAssetBundles(outDir, options, target);

        Debug.Log($"AssetBundles built to: {outDir}");
    }

    [MenuItem("Tools/AssetBundles/Build Selected Bundle(s)")]
    public static void BuildSelectedBundles()
    {
        var selection = Selection.objects;
        if (selection == null || selection.Length == 0)
        {
            Debug.LogWarning("No asset selected. Use Build AssetBundles to build all.");
            return;
        }

        // Collect distinct bundle names from selected assets
        var bundleNames = selection
            .Select(o => AssetDatabase.GetAssetPath(o))
            .Where(p => !string.IsNullOrEmpty(p))
            .Select(p => AssetImporter.GetAtPath(p))
            .Where(i => i != null)
            .Select(i => i.assetBundleName)
            .Where(n => !string.IsNullOrEmpty(n))
            .Distinct()
            .ToArray();

        if (bundleNames.Length == 0)
        {
            Debug.LogWarning("Selected assets have no assigned bundle names.");
            return;
        }

        string outDir = Path.Combine(Directory.GetCurrentDirectory(), OutputFolderName);
        if (!Directory.Exists(outDir))
            Directory.CreateDirectory(outDir);

        var target = EditorUserBuildSettings.activeBuildTarget;

        // Build AssetBundleBuild list only for selected bundle names
        var builds = bundleNames.Select(name =>
        {
            // Find all asset paths for that bundle
            var assetPaths = AssetDatabase.GetAssetPathsFromAssetBundle(name);
            return new AssetBundleBuild
            {
                assetBundleName = name,
                assetNames = assetPaths
            };
        }).ToArray();

        if (builds.Length == 0)
        {
            Debug.LogWarning("No asset paths found for selected bundle names.");
            return;
        }

        Debug.Log($"Building {builds.Length} selected bundle(s) -> {outDir} (target: {target})");
        BuildPipeline.BuildAssetBundles(outDir, builds, BuildAssetBundleOptions.None, target);
        Debug.Log("Selected AssetBundle build complete.");
    }

    // Utility: sanitize bundle name (lowercase, remove spaces)
    private static string SanitizeBundleName(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var cleaned = new string(name.Where(c => !invalid.Contains(c)).ToArray());
        cleaned = cleaned.Replace(' ', '_');
        cleaned = cleaned.ToLowerInvariant();
        return cleaned;
    }
}
