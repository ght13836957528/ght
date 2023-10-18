using UnityEngine;

namespace AssetBundles
{
    /// <summary>
    /// 真机加载资源操作的抽象对象
    /// </summary>
    public abstract class AssetBundleLoadAssetOperation : AssetBundleLoadOperation
    {
        public abstract Object GetAsset();
        public abstract T GetAsset<T>() where T : UnityEngine.Object;
        public abstract Object[] GetAllAssets();
        public abstract T[] GetAllAssets<T>() where T : UnityEngine.Object;

        public string assetBundleName;
        public string assetBundleVariant;
        public string assetName;
        public System.Type type;
    }
}