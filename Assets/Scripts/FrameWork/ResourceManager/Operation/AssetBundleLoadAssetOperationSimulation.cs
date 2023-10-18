using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace AssetBundles
{
#if UNITY_EDITOR
    /// <summary>
    /// editor下模拟加载资源操作
    /// </summary>
    public class AssetBundleLoadAssetOperationSimulation : AssetBundleLoadAssetOperation
    {
        Object m_SimulatedObject;
        Object[] m_SimulatedObjects;

        public AssetBundleLoadAssetOperationSimulation(string assetBundleName, string assetBundleVariant,
            string assetName, System.Type type)
        {
            AssetBundleManager.Log(AssetBundleManager.LogType.Info,
                "assetBundleName: " + assetBundleName + "  assetBundleVariant:" + assetBundleVariant + "  assetName:" +
                assetName);
            this.assetBundleName = assetBundleName;
            this.assetBundleVariant = assetBundleVariant;
            this.assetName = assetName;
            this.type = type;
        }

        public override Object GetAsset()
        {
            return m_SimulatedObject;
        }

        public override T GetAsset<T>()
        {
            return m_SimulatedObject as T;
        }

        public override Object[] GetAllAssets()
        {
            return m_SimulatedObjects;
        }

        public override T[] GetAllAssets<T>()
        {
            T[] array = new T[m_SimulatedObjects.Length];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = m_SimulatedObjects[i] as T;
            }

            return array;
        }

        public override bool Update()
        {
            if (!string.IsNullOrEmpty(assetName))
            {
                // var arr = AssetDatabase.GetAllAssetBundleNames();
                // for (int i = 0; i < arr.Length; i++)
                // {
                //     Debug.Log("--------------: " + arr[i]);
                // }
                //
                // var arr2 =  AssetDatabase.GetAssetPathsFromAssetBundle(assetBundleVariant);
                // for (int i = 0; i < arr2.Length; i++)
                // {
                //     Debug.Log("-==============: " + arr2[i]);
                // }

                string[] assetPaths =
                    UnityEditor.AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(assetBundleVariant, assetName);
                for (int i = 0; i < assetPaths.Length; ++i)
                {
                    // Debug.Log("=========--------------: " + assetPaths[i]);
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
                        AssetBundleManager.Log(AssetBundleManager.LogType.Info, "get asset: " + assetName);
                        m_SimulatedObject = target;
                        break;
                    }
                }

                if (m_SimulatedObject == null)
                {
                    m_DownloadingError = string.Format("There is no asset with name \"{0}\" in \"{1}\" with type {2}",
                        assetName, assetBundleVariant, type);
                }
            }
            else
            {
                string[] assetPaths = AssetDatabase.GetAssetPathsFromAssetBundle(assetBundleVariant);

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

                    m_SimulatedObjects = targets.ToArray();
                }
                else
                {
                    m_DownloadingError = "There is no assetbundle with name " + assetBundleVariant;
                }
            }


            return false;
        }

        public override bool IsDone()
        {
            return m_SimulatedObject != null || m_SimulatedObjects != null || !string.IsNullOrEmpty(m_DownloadingError);
        }

        public override float Progress()
        {
            return 1;
        }
    }
}
#endif