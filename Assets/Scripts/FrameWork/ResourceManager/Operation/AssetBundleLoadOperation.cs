using System.Collections;

namespace AssetBundles
{
    /// <summary>
    /// 加载操作
    /// </summary>
    public abstract class AssetBundleLoadOperation : IEnumerator
    {
        public object Current
        {
            get { return null; }
        }

        public bool MoveNext()
        {
            return !IsDone();
        }

        public void Reset()
        {
        }

        abstract public bool Update();

        abstract public bool IsDone();

        abstract public float Progress();

        protected string m_DownloadingError;
        public string Error => m_DownloadingError;
    }
}