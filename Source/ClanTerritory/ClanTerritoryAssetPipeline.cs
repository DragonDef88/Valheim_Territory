using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ClanTerritory.Editor
{
    internal static class ClanTerritoryAssetPipeline
    {
        private const string MapIconsFolder = "Assets/Icons/Map";
        private const string MapIconsBundleName = "clanterritory_mapicons";
        private const string OutputFolder = "AssetBundles";

        [MenuItem("Tools/Clan Territory/Prepare Map Icons")]
        public static void PrepareMapIcons()
        {
            if (!Directory.Exists(MapIconsFolder))
            {
                Directory.CreateDirectory(MapIconsFolder);
                AssetDatabase.Refresh();
            }

            string[] textureGuids = AssetDatabase.FindAssets("t:Texture2D", new[] { MapIconsFolder });

            foreach (string guid in textureGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;

                if (importer == null)
                    continue;

                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.mipmapEnabled = false;
                importer.alphaIsTransparency = true;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.filterMode = FilterMode.Bilinear;
                importer.spritePixelsPerUnit = 100f;
                importer.assetBundleName = MapIconsBundleName;

                importer.SaveAndReimport();

                Debug.Log("[Clan Territory] Prepared map icon: " + path);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        [MenuItem("Tools/Clan Territory/Build Asset Bundles")]
        public static void BuildAssetBundles()
        {
            PrepareMapIcons();

            string outputPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                OutputFolder);

            if (!Directory.Exists(outputPath))
                Directory.CreateDirectory(outputPath);

            BuildPipeline.BuildAssetBundles(
                outputPath,
                BuildAssetBundleOptions.None,
                EditorUserBuildSettings.activeBuildTarget);

            Debug.Log("[Clan Territory] AssetBundles built: " + outputPath);
        }

        [MenuItem("Tools/Clan Territory/Open AssetBundle Folder")]
        public static void OpenAssetBundleFolder()
        {
            string outputPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                OutputFolder);

            if (!Directory.Exists(outputPath))
                Directory.CreateDirectory(outputPath);

            EditorUtility.RevealInFinder(outputPath);
        }
    }
}