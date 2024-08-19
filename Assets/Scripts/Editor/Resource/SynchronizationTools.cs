using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using Tools;
using UnityEditor;
using UnityEngine;

namespace Main.Scripts.Editor.Resource
{
    public class SynchronizationTools : EditorWindow
    {
        string _sourceFolder = ""; // 源文件夹路径
        string _hotUpdateSourceFolder = ""; // 动更文件夹路径
        bool _imageSelected;
        bool _innerSpriteSelected;
        bool _hotUpdateSpriteSelected;
        private bool _onlyCompare;
        private readonly List<string> _addImageList = new List<string>();

        private List<string> _cocosUnProperSourcePath = new List<string>()
        {
            "Assets/Art/Sprites/SynchronizationToolsTest/Common1",
            "Assets/Main/Arts/Sprites/Origin/Common/Common_1",
            "Assets/Main/Arts/Sprites/Origin/Common/Common_42",
            "Assets/Main/Arts/Sprites/Origin/Expack/AccountBindGuide",
            "Assets/Main/Arts/Sprites/Origin/Expack/Battle_soldier",
            "Assets/Main/Arts/Sprites/Origin/Expack/Battle_soldier2",
            "Assets/Main/Arts/Sprites/Origin/Expack/Common_12",
            "Assets/Main/Arts/Sprites/Origin/Expack/Common_15",

            "Assets/Main/Arts/Sprites/Origin/Expack/Common_1000",
            "Assets/Main/Arts/Sprites/Origin/Expack/Common_27",
            "Assets/Main/Arts/Sprites/Origin/Expack/Expack_119",
            "Assets/Main/Arts/Sprites/Origin/Expack/Expack_250",
            "Assets/Main/Arts/Sprites/Origin/Expack/spineScienceUnlock/effect",
            "Assets/Main/Arts/Sprites/Origin/Expack/spineScienceUnlock",
            "Assets/Main/Arts/Sprites/Origin/Expack/World_2",
            "Assets/Main/Arts/Sprites/Origin/Imperial/Imperial_250",
            "Assets/Main/Arts/Sprites/Origin/World/World_4",
           
        };

        [MenuItem("Tools/通用工具/资源同步", false, 2)]
        static void SynchronizationResource()
        {
            GetWindow<SynchronizationTools>("资源同步");
        }

        void OnGUI()
        {
            GUILayout.Label("资源同步", EditorStyles.boldLabel);
            _sourceFolder = EditorGUILayout.TextField("CocosProjectRoot", _sourceFolder);
            _hotUpdateSourceFolder = EditorGUILayout.TextField("Cocos dynamicResource Path", _hotUpdateSourceFolder);
            GUILayout.Label("勾选同步的资源类型");
            _imageSelected = EditorGUILayout.Toggle("PNG", _imageSelected);
            GUILayout.Label("是否只是生成对比文件，而不同步");
            _onlyCompare = EditorGUILayout.Toggle("only compare", _onlyCompare);

            GUILayout.Label("勾选同步的资源路径");
            _innerSpriteSelected = EditorGUILayout.Toggle("包内资源同步", _innerSpriteSelected);
            _hotUpdateSpriteSelected = EditorGUILayout.Toggle("热更路径资源同步", _hotUpdateSpriteSelected);

            if (GUILayout.Button("开始同步"))
            {
                ExecuteCopyFiles();
            }

            if (GUILayout.Button("检查文件夹是否存在文件夹与png同级情况"))
            {
                CheckPngAndFolders();
            }

            if (GUILayout.Button("检查没有图集的文件夹"))
            {
                CheckSpriteAtlasFiles();
            }
        }

