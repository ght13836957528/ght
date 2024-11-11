using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Spine;
using Spine.Unity;
using Spine.Unity.Editor;
using UnityEditor;
using UnityEngine;

namespace Main.Scripts.Editor.Resource
{
    public class SynchronizationModelTools : EditorWindow
    {

        string _sourceFolder = ""; // 源文件夹路径
        string _hotUpdateSourceFolder = ""; // 动更文件夹路径
        readonly string _targetFolder = "Assets/Main/Arts/model3d";
        private Dictionary<string, List<string>> _allC3BInfoDic;
        private List<string> _notFoundList;

        private string _txtFilePath = "";

        [MenuItem("Tools/通用工具/模型同步", false, 3)]
        static void SynchronizationResource()
        {
            GetWindow<SynchronizationModelTools>("模型同步");
        }

        private readonly HashSet<string> _addModelList = new HashSet<string>();
        private string[] _searchFilePathStrings = { @"\\10.0.0.26\美术\00_Project\Last Shelter\2021新整理  美术资产目录\3D场景\大本皮肤+运兵车+宝箱\女神雕像\FBX" };

        void OnGUI()
        {
            _sourceFolder = EditorGUILayout.TextField("CocosProjectRoot", _sourceFolder);
            _hotUpdateSourceFolder = EditorGUILayout.TextField("Cocos dynamicResource Path", _hotUpdateSourceFolder);
            _txtFilePath = EditorGUILayout.TextField("c3bList 文件路径", _txtFilePath);

            if (GUILayout.Button("开始解析"))
            {
                ParseTxtFile();
            }
            
            if (GUILayout.Button("test "))
            {
                TestSSH();
            }
        }

        public void ParseTxtFile()
        {

            if (!File.Exists(_txtFilePath))
            {
                Debug.LogError("_txtFilePath not exist");

            }
            else
            {
                _allC3BInfoDic = new Dictionary<string, List<string>>();
                _notFoundList = new List<string>();
                string[] fileContents = File.ReadAllLines(_txtFilePath);
                foreach (string path in fileContents)
                {
                    // 获取文件名（不包含扩展名）
                    string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(path);

                    // 提取相同部分的key
                    string key = fileNameWithoutExtension.Split('@')[0];
                    if (string.IsNullOrEmpty(key)) continue;
                    if (!_allC3BInfoDic.ContainsKey(key))
                    {
                        _allC3BInfoDic[key] = new List<string>();
                    }

                    _allC3BInfoDic[key].Add(path);
                }
            }

            foreach (var item in _allC3BInfoDic)
            {
                FindFBXFile(item.Key);
            }

            foreach (var item in _notFoundList)
            {
                Print("not found");
                Print(item);
            }

            LogFlush();
            AssetDatabase.Refresh(); 

            foreach (var targetPath in _addModelList)
            {
                // Log.Info("FBXAnimationTools.Process"+ targetPath);
                // FBXAnimationTools.Process(targetPath);
            }

            
            AssetDatabase.Refresh(); 
        }

        public void FindFBXFile(string fbxName)
        {
            foreach (var searchPath in _searchFilePathStrings)
            {
                ProcessFolder(searchPath, _targetFolder, fbxName);
            }
        }


        private void ProcessFolder(string sourceFolder, string targetFolder, string targetFbxName)
        {
            // 获取当前文件夹中的所有 .fbx 文件
            string[] fbxFiles = Directory.GetFiles(sourceFolder, "*.fbx", SearchOption.AllDirectories);

            bool found = false;
            foreach (string fbxFilePath in fbxFiles)
            {
                if (fbxFilePath.Contains(targetFbxName))
                {
                    found = true;
                    // 在目标文件夹中构建新文件夹路径
                    string newFolderInTarget = Path.Combine(targetFolder, targetFbxName);

                    // 如果目标文件夹中不存在相同名字的文件夹，则创建它
                    if (!Directory.Exists(newFolderInTarget))
                    {
                        Directory.CreateDirectory(newFolderInTarget);
                    }

                    // 复制 .fbx 文件到目标文件夹的新文件夹中
                    string destFilePath = Path.Combine(newFolderInTarget, Path.GetFileName(fbxFilePath));
                    if (!File.Exists(destFilePath))
                    {
                        File.Copy(fbxFilePath, destFilePath, true);
                        Print(destFilePath);
                        if (!_addModelList.Contains(newFolderInTarget))
                        {
                            _addModelList.Add(newFolderInTarget);
                        }
                    }

                    string directory = Path.GetDirectoryName(fbxFilePath);
                    string fbxFileNameWithoutExtension = Path.GetFileNameWithoutExtension(fbxFilePath);

                    // 构建同名的PNG文件路径
                    string pngFilePath = Path.Combine(directory, fbxFileNameWithoutExtension + ".png");
                    if (File.Exists(pngFilePath))
                    {
                        Debug.Log("PNG file found: " + pngFilePath);
                        string modelPngFilePath = Path.Combine(newFolderInTarget, fbxFileNameWithoutExtension + ".png");
                        File.Copy(pngFilePath, modelPngFilePath, true);
                    }

                }
            }

            if (!found)
            {
                if (_allC3BInfoDic.ContainsKey(targetFbxName))
                {
                    _notFoundList.AddRange(_allC3BInfoDic[targetFbxName]);
                }
            }
        }

