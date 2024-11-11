using Framework.Pool;
using System;
using System.IO;
using UnityEngine;

namespace Framework
{
    /// <summary>
    /// 通用实例对象管理器
    /// </summary>
    public class InstanceObjectManager : GameBaseSingletonModule<InstanceObjectManager>
    {
        private ObjectPool<InstanceObject> m_InstancePool = null;

        private float m_AutoCheckInterval = 60;
        private float m_AutoCheckTime;

        private int m_releaseTimer = -1;

        /// <summary>
        /// 设置对象池管理器。
        /// </summary>
        public void CreateInstancePool()
        {
            if (null != m_InstancePool)
                return;

            m_InstancePool = ObjectPoolManager.Instance.CreateSingleSpawnObjectPool<InstanceObject>("Common Instance Pool");
            m_InstancePool.AutoReleaseInterval = 10;
            m_InstancePool.Capacity = 0;
            m_InstancePool.ExpireTime = 20;
            m_InstancePool.Priority = 0;
        }

        public string LoadNew(string assetPath, ResourceManager.OnLoadComplete loadCallback)
        {
            ResourceManager.ParseAssetPath(assetPath, true, out string assetName, out string assetBundle);
            return ResourceManager.Instance?.LoadAssetAsync<GameObject>(assetBundle, assetName, loadCallback);
        }

        public void Spawn(string assetPath, Transform parent = null, Action<InstanceObject> loadCallback = null)
        {
            assetPath = assetPath.Replace("\\", "/");
            var instanceObject = m_InstancePool.Spawn(assetPath);
            if (null == instanceObject || null == instanceObject.Instance)
            {
                var tmpAssetPath = assetPath;
                var tmpParent = parent;

                LoadAsset(tmpAssetPath, (asset) =>
                {
                    // 资源不存在
                    if(null == asset)
                    {
                        Debug.LogWarning($"资源不存在: {tmpAssetPath}");
                        loadCallback?.Invoke(null);
                        return;
                    }

                    instanceObject = new InstanceObject(tmpAssetPath, asset);
                    m_InstancePool.Register(instanceObject, true);

                    instanceObject.ResetParams();
                    loadCallback?.Invoke(instanceObject);
                    instanceObject.SetParent(parent);
                });
            }
            else
            {
                instanceObject.ResetParams();
                loadCallback?.Invoke(instanceObject);
                instanceObject.SetParent(parent);
            }
        }

        public void Unspawn(GameObject obj, bool immediate = false)
        {
            m_InstancePool.Unspawn(obj, immediate);
        }

        public void SetParent(GameObject obj, Transform parent, bool worldPositionStays = false)
        {
            var target = m_InstancePool.GetObject(obj);
            if(null != target)
            {
                target.Peek().SetParent(parent, worldPositionStays);
            }
            else
            {
                obj.transform.SetParent(parent, worldPositionStays);
            }
        }

        public Object<InstanceObject> GetInstanceObject(GameObject obj)
        {
            return m_InstancePool.GetObject(obj);
        }

        public override void Update(float elapseSeconds, float realElapseSeconds)
        {
            m_AutoCheckTime += realElapseSeconds;
            if (m_AutoCheckTime < m_AutoCheckInterval)
                return;

            var allObjects = m_InstancePool.GetAllObjects();
            foreach (var obj in allObjects)
            {
                if (obj.Locked)
                    continue;

                if (!obj.IsInUse)
                    continue;

                if (null == obj.Peek().Target)
                {
                    obj.Unspawn();
                    continue;
                }

                if(obj.Peek().HasOwner && null == obj.Peek().Owner)
                {
                    obj.Unspawn();
                    continue;
                }
            }

            m_AutoCheckTime = 0;
        }

        /// <summary>
        /// 实例池释放资源
        /// </summary>
        /// <param name="immediate">是否立即释放</param>
        public void Release()
        {
            m_InstancePool.Release();
        }

        private void LoadAsset(string assetPath, Action<object> loadCallback)
        {
            ResourceManager.ParseAssetPath(assetPath, true, out string assetName, out string assetBundle);

            ResourceManager.Instance?.LoadAssetAsync<GameObject>(assetBundle, assetName, (key, asset, err) =>
            {
                if (null == asset)
                {
                    loadCallback?.Invoke(null);
                    return;
                }

                if (string.IsNullOrEmpty(err))
                {
                    loadCallback?.Invoke(null);
                    return;
                }

                loadCallback?.Invoke(asset);
            });
        }
    }
}
