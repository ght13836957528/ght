using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PackPrefabMemoryTest
{
    public class BuildBundleTest
    {
        [MenuItem("Test/TestBundle")]
        public static void BuildBundle()
        {
            // 一个文件一个AssetBundle;
            AssetBundleBuild[] buildMap = new AssetBundleBuild[1];

            //打包出来的AssetBundle包文件名字;
            buildMap[0].assetBundleName = "assets/UnpackPrefabTest";
            buildMap[0].assetBundleVariant = "bundle";
            string[] guids = AssetDatabase.FindAssets("t:Object", new[] {"Assets/Art/Prefab/UnpackPrefabTest"});
            List<string> scriptList = new List<string>(); 
            foreach (var guid in guids)
            {
                string filePath = AssetDatabase.GUIDToAssetPath(guid);
                scriptList.Add(filePath);
            }

            buildMap[0].assetNames = scriptList.ToArray();
            
            BuildPipeline.BuildAssetBundles(Application.streamingAssetsPath,buildMap,BuildAssetBundleOptions.UncompressedAssetBundle,EditorUserBuildSettings.activeBuildTarget);
            Debug.Log("BuildAssetBundles");
        }
    }
}