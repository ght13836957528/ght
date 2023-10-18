using UnityEngine;
using UnityEngine.SceneManagement;

namespace AssetBundles
{
    /// <summary>
    /// 真机加载场景操作
    /// </summary>
    public class AssetBundleLoadLevelOperation : AssetBundleLoadOperation
    {
        protected string m_AssetBundleName;
        protected string m_LevelName;
        protected bool m_IsAdditive;
        protected bool m_allowSceneActivation;

        protected AsyncOperation m_Request;

        public string AssetBundle => m_AssetBundleName;
        public string LevelName => m_LevelName;

        public AssetBundleLoadLevelOperation(string assetbundleName, string levelName, bool isAdditive, bool allowSceneActivation)
        {
            m_AssetBundleName = assetbundleName;
            m_LevelName = levelName;
            m_IsAdditive = isAdditive;
            m_allowSceneActivation = allowSceneActivation;

        }

        public override bool Update()
        {
            if (m_Request != null)
                return false;

            LoadedAssetBundle bundle = AssetBundleManager.GetLoadedAssetBundle(m_AssetBundleName, out m_DownloadingError);
            if (bundle != null)
            {
                if (m_IsAdditive)
                    m_Request = SceneManager.LoadSceneAsync(m_LevelName, LoadSceneMode.Additive);
                else
                    m_Request = SceneManager.LoadSceneAsync(m_LevelName, LoadSceneMode.Single);

                m_Request.allowSceneActivation = m_allowSceneActivation;

                return false;
            }
            else
                return true;
        }

        public override bool IsDone()
        {
            // Return if meeting downloading error.
            // m_DownloadingError might come from the dependency downloading.
            if (m_Request == null && !string.IsNullOrEmpty(m_DownloadingError))
            {
                return true;
            }

            return m_Request != null && m_Request.isDone;
        }

        public override float Progress()
        {
            return m_Request == null ? 1 : m_Request.progress;
        }
    }
}