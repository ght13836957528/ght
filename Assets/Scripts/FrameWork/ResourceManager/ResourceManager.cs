using System;
using System.Collections;
using System.Collections.Generic;
using AssetBundles;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;
using UnityEngine.U2D;
using Object = UnityEngine.Object;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace GameManager
{
    public class ResourceManager : SingletonBehaviour<ResourceManager>
    {
        public enum MemeryHold
        {
            Once = 0,   // 进出牌局自动释放或者自己调用释放
            Normal = 1, // 自己调用释放
            Always = 2, // 在关闭游戏的时候释放
        }

        [System.Serializable]
        public class AssetCache
        {
            public string assetName;            // AssetBundle中的资源名字
            public string assetbundleName;      // AssetBundle的名字
            public string assetbundleVariant;   // 实际加载的AssetBundle的变体的名字
            public System.Type type;            // 资源类型
            public MemeryHold hold;             // 内存持久类型
            public int refCount;                // 被引用次数

            public object obj;                  // 缓存中的资源对象

            public void Release()
            {
                if (obj != null)
                {
                    if (obj is GameObject prefab)
                    {
                        // prefab.RecycleAll();
                        // prefab.DestroyPooled();
                    }
                }

                obj = null;

                Debug.Log("Release " + assetName + " cache, it's ABVariant is: "+ assetbundleVariant);

                if (!string.IsNullOrEmpty(assetbundleVariant))
                    AssetBundles.AssetBundleManager.UnloadAssetBundle(assetbundleVariant);
            }
        }

        private const string AssetKeyFormat = "{0}@{1}@{2}";
        private const int RemoveAssetDelay = 30;

        private bool IsInited;
        private AssetBundleLoadLevelOperation m_LoadSceneOpration;  // 单场景读取进程
        private AssetBundleLoadManifestOperation m_AssetBundleLoadManifestOperation;

        public delegate void OnLoadComplete(string key, object asset, string err);

#if ODIN_INSPECTOR
        [ShowInInspector, ShowIf("showOdinInfo")]
#endif
        public float LoadingSceneProgress
        {
            get;
            private set;
        }

#if ODIN_INSPECTOR
        [ShowInInspector, ShowIf("showOdinInfo")]
#endif
        public bool IsLoadingScene
        {
            get;
            private set;
        }

#if ODIN_INSPECTOR
        [ShowInInspector, ShowIf("showOdinInfo"), ListDrawerSettings(IsReadOnly = true)]
#endif
        private List<AssetBundleLoadAssetOperation> m_InProgressOperations = new List<AssetBundleLoadAssetOperation>(); //正在加载的资源
#if ODIN_INSPECTOR
        [ShowInInspector, ShowIf("showOdinInfo"), DictionaryDrawerSettings(IsReadOnly = true)]
#endif
        private readonly Dictionary<string, List<System.Delegate>> m_CallbackStack = new Dictionary<string, List<System.Delegate>>();
#if ODIN_INSPECTOR
        [ShowInInspector, ShowIf("showOdinInfo"), DictionaryDrawerSettings(IsReadOnly = true, DisplayMode = DictionaryDisplayOptions.Foldout)]
#endif
        private readonly Dictionary<string, AssetCache> m_AssetCaches = new Dictionary<string, AssetCache>();
#if ODIN_INSPECTOR
        [ShowInInspector, ShowIf("showOdinInfo"), DictionaryDrawerSettings(IsReadOnly = true)]
#endif
        private readonly Dictionary<string, string> m_LoadedScenesAssetBundle = new Dictionary<string, string>();
#if ODIN_INSPECTOR
        [ShowInInspector, ShowIf("showOdinInfo"), DictionaryDrawerSettings(IsReadOnly = true)]
#endif
        private readonly Dictionary<object, string> m_ObjectKeyMap = new Dictionary<object, string>();

        private readonly List<string> m_CallbackTemp = new List<string>();


        private Dictionary<string, SpriteAtlas> _atlasCache = new Dictionary<string, SpriteAtlas>(); // 图集缓存

        
        
#if UNITY_EDITOR
        private bool isDirty;
#if ODIN_INSPECTOR
        [ShowInInspector, HideIf("showOdinInfo"), ListDrawerSettings(IsReadOnly = true, Expanded = true)]
#endif
        private List<AssetCache> InspectorShower = new List<AssetCache>();
#endif

        //释放接口
        public override void Release()
        {
            base.Release();

            m_LoadSceneOpration = null;
            m_AssetBundleLoadManifestOperation = null;
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;

            //停止所有的加载回调
            m_LoadedScenesAssetBundle.Clear();
            m_CallbackTemp.Clear();
            m_CallbackStack.Clear();  //清空回调
            m_AssetCaches.Clear();  //清空cache
            m_ObjectKeyMap.Clear();
            _atlasCache.Clear();
            StopAllLoadingProgress();
            //强制释放所有资源释放
            UnloadAllAssets();
            IsLoadingScene = false;
            IsInited = false;
#if UNITY_EDITOR
            StopCoroutine(nameof(EditorInitCallback));
            _startedCoroutine = false;
#endif
            //assetbundle 释放
            AssetBundles.AssetBundleManager.Release();
        }

        public void Initialize(OnLoadComplete callback = null, AssetBundles.AssetBundleManager.LoadMode loadMode = AssetBundles.AssetBundleManager.LoadMode.Internal, AssetBundles.AssetBundleManager.LogMode logMode = AssetBundles.AssetBundleManager.LogMode.JustErrors)
        {
            Initialize(null, null, callback, loadMode, logMode);
        }

        public void Initialize(string localAssetBundlePath, string remoteAssetBundlePath, OnLoadComplete callback, AssetBundles.AssetBundleManager.LoadMode loadMode = AssetBundles.AssetBundleManager.LoadMode.Internal, AssetBundles.AssetBundleManager.LogMode logMode = AssetBundles.AssetBundleManager.LogMode.JustErrors)
        {
            if (!AssetBundles.AssetBundleManager.IsInited)
            {
                //1.场景的加载/卸载管理
                SceneManager.sceneLoaded += OnSceneLoaded;
                SceneManager.sceneUnloaded += OnSceneUnloaded;

                //bundle的加载/卸载管理
                AssetBundles.AssetBundleManager.loadMode = loadMode;
                AssetBundles.AssetBundleManager.logMode = logMode;
                //设置本地资源加载路径，和远端资源加载路径
                AssetBundles.AssetBundleManager.SetLocalAssetBundleDirectory(localAssetBundlePath);
                AssetBundles.AssetBundleManager.SetRemoteAssetBundleURL(remoteAssetBundlePath);

                //todo 需要和热更逻辑配合
                // string file = Path.Combine(AssetBundleManager.BaseLocalURL, Utility.GetPlatformName());
                // if (File.Exists(file))
                // {
                //     File.Delete(file);
                // }

                //AssetBundleManager.ActiveVariants = new string[] { "bundle" };
                RegistCallback(Utility.GetPlatformName(), callback); //资源初始化完成的回调
                m_AssetBundleLoadManifestOperation = AssetBundles.AssetBundleManager.Initialize();
            }
            else
            {
                StartCoroutine(_YieldCallback(callback));
            }
        }

        IEnumerator _YieldCallback(OnLoadComplete callback)
        {
            yield return new WaitForEndOfFrame();
            callback(Utility.GetPlatformName(), AssetBundles.AssetBundleManager.AssetBundleManifestObject, null);
        }

        #region load scene
        public string LoadSceneAsync(string assetBundle, string sceneName, bool isAdditive, bool allowSceneActivation = true, OnLoadComplete callback = null)
        {
            if (string.IsNullOrEmpty(assetBundle))
                return string.Empty;

            assetBundle = assetBundle.ToLower();

            if (IsInLoading(sceneName))
            {
                RegistCallback(sceneName, callback);
                return sceneName;
            }

            RegistCallback(sceneName, callback);
            m_LoadSceneOpration = AssetBundles.AssetBundleManager.LoadLevelAsync(assetBundle, sceneName, isAdditive, allowSceneActivation);
            LoadingSceneProgress = 0;

            return sceneName;
        }

        public AsyncOperation UnloadScene(string levelName)
        {
            AsyncOperation ao = SceneManager.UnloadSceneAsync(levelName);

            return ao;
        }

        public string[] GetLoadedScenes()
        {
            string[] array = new string[m_LoadedScenesAssetBundle.Keys.Count];
            m_LoadedScenesAssetBundle.Keys.CopyTo(array, 0);
            return array;
        }

        public bool SceneIsLoaded(string sceneName)
        {
            return m_LoadedScenesAssetBundle.ContainsKey(sceneName);
        }
        #endregion

        #region load asset async
        /// <summary>
        /// Loads the asset async.
        /// </summary>
        /// <param name="assetBundle">Asset bundle.</param>
        /// <param name="assetName">Asset name.</param>
        /// <param name="callback">Callback.</param>
        /// <param name="preload">If set to <c>true</c> preload.</param>
        /// <param name="memeryHold">Memery hold.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public string LoadAssetAsync<T>(string assetBundle, string assetName = null, OnLoadComplete callback = null, bool preload = false, MemeryHold memeryHold = MemeryHold.Once) where T : Object
        {
            return LoadAssetAsync(assetBundle, assetName, typeof(T), callback, preload, memeryHold);
        }

        /// <summary>
        /// Loads the asset async.
        /// </summary>
        /// <param name="assetBundle">Asset bundle.</param>
        /// <param name="assetName">Asset name.</param>
        /// <param name="type">Type.</param>
        /// <param name="callback">Callback.</param>
        /// <param name="preload">If set to <c>true</c> preload asset and if <paramref name="memeryHold"/> is not "Always", ref count will no increase.</param>
        /// <param name="memeryHold">Memery hold.</param>
        public string LoadAssetAsync(string assetBundle, string assetName, System.Type type, OnLoadComplete callback = null, bool preload = false, MemeryHold memeryHold = MemeryHold.Once)
        {

            if (string.IsNullOrEmpty(assetBundle))
                return string.Empty;

            //Debug.Log(string.Format("LoadAssetAsync {0}, {1}", assetName, Time.frameCount));

            // if (type != typeof(UnityEngine.U2D.SpriteAtlas))
            //     assetBundle = assetBundle.ToLower();
            //根据项目 获得资源的主键，主要是处理特殊资源
            string key = AssetKey(assetBundle, assetName, type);

            // 查找或创建一个AssetCache
            AssetCache cache = GenAssetCache(key, assetBundle, assetName, type, memeryHold, preload);

            // 判断是否正在Loading
            if (IsInLoading(key))
            {
                // 如果同一个资源已经在Loading，不用再新开加载
                RegistCallback(key, callback);
                return key;
            }

            // 注册回调
            RegistCallback(key, callback);

            // 如果缓存区已经有资源
            if (cache.obj != null)
            {
                m_CallbackTemp.Add(key);
                return key;
            }

            AssetBundleLoadAssetOperation ao = AssetBundles.AssetBundleManager.LoadAssetAsync(assetBundle, assetName, type);

            if (ao != null)
            {
                m_InProgressOperations.Add(ao);
            }

            return key;
        }
        #endregion

        #region load asset sync

        /// <summary>
        /// Gets the cached asset.
        /// </summary>
        /// <returns>The cached asset.</returns>
        /// <param name="assetBundleName">Asset bundle name.</param>
        /// <param name="assetName">Asset name.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public T GetCachedAsset<T>(string assetBundleName, string assetName) where T : Object
        {
            string key = AssetKey(assetBundleName, assetName, typeof(T));

            object obj = GetCachedAsset(key);
            if (obj != null)
                return obj as T;
            else
                return null;
        }

        public object GetCachedAsset(string key)
        {
            if (m_AssetCaches.TryGetValue(key, out AssetCache cache))
            {
                if (cache.obj != null)
                {
#if UNITY_EDITOR
                    isDirty = true;
#endif
                    cache.refCount++;
                    return cache.obj;
                }
            }

            return null;
        }

        /// <summary>
        /// 同步加载接口
        /// </summary>
        public object LoadAssetSync(string assetBundleName, string assetName, Type type)
        {
            string key = AssetKey(assetBundleName, assetName, type);
            object obj = GetCachedAsset(key); // 先从Cache中取，取到了就自动++引用计数

            if (obj == null) // 如果Cache中取不到
            {
                obj = AssetBundles.AssetBundleManager.LoadAssetSync(assetBundleName, assetName, type); // 说明需要加载资源

                if (obj != null) // 如果加载完资源不为空
                {
                    GenAssetCache(key, assetBundleName, assetName, type, 0, false); // 创建Cache，引用计数为1
                    AssetCache cache = CacheAsset(key, obj); // 添加缓存
                    cache.assetbundleVariant = assetBundleName; // 同步加载无需等待，直接赋assetbundleVariant // TODO 目前AB包没使变体，为了美观直接设置了。
                }
            }
            return obj;
        }

        /// <summary>
        /// 同步加载接口
        /// </summary>
        private object LoadAnimatorControllerAssetSync(string assetBundleName, string assetName, Type type)
        {
            string key = AssetKey(assetBundleName, assetName, type);
            object obj = GetCachedAsset(key); // 先从Cache中取，取到了就自动++引用计数
            if (obj == null) // 如果Cache中取不到
            {
                obj = AssetBundles.AssetBundleManager.LoadAssetAniSync(assetBundleName, assetName); // 说明需要加载资源

                if (obj != null) // 如果加载完资源不为空
                {
                    GenAssetCache(key, assetBundleName, assetName, type, 0, false); // 创建Cache，引用计数为1
                    CacheAsset(key, obj); // 添加缓存
                }
            }
            return obj;
        }

        #endregion

        [System.Obsolete]
        public bool HasCachedAsset(object obj)
        {
            return m_ObjectKeyMap.ContainsKey(obj);
        }

        #region unload assets
        /// <summary>
        /// Unloads the asset.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <param name="immediately">引用计数为0时是否直接移除cache.</param>
        /// <param name="directly">是否不管引用计数直接删除.</param>
        public void UnloadAssetWithKey(string key, bool immediately = false, bool directly = false)
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError("key is null");
                return;
            }

            AssetCache cache = UnloadCachedAsset(key); //减少计数
            if (cache != null)
            {
                Debug.Log("Unload Asset: " + key + ", after -1 then count: " + cache.refCount + ". (immediately = " + immediately + ", directly = " + directly + ")");
            }

            if (cache != null && ((cache.refCount == 0 && cache.hold != MemeryHold.Always) || directly))
            {
                if (applicationIsQuitting || immediately)
                    RemoveCachedAsset(key);
                else
                    RemoveCachedAssetDelay(key, RemoveAssetDelay);
            }
        }

        /// <summary>
        /// Unloads the unused assets.
        /// </summary>
        public void UnloadUnusedAssets(MemeryHold hold = MemeryHold.Once)
        {
            List<string> tempList = new List<string>();
            Dictionary<string, AssetCache>.Enumerator e = m_AssetCaches.GetEnumerator();
            while (e.MoveNext())
            {
                if (e.Current.Value.hold <= hold && e.Current.Value.refCount == 0)
                {
                    tempList.Add(e.Current.Key);
                }
            }
            for (int i = 0; i < tempList.Count; ++i)
            {
                // 立即移除
                RemoveCachedAsset(tempList[i]);
            }
            tempList.Clear();
            tempList = null;

            System.GC.Collect();
        }

        /// <summary>
        /// 强制卸载所有资源
        /// </summary>
        /// <param name="hold">卸载所有小于等于hold的资源</param>
        public void ForceUnloadAssets(MemeryHold hold = MemeryHold.Once)
        {
            List<string> tempList = new List<string>();
            Dictionary<string, AssetCache>.Enumerator e = m_AssetCaches.GetEnumerator();
            while (e.MoveNext())
            {
                if (e.Current.Value.hold <= hold)
                {
                    tempList.Add(e.Current.Key);
                }
            }
            for (int i = 0; i < tempList.Count; ++i)
            {
                // 立即移除
                RemoveCachedAsset(tempList[i]);
            }
            tempList.Clear();
            tempList = null;

            System.GC.Collect();
        }
        #endregion

        public void StopAllLoadingProgress()
        {
            AssetBundleLoadAssetOperation operation = null;
            for (int i = 0; i < m_InProgressOperations.Count; i++)
            {
                operation = m_InProgressOperations[i];
                string key = AssetKey(operation.assetBundleName, operation.assetName, operation.type);
                UnregistCallback(key);
            }
        }
        
#if UNITY_EDITOR
        private bool _startedCoroutine = false;
        private IEnumerator EditorInitCallback()
        {
            yield return new WaitForSeconds(1f);
            IsInited = true;
            Callback(Utility.GetPlatformName());
        }
#endif
        
        protected override void OnUpdate(float delta)
        {
            base.OnUpdate(delta);

            if (!IsInited)
            {
                // 等AssetBundleManifest初始化完成，再继续更新其他Operation
#if UNITY_EDITOR
                if (AssetBundles.AssetBundleManager.SimulateAssetBundleInEditor && !_startedCoroutine)
                {
                    StopCoroutine(nameof(EditorInitCallback));
                    StartCoroutine(nameof(EditorInitCallback));
                    _startedCoroutine = true;
                }
                else
                {
#endif
                    if (m_AssetBundleLoadManifestOperation != null && m_AssetBundleLoadManifestOperation.IsDone())
                    {
                        IsInited = true;
                        Callback(Utility.GetPlatformName(), m_AssetBundleLoadManifestOperation.GetAsset<AssetBundleManifest>(), m_AssetBundleLoadManifestOperation.Error);
                        m_AssetBundleLoadManifestOperation = null;
                    }
#if UNITY_EDITOR
                }
#endif
            }
            else
            {

                //bundle
                UpdateLoadAssets();

                //场景
                if (m_LoadSceneOpration != null)
                {
                    IsLoadingScene = true;
                    LoadingSceneProgress = m_LoadSceneOpration.Progress();

                    if (m_LoadSceneOpration.IsDone() && !string.IsNullOrEmpty(m_LoadSceneOpration.Error))
                    {
                        IsLoadingScene = false;
                        Callback(m_LoadSceneOpration.LevelName, null, m_LoadSceneOpration.Error);
                        m_LoadSceneOpration = null;
                    }
                }
            }

#if UNITY_EDITOR
            if (isDirty)
            {
                isDirty = false;
                InspectorShower.Clear();
                InspectorShower.AddRange(m_AssetCaches.Values);
                InspectorShower.Sort(
                    (a, b) =>
                    {
                        if (a.hold != b.hold)
                            return a.hold - b.hold;
                        else if (a.refCount != b.refCount)
                            return b.refCount - a.refCount;
                        else
                            return string.Compare(a.assetName, b.assetName);
                    });
            }
#endif
        }

        private void UpdateLoadAssets()
        {
            AssetBundleLoadAssetOperation operation = null;
            for (int i = 0; i < m_InProgressOperations.Count;)
            {
                operation = m_InProgressOperations[i];
                if (operation.IsDone())
                {
                    // 如果完成直接移除，避免后面回调方法中上层逻辑错误导致不停Update
                    m_InProgressOperations.RemoveAt(i);
                    string key = AssetKey(operation.assetBundleName, operation.assetName, operation.type);

                    if (string.IsNullOrEmpty(operation.Error))
                    {
                        AssetCache asset = CacheAsset(key, operation.GetAsset());
                        if (asset != null)
                        {
                            asset.assetbundleVariant = operation.assetBundleVariant;

                            m_CallbackTemp.Add(key);
                        }
                        else
                        {
                            AssetBundles.AssetBundleManager.UnloadAssetBundle(operation.assetBundleVariant);
                            Callback(key, null, string.Format("[{0} no asset error!]", key));
                        }
                    }
                    else
                    {
                        if (m_AssetCaches.TryGetValue(key, out AssetCache cache))
                        {
                            cache.assetbundleVariant = operation.assetBundleVariant;
                        }

                        RemoveCachedAsset(key);
                        Callback(key, null, operation.Error);
                    }
                }
                else
                {
                    i++;
                }
            }

            //todo 同一帧可能返回多个，这里还是需要保护一下，
            while (m_CallbackTemp.Count > 0)
            {
                string key = m_CallbackTemp[0];

                if (m_AssetCaches.TryGetValue(key, out AssetCache cache))
                {
                    if (cache != null && cache.obj != null)
                    {
                        Callback(key, cache.obj, string.Empty);
                    }
                    else
                    {
                        Debug.LogErrorFormat("{0} is Null", key);
                    }
                }

                m_CallbackTemp.RemoveAt(0);
            }
        }

        private void Callback(string key, object t = null, string err = null)
        {
            if (!string.IsNullOrEmpty(err))
                Debug.Log("<color=#FFB6C1>" + err + "</color>");

            if (m_CallbackStack.TryGetValue(key, out List<System.Delegate> callbackList))
            {
                m_CallbackStack.Remove(key);
                foreach (OnLoadComplete callback in callbackList)
                {
                    try
                    {
                        if (callback != null)
                            callback.Invoke(key, t, err);
                        else
                            UnloadAssetWithKey(key);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError(e);
                    }
                }
            }
            else
            {
                RemoveCachedAsset(key);
            }
        }

        private void RegistCallback(string key, System.Delegate callback)
        {
            if (callback != null && !string.IsNullOrEmpty(key))
            {
                if (!m_CallbackStack.ContainsKey(key))
                    m_CallbackStack.Add(key, new List<System.Delegate>());
                if (!m_CallbackStack[key].Contains(callback))
                    m_CallbackStack[key].Add(callback);
            }
        }

        private void UnregistCallback(string key)
        {
            if (m_CallbackStack.TryGetValue(key, out List<System.Delegate> callbackList))
            {
                m_CallbackStack.Remove(key);
            }
        }

        private bool IsInLoading(string key)
        {
            return m_CallbackStack.ContainsKey(key);
        }

        private void RemoveCachedAssetDelay(string key, float delay)
        {
            StartCoroutine(_RemoveCachedAssetDelay(key, delay));
        }

        private IEnumerator _RemoveCachedAssetDelay(string key, float delay)
        {
            yield return new WaitForSeconds(delay);

            if (m_AssetCaches.TryGetValue(key, out AssetCache cache))
            {
                // 避免延迟期间，又被重新引用
                if (cache.refCount <= 0)
                {
                    if (cache.obj != null)
                        m_ObjectKeyMap.Remove(cache.obj);
                    cache.Release();
                    m_AssetCaches.Remove(key);
#if UNITY_EDITOR
                    isDirty = true;
#endif
                }
            }
        }

        private AssetCache RemoveCachedAsset(string key)
        {
            if (m_AssetCaches.TryGetValue(key, out AssetCache cache))
            {
                if (cache.obj != null)
                    m_ObjectKeyMap.Remove(cache.obj);
                cache.Release();
                m_AssetCaches.Remove(key);
#if UNITY_EDITOR
                isDirty = true;
#endif
            }

            return cache;
        }

        private AssetCache UnloadCachedAsset(string key)
        {
            if (m_AssetCaches.TryGetValue(key, out AssetCache cache))
            {
                cache.refCount--;
#if UNITY_EDITOR
                isDirty = true;
#endif
            }

            return cache;
        }

        private AssetCache CacheAsset(string key, object obj)
        {
            if (m_AssetCaches.TryGetValue(key, out AssetCache cache))
            {
                cache.obj = obj;
                if (m_ObjectKeyMap.ContainsKey(obj))
                {
                    if (!m_ObjectKeyMap[obj].Equals(key))
                        Debug.LogErrorFormat("{0} and {1} has same object", m_ObjectKeyMap[obj], key);
                }
                else
                    m_ObjectKeyMap.Add(obj, key);

                // 处理Preload的情况下，自动释放
                if (cache.refCount == 0 && cache.hold != MemeryHold.Always)
                    RemoveCachedAssetDelay(key, RemoveAssetDelay);

#if UNITY_EDITOR
                isDirty = true;
#endif
            }
            else
            {
                Debug.LogWarningFormat("There is no asset cache with key : {0}, so {1} can not be cached.", key, ((Object)obj).name);
            }

            return cache;
        }

        /// <summary>
        /// 查找或创建 AssetCache
        /// </summary>
        /// <param name="key"></param>
        /// <param name="assetBundle"></param>
        /// <param name="assetName"></param>
        /// <param name="type"></param>
        /// <param name="hold"></param>
        /// <param name="preload"></param>
        /// <returns></returns>
        private AssetCache GenAssetCache(string key, string assetBundle, string assetName, System.Type type, MemeryHold hold, bool preload = false)
        {
            if (m_AssetCaches.TryGetValue(key, out AssetCache cache))
            {
                Debug.Assert(assetBundle == cache.assetbundleName && assetName == cache.assetName && type == cache.type, "Cached Asset is not same.");

                if (cache.hold < hold)
                    cache.hold = hold;

                if (!preload)
                    cache.refCount++;
            }
            else
            {
                cache = new AssetCache
                {
                    assetName = assetName,
                    assetbundleName = assetBundle,
                    type = type,
                    hold = hold,
                    refCount = preload ? 0 : 1,
                };

                m_AssetCaches.Add(key, cache);
            }

#if UNITY_EDITOR
            isDirty = true;
#endif

            return cache;
        }

        #region SceneManager callback
        private LoadSceneMode m_LoadSceneMode;
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            m_LoadSceneMode = mode;

            if (m_LoadSceneOpration != null)
            {
                m_LoadedScenesAssetBundle[scene.name] = m_LoadSceneOpration.AssetBundle;
            }

            if (mode == LoadSceneMode.Single)
            {
                IsLoadingScene = false;
                LoadingSceneProgress = 1;
                Callback(scene.name);
                m_LoadSceneOpration = null;
            }
            else if (mode == LoadSceneMode.Additive)
            {
                IsLoadingScene = false;
                LoadingSceneProgress = 1;
                Callback(scene.name);
                m_LoadSceneOpration = null;
            }
        }

        private void OnSceneUnloaded(Scene scene)
        {
            if (m_LoadedScenesAssetBundle.ContainsKey(scene.name))
            {
                AssetBundles.AssetBundleManager.UnloadAssetBundle(m_LoadedScenesAssetBundle[scene.name]);
                m_LoadedScenesAssetBundle.Remove(scene.name);
            }
        }
        #endregion

        public int GetInProgressCount()
        {
            return m_InProgressOperations.Count;
        }


        #region static methods

        public static string AssetKey(string assetBundle, string assetName, System.Type type)
        {
            if (string.IsNullOrEmpty(assetBundle))
            {
                Debug.LogError("AssetBundleName is Null");
                return string.Empty;
            }

            /*if (type == typeof(UnityEngine.U2D.SpriteAtlas))
            {
                return assetBundle;
            }*/

            assetBundle = assetBundle.ToLower();

            return string.Format(AssetKeyFormat, assetName ?? "", assetBundle, type);
        }

        public static void ParseAssetKey(string key, out string assetName, out string assetBundleName, out System.Type type)
        {
            assetName = "";
            assetBundleName = "";
            type = null;

            if (string.IsNullOrEmpty(key))
                return;

            string[] split = key.Split('@');
            if (split.Length < 3)
                return;

            assetName = split[0];
            assetBundleName = split[1];
            type = System.Type.GetType(split[2]);
        }
        #endregion

        /// <summary>
        /// 加载AB包
        /// </summary>
        /// <param name="assetBundleName">AB包名</param>
        /// <param name="isLoadingAssetBundleManifest"></param>
        /// <param name="callback">回调方法</param>
        public void LoadAssetBundle(string assetBundleName, OnLoadComplete callback = null, bool isLoadingAssetBundleManifest = false)
        {
            AssetBundles.AssetBundleManager.LoadAssetBundle(assetBundleName, isLoadingAssetBundleManifest, callback);
        }

        /// <summary>
        /// 卸载AB包
        /// </summary>
        public void UnloadAssetBundle(string assetBundleName)
        {
            AssetBundles.AssetBundleManager.UnloadAssetBundle(assetBundleName);
        }

        /// <summary>
        /// 加载AB包内的资源
        /// </summary>
        public string LoadAsset(string assetBundle, string type, string assetName = null, OnLoadComplete callback = null, bool preload = false, int memeryHold = 0)
        {
            return LoadAssetAsync(assetBundle, assetName, System.Reflection.Assembly.Load("UnityEngine").GetType(type), callback, preload, (MemeryHold)memeryHold);
        }

        /// <summary>
        /// 卸载AB包内的资源
        /// </summary>
        public void UnloadAsset(string key, bool immediately = false, bool directly = false)
        {
            UnloadAssetWithKey(key, immediately, directly);
        }

        /// <summary>
        /// 卸载所有缓存中的资源
        /// </summary>
        public void UnloadAllAssets()
        {
            ForceUnloadAssets(MemeryHold.Always);
        }

        
        
        
        
        
        /// <summary>
        /// 获得GameObject物体
        /// </summary>
        public GameObject GetGameObjectAsset(string bundleName, string assetName)
        {
            return LoadAssetSync(bundleName, assetName, typeof(GameObject)) as GameObject;
        }

        /// <summary>
        /// 获得GameObject物体
        /// </summary>
        public GameObject GetGameObjectAsset(Dictionary<string, string> resInfo)
        {
            resInfo.TryGetValue("bundleName", out string bundleName);
            resInfo.TryGetValue("assetName", out string assetName);

            return LoadAssetSync(bundleName, assetName, typeof(GameObject)) as GameObject;
        }
        /// <summary>
        /// 获得AnimatorController物体
        /// </summary>
        public object GetAnimatorControllerAsset(Dictionary<string, string> resInfo)
        {
            resInfo.TryGetValue("bundleName", out string bundleName);
            resInfo.TryGetValue("assetName", out string assetName);

            return LoadAnimatorControllerAssetSync(bundleName, assetName, typeof(RuntimeAnimatorController));
        }

        /// <summary>
        /// 获得Sprite物体
        /// </summary>
        public Sprite GetSpriteAsset(Dictionary<string, string> resInfo, string spriteName)
        {
            resInfo.TryGetValue("bundleName", out string bundleName);
            resInfo.TryGetValue("assetName", out string assetName);

            return (LoadAssetSync(bundleName, assetName, typeof(SpriteAtlas)) as SpriteAtlas)?.GetSprite(spriteName);
        }
    
        /// <summary>
        /// 设置Sprite
        /// </summary>
        /// <returns>成功返回true, 失败返回false</returns>
        public bool SetSprite(Image image, string bundleName, string atlasName, string spriteName)
        {
            if (image == null)
            {
                return false;
            }

            if (string.IsNullOrEmpty(bundleName) || string.IsNullOrEmpty(atlasName) || string.IsNullOrEmpty(spriteName))
            {
                return false;
            }

            string atlasKey = string.Format($"{bundleName}_{atlasName}");
            Sprite spriteAsset = null;
            if (_atlasCache.TryGetValue(atlasKey, out SpriteAtlas spriteAtlas))
            {
                if (spriteAtlas != null)
                {
                    spriteAsset = spriteAtlas.GetSprite(spriteName);
                }
            }
            else
            {
                spriteAtlas = LoadAssetSync(bundleName, atlasName, typeof(SpriteAtlas)) as SpriteAtlas;
                if (spriteAtlas != null)
                {
                    spriteAsset = spriteAtlas.GetSprite(spriteName);
                    _atlasCache.Add(atlasKey, spriteAtlas);
                }
            }

            if (spriteAsset == null)
            {
                return false;
            }
        
            image.sprite = spriteAsset;
            return true;
        }
        
        /// <summary>
        /// 获得Sprite物体
        /// </summary>
        public Texture GetTextureAsset(string bundleName, string assetName)
        {
            return LoadAssetSync(bundleName, assetName, typeof(Texture)) as Texture;
        }

        /// <summary>
        /// 获得AudioClip物体
        /// </summary>
        public AudioClip GetAudioClipAsset(Dictionary<string, string> resInfo)
        {
            resInfo.TryGetValue("bundleName", out string bundleName);
            resInfo.TryGetValue("assetName", out string assetName);

            return LoadAssetSync(bundleName, assetName, typeof(AudioClip)) as AudioClip;
        }

    }
}
