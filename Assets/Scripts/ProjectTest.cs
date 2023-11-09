using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;

public class ProjectTest : MonoBehaviour
{
    public void AddItem()
    {
        
        string assetPath = "Assets/StreamingAssets/assets/packprefabtest.bundle";
        AssetBundle bundle = AssetBundle.LoadFromFile(assetPath);
        Profiler.BeginSample("startLoad");
        var target = bundle.LoadAsset("PackedObject");//通过资源名加载单个资源
        var obj = target as GameObject;
        var cell = Instantiate(obj, transform);
        cell.transform.localPosition = Vector3.zero;
        Profiler.EndSample();
    }
}