        #region 处理日志

        private static StreamWriter sw = null;

        private static void SetLogFile(string file = null)
        {
            string logPath = "D:/output";

            if (!Directory.Exists(logPath))
                Directory.CreateDirectory(logPath);

            string logFileName = string.Format("{0}_{1}.log",
                string.IsNullOrEmpty(file) ? "Synchronization-model-ToolsLog" : file,
                DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-ms"));
            string logFile = Path.Combine(logPath, logFileName);

            if (File.Exists(logFile))
                File.Delete(logFile);

            sw = new StreamWriter(logFile, false, Encoding.UTF8);
        }

        private static void LogFlush()
        {
            if (null != sw)
            {
                sw.Flush();
                sw.Close();
                sw = null;
            }
        }

        private static void Print(string output)
        {
            if (null == sw)
                SetLogFile();
            sw.WriteLine(output);
        }

        #endregion


        private void TestSSH()
        {
            string filePath = @"\\10.0.0.26\美术\00_Project\Last Shelter\2021新整理  美术资产目录\3D场景\大本皮肤+运兵车+宝箱\女神雕像\FBX\女神雕像.FBX"; // 替换为你的网络共享路径

            if (File.Exists(filePath))
            {
                Debug.Log("文件存在。");
            }
            else
            {
                Debug.Log("文件不存在。");
            }
            
        }
        static GameObject NewSkeletonGraphicGameObject(string gameObjectName)
        {
            var go = EditorInstantiation.NewGameObject(gameObjectName, true, typeof(RectTransform), typeof(CanvasRenderer), typeof(SkeletonGraphic));
            var graphic = go.GetComponent<SkeletonGraphic>();
            graphic.material = SkeletonGraphicInspector.DefaultSkeletonGraphicMaterial;
            graphic.additiveMaterial = SkeletonGraphicInspector.DefaultSkeletonGraphicAdditiveMaterial;
            graphic.multiplyMaterial = SkeletonGraphicInspector.DefaultSkeletonGraphicMultiplyMaterial;
            graphic.screenMaterial = SkeletonGraphicInspector.DefaultSkeletonGraphicScreenMaterial;
            return go;
        }

        // [MenuItem("Tools/通用工具/spine", false, 1)]
        // public static void ProcessSpine()
        // {
        //     string modelPath = "Assets/Art/spineTest";
        //     string[] skeletonFiles = Directory.GetFiles(modelPath, "*.asset", SearchOption.AllDirectories);
        //     foreach (var skeleton in skeletonFiles)
        //     {
        //         SkeletonDataAsset skeletonDataAsset = AssetDatabase.LoadAssetAtPath<SkeletonDataAsset>(skeleton);
        //         if(skeletonDataAsset == null) continue;
        //         string goName =  skeletonDataAsset.name.Replace("_SkeletonData", "");
        //         // var go = NewSkeletonGraphicGameObject(spineGameObjectName);
        //         // var graphic = go.GetComponent<SkeletonGraphic>();
        //         // graphic.skeletonDataAsset = skeletonDataAsset;
        //         //
        //         // SkeletonData data = skeletonDataAsset.GetSkeletonData(true);
        //         //
        //         // if (data == null)
        //         // {
        //         //     for (int i = 0; i < skeletonDataAsset.atlasAssets.Length; i++)
        //         //     {
        //         //         string reloadAtlasPath = AssetDatabase.GetAssetPath(skeletonDataAsset.atlasAssets[i]);
        //         //         skeletonDataAsset.atlasAssets[i] =
        //         //             (AtlasAssetBase)AssetDatabase.LoadAssetAtPath(reloadAtlasPath, typeof(AtlasAssetBase));
        //         //     }
        //         //
        //         //     data = skeletonDataAsset.GetSkeletonData(true);
        //         // }
        //         //
        //         // var skin = data.DefaultSkin;
        //         // graphic.MeshGenerator.settings.zSpacing = SpineEditorUtilities.Preferences.defaultZSpacing;
        //         // graphic.startingLoop = SpineEditorUtilities.Preferences.defaultInstantiateLoop;
        //         // graphic.Initialize(false);
        //         // if (skin != null) graphic.Skeleton.SetSkin(skin);
        //         // graphic.initialSkinName = skin.Name;
        //         // graphic.Skeleton.UpdateWorldTransform();
        //         // graphic.UpdateMesh();
        //         
        //         SkeletonGraphic go = SkeletonGraphic.NewSkeletonGraphicGameObject(skeletonDataAsset,tra);
        //         go.transform.name = goName;
        //         string skeletonPath = Path.Combine(modelPath, go.name + ".prefab");
        //         PrefabUtility.SaveAsPrefabAsset(go.gameObject, skeletonPath);
        //        
        //         DestroyImmediate(go);
        //         AssetDatabase.Refresh();
        //     }
        // }


    }
}
