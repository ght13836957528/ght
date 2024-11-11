#if !FINAL_RELEASE
#if !UNITY_EDITOR
//#define WRITE_BUNDLE_LOG
#endif
#endif

using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;

#if UNITY_EDITOR
using UnityEditor;
#endif
using Object = UnityEngine.Object;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

/*
 	In this demo, we demonstrate:
	1.	Automatic asset bundle dependency resolving & loading.
		It shows how to use the manifest assetbundle like how to get the dependencies etc.
	2.	Automatic unloading of asset bundles (When an asset bundle or a dependency thereof is no longer needed, the asset bundle is unloaded)
	3.	Editor simulation. A bool defines if we load asset bundles from the project or are actually using asset bundles(doesn't work with assetbundle variants for now.)
		With this, you can player in editor mode without actually building the assetBundles.
	4.	Optional setup where to download all asset bundles
	5.	Build pipeline build postprocessor, integration so that building a player builds the asset bundles and puts them into the player data (Default implmenetation for loading assetbundles from disk on any platform)
	6.	Use WWW.LoadFromCacheOrDownload and feed 128 bit hash to it when downloading via web
		You can get the hash from the manifest assetbundle.
	7.	AssetBundle variants. A prioritized list of variants that should be used if the asset bundle with that variant exists, first variant in the list is the most preferred etc.
*/

namespace AssetBundles
{
    // Loaded assetBundle contains the references count which can be used to unload dependent assetBundles automatically.
    [System.Serializable]
    public class LoadedAssetBundle
    {
        public string name;
        public AssetBundle m_AssetBundle;
        public int m_ReferencedCount;

        public LoadedAssetBundle(AssetBundle assetBundle)
        {
            name = assetBundle != null ? assetBundle.name : "";
            m_AssetBundle = assetBundle;
            m_ReferencedCount = 1;

            if (string.IsNullOrEmpty(name))
                name = Framework.Utility.GetPlatformName();
        }

        public LoadedAssetBundle(AssetBundle assetBundle, int referencedCount)
        {
            name = assetBundle.name;
            m_AssetBundle = assetBundle;
            m_ReferencedCount = referencedCount;
        }
    }

    public class ABCreateRequest
    {
        public float startTime;
        public AssetBundleCreateRequest request;
    }

    // Class takes care of loading assetBundle and its dependencies automatically, loading variants automatically.
    public class AssetBundleManager : MonoBehaviour
    {
#if ODIN_INSPECTOR
        [SerializeField]
        private bool showOdinInfo;
#endif
        public enum LogMode { All, JustErrors };
        public enum LoadMode { Local, Remote, LocalFirst, RemoteFirst };
        
        private const string BUNDLE_EXTENSION = ".bundle";
        private const string LOADSTR = "Loading [{0}][{1}] from assetbundle [{2}] at realtime since startup [{3}]";
        private const string UNLOADSTR = "Unloading assetbundle reference [{0}], now ref count [{1}]";
        private const string LOADREFSTR = "loading assetbundle reference [{0}], now ref count [{1}]";

        private static AssetBundleManifest m_AssetBundleManifest = null;

#if UNITY_EDITOR
        private static int m_SimulateAssetBundleInEditor = -1;
        private const string kSimulateAssetBundles = "SimulateAssetBundles";
        public static List<string> m_SimulateAssetBundleList = new List<string>();
#endif

        private const int BUNDLE_CAPACITY = 4096;
        private static StringBuilder s_StringBuilder = new StringBuilder(256);
        private static Dictionary<string, string> m_bundleNameRemapDict = new Dictionary<string, string>(BUNDLE_CAPACITY);
#if ODIN_INSPECTOR
        [ShowInInspector, ShowIf("showOdinInfo"), DictionaryDrawerSettings(IsReadOnly = true, DisplayMode = DictionaryDisplayOptions.Foldout)]
#endif
        public static Dictionary<string, LoadedAssetBundle> m_LoadedAssetBundles = new Dictionary<string, LoadedAssetBundle>(BUNDLE_CAPACITY);
        private static Dictionary<string, UnityWebRequestAsyncOperation> m_UnityWebRequests = new Dictionary<string, UnityWebRequestAsyncOperation>(BUNDLE_CAPACITY);
        private static Dictionary<string, ABCreateRequest> m_CreatingAssetBundles = new Dictionary<string, ABCreateRequest>(BUNDLE_CAPACITY);

        private static Dictionary<string, string> m_DownloadingErrors = new Dictionary<string, string>();
        private static List<AssetBundleLoadOperation> m_InProgressOperations = new List<AssetBundleLoadOperation>(128);
        private static Dictionary<string, string[]> m_Dependencies = new Dictionary<string, string[]>(BUNDLE_CAPACITY);

