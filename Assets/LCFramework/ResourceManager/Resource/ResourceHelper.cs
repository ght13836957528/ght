using System;
using UnityEngine;
using Spine.Unity;
using XLua;
using Framework;

/// <summary>
/// 资源助手
/// </summary>
public static class ResourceHelper
{
    private static Type GameObjectType = typeof(GameObject);
    private static Type AudioClipType = typeof(AudioClip);
    private static Type Texture2DType = typeof(Texture2D);
    private static Type TextAssetType = typeof(TextAsset);
    private static Type SpriteAtlasType = typeof(UnityEngine.U2D.SpriteAtlas);
    private static Type SpriteType = typeof(UnityEngine.Sprite);
    private static Type MaterialType = typeof(UnityEngine.Material);
    private static Type SkeletonDataAsset = typeof(Spine.Unity.SkeletonDataAsset);
    private static Type ScriptableObjectType = typeof(ScriptableObject);

    /// <summary>
    /// 泛型异步加载，类型不能使用Unity.Object基类，否则可能会和非泛型接口出现同样的问题，加载同一个对象生成两份缓存
    /// </summary>
    /// <param name="assetPath"></param>
    /// <param name="callback"></param>
    /// <param name="hold"></param>
    /// <param name="preload"></param>
    /// <param name="isSingleBundle"></param>
    /// <typeparam name="T"></typeparam>
    [BlackList]
    public static string LoadAsset<T>(string assetPath, ResourceManager.OnLoadComplete callback = null, MemeryHold hold = MemeryHold.Normal, bool isSingleBundle = true) where T : UnityEngine.Object
    {
        ResourceManager.ParseAssetPath(assetPath, isSingleBundle, out string assetName, out string assetBundle);

        return ResourceManager.Instance?.LoadAssetAsync<T>(assetBundle, assetName, callback, hold);
    }

    public static void LoadAsset(Type t, string address, ResourceManager.OnLoadComplete callback = null)
    {
        if (t == GameObjectType) LoadAsset<GameObject>(address, callback);
        else if (t == AudioClipType) LoadAsset<AudioClip>(address, callback);
        else if (t == Texture2DType) LoadAsset<Texture2D>(address, callback);
        else if (t == TextAssetType) LoadAsset<TextAsset>(address, callback);
        else if (t == SpriteAtlasType) LoadAsset<UnityEngine.U2D.SpriteAtlas>(address, callback);
        else if (t == SpriteType) LoadAsset<Sprite>(address, callback);
        else if (t == MaterialType) LoadAsset<Material>(address, callback);
        else if (t == SkeletonDataAsset) LoadAsset<Spine.Unity.SkeletonDataAsset>(address, callback);
        else if (t == ScriptableObjectType) LoadAsset<ScriptableObject>(address, callback);
        else { Debug.LogErrorFormat("unknow asset type: " + t.Name); }
    }

    public static void SpawnGameObject(string assetPath, Transform parent = null, Action<InstanceObject> loadCallback = null)
    {
        InstanceObjectManager.Instance.Spawn(assetPath, parent, loadCallback);
    }

    public static string LoadGameObjectAsset(string assetPath, ResourceManager.OnLoadComplete callback = null, MemeryHold hold = MemeryHold.Normal, bool isSingleBundle = true)
    {
        return LoadAsset<GameObject>(assetPath, callback, hold, isSingleBundle);
    }

    public static string LoadSpriteAsset(string assetPath, ResourceManager.OnLoadComplete callback = null, MemeryHold hold = MemeryHold.Normal, bool isSingleBundle = true)
    {
        return LoadAsset<Sprite>(assetPath, callback, hold, isSingleBundle);
    }

    public static string LoadTextAsset(string assetPath, ResourceManager.OnLoadComplete callback = null, MemeryHold hold = MemeryHold.Normal, bool isSingleBundle = true)
    {
        return LoadAsset<TextAsset>(assetPath, callback, hold, isSingleBundle);
    }

    public static string LoadMaterialAsset(string assetPath, ResourceManager.OnLoadComplete callback = null, MemeryHold hold = MemeryHold.Normal, bool isSingleBundle = true)
    {
        return LoadAsset<Material>(assetPath, callback, hold, isSingleBundle);
    }

    public static string LoadSkeletonAsset(string assetPath, ResourceManager.OnLoadComplete callback = null, MemeryHold hold = MemeryHold.Normal, bool isSingleBundle = true)
    {
        return LoadAsset<SkeletonDataAsset>(assetPath, callback, hold, isSingleBundle);
    }

    public static string GetKeyFromAssetPath<T>(string assetPath, bool isSingleBundle, out string assetName, out string assetBundleName) where T : UnityEngine.Object
    {
        ResourceManager.ParseAssetPath(assetPath, isSingleBundle, out assetName, out assetBundleName);
        return ResourceManager.AssetKeyLower(assetBundleName, assetName, typeof(T));
    }
    [BlackList]
    public static void UnloadAssetWithPath<T>(string assetPath, bool immediately = false) where T : UnityEngine.Object
    {
        string key = GetKeyFromAssetPath<T>(assetPath, true, out string assetName, out string assetBundle);
        UnloadAssetWithKey(key, immediately);
    }

    public static void UnloadAssetWithPath(Type t, string assetPath, bool immediately = false)
    {
        if (t == GameObjectType) UnloadAssetWithPath<GameObject>(assetPath, immediately);
        else if (t == AudioClipType) UnloadAssetWithPath<AudioClip>(assetPath, immediately);
        else if (t == Texture2DType) UnloadAssetWithPath<Texture2D>(assetPath, immediately);
        else if (t == TextAssetType) UnloadAssetWithPath<TextAsset>(assetPath, immediately);
        else if (t == SpriteAtlasType) UnloadAssetWithPath<UnityEngine.U2D.SpriteAtlas>(assetPath, immediately);
        else if (t == SpriteType) UnloadAssetWithPath<Sprite>(assetPath, immediately);
        else if (t == MaterialType) UnloadAssetWithPath<Material>(assetPath, immediately);
        else if (t == SkeletonDataAsset) UnloadAssetWithPath<Spine.Unity.SkeletonDataAsset>(assetPath, immediately);
        else if (t == ScriptableObjectType) UnloadAssetWithPath<ScriptableObject>(assetPath, immediately);
        else { Debug.LogErrorFormat("unknow asset type: " + t.Name); }
    }

    public static void UnloadAssetWithKey(string key, bool immdiately = false)
    {
        if (ResourceManager.Instance != null)
            ResourceManager.Instance.UnloadAssetWithKey(key, immdiately);
    }

    public static void UnloadAssetWithObject(object obj, bool immdiately = false)
    {
        if (ResourceManager.Instance != null)
            ResourceManager.Instance.UnloadAssetWithObject(obj, immdiately);
    }
}