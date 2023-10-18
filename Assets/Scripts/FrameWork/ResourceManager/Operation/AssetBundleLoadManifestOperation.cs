using UnityEngine;

namespace AssetBundles
{
    /// <summary>
    /// 加载 AssetBundleManifest 的进程
    /// </summary>
    public class AssetBundleLoadManifestOperation : AssetBundleLoadAssetOperationFull
    {
        public AssetBundleLoadManifestOperation(string bundleName, string assetName, System.Type type)
            : base(bundleName, bundleName, assetName, type)
        {
        }

        public override bool Update()
        {
            base.Update();

            if (m_Request != null && m_Request.isDone)
            {
                AssetBundleManager.AssetBundleManifestObject = GetAsset<AssetBundleManifest>();
                return false;
            }
            else
                return true;
        }

        public override bool IsDone()
        {
            if (m_Request == null && Error != null)
            {
                return true;
            }

            return m_Request != null && m_Request.isDone && AssetBundleManager.AssetBundleManifestObject != null;
        }
    }

}