        private static Dictionary<string, int> m_LocalLoadingReferenceCount = new Dictionary<string, int>(BUNDLE_CAPACITY);
        private static Dictionary<string, int> m_RemoteLoadingReferencedCount = new Dictionary<string, int>(BUNDLE_CAPACITY);

        private static HashSet<string> m_AllAssetBundlesWithVariant = new HashSet<string>();
        
        private static Dictionary<string, string> m_LocalUrlDict = new Dictionary<string, string>(BUNDLE_CAPACITY);
        private static Dictionary<string, string> m_RemoteUrlDict = new Dictionary<string, string>(BUNDLE_CAPACITY);
        

        public static LoadMode loadMode { get; set; } = LoadMode.Local;

        public static LogMode logMode { get; set; } = LogMode.All;

        // The base downloading url which is used to generate the full downloading url with the assetBundle names.
        public static string BaseDownloadingURL { get; set; } = "";

        public static string BaseLocalURL { get; set; } = "";


        // for GC
        public List<string> keysToRemove = new List<string>();

        // AssetBundleManifest object which can be used to load the dependecies and check suitable assetBundle variants.
        public static AssetBundleManifest AssetBundleManifestObject
        {
            get { return m_AssetBundleManifest; }
            set
            {
                if (m_AssetBundleManifest != null)
                    UnloadAssetBundle(Framework.Utility.GetPlatformName());

                m_AssetBundleManifest = value;

                string[] bundlesWithVariant = m_AssetBundleManifest.GetAllAssetBundlesWithVariant();

                InitAllAssetBundleWithVariants(bundlesWithVariant);
            }
        }

#if UNITY_EDITOR
        // Flag to indicate if we want to simulate assetBundles in Editor without building them actually.
        public static bool SimulateAssetBundleInEditor
        {
            get
            {
                if (m_SimulateAssetBundleInEditor == -1)
                    m_SimulateAssetBundleInEditor = EditorPrefs.GetBool(kSimulateAssetBundles, true) ? 1 : 0;

                return m_SimulateAssetBundleInEditor != 0;
            }
            set
            {
                int newValue = value ? 1 : 0;
                if (newValue != m_SimulateAssetBundleInEditor)
                {
                    m_SimulateAssetBundleInEditor = newValue;
                    EditorPrefs.SetBool(kSimulateAssetBundles, value);
                }
            }
        }
        
        //private static List<Renderer> results = new List<Renderer>(64);
        //private static List<Image> imageResults = new List<Image>(64);

        // 对ab中的材质做统一处理
        //private static void ResetAllMaterials(AssetBundle bundle)
        //{
        //    var materials = bundle.LoadAllAssets<Material>();
        //    foreach (Material m in materials)
        //    {
        //        var shaderName = m.shader.name;
        //        if(shaderName == "Hidden/InternalErrorShader")
        //            continue;
        //        var newShader = ShaderManager.Instance.Find(shaderName);
        //        if (newShader != null)
        //        {
        //            m.shader = newShader;
        //        }
        //        else
        //        {
        //            Debug.LogWarning("unable to refresh shader: " + shaderName + " in material " + m.name);
        //        }
        //    }

        //    var gameObjects = bundle.LoadAllAssets<GameObject>();
        //    foreach (var go in gameObjects)
        //    {
        //        results.Clear();
        //        go.GetComponentsInChildren<Renderer>(true, results);
        //        if (results.Count > 0)
        //        {
        //            for (int ii = 0; ii < results.Count; ii++)
        //            {
        //                for (int k = 0; k < results[ii].sharedMaterials.Length; ++k)
        //                {
        //                    var m = results[ii].sharedMaterials[k];
        //                    ShaderManager.Instance.UseEditorShader(m);
        //                }

        //                if (results[ii] is ParticleSystemRenderer particleRender)
        //                {
        //                    ShaderManager.Instance.UseEditorShader(particleRender.sharedMaterial);
        //                    ShaderManager.Instance.UseEditorShader(particleRender.trailMaterial);
        //                }
        //            }
        //        }
        //        imageResults.Clear();
        //        go.GetComponentsInChildren<Image>(true,imageResults);
        //        if (imageResults.Count > 0)
        //        {
        //            for (int ii = 0; ii < imageResults.Count; ii++)
        //            {
        //                ShaderManager.Instance.UseEditorShader(imageResults[ii].material);
        //            }
        //        }
        //    }
            
        //}

        

        // 对某一个GameObject对象下进行材质的替换

        //public static void resetEditorShader(GameObject go)
        //{
        //    if (go == null)
        //    {
        //        return;
        //    }