        void ExecuteCopyFiles()
        {
            if (_innerSpriteSelected)
            {
                string targetFolder = "Assets/Art/Sprites/SynchronizationToolsTest";
                string sourceFolder = Path.Combine(_sourceFolder, "IF");
                CopyFiles(sourceFolder, targetFolder);
            }

            if (_hotUpdateSpriteSelected)
            {
                if (string.IsNullOrEmpty(_hotUpdateSourceFolder))
                {
                    Debug.LogError("Cocos dynamicResource Path is empty");
                    return;
                }

                string targetFolder = "Assets/Art/Sprites/SynchronizationToolsHotUpdateTest";
                CopyFiles(_hotUpdateSourceFolder, targetFolder);
            }


            Debug.Log("_addImageList length==" + _addImageList.Count);
            foreach (var path in _addImageList)
            {
                ArtistTools.SetTextureSettings(path);
            }

            LogFlush();
        }


        void CopyFiles(string sourceDir, string targetDir)
        {
            if (!Directory.Exists(sourceDir))
            {
                Debug.LogError($"Source folder does not exist: {sourceDir}");
                return;
            }

            if (sourceDir.ToLower().Contains("towerdefense"))
            {
                Debug.Log($"Skipping folder: {sourceDir}");
                return;
            }

            if (IsSpine(sourceDir))
            {
                Debug.Log($"Skipping folder: {sourceDir}");
                return;
            }
            
            if (!Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }
            
            string[] fileExtensions = GetSelectedExtensions();
            // 复制文件
            foreach (string extension in fileExtensions)
            {
                string[] files = Directory.GetFiles(sourceDir, $"*{extension}");
                foreach (string sourceFilePath in files)
                {
                    string fileName = Path.GetFileName(sourceFilePath);
                    string targetFilePath = Path.Combine(targetDir, fileName);
                    if (IfCocosUnProperSourcePath(sourceDir))
                    {
                        var directoryName = Path.GetFileName(targetDir);
                        if (!string.IsNullOrEmpty(directoryName))
                        {
                            targetFilePath = Path.Combine(targetDir, directoryName);
                            if (!Directory.Exists(targetFilePath))
                            {
                                Directory.CreateDirectory(targetFilePath);
                            }
                            targetFilePath = Path.Combine(targetFilePath, fileName);
                        }
                    }

                    if (!File.Exists(targetFilePath))
                    {
                        if (!_onlyCompare)
                        {
                            File.Copy(sourceFilePath, targetFilePath);
                            _addImageList.Add(targetFilePath);
                        }

                        Print(sourceFilePath);
                    }
                }
            }

            // 递归处理子目录
            string[] subDirs = Directory.GetDirectories(sourceDir);
            foreach (string subDir in subDirs)
            {
                string subDirName = Path.GetFileName(subDir);
                string targetSubDir = Path.Combine(targetDir, subDirName);
                CopyFiles(subDir, targetSubDir);
            }

            AssetDatabase.Refresh();
        }

        string[] GetSelectedExtensions()
        {
            var extensions = new List<string>();
            if (_imageSelected)
                extensions.Add(".png");
            // TODO 可扩展多种类型
            return extensions.ToArray();
        }

        bool IsSpine(string sourceDir)
        {
            string fileName = Path.GetFileName(sourceDir);
            string tpsFileName = "_alpha_" + fileName;
            var parentPath = Directory.GetParent(sourceDir)?.FullName;
            if (string.IsNullOrEmpty(parentPath))
                return false;
            string[] files = Directory.GetFiles(parentPath);
            string tpsFilePath = "";
            foreach (string filePath in files)
            {
                if (Path.GetFileNameWithoutExtension(filePath) == tpsFileName)
                {
                    tpsFilePath = filePath;
                }
            }

            if (string.IsNullOrEmpty(tpsFilePath))
                return false;
            string xmlContent = File.ReadAllText(tpsFilePath, System.Text.Encoding.UTF8);
            XmlDocument doc = new XmlDocument
            {
                XmlResolver = null
            };
            doc.LoadXml(xmlContent);
            bool result = false;
            XmlNode structNode = doc.SelectSingleNode("//struct");
            if (structNode != null)
            {
                foreach (var node in structNode.ChildNodes)
                {
                    if (node is XmlNode childNode && childNode.InnerText == "libgdx")
                    {
                        result = true;
                        break;
                    }
                }
            }

            return result;
        }

