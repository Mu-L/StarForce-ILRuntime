using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityGameFramework.Editor.ResourceTools;

namespace UGFExtensions.Build.Editor
{
    public class BuildEventHandle : IBuildEventHandler
    {
        public bool ContinueOnFailure
        {
            get { return false; }
        }


        public void OnPreprocessAllPlatforms(string productName, string companyName, string gameIdentifier,
            string gameFrameworkVersion, string unityVersion, string applicableGameVersion, int internalResourceVersion,
            UnityGameFramework.Editor.ResourceTools.Platform platforms, AssetBundleCompressionType assetBundleCompression, string compressionHelperTypeName,
            bool additionalCompressionSelected, bool forceRebuildAssetBundleSelected, string buildEventHandlerTypeName,
            string outputDirectory, BuildAssetBundleOptions buildAssetBundleOptions, string workingPath,
            bool outputPackageSelected, string outputPackagePath, bool outputFullSelected, string outputFullPath,
            bool outputPackedSelected, string outputPackedPath, string buildReportPath)
        {
            string streamingAssetsPath = GameFramework.Utility.Path.GetRegularPath(Path.Combine(Application.dataPath, "StreamingAssets"));
            string[] fileNames = Directory.GetFiles(streamingAssetsPath, "*", SearchOption.AllDirectories);
            foreach (string fileName in fileNames)
            {
                if (fileName.Contains(".gitkeep"))
                {
                    continue;
                }

                File.Delete(fileName);
            }

            GameFramework.Utility.Path.RemoveEmptyDirectory(streamingAssetsPath);
        }

        public void OnPreprocessPlatform(UnityGameFramework.Editor.ResourceTools.Platform platform, string workingPath, bool outputPackageSelected,
            string outputPackagePath,
            bool outputFullSelected, string outputFullPath, bool outputPackedSelected, string outputPackedPath)
        {
        }

        public void OnBuildAssetBundlesComplete(UnityGameFramework.Editor.ResourceTools.Platform platform, string workingPath, bool outputPackageSelected,
            string outputPackagePath, bool outputFullSelected, string outputFullPath, bool outputPackedSelected,
            string outputPackedPath, AssetBundleManifest assetBundleManifest)
        {
        }

        public void OnOutputUpdatableVersionListData(UnityGameFramework.Editor.ResourceTools.Platform platform, string versionListPath, int versionListLength,
            int versionListHashCode, int versionListZipLength, int versionListZipHashCode)
        {
            Type resourceBuilderType =
                Type.GetType("UnityGameFramework.Editor.ResourceTools.ResourceBuilder,UnityGameFramework.Editor");
            var window = EditorWindow.GetWindow(resourceBuilderType);
            ResourceBuilderController builderController =
                window.GetType().GetField("m_Controller", BindingFlags.Instance | BindingFlags.NonPublic)
                    ?.GetValue(window) as ResourceBuilderController;
            if (builderController == null)
            {
                return;
            }
            
            VersionInfoEditorData versionInfoEditorData =
                AssetDatabase.LoadAssetAtPath<VersionInfoEditorData>("Assets/Res/Configs/VersionInfoEditorData.asset");
            if (versionInfoEditorData == null)
            {
                versionInfoEditorData = ScriptableObject.CreateInstance<VersionInfoEditorData>();
                versionInfoEditorData.VersionInfos = new List<VersionInfoWrapData>
                    { new VersionInfoWrapData() { Key = "Normal", Value = new VersionInfoData() } };
                AssetDatabase.CreateAsset(versionInfoEditorData, "Assets/Res/Configs/VersionInfoEditorData.asset");
                Debug.Log("CreateVersionInfoEditorData Success!");
                AssetDatabase.Refresh();
                Selection.activeObject = versionInfoEditorData;
            }

            VersionInfoData versionInfoData = versionInfoEditorData.GetActiveVersionInfoData();
            versionInfoData.AutoIncrementInternalGameVersion();
            versionInfoData.ForceUpdateGame = false;
            versionInfoData.ResourceVersion = builderController.ApplicableGameVersion.Replace('.', '_')+ "_"+builderController.InternalResourceVersion;
            versionInfoData.Platform = (Platform)(int)platform;
            versionInfoData.LatestGameVersion = builderController.ApplicableGameVersion;
            versionInfoData.InternalResourceVersion = builderController.InternalResourceVersion;
            versionInfoData.VersionListLength = versionListLength;
            versionInfoData.VersionListHashCode = versionListHashCode;
            versionInfoData.VersionListCompressedLength = versionListZipLength;
            versionInfoData.VersionListCompressedHashCode = versionListZipHashCode;
            EditorUtility.SetDirty(versionInfoEditorData);
            AssetDatabase.SaveAssets();

            if (versionInfoEditorData.IsGenerateToFullPath)
            {
                versionInfoEditorData.Generate(Path.Combine(builderController.OutputFullPath, platform.ToString(), $"{platform}Version.txt"));
            }
        }

        public void OnPostprocessPlatform(UnityGameFramework.Editor.ResourceTools.Platform platform, string workingPath, bool outputPackageSelected,
            string outputPackagePath,
            bool outputFullSelected, string outputFullPath, bool outputPackedSelected, string outputPackedPath,
            bool isSuccess)
        {
            if (!outputPackageSelected)
            {
                return;
            }

            if (platform != UnityGameFramework.Editor.ResourceTools.Platform.Windows)
            {
                return;
            }

            string streamingAssetsPath = GameFramework.Utility.Path.GetRegularPath(Path.Combine(Application.dataPath, "StreamingAssets"));
            string[] fileNames = Directory.GetFiles(outputPackagePath, "*", SearchOption.AllDirectories);
            foreach (string fileName in fileNames)
            {
                string destFileName = GameFramework.Utility.Path.GetRegularPath(Path.Combine(streamingAssetsPath, fileName.Substring(outputPackagePath.Length)));
                FileInfo destFileInfo = new FileInfo(destFileName);
                if (!destFileInfo.Directory.Exists)
                {
                    destFileInfo.Directory.Create();
                }

                File.Copy(fileName, destFileName);
            }
        }

        public void OnPostprocessAllPlatforms(string productName, string companyName, string gameIdentifier,
            string gameFrameworkVersion, string unityVersion, string applicableGameVersion, int internalResourceVersion,
            UnityGameFramework.Editor.ResourceTools.Platform platforms, AssetBundleCompressionType assetBundleCompression, string compressionHelperTypeName,
            bool additionalCompressionSelected, bool forceRebuildAssetBundleSelected, string buildEventHandlerTypeName,
            string outputDirectory, BuildAssetBundleOptions buildAssetBundleOptions, string workingPath,
            bool outputPackageSelected, string outputPackagePath, bool outputFullSelected, string outputFullPath,
            bool outputPackedSelected, string outputPackedPath, string buildReportPath)
        {
        }
        
        
    }
}