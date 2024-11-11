using System.IO;
using Spine.Unity;
using UnityEditor;
using UnityEngine;


namespace Editor.Resource
{
    public class SpineTools
    {
        [MenuItem("Assets/Spine Helper/生成spine预制（SkeletonAnimation）")]
    public static void GenerateSkeletonAnimation()
    {
        var guids = Selection.assetGUIDs;
        if (guids == null || guids.Length < 1)
        {
            Debug.LogError("Please select files  ----- 请选中文件（支持多选）");
            return;
        }

        foreach (var guid in guids)
        {
            var assetPath = AssetDatabase.GUIDToAssetPath(guid); //通过GUID获取路径
            if (string.IsNullOrEmpty(assetPath))
            {
                continue;
            }
            string[] skeletonFiles = Directory.GetFiles(assetPath, "*.asset", SearchOption.AllDirectories);
            foreach (var skeleton in skeletonFiles)
            {
                SkeletonDataAsset skeletonDataAsset = AssetDatabase.LoadAssetAtPath<SkeletonDataAsset>(skeleton);
                if (skeletonDataAsset == null) continue;
                string goName = skeletonDataAsset.name.Replace("_SkeletonData", "");
                var prefabPath = Path.GetDirectoryName(assetPath);
                string skeletonPath = Path.Combine(prefabPath, goName + ".prefab");
                if (File.Exists(skeletonPath))
                {
                    Debug.LogError("skeleton prefab exist,path==" + skeletonPath);
                    continue;
                }
                
                PrefabUtility.CreateEmptyPrefab(skeletonPath);
                GameObject prefab = PrefabUtility.LoadPrefabContents(skeletonPath);
                GameObject instant = GameObject.Instantiate(prefab);
                SkeletonAnimation go = SkeletonAnimation.NewSkeletonAnimationGameObject(skeletonDataAsset);
                go.transform.name = "spine";
                go.transform.SetParent(instant.transform);
                PrefabUtility.SaveAsPrefabAsset(instant.gameObject, skeletonPath);
                GameObject.DestroyImmediate(instant.gameObject);
                AssetDatabase.Refresh();
            }
        }
    }

    [MenuItem("Assets/Spine Helper/生成spine预制（SkeletonGraphic）")]
    public static void GenerateSkeletonGraphic()
    {
        var guids = Selection.assetGUIDs;
        if (guids == null || guids.Length < 1)
        {
            Debug.LogError("Please select files  ----- 请选中文件（支持多选）");
            return;
        }

        foreach (var guid in guids)
        {
            var assetPath = AssetDatabase.GUIDToAssetPath(guid); //通过GUID获取路径
            if (string.IsNullOrEmpty(assetPath))
            {
                continue;
            }

            string[] skeletonFiles = Directory.GetFiles(assetPath, "*.asset", SearchOption.AllDirectories);
            foreach (var skeleton in skeletonFiles)
            {
                SkeletonDataAsset skeletonDataAsset = AssetDatabase.LoadAssetAtPath<SkeletonDataAsset>(skeleton);
                if (skeletonDataAsset == null) continue;
                string goName = skeletonDataAsset.name.Replace("_SkeletonData", "");
                var prefabPath = Path.GetDirectoryName(assetPath);
                string skeletonPath = Path.Combine(prefabPath, goName + ".prefab");
                if (File.Exists(skeletonPath))
                {
                    Debug.LogError("skeleton prefab exist,path==" + skeletonPath);
                    continue;
                }
                PrefabUtility.CreateEmptyPrefab(skeletonPath);
                GameObject prefab = PrefabUtility.LoadPrefabContents(skeletonPath);
                var parentNode =GameObject.Find("Launcher/UI/UIContainer");
                GameObject instant = GameObject.Instantiate(prefab,parentNode.transform);
                instant.AddComponent<RectTransform>();
                Material defaultMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/3rdParty/Spine/Runtime/spine-unity/Materials/SkeletonGraphic-PMATexture/SkeletonGraphicDefault.mat");
                if (defaultMaterial == null)
                {
                    Debug.LogError("SkeletonGraphicDefault is null");
                    continue;
                }
                SkeletonGraphic go = SkeletonGraphic.NewSkeletonGraphicGameObject(skeletonDataAsset, instant.transform, defaultMaterial);
                go.transform.name = "spine";
                PrefabUtility.SaveAsPrefabAsset(instant.gameObject, skeletonPath);
                GameObject.DestroyImmediate(instant);
                AssetDatabase.Refresh();
            }
        }
    }
    }
}