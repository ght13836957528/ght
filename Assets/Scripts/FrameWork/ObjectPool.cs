using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using GameManager;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif


    public sealed class ObjectPool : SingletonBehaviour<ObjectPool>
    {
        public enum StartupPoolMode { Awake, Start, CallManually };

        [System.Serializable]
        public class StartupPool
        {
            public int size;
            public GameObject prefab;
        }

#if ODIN_INSPECTOR
        [ShowInInspector, ShowIf("showOdinInfo"), DictionaryDrawerSettings(IsReadOnly = true, DisplayMode = DictionaryDisplayOptions.Foldout)]
#endif
        public Dictionary<GameObject, List<GameObject>> pooledObjects = new Dictionary<GameObject, List<GameObject>>(); // prefab, List<obj>
#if ODIN_INSPECTOR
        [ShowInInspector, ShowIf("showOdinInfo"), DictionaryDrawerSettings(IsReadOnly = true, DisplayMode = DictionaryDisplayOptions.OneLine)]
#endif
        public Dictionary<GameObject, GameObject> spawnedObjects = new Dictionary<GameObject, GameObject>(); // obj, prefab
#if ODIN_INSPECTOR
        [ShowInInspector, ShowIf("showOdinInfo"), DictionaryDrawerSettings(IsReadOnly = true, DisplayMode = DictionaryDisplayOptions.OneLine)]
#endif
        // 加一个已经回收的对象表，避免重复回收导致对象删除，而同一帧创建对象，下一帧被删除的问题。
        public Dictionary<GameObject, GameObject> recycledObjects = new Dictionary<GameObject, GameObject>(); // obj, prefab

        public StartupPoolMode startupPoolMode = StartupPoolMode.CallManually;
        public StartupPool[] startupPools;

        bool startupPoolsCreated;

        public override void Release()
        {
            base.Release();
        }

        void Awake()
        {
            if (startupPoolMode == StartupPoolMode.Awake)
                CreateStartupPools();
        }

        private new void Start()
        {
            if (startupPoolMode == StartupPoolMode.Start)
                CreateStartupPools();
        }

        public static void CreateStartupPools()
        {
            if (!Instance.startupPoolsCreated)
            {
                Instance.startupPoolsCreated = true;
                var pools = Instance.startupPools;
                if (pools != null && pools.Length > 0)
                    for (int i = 0; i < pools.Length; ++i)
                        CreatePool(pools[i].prefab, pools[i].size);
            }
        }

        public static void CreatePool<T>(T prefab, int initialPoolSize) where T : Component
        {
            CreatePool(prefab.gameObject, initialPoolSize);
        }
        public static void CreatePool(GameObject prefab, int initialPoolSize)
        {
            if (prefab != null && !Instance.pooledObjects.ContainsKey(prefab))
            {
                var list = new List<GameObject>();
                Instance.pooledObjects[prefab] = list;
                
                if (initialPoolSize > 0)
                {
                    bool active = prefab.activeSelf;
                    prefab.SetActive(false);
                    Transform parent = Instance.transform;
                    while (list.Count < initialPoolSize)
                    {
                        var obj = Instantiate(prefab);
                        obj.name = obj.name.Replace("(Clone)", "(Spawn)"); // mark object as "Spawn"
                        obj.transform.SetParent(parent, false);
                        list.Add(obj);
                    }
                    prefab.SetActive(active);
                }
            }
        }

        public static T Spawn<T>(T prefab, Transform parent, Vector3 position, Quaternion rotation) where T : Component
        {
            return Spawn(prefab.gameObject, parent, position, rotation).GetComponent<T>();
        }
        public static T Spawn<T>(T prefab, Vector3 position, Quaternion rotation) where T : Component
        {
            return Spawn(prefab.gameObject, null, position, rotation).GetComponent<T>();
        }
        public static T Spawn<T>(T prefab, Transform parent, Vector3 position) where T : Component
        {
            return Spawn(prefab.gameObject, parent, position, Quaternion.identity).GetComponent<T>();
        }
        public static T Spawn<T>(T prefab, Vector3 position) where T : Component
        {
            return Spawn(prefab.gameObject, null, position, Quaternion.identity).GetComponent<T>();
        }
        public static T Spawn<T>(T prefab, Transform parent) where T : Component
        {
            return Spawn(prefab.gameObject, parent, Vector3.zero, Quaternion.identity).GetComponent<T>();
        }
        public static T Spawn<T>(T prefab) where T : Component
        {
            return Spawn(prefab.gameObject, null, Vector3.zero, Quaternion.identity).GetComponent<T>();
        }
        public static GameObject Spawn(GameObject prefab, Transform parent, Vector3 position, Quaternion rotation)
        {
            if (prefab == null)
            {
                Debug.LogError($"ObjectPool Spawn prefab is null! baccktrace:{new System.Diagnostics.StackTrace()}");
                if (parent != null)
                {
                    Debug.LogError($"ObjectPool parent path:{parent.name}");
                }
                else
                {
                    Debug.Log("parent is null!");
                }
                return new GameObject("Exception");
            }
            else
            {
                //Log.Info("Spawn - {0}", prefab.transform.GetPath());
            }

            Transform trans;
            GameObject obj;

            if (Instance.pooledObjects.TryGetValue(prefab, out List<GameObject> list))
            {
                obj = null;
                if (list.Count > 0)
                {
                    // the object in list maybe null
                    while (obj == null && list.Count > 0)
                    {
                        obj = list[0];
                        list.RemoveAt(0);
                    }
                    if (obj != null)
                    {                        
                        trans = obj.transform;
                        trans.SetParent(parent, false);
                        trans.localPosition = position;
                        trans.localRotation = rotation;
                        obj.SetActive(true);
                        Instance.spawnedObjects.Add(obj, prefab);
                        Instance.recycledObjects.Remove(obj);
                        return obj;
                    }
                }
                obj = Instantiate(prefab);
                obj.name = obj.name.Replace("(Clone)", "(Spawn)");  // mark object as "Spawn"

                trans = obj.transform;
                trans.SetParent(parent, false);
                trans.localPosition = position;
                trans.localRotation = rotation;
                obj.SetActive(true);
                Instance.spawnedObjects.Add(obj, prefab);
                return obj;
            }
            else
            {
                obj = Instantiate(prefab);
                trans = obj.GetComponent<Transform>();
                trans.SetParent(parent, false);
                trans.localPosition = position;
                trans.localRotation = rotation;
                obj.SetActive(true);
                return obj;
            }
        }
        public static GameObject Spawn(GameObject prefab, Transform parent, Vector3 position)
        {
            return Spawn(prefab, parent, position, Quaternion.identity);
        }
        public static GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            return Spawn(prefab, null, position, rotation);
        }
        public static GameObject Spawn(GameObject prefab, Transform parent)
        {
            return Spawn(prefab, parent, Vector3.zero, Quaternion.identity);
        }
        public static GameObject Spawn(GameObject prefab, Vector3 position)
        {
            return Spawn(prefab, null, position, Quaternion.identity);
        }
        public static GameObject Spawn(GameObject prefab)
        {
            return Spawn(prefab, null, Vector3.zero, Quaternion.identity);
        }

        public static void Recycle<T>(T obj) where T : Component
        {
            if (obj != null)
                Recycle(obj.gameObject);
        }

        public static void Recycle(GameObject obj)
        {
            // 已经被回收，不要重复回收
            if (Instance != null && Instance.recycledObjects.ContainsKey(obj))
            {
                return;
            }

            if (Instance != null && Instance.spawnedObjects.TryGetValue(obj, out GameObject prefab))
            {
                if (obj != null)
                    Recycle(obj, prefab);
                else
                    Instance.spawnedObjects.Remove(obj);
            }
            else
            {
                Debug.LogWarning("{0} has been recycled or not in pooled, it will be destroyed", obj);
                //有个坑 因为destro不是立马销毁的 
                //假设你在前一步程序中调用了到此（通常是因为一个pool调了两次recycle）
                //下一步调用了spawn 此时可能发生destroy销毁了就会出现明明创建了但是发现没有这个实例
                Destroy(obj);
            }
        }
        static void Recycle(GameObject obj, GameObject prefab)
        {
            Instance.spawnedObjects.Remove(obj);
            Instance.recycledObjects.Add(obj, prefab);

            if (Instance.pooledObjects.TryGetValue(prefab, out List<GameObject> list))
            {
                list.Add(obj); 
                obj.transform.SetParent(Instance.transform, false);
                obj.SetActive(false);
            }
            else
            {
                Debug.LogWarning("{0} has been recycled or not in pooled, it will be destroyed", obj);
                Destroy(obj);
            }
        }

        public static void RecycleAll<T>(T prefab) where T : Component
        {
            RecycleAll(prefab.gameObject);
        }
        public static void RecycleAll(GameObject prefab)
        {
            if (Instance != null)
            {
                // FIXME: 做成栈对象？
                List<GameObject> tempList = new List<GameObject>();
                foreach (var item in Instance.spawnedObjects)
                {
                    if (item.Value == prefab)
                        tempList.Add(item.Key);
                }
                for (int i = 0; i < tempList.Count; ++i)
                {
                    Recycle(tempList[i]);
                }

                tempList.Clear();
            }
        }

        /// <summary>
        /// 清除所有缓存的对象
        /// </summary>
        public static void ResetAll()
        {
            Instance.recycledObjects.Clear();

            foreach (var item in Instance.spawnedObjects)
            {
                if (item.Key != null)
                {
                    Destroy(item.Key);
                }
            }
            Instance.spawnedObjects.Clear();

            foreach (var it in Instance.pooledObjects)
            {
                foreach(var obj in it.Value)
                {
                    Destroy(obj);
                }
            }            
            Instance.pooledObjects.Clear();
        }

        public static bool IsSpawned(GameObject obj)
        {
            return Instance.spawnedObjects.ContainsKey(obj);
        }

        public static int CountPooled<T>(T prefab) where T : Component
        {
            return CountPooled(prefab.gameObject);
        }
        public static int CountPooled(GameObject prefab)
        {
            return Instance.pooledObjects.TryGetValue(prefab, out List<GameObject> list) ? list.Count : 0;
        }

        public static int CountSpawned<T>(T prefab) where T : Component
        {
            return CountSpawned(prefab.gameObject);
        }
        public static int CountSpawned(GameObject prefab)
        {
            int count = 0;
            foreach (var instancePrefab in Instance.spawnedObjects.Values)
                if (prefab == instancePrefab)
                    ++count;
            return count;
        }

        public static int CountAllPooled()
        {
            int count = 0;
            foreach (var list in Instance.pooledObjects.Values)
                count += list.Count;
            return count;
        }

        public static List<GameObject> GetPooled(GameObject prefab, List<GameObject> list, bool appendList)
        {
            if (list == null)
                list = new List<GameObject>();
            if (!appendList)
                list.Clear();
            if (Instance.pooledObjects.TryGetValue(prefab, out List<GameObject> pooled))
            {
                list.AddRange(pooled);
            }

            return list;
        }
        public static List<T> GetPooled<T>(T prefab, List<T> list, bool appendList) where T : Component
        {
            if (list == null)
                list = new List<T>();
            if (!appendList)
                list.Clear();
            if (Instance.pooledObjects.TryGetValue(prefab.gameObject, out List<GameObject> pooled))
            {
                for (int i = 0; i < pooled.Count; ++i)
                    list.Add(pooled[i].GetComponent<T>());
            }

            return list;
        }

        public static List<GameObject> GetSpawned(GameObject prefab, List<GameObject> list, bool appendList)
        {
            if (list == null)
                list = new List<GameObject>();
            if (!appendList)
                list.Clear();
            foreach (var item in Instance.spawnedObjects)
                if (item.Value == prefab)
                    list.Add(item.Key);
            return list;
        }
        public static List<T> GetSpawned<T>(T prefab, List<T> list, bool appendList) where T : Component
        {
            if (list == null)
                list = new List<T>();
            if (!appendList)
                list.Clear();
            var prefabObj = prefab.gameObject;
            foreach (var item in Instance.spawnedObjects)
                if (item.Value == prefabObj)
                    list.Add(item.Key.GetComponent<T>());
            return list;
        }

        public static void DestroyPooled(GameObject prefab)
        {
            if (Instance != null && Instance.pooledObjects.TryGetValue(prefab, out List<GameObject> pooled))
            {
                Instance.pooledObjects.Remove(prefab);
                for (int i = 0; i < pooled.Count; ++i)
                    Destroy(pooled[i]);
                pooled.Clear();
            }
        }
        public static void DestroyPooled<T>(T prefab) where T : Component
        {
            DestroyPooled(prefab.gameObject);
        }

        public static GameObject GetPrefab(GameObject obj)
        {
            return Instance.spawnedObjects.TryGetValue(obj, out GameObject prefab) ? prefab : null;
        }

        public static GameObject GetPrefab<T>(T obj) where T : Component
        {
            return GetPrefab(obj.gameObject);
        }

    }

    public static class ObjectPoolExtensions
    {
        public static void CreatePool<T>(this T prefab) where T : Component
        {
            ObjectPool.CreatePool(prefab, 0);
        }
        public static void CreatePool<T>(this T prefab, int initialPoolSize) where T : Component
        {
            ObjectPool.CreatePool(prefab, initialPoolSize);
        }
        public static void CreatePool(this GameObject prefab)
        {
            ObjectPool.CreatePool(prefab, 0);
        }
        public static void CreatePool(this GameObject prefab, int initialPoolSize)
        {
            ObjectPool.CreatePool(prefab, initialPoolSize);
        }

        public static T Spawn<T>(this T prefab, Transform parent, Vector3 position, Quaternion rotation) where T : Component
        {
            return ObjectPool.Spawn(prefab, parent, position, rotation);
        }
        public static T Spawn<T>(this T prefab, Vector3 position, Quaternion rotation) where T : Component
        {
            return ObjectPool.Spawn(prefab, null, position, rotation);
        }
        public static T Spawn<T>(this T prefab, Transform parent, Vector3 position) where T : Component
        {
            return ObjectPool.Spawn(prefab, parent, position, Quaternion.identity);
        }
        public static T Spawn<T>(this T prefab, Vector3 position) where T : Component
        {
            return ObjectPool.Spawn(prefab, null, position, Quaternion.identity);
        }
        public static T Spawn<T>(this T prefab, Transform parent) where T : Component
        {
            return ObjectPool.Spawn(prefab, parent, Vector3.zero, Quaternion.identity);
        }
        public static T Spawn<T>(this T prefab) where T : Component
        {
            return ObjectPool.Spawn(prefab, null, Vector3.zero, Quaternion.identity);
        }
        public static GameObject Spawn(this GameObject prefab, Transform parent, Vector3 position, Quaternion rotation)
        {
            return ObjectPool.Spawn(prefab, parent, position, rotation);
        }
        public static GameObject Spawn(this GameObject prefab, Vector3 position, Quaternion rotation)
        {
            return ObjectPool.Spawn(prefab, null, position, rotation);
        }
        public static GameObject Spawn(this GameObject prefab, Transform parent, Vector3 position)
        {
            return ObjectPool.Spawn(prefab, parent, position, Quaternion.identity);
        }
        public static GameObject Spawn(this GameObject prefab, Vector3 position)
        {
            return ObjectPool.Spawn(prefab, null, position, Quaternion.identity);
        }
        public static GameObject Spawn(this GameObject prefab, Transform parent)
        {
            return ObjectPool.Spawn(prefab, parent, Vector3.zero, Quaternion.identity);
        }
        public static GameObject Spawn(this GameObject prefab)
        {
            return ObjectPool.Spawn(prefab, null, Vector3.zero, Quaternion.identity);
        }

        public static void Recycle<T>(this T obj) where T : Component
        {
            ObjectPool.Recycle(obj);
        }
        public static void Recycle(this GameObject obj)
        {
            ObjectPool.Recycle(obj);
        }

        public static void RecycleAll<T>(this T prefab) where T : Component
        {
            ObjectPool.RecycleAll(prefab);
        }
        public static void RecycleAll(this GameObject prefab)
        {
            ObjectPool.RecycleAll(prefab);
        }

        public static int CountPooled<T>(this T prefab) where T : Component
        {
            return ObjectPool.CountPooled(prefab);
        }
        public static int CountPooled(this GameObject prefab)
        {
            return ObjectPool.CountPooled(prefab);
        }

        public static int CountSpawned<T>(this T prefab) where T : Component
        {
            return ObjectPool.CountSpawned(prefab);
        }
        public static int CountSpawned(this GameObject prefab)
        {
            return ObjectPool.CountSpawned(prefab);
        }

        public static List<GameObject> GetSpawned(this GameObject prefab, List<GameObject> list, bool appendList)
        {
            return ObjectPool.GetSpawned(prefab, list, appendList);
        }
        public static List<GameObject> GetSpawned(this GameObject prefab, List<GameObject> list)
        {
            return ObjectPool.GetSpawned(prefab, list, false);
        }
        public static List<GameObject> GetSpawned(this GameObject prefab)
        {
            return ObjectPool.GetSpawned(prefab, null, false);
        }
        public static List<T> GetSpawned<T>(this T prefab, List<T> list, bool appendList) where T : Component
        {
            return ObjectPool.GetSpawned(prefab, list, appendList);
        }
        public static List<T> GetSpawned<T>(this T prefab, List<T> list) where T : Component
        {
            return ObjectPool.GetSpawned(prefab, list, false);
        }
        public static List<T> GetSpawned<T>(this T prefab) where T : Component
        {
            return ObjectPool.GetSpawned(prefab, null, false);
        }

        public static List<GameObject> GetPooled(this GameObject prefab, List<GameObject> list, bool appendList)
        {
            return ObjectPool.GetPooled(prefab, list, appendList);
        }
        public static List<GameObject> GetPooled(this GameObject prefab, List<GameObject> list)
        {
            return ObjectPool.GetPooled(prefab, list, false);
        }
        public static List<GameObject> GetPooled(this GameObject prefab)
        {
            return ObjectPool.GetPooled(prefab, null, false);
        }
        public static List<T> GetPooled<T>(this T prefab, List<T> list, bool appendList) where T : Component
        {
            return ObjectPool.GetPooled(prefab, list, appendList);
        }
        public static List<T> GetPooled<T>(this T prefab, List<T> list) where T : Component
        {
            return ObjectPool.GetPooled(prefab, list, false);
        }
        public static List<T> GetPooled<T>(this T prefab) where T : Component
        {
            return ObjectPool.GetPooled(prefab, null, false);
        }

        public static void DestroyPooled(this GameObject prefab)
        {
            ObjectPool.DestroyPooled(prefab);
        }
        public static void DestroyPooled<T>(this T prefab) where T : Component
        {
            ObjectPool.DestroyPooled(prefab.gameObject);
        }

        public static GameObject GetPrefab(this GameObject obj)
        {
            return ObjectPool.GetPrefab(obj);
        }

        public static GameObject GetPrefab<T>(this T obj) where T : Component
        {
            return ObjectPool.GetPrefab(obj.gameObject);
        }
    }