        bool IfCocosUnProperSourcePath(string sourceFilePath)
        {
            bool result = false;
            // foreach (var path in _cocosUnProperSourcePath)
            // {
            //     targetPath = targetPath.Replace("\\", "/");
            //     if (targetPath.Contains(path))
            //     {
            //         result = true;
            //         break;
            //     }
            // }
            bool containsPng = false;
            bool containsSubfolder = false;
            // 检查文件夹中的所有文件
            foreach (string file in Directory.GetFiles(sourceFilePath))
            {
                if (Path.GetExtension(file).ToLower() == ".png")
                {
                    containsPng = true;
                    break;
                }
            }
            // 检查文件夹中的所有子文件夹
           if(Directory.GetDirectories(sourceFilePath).Length > 0)
            {
                containsSubfolder = true;
                // 递归检查子文件夹
            }

            // 如果当前文件夹中同时存在 .png 文件和子文件夹
            if (containsPng && containsSubfolder)
            {
                result = true;
            }

            return result;
        }


        #region 处理日志

        private static StreamWriter sw = null;

        private static void SetLogFile(string file = null)
        {
            string logPath = "D:/output";

            if (!Directory.Exists(logPath))
                Directory.CreateDirectory(logPath);

            string logFileName = string.Format("{0}_{1}.log",
                string.IsNullOrEmpty(file) ? "SynchronizationToolsLog" : file,
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

        public static void CheckPngAndFolders()
        {
            // 提示用户选择一个文件夹
            string folderPath = EditorUtility.OpenFolderPanel("Select Folder", Application.dataPath, "");

            if (string.IsNullOrEmpty(folderPath))
            {
                Debug.LogWarning("No folder selected.");
                return;
            }

            // 递归检查文件夹
            CheckFolderForPngAndSubfolders(folderPath);
            LogFlush();
        }

        private static void CheckFolderForPngAndSubfolders(string folderPath)
        {
            bool containsPng = false;
            bool containsSubfolder = false;
            // 检查文件夹中的所有文件
            foreach (string file in Directory.GetFiles(folderPath))
            {
                if (Path.GetExtension(file).ToLower() == ".png")
                {
                    containsPng = true;
                    break;
                }
            }

            // 检查文件夹中的所有子文件夹
            foreach (string subfolder in Directory.GetDirectories(folderPath))
            {
                containsSubfolder = true;
                // 递归检查子文件夹
                CheckFolderForPngAndSubfolders(subfolder);
            }

            // 如果当前文件夹中同时存在 .png 文件和子文件夹
            if (containsPng && containsSubfolder)
            {
                Debug.Log($"Folder with PNG files and subfolders found: {folderPath}");
                Print(folderPath);
            }
        }


        public static void CheckSpriteAtlasFiles()
        {
            // 提示用户选择一个文件夹
            string folderPath = EditorUtility.OpenFolderPanel("Select Folder", Application.dataPath, "");
            if (string.IsNullOrEmpty(folderPath))
            {
                Debug.LogWarning("No folder selected.");
                return;
            }

            // 遍历并检查文件夹
            string[] subfolders = Directory.GetDirectories(folderPath);
            foreach (string subfolder in subfolders)
            {
                // 获取子文件夹的名称
                string folderName = Path.GetFileName(subfolder);
                string parentName = Directory.GetParent(subfolder)?.FullName;
                string spriteAtlasPath = Path.Combine(parentName, folderName + ".spriteatlas");
                if (!File.Exists(spriteAtlasPath))
                {
                    Debug.Log($"Missing .spriteatlas file in folder: {folderName}");
                    Print(folderName);
                }
            }

            LogFlush();
        }
    }
}