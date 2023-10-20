using UnityEngine;
namespace AssetBundles
{
    /// <summary>
    /// 真机加载资源的操作
    /// </summary>
    public class AssetBundleLoadAssetOperationFull : AssetBundleLoadAssetOperation
    {
        protected AssetBundleRequest m_Request = null;

        public AssetBundleLoadAssetOperationFull(string assetBundleName, string assetBundleVariant, string assetName,
            System.Type type)
        {
            AssetBundleManager.Log(AssetBundleManager.LogType.Info, "assetBundleName: " + assetBundleName + "  assetBundleVariant:" + assetBundleVariant + "  assetName:" + assetName);
            this.assetBundleName = assetBundleName;
            this.assetBundleVariant = assetBundleVariant;
            this.assetName = assetName;
            this.type = type;
        }

        public override Object GetAsset()
        {
            if (m_Request != null && m_Request.isDone)
                return m_Request.asset;
            else
                return null;
        }

        public override T GetAsset<T>()
        {
            if (m_Request != null && m_Request.isDone)
                return m_Request.asset as T;
            else
                return null;
        }

        public override Object[] GetAllAssets()
        {
            if (m_Request != null && m_Request.isDone)
                return m_Request.allAssets;
            else
                return null;
        }

        public override T[] GetAllAssets<T>()
        {
            if (m_Request != null && m_Request.isDone)
            {
                T[] array = new T[m_Request.allAssets.Length];
                for (int i = 0; i < array.Length; i++)
                {
                    array[i] = m_Request.allAssets[i] as T;
                }

                return array;
            }
            else
                return null;
        }

        // 因为要等所有的依赖项全部加载，所以平时都不会返回false
        public override bool Update()
        {
            if (m_Request != null)
                return false;

            LoadedAssetBundle bundle = AssetBundleManager.GetLoadedAssetBundle(assetBundleVariant, out m_DownloadingError);
            if (bundle != null)
            {
                //需要检查是否加载出现异常

                ///@TODO: When asset bundle download fails this throws an exception...

                if (!string.IsNullOrEmpty(assetName)) //只下载bundle
                    m_Request = bundle.m_AssetBundle.LoadAssetAsync(assetName, type);
                else
                    m_Request = bundle.m_AssetBundle.LoadAllAssetsAsync(type);

                return false;
            }

            if (Error != null)
                return false;

            return true;
        }

        public override bool IsDone()
        {
            // Return if meeting downloading error.
            // m_DownloadingError might come from the dependency downloading.
            if (m_Request == null && Error != null)
            {
                return true;
            }

            if (m_Request != null && m_Request.isDone && m_Request.asset == null)
            {
                m_DownloadingError = string.Format("There is no asset with name \"{0}\" in \"{1}\" with type {2}",
                    assetName, assetBundleVariant, type);
            }

            return m_Request != null && m_Request.isDone;
        }

        public override float Progress()
        {
            return m_Request == null ? 1 : m_Request.progress;
        }
    }
}