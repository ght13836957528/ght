using UnityEngine.SceneManagement;

namespace AssetBundles
{
#if UNITY_EDITOR
    /// <summary>
    /// editor模拟加载场景操作
    /// </summary>
    public class AssetBundleLoadLevelSimulationOperation : AssetBundleLoadLevelOperation
    {
        public AssetBundleLoadLevelSimulationOperation(string assetbundleName, string levelName, bool isAdditive,
            bool allowSceneActivation)
            : base(assetbundleName, levelName, isAdditive, allowSceneActivation)
        {
        }

        public override bool Update()
        {
            string[] levelPaths =
                UnityEditor.AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(m_AssetBundleName, m_LevelName);
            if (levelPaths.Length == 0)
            {
                ///@TODO: The error needs to differentiate that an asset bundle name doesn't exist
                //        from that there right scene does not exist in the asset bundle...

                m_DownloadingError =
                    string.Format("There is no scene with name {0} in {1}", m_LevelName, m_AssetBundleName);
                return false;
            }

#if UNITY_2018_3_OR_NEWER
            m_Request = UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(levelPaths[0],
                new LoadSceneParameters(m_IsAdditive ? LoadSceneMode.Additive : LoadSceneMode.Single));
#else
            if (m_IsAdditive)
                m_Operation = UnityEditor.EditorApplication.LoadLevelAdditiveAsyncInPlayMode(levelPaths[0]);
            else
                m_Operation = UnityEditor.EditorApplication.LoadLevelAsyncInPlayMode(levelPaths[0]);
#endif

            m_Request.allowSceneActivation = m_allowSceneActivation;

            return false;
        }
    }

#endif
}