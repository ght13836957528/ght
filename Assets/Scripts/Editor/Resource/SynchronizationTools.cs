using System.IO;
using UnityEditor;
using UnityEngine;

namespace Main.Scripts.Editor.Resource
{
    public class SynchronizationTools : EditorWindow
    {
        string sourceFolder = ""; // 源文件夹路径
        string targetFolder = "Assets/Art/Sprites/SynchronizationToolsTest"; 
        bool imageSelected = false;
        
        [MenuItem("Tools/通用工具/资源同步", false, 2)]
        static void SynchronizationResource()
        {
            GetWindow<SynchronizationTools>("资源同步");
        }
        
        
        void OnGUI()
        {
            GUILayout.Label("资源同步", EditorStyles.boldLabel);
            sourceFolder = EditorGUILayout.TextField("CocosProjectRoot", sourceFolder);
            
            
            GUILayout.Label("勾选同步的资源类型");
            imageSelected = EditorGUILayout.Toggle("PNG", imageSelected);

            if (GUILayout.Button("开始同步"))
            {
                ExecuteCopyFiles();
            }
        }

        void ExecuteCopyFiles()
        {
            sourceFolder = Path.Combine(sourceFolder, "IF");
            CopyFiles(sourceFolder,targetFolder);
        }


        void CopyFiles(string sourceDir, string targetDir)
        {
            if (sourceDir.ToLower().Contains("spine"))
            {
                Debug.Log($"Skipping folder: {sourceDir}");
                return;
            }

            if (!Directory.Exists(sourceDir))
            {
                Debug.LogError($"Source folder does not exist: {sourceDir}");
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

                    if (!File.Exists(targetFilePath))
                    {
                        File.Copy(sourceFilePath, targetFilePath);
                        Debug.Log($"Copied {fileName} to {targetDir}");
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
            var extensions = new System.Collections.Generic.List<string>();
            if (imageSelected) 
                extensions.Add(".png");
            // TODO 可扩展多种类型
            return extensions.ToArray();
        }
    }
}