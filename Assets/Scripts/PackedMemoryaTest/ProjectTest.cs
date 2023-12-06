using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;

public class ProjectTest : MonoBehaviour
{
    public void AddItem()
    {
        //bundle加载
        // string assetPath = "Assets/StreamingAssets/assets/packprefabtest.bundle";
        // AssetBundle bundle = AssetBundle.LoadFromFile(assetPath);
        // Profiler.BeginSample("startLoad");
        // var target = bundle.LoadAsset("PackedObject");//通过资源名加载单个资源
        // var obj = target as GameObject;
        // var cell = Instantiate(obj, transform);
        // cell.transform.localPosition = Vector3.zero;
        // Profiler.EndSample();
        
        
        string assetPath = "Assets/StreamingAssets/assets/unpackprefabtest.bundle";
        AssetBundle bundle = AssetBundle.LoadFromFile(assetPath);
        Profiler.BeginSample("startLoad");
        var target = bundle.LoadAsset("UnPackObject");//通过资源名加载单个资源
        var obj = target as GameObject;
        var cell = Instantiate(obj, transform);
        cell.transform.localPosition = Vector3.zero;
        Profiler.EndSample();

        // 资源路径加载
        // string path = "Assets/Art/Prefab/PackPrefabTest/UnPackObject.prefab";
        // var prefab = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject));
        // Profiler.BeginSample("startLoad");
        // var obj = prefab as GameObject;
        // var gameObj = Instantiate(obj, transform);
        // gameObj.transform.localPosition = Vector3.zero;
        // Profiler.EndSample();
    }
}