        //    // 把所有的renderer遍历出来，每个材质的shader重新定位一下。
        //    results.Clear();
        //    go.GetComponentsInChildren<Renderer>(true, results);
        //    if (results.Count > 0)
        //    {
        //        for (int ii = 0; ii < results.Count; ii++)
        //        {
        //            for (int k = 0; k < results[ii].sharedMaterials.Length; ++k)
        //            {
        //                var m = results[ii].sharedMaterials[k];
        //                ShaderManager.Instance.UseEditorShader(m);
        //            }
        //        }
        //    }
        //    // imageResults.Clear();
        //    // go.GetComponentsInChildren<Image>(true,imageResults);
        //    // if (imageResults.Count > 0)
        //    // {
        //    //     for (int ii = 0; ii < imageResults.Count; ii++)
        //    //     {
        //    //         ShaderManager.Instance.UseEditorShader(imageResults[ii].material);
        //    //     }
        //    // }
            
        //}
#endif

        public static void SetLocalAssetBundleDirectory(string path)
        {
            BaseLocalURL = string.IsNullOrEmpty(path) ? Framework.Utility.GetStreamingAssetsDirectory() : path;
            Debug.Log($"set local assetbundle directory : {BaseLocalURL}");
        }

        public static void SetRemoteAssetBundleURL(string url)
        {
            BaseDownloadingURL = string.IsNullOrEmpty(url) ? Application.temporaryCachePath : url;
            Debug.Log($"set remote assetbundle directory : {BaseDownloadingURL}");
        }

   
        /// <summary>
        /// has the asset bundle asset.
        /// </summary>
        /// <returns><c>true</c>, if asset bundle asset was hased, <c>false</c> otherwise.</returns>
        /// <param name="assetBundleName">Asset bundle name with variant.</param>
        public static bool HasAssetBundleAsset(string assetBundleName)
        {
            return m_AllAssetBundlesWithVariant.Contains(assetBundleName);
        }



        // Get loaded AssetBundle, only return valid object when all the dependencies are downloaded successfully.
        internal static LoadedAssetBundle GetLoadedAssetBundle(string assetBundleName, out string error)
        {
            if (m_DownloadingErrors.TryGetValue(assetBundleName, out error))
                return null;

            m_LoadedAssetBundles.TryGetValue(assetBundleName, out LoadedAssetBundle bundle);
            if (bundle == null)
                return null;

            // No dependencies are recorded, only the bundle itself is required.
            if (!m_Dependencies.TryGetValue(assetBundleName, out string[] dependencies))
                return bundle;

            // 这里设置一个全局标记即可
            // 如果有错误的话，那么就记录一下；但是如果有没加载完的，必须要先等到加载
            bool hasError = false;
            string strError = null;

            // Make sure all dependencies are loaded
            foreach (var dependency in dependencies)
            {
                // Wait all the dependent assetBundles being loaded.
                m_LoadedAssetBundles.TryGetValue(dependency, out LoadedAssetBundle dependentBundle);
                if (dependentBundle == null)
                {
                    if (m_DownloadingErrors.TryGetValue(dependency, out strError))
                    {
                        //return bundle;
                        hasError = true;
                    }
                    else
                    {
                        return null;
                    }
                }
            }

            if (hasError)
            {
                error = strError;
            }

            return bundle;
        }

        public static AssetBundleLoadManifestOperation Initialize()
        {
            return Initialize(Framework.Utility.GetPlatformName());
        }

        public static bool IsInited = false;
        // Load AssetBundleManifest.
        private static AssetBundleLoadManifestOperation Initialize(string manifestAssetBundleName)
        {
            IsInited = true;
            
            AssetBundleManager mgr = FindObjectOfType<AssetBundleManager>();
            if (mgr == null)
                mgr = new GameObject("AssetBundleManager").AddComponent<AssetBundleManager>();
            DontDestroyOnLoad(mgr.gameObject);
            
#if UNITY_EDITOR
           Debug.Log("Simulation Mode: " + (SimulateAssetBundleInEditor ? "Enabled" : "Disabled"));
            
            // If we're in Editor simulation mode, we don't need the manifest assetBundle.
            if (SimulateAssetBundleInEditor)
            {
                InitAllAssetBundleWithVariants(AssetDatabase.GetAllAssetBundleNames());
                return null;
            }
#endif

            LoadAssetBundle(manifestAssetBundleName, true);
            var operation = new AssetBundleLoadManifestOperation(manifestAssetBundleName, "AssetBundleManifest", typeof(AssetBundleManifest));
            m_InProgressOperations.Add(operation);
            return operation;
        }

        static void InitAllAssetBundleWithVariants(string[] bundlesWithVariant)
        {
            m_AllAssetBundlesWithVariant.Clear();

            // 保存的时候，集体去掉bundle吧。
            for (int i = 0; i < bundlesWithVariant.Length; i++)
            {
                int idx = bundlesWithVariant[i].LastIndexOf('.');
                if (idx >= 0)
                {
                    m_AllAssetBundlesWithVariant.Add(bundlesWithVariant[i].Substring(0, idx));
                }
                else
                {
                    m_AllAssetBundlesWithVariant.Add(bundlesWithVariant[i]);
                }
            }

        }

        // Load AssetBundle and its dependencies.
        private static void LoadAssetBundle(string assetBundleName, bool isLoadingAssetBundleManifest = false)
        {
#if UNITY_EDITOR
            Debug.Log("Loading Asset Bundle " + (isLoadingAssetBundleManifest ? "Manifest: " : ": ") + assetBundleName);
            
            // If we're in Editor simulation mode, we don't have to really load the assetBundle and its dependencies.
            if (SimulateAssetBundleInEditor)
                return;
#endif

            if (!isLoadingAssetBundleManifest)
            {
                if (m_AssetBundleManifest == null)
                {
                    Debug.LogError("Please initialize AssetBundleManifest by calling AssetBundleManager.Initialize()");
                    return;
                }
            }

            // Check if the assetBundle has already been processed.
            bool isAlreadyProcessed = LoadAssetBundleInternal(assetBundleName, isLoadingAssetBundleManifest, loadMode);

            // Load dependencies.
            if (!isAlreadyProcessed && !isLoadingAssetBundleManifest)
                LoadDependencies(assetBundleName);
        }

        public static string RemapVariantName(string assetBundleName)
        {
            if (string.IsNullOrEmpty(assetBundleName))
            {
                Debug.LogError("assetbundle name is null or empty");
                return "";
            }
            
            if (m_bundleNameRemapDict.TryGetValue(assetBundleName, out var tpName))
            {
                return tpName;
            }

            if (assetBundleName.EndsWith(BUNDLE_EXTENSION))
            {
                m_bundleNameRemapDict[assetBundleName] = assetBundleName;
                return assetBundleName;
            }

            var extension = Path.GetExtension(assetBundleName);
            if (!string.IsNullOrEmpty(extension))
            {
                assetBundleName = assetBundleName.Replace(extension, "");
            }
            s_StringBuilder.Clear();
            s_StringBuilder.Append(assetBundleName);
            s_StringBuilder.Append(BUNDLE_EXTENSION);
            tpName = s_StringBuilder.ToString();
            m_bundleNameRemapDict[assetBundleName] = tpName;
            return tpName;
        }

        /// <summary>
        /// Loads the asset bundle internal. Where we actuall call WWW to download the assetBundle.
        /// </summary>
        /// <returns><c>true</c>, if asset bundle internal was already processed, <c>false</c> otherwise.</returns>
        /// <param name="assetBundleName">Asset bundle name.</param>
        /// <param name="isLoadingAssetBundleManifest">If set to <c>true</c> is loading asset bundle manifest.</param>
        private static bool LoadAssetBundleInternal(string assetBundleName, bool isLoadingAssetBundleManifest, LoadMode mode, int refCount = 1)
        {
            // Already loaded.
            if (m_LoadedAssetBundles.TryGetValue(assetBundleName, out LoadedAssetBundle bundle))
            {
                bundle.m_ReferencedCount += refCount;
#if UNITY_EDITOR
                Debug.LogFormat(LOADREFSTR, assetBundleName, bundle.m_ReferencedCount);
#endif
                // fix: 自己的引用计数增加还不行，所有的依赖项引用计数也要增加，要不会出现显示的时候丢失资源的问题
                string[] dependencies = m_AssetBundleManifest.GetAllDependencies(assetBundleName);
                for (int i = 0; i < dependencies.Length; i++)
                {
                    var dependency = dependencies[i];
                    if (!string.IsNullOrEmpty(dependency))
                    {
                        dependencies[i] = RemapVariantName(dependency);
                        if ((m_LoadedAssetBundles.TryGetValue(dependency, out LoadedAssetBundle dependenceBundle)))
                        {
                            dependenceBundle.m_ReferencedCount += refCount;
#if UNITY_EDITOR
                            Debug.LogFormat(LOADREFSTR, dependenceBundle.name, dependenceBundle.m_ReferencedCount);
#endif
                        }
                    }
                }
            
                return true;
            }

            // @TODO: Do we need to consider the referenced count of WWWs?
            // In the demo, we never have duplicate WWWs as we wait LoadAssetAsync()/LoadLevelAsync() to be finished before calling another LoadAssetAsync()/LoadLevelAsync().
            // But in the real case, users can call LoadAssetAsync()/LoadLevelAsync() several times then wait them to be finished which might have duplicate WWWs.


            if (mode == LoadMode.Local || mode == LoadMode.LocalFirst)
            {
                if (m_LocalLoadingReferenceCount.ContainsKey(assetBundleName))
                {
                    m_LocalLoadingReferenceCount[assetBundleName] += refCount;
                    return true;
                }
                m_LocalLoadingReferenceCount.Add(assetBundleName, refCount);

                string url = GetLocalUrl(assetBundleName);
                AssetBundleCreateRequest request = null;

                if (isLoadingAssetBundleManifest)
                    request = AssetBundle.LoadFromFileAsync(url);
                else
                    request = AssetBundle.LoadFromFileAsync(url, 0); // TODO CRC校验

                request.priority = (int)ThreadPriority.BelowNormal;

                ABCreateRequest abr = new ABCreateRequest();
                abr.request = request;
                abr.startTime = Time.realtimeSinceStartup;
                m_CreatingAssetBundles.Add(assetBundleName, abr);
#if WRITE_BUNDLE_LOG
                Debug.LogFormat("******** LoadAssetBundleInternal: {0}", assetBundleName);
#endif
            }
            else if (mode == LoadMode.Remote || mode == LoadMode.RemoteFirst)
            {
                if (m_RemoteLoadingReferencedCount.ContainsKey(assetBundleName))
                {
                    m_RemoteLoadingReferencedCount[assetBundleName] += refCount;
                    return true;
                }
                m_RemoteLoadingReferencedCount.Add(assetBundleName, refCount);

                string url = GetRemoteUrl(assetBundleName);

                UnityWebRequest download = null;

                //For manifest assetbundle, always download it as we don't have hash for it.
                if (isLoadingAssetBundleManifest)
                    download = UnityWebRequestAssetBundle.GetAssetBundle(url);
                else
                    download = UnityWebRequestAssetBundle.GetAssetBundle(url, m_AssetBundleManifest.GetAssetBundleHash(assetBundleName), 0); // TODO CRC校验

                UnityWebRequestAsyncOperation request = download.SendWebRequest();
                request.priority = (int)ThreadPriority.BelowNormal;

                m_UnityWebRequests.Add(assetBundleName, request);
            }


            return false;
        }

        // Where we get all the dependencies and load them all.
        private static void LoadDependencies(string assetBundleName)
        {
            //如果已经解析过依赖项了 就直接走load (游戏过程中 依赖项应该不会有变化)
            if (m_Dependencies.TryGetValue(assetBundleName, out var ds))
            {
                for (int i = 0; i < ds.Length; i++)
                {
                    LoadAssetBundleInternal(ds[i], false, loadMode);
                }
                
                return;
            }
        
            if (m_AssetBundleManifest == null)
            {
                Debug.LogError( "Please initialize AssetBundleManifest by calling AssetBundleManager.Initialize()");
                return;
            }

            // Get dependecies from the AssetBundleManifest object..
            string[] dependencies = m_AssetBundleManifest.GetAllDependencies(assetBundleName);
            if (dependencies.Length == 0)
            {
                return;
            }

            for (int i = 0; i < dependencies.Length; i++)
            {
                var dependency = dependencies[i];
                if (!string.IsNullOrEmpty(dependency))
                {
                    dependencies[i] = RemapVariantName(dependency);
                }
            }

            // Record and load all dependencies.
            m_Dependencies.Add(assetBundleName, dependencies);
            for (int i = 0; i < dependencies.Length; i++)
            {
#if WRITE_BUNDLE_LOG
                Debug.LogFormat("****     LoadAssetBundleInternal before: {0}", dependencies[i]);
#endif
                LoadAssetBundleInternal(dependencies[i], false, loadMode);
            }
        }

        // Unload assetbundle and its dependencies.
        public static void UnloadAssetBundle(string assetBundleName)
        {
#if UNITY_EDITOR
            // If we're in Editor simulation mode, we don't have to load the manifest assetBundle.
            if (SimulateAssetBundleInEditor)
            {
                if (m_SimulateAssetBundleList.Contains(assetBundleName))
                    m_SimulateAssetBundleList.Remove(assetBundleName);

                return;
            }
#endif
#if WRITE_BUNDLE_LOG
            Debug.LogFormat("**** UnloadAssetBundle: {0}", assetBundleName);
#endif

            UnloadAssetBundleInternal(assetBundleName);
            UnloadDependencies(assetBundleName);

            if (m_DownloadingErrors.ContainsKey(assetBundleName))
                m_DownloadingErrors.Remove(assetBundleName);

        }

        private static void UnloadDependencies(string assetBundleName)
        {
            if (!m_Dependencies.TryGetValue(assetBundleName, out string[] dependencies))
                return;

            // Loop dependencies.
            foreach (var dependency in dependencies)
            {
                UnloadAssetBundleInternal(dependency);
            }

            m_Dependencies.Remove(assetBundleName);
        }

        private static void UnloadAssetBundleInternal(string assetBundleName)
        {
            LoadedAssetBundle bundle = GetLoadedAssetBundle(assetBundleName, out _);
            if (bundle == null)
                return;

            if (--bundle.m_ReferencedCount <= 0)
            {

#if WRITE_BUNDLE_LOG
                Debug.LogFormat("******** UnloadAssetBundleInternal: {0}", assetBundleName);
#endif
                bundle.m_AssetBundle.Unload(true);
                m_LoadedAssetBundles.Remove(assetBundleName);

            }

#if UNITY_EDITOR
            Debug.LogFormat(UNLOADSTR, assetBundleName, bundle.m_ReferencedCount);
#endif
        }


        void UpdateLocal()
        {
            keysToRemove.Clear();

            Dictionary<string, ABCreateRequest>.Enumerator e = m_CreatingAssetBundles.GetEnumerator();
            while (e.MoveNext())
            {
                string key = e.Current.Key;
                var value = e.Current.Value;
                AssetBundleCreateRequest request = value.request;

                if (request.isDone)
                {
                    int refCount = m_LocalLoadingReferenceCount[key];
                    m_LocalLoadingReferenceCount.Remove(key);

                    keysToRemove.Add(key);

                    AssetBundle bundle = request.assetBundle;
                    if (bundle == null)
                    {
                        if (loadMode == LoadMode.LocalFirst)
                        {
                            Debug.LogFormat("No asset in local {0}, try remote {1}: {2}", BaseLocalURL, BaseDownloadingURL, key);
                            LoadAssetBundleInternal(key, m_AssetBundleManifest == null, LoadMode.Remote, refCount);
                        }
                        else
                        {
                            if (!m_DownloadingErrors.ContainsKey(key))
                                m_DownloadingErrors.Add(key, string.Format("{0} is not exist.", key));
                        }
                    }
                    else
                    {
                        if (!m_LoadedAssetBundles.ContainsKey(key))
                        {
                            if (refCount > 1)
                                m_LoadedAssetBundles.Add(key, new LoadedAssetBundle(bundle, refCount));
                            else
                                m_LoadedAssetBundles.Add(key, new LoadedAssetBundle(bundle));
                        }
                        else
                        {
                            m_LoadedAssetBundles[key].m_ReferencedCount = refCount;
                        }
                        

                        if (m_DownloadingErrors.ContainsKey(key))
                            m_DownloadingErrors.Remove(key);
//#if UNITY_EDITOR
//                        // 不在Editor中处理
//                        if (!SimulateAssetBundleInEditor)
//                        {
//                            ResetAllMaterials(bundle);
//                        }

//#endif
                    }
                }
            }

            for (int i = 0; i < keysToRemove.Count; ++i)
            {
                string key = keysToRemove[i];
                m_CreatingAssetBundles.Remove(key);
            }
        
        }

        void UpdateRemote()
        {
            keysToRemove.Clear();

            Dictionary<string, UnityWebRequestAsyncOperation>.Enumerator e = m_UnityWebRequests.GetEnumerator();

            while (e.MoveNext())
            {
                string key = e.Current.Key;
                UnityWebRequestAsyncOperation download = e.Current.Value;

                if (download.isDone)
                {
                    int refCount = m_RemoteLoadingReferencedCount[key];
                    m_RemoteLoadingReferencedCount.Remove(key);
                    keysToRemove.Add(key);

                    if (download.webRequest.isNetworkError || download.webRequest.isHttpError)
                    {
                        if (loadMode == LoadMode.RemoteFirst)
                        {
                            //Log(LogType.Info, string.Format("No asset in remote {0}, try local {1}: {2}", BaseDownloadingURL, BaseLocalURL, key));
                            LoadAssetBundleInternal(key, m_AssetBundleManifest == null, LoadMode.Local, refCount);
                        }
                        else
                        {
                            if (!m_DownloadingErrors.ContainsKey(key))
                                m_DownloadingErrors.Add(key, string.Format("Failed downloading bundle {0} from {1}: {2}", key, download.webRequest.url, download.webRequest.error));
                        }
                        continue;
                    }

                    AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(download.webRequest);

                    if (bundle == null)
                    {
                        if (loadMode == LoadMode.RemoteFirst)
                        {
                            Debug.LogFormat("No asset in remote {0}, try local {1}: {2}", BaseDownloadingURL, BaseLocalURL, key);
                            LoadAssetBundleInternal(key, m_AssetBundleManifest == null, LoadMode.Local, refCount);
                        }
                        else
                        {
                            if (!m_DownloadingErrors.ContainsKey(key))
                                m_DownloadingErrors.Add(key, string.Format("{0} is not a valid asset bundle.", key));
                        }
                    }
                    else
                    {
                        if (refCount > 1)
                            m_LoadedAssetBundles.Add(key, new LoadedAssetBundle(bundle, refCount));
                        else
                            m_LoadedAssetBundles.Add(key, new LoadedAssetBundle(bundle));

                        if (m_DownloadingErrors.ContainsKey(key))
                            m_DownloadingErrors.Remove(key);
                    }
                }
            }

            for (int i = 0; i < keysToRemove.Count; ++i)
            {
                string key = keysToRemove[i];

                UnityWebRequestAsyncOperation download = m_UnityWebRequests[key];
                m_UnityWebRequests.Remove(key);
                download.webRequest.Dispose();
                System.GC.SuppressFinalize(download.webRequest);
            }
        }

        void Update()
        {
            if (loadMode != LoadMode.Remote)
                UpdateLocal();

            if (loadMode != LoadMode.Local)
                UpdateRemote();

            // Update all in progress operations, as smooth as possible
            if (m_InProgressOperations.Count > 0)
            {
                int count = m_InProgressOperations.Count;
                float time = Time.realtimeSinceStartup;
                int i = 0;
                while (i < m_InProgressOperations.Count)
                {
                    if (!m_InProgressOperations[i].Update())
                    {
                        // 打印一个log
                        m_InProgressOperations.RemoveAt(i);
                    }
                    else
                    {
                        i++;
                    }

                    if (Time.realtimeSinceStartup - time >= Time.fixedDeltaTime)
                    {
                        //if (Log.IsLoad())
                            Debug.Log("Handled " + (count - m_InProgressOperations.Count));
                        break;
                    }
                }
            }


        }
        //同步加载编辑器asset资产
        private static object LoadSimulateAssetSync(string assetBundleName, string assetName, System.Type type)
        {
#if UNITY_EDITOR
            m_SimulateAssetBundleList.Add(assetBundleName);
            string variant = RemapVariantName(assetBundleName);
            if (!string.IsNullOrEmpty(assetName))
            {
                Object simulatedObject = null;
                string[] assetPaths = AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(variant, assetName);
                for (int i = 0; i < assetPaths.Length; ++i)
                {
                    Object target;
                    if (type != typeof(Object))
                    {
                        target = AssetDatabase.LoadAssetAtPath(assetPaths[i], type);
                    }
                    else
                    {
                        target = AssetDatabase.LoadMainAssetAtPath(assetPaths[i]);
                    }

                    if (target)
                    {
                        simulatedObject = target;
                        break;
                    }
                }

                if (simulatedObject == null)
                {
                    Debug.LogWarning(string.Format("There is no asset with name \"{0}\" in \"{1}\" with type {2}", assetName, variant, type));
                }

                return simulatedObject;
            }
            else
            {
                string[] assetPaths = AssetDatabase.GetAssetPathsFromAssetBundle(variant);

                if (assetPaths.Length > 0)
                {
                    var targets = new List<Object>();
                    for (int i = 0; i < assetPaths.Length; i++)
                    {
                        Object target;
                        if (type != typeof(Object))
                        {
                            target = AssetDatabase.LoadAssetAtPath(assetPaths[i], type);
                        }
                        else
                        {
                            target = AssetDatabase.LoadMainAssetAtPath(assetPaths[i]);
                        }

                        if (target)
                            targets.Add(target);
                    }

                    return targets.ToArray();
                }
                else
                {
                    Debug.LogWarning("There is no assetbundle with name " + variant);
                }
            }
#endif
            return null;
        }

        private static LoadedAssetBundle LoadBundleSync(string assetBundleName)
        {
            if (m_AssetBundleManifest == null)
            {
                Debug.LogError("Please initialize AssetBundleManifest by calling AssetBundleManager.Initialize()");
                return null;
            }
            string[] dependencies = null;
            LoadedAssetBundle loadedBundle;
            // 检查缓存有没有，有了直接返回
            if (m_LoadedAssetBundles.TryGetValue(assetBundleName, out loadedBundle))
            {
                loadedBundle.m_ReferencedCount += 1;
#if UNITY_EDITOR
                Debug.LogFormat(LOADREFSTR, assetBundleName, loadedBundle.m_ReferencedCount);
#endif
                // 自己的引用计数增加还不行，所有的依赖项引用计数也要增加，要不会出现显示的时候丢失资源的问题
                dependencies = m_AssetBundleManifest.GetAllDependencies(assetBundleName);
                for (int i = 0; i < dependencies.Length; i++)
                {
                    var dependency = dependencies[i];
                    if (!string.IsNullOrEmpty(dependency))
                    {
                        dependencies[i] = RemapVariantName(dependency);
                        if ((m_LoadedAssetBundles.TryGetValue(dependency, out LoadedAssetBundle dependenceBundle)))
                        {
                            dependenceBundle.m_ReferencedCount += 1;
#if UNITY_EDITOR
                            Debug.LogFormat(LOADREFSTR, dependenceBundle.name, dependenceBundle.m_ReferencedCount);
#endif
                        }
                    }
                }
                return loadedBundle;
            }
            //加载包内资源，如果之后想加载热更资源再说！！
            string url = GetLocalUrl(assetBundleName);
            
            var bundle = AssetBundle.LoadFromFile(url);
            
            if (bundle == null)
            {
                Debug.LogWarning($"{assetBundleName} is not exist.");
            }
            else
            {
#if WRITE_BUNDLE_LOG
                Debug.LogFormat("****     LoadBundleSync : {0}", url);
#endif
                loadedBundle = new LoadedAssetBundle(bundle);
                m_LoadedAssetBundles.Add(assetBundleName, loadedBundle);

//#if UNITY_EDITOR
//                if (!SimulateAssetBundleInEditor)
//                    ResetAllMaterials(bundle);
//#endif
            }
            
            //如果已经解析过依赖项了 就直接走load (游戏过程中 依赖项应该不会有变化)
            if (m_Dependencies.TryGetValue(assetBundleName, out var ds))
            {
                for (int i = 0; i < ds.Length; i++)
                {
                    LoadBundleSync(ds[i]);
                }
            }
            else
            {
                dependencies = m_AssetBundleManifest.GetAllDependencies(assetBundleName);
      
                for (int i = 0; i < dependencies.Length; i++)
                {
                    var dependency = dependencies[i];
                    if (!string.IsNullOrEmpty(dependency))
                    {
                        dependencies[i] = RemapVariantName(dependency);
                    }
                }
   
                m_Dependencies.Add(assetBundleName, dependencies);
                foreach (var t in dependencies)
                {
                    LoadBundleSync(t);
                }
            }
        

            return loadedBundle;
        }

        public static object LoadAssetSync(string assetBundleName, string assetName, System.Type type)
        {
#if UNITY_EDITOR

            if (SimulateAssetBundleInEditor)
            {
                return LoadSimulateAssetSync(assetBundleName,assetName,type);
            }
            else
#endif
            {
                string variant = RemapVariantName(assetBundleName);
                var bundle =  LoadBundleSync(variant);
                if (bundle == null)
                    return null;
                object obj;
                if (!string.IsNullOrEmpty(assetName))
                {
                    obj = bundle.m_AssetBundle.LoadAsset(assetName, type);
                }
                else
                {
                    obj = bundle.m_AssetBundle.LoadAllAssets(type);
                }

                return obj;
            }
        }

        // Load asset from the given assetBundle.
        public static AssetBundleLoadAssetOperation LoadAssetAsync(string assetBundleName, string assetName, System.Type type)
        {
            AssetBundleLoadAssetOperation operation = null;
#if UNITY_EDITOR
            if (SimulateAssetBundleInEditor)
            {
                m_SimulateAssetBundleList.Add(assetBundleName);
                string variant = RemapVariantName(assetBundleName);
                operation = new AssetBundleLoadAssetOperationSimulation(assetBundleName, variant, assetName, type);
                operation.StartTime = Time.realtimeSinceStartup;
                m_InProgressOperations.Add(operation);
            }
            else
#endif
            {
                string variant = RemapVariantName(assetBundleName);
                LoadAssetBundle(variant);
                operation = new AssetBundleLoadAssetOperationFull(assetBundleName, variant, assetName, type);
                operation.StartTime = Time.realtimeSinceStartup;
                m_InProgressOperations.Add(operation);
            }

            return operation;
        }

        // Load level from the given assetBundle.
        public static AssetBundleLoadLevelOperation LoadLevelAsync(string assetBundleName, string levelName, bool isAdditive, bool allowSceneActivation)
        {
            AssetBundleLoadLevelOperation operation = null;
#if UNITY_EDITOR
            
            if (SimulateAssetBundleInEditor)
            {
                m_SimulateAssetBundleList.Add(assetBundleName);
                assetBundleName = RemapVariantName(assetBundleName);
                operation = new AssetBundleLoadLevelSimulationOperation(assetBundleName, levelName, isAdditive, allowSceneActivation);
            }
            else
#endif
            {
                assetBundleName = RemapVariantName(assetBundleName);
                LoadAssetBundle(assetBundleName);
                operation = new AssetBundleLoadLevelOperation(assetBundleName, levelName, isAdditive, allowSceneActivation);
            }

            m_InProgressOperations.Add(operation);
            return operation;
        }

        private static string GetLocalUrl(string assetBundleName)
        {
            if (m_LocalUrlDict.TryGetValue(assetBundleName, out string url))
            {
                return url;
            }

            url = Path.Combine(BaseLocalURL, assetBundleName);
            m_LocalUrlDict.Add(assetBundleName, url);
            return url;
        }

        private static string GetRemoteUrl(string assetBundleName)
        {
            if (m_RemoteUrlDict.TryGetValue(assetBundleName, out string url))
            {
                return url;
            }

            url = Path.Combine(BaseDownloadingURL, assetBundleName);
            m_RemoteUrlDict.Add(assetBundleName, url);
            return url;
        }
        
    } // End of AssetBundleManager.
}
