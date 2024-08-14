using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace ImageTools
{
    public class ImageTools
    {
        #region 批量替换UI重复图片

        private static Dictionary<string, SpriteInfo> spritesCaches = new Dictionary<string, SpriteInfo>();
        private static Dictionary<string, SpriteInfo> repeatSprites = new Dictionary<string, SpriteInfo>();

        [MenuItem("Tools/美术工具/打印重名的UI图片", false, 63)]
        private static void PrintRepeatSpritesForUI()
        {
            SetLogFile("repeat_image");

            // 1.获得所有UI图片
            GetAllSprites();

            // 2.筛选重复图片
            GetRepeatSprites();

            LogFlush();

            if(EditorUtility.DisplayDialog("提示", "打印完毕,点击确定查看!", "OK"))
            {
                InternalOpenFolder("D:/output");
            }
        }

        [MenuItem("Tools/美术工具/批量替换UI重复图片", false, 64)]
        private static void ReplaceRepeatSpritesForUI()
        {
            // 1.获得所有UI图片
            GetAllSprites();

            // 2.筛选重复图片
            GetRepeatSprites();

            // 3.扫描prefab, 查找并替换重复图片
            ReplaceSprites();

            // 4.删除重复图片
            //DeleteRepeatSprites();

            LogFlush();

            if(EditorUtility.DisplayDialog("提示", "处理完毕,点击确定查看处理结果!", "OK"))
            {
                InternalOpenFolder("D:/output");
            }
        }

        private static void GetAllSprites()
        {
            spritesCaches.Clear();

            var spritePaths = GetSpritesPath();
            if (null == spritePaths || spritePaths.Count <= 0)
                return;

            for (int i = 0; i < spritePaths.Count; i++)
            {
                if (!Directory.Exists(spritePaths[i]))
                    continue;

                DirectoryInfo directoryInfo = new DirectoryInfo(spritePaths[i]);
                DirectoryInfo[] subDirectoryInfos = directoryInfo.GetDirectories();
                for (int j = 0; j < subDirectoryInfos.Length; j++)
                {
                    var fileInfos = subDirectoryInfos[j].GetFiles("*.png");
                    foreach (var item in fileInfos)
                    {
                        string spriteName = Path.GetFileNameWithoutExtension(item.Name);
                        if (!spritesCaches.ContainsKey(spriteName))
                            spritesCaches.Add(spriteName.Trim(), new SpriteInfo(item.FullName.Trim()));
                        else
                            spritesCaches[spriteName].Add(item.FullName.Trim());
                    }
                }
            }
        }

        private static void GetRepeatSprites()
        {
            repeatSprites.Clear();

            string md5;
            bool isSameSprite = true;
            
            foreach (var item in spritesCaches)
            {
                if (item.Value.spriteItemList.Count <= 1)
                    continue;

                md5 = item.Value.spriteItemList[0].GetSpriteMD5();

                if (string.IsNullOrEmpty(md5))
                    continue;

                isSameSprite = true;

                for (int i = 1; i < item.Value.spriteItemList.Count; i++)
                {
                    if (md5 != item.Value.spriteItemList[i].GetSpriteMD5())
                    {
                        isSameSprite = false;
                        break;
                    }
                }

                if (isSameSprite)
                {
                    repeatSprites.Add(item.Key, item.Value);

                    Print("repeat image => " + item.Key);

                    for(int i = 0; i < item.Value.spriteItemList.Count; i++)
                        Print("\t" + item.Value.spriteItemList[i].GetSpriteFile());

                    Print("");
                }
                else
                {
                    Print("different image content => " + item.Key);

                    for (int i = 0; i < item.Value.spriteItemList.Count; i++)
                        Print("\t" + item.Value.spriteItemList[i].GetSpriteFile());

                    Print("");
                }
            }
        }

        private static void ReplaceSprites()
        {
            if (repeatSprites.Count <= 0)
                return;

            var prefabPaths = GetPrefabsPath();
            if (null == prefabPaths || prefabPaths.Count <= 0)
                return;

            for (int i = 0; i < prefabPaths.Count; i++)
                HandleDirectory(prefabPaths[i]);
        }

        private static void HandleDirectory(string path)
        {
            if (!Directory.Exists(path))
                return;

            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            var fileInfos = directoryInfo.GetFiles("*.prefab");
            foreach (var item in fileInfos)
                HandlePrefab(item.FullName);

            DirectoryInfo[] subDirectoryInfos = directoryInfo.GetDirectories();
            if (null == subDirectoryInfos || subDirectoryInfos.Length <= 0)
                return;

            for (int i = 0; i < subDirectoryInfos.Length; i++)
                HandleDirectory(subDirectoryInfos[i].FullName);
        }

        private static void HandlePrefab(string file)
        {
            byte[] buffers = File.ReadAllBytes(file.Trim());
            string content = Encoding.UTF8.GetString(buffers);

            bool isChange = false;

            string targetGuid;
            string replaceGuid;

            Print("开始替换 => " + file);

            foreach (var item in repeatSprites)
            {
                if (null == item.Value || item.Value.spriteItemList.Count <= 1)
                    continue;

                targetGuid = item.Value.spriteItemList[0].GetSpriteGuid();

                for(int i = 1; i < item.Value.spriteItemList.Count; i++)
                {
                    replaceGuid = item.Value.spriteItemList[i].GetSpriteGuid();

                    if(content.Contains(replaceGuid))
                    {
                        content = content.Replace(replaceGuid, targetGuid);

                        isChange = true;

                        Print(string.Format("\t替换: {0} => {1}", item.Value.spriteItemList[i].GetSpriteFile(), item.Value.spriteItemList[0].GetSpriteFile()));
                    }
                }
            }

            // 没有变化,那么返回
            if (!isChange)
            {
                Print("无重复图片 => " + file);

                return;
            }

            // 获得要写入文件的字节buffer;
            byte[] replaceContent = Encoding.UTF8.GetBytes(content);

            // 创建并写入文件;
            FileStream fileWrite = new FileStream(file, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            fileWrite.Write(replaceContent, 0, replaceContent.Length);
            fileWrite.Flush();
            fileWrite.Close();
            fileWrite.Dispose();

            Print("替换完毕 => " + file);
        }

        private static void DeleteRepeatSprites()
        {
            foreach (var item in repeatSprites)
            {
                if (null == item.Value || item.Value.spriteItemList.Count <= 1)
                    continue;

                string file;

                for (int i = 1; i < item.Value.spriteItemList.Count; i++)
                {
                    file = item.Value.spriteItemList[i].GetSpriteFile();

                    AssetDatabase.DeleteAsset(file);

                    Print("删除文件 => " + file);
                }
            }
        }

        #endregion

        #region 重复图片替换成指定图片

        [MenuItem("Assets/美术工具/将prefab中引用的重复图片替换成本图片", false, 63)]
        private static void ReplaceThisSpriteForUI()
        {
            var guids = Selection.assetGUIDs;
            if (guids == null || guids.Length != 1)
            {
                EditorUtility.DisplayDialog("提示", "请选中一张UI图片!", "OK");
                return;
            }

            var asset = AssetDatabase.GUIDToAssetPath(guids[0]);
            if (string.IsNullOrEmpty(asset) || !File.Exists(asset))
            {
                EditorUtility.DisplayDialog("提示", "选择的文件不存在!", "OK");
                return;
            }

            if (!CheckAssets(asset))
                return;

            // 1.获取重复图片
            var spriteInfo = GetSpriteInfo(asset);
            if(null == spriteInfo)
            {
                EditorUtility.DisplayDialog("提示", "没有找到重复的图片!", "OK");
                return;
            }

            spriteInfo.Insert(asset, 0);

            repeatSprites.Clear();
            repeatSprites.Add(spriteInfo.GetSpriteName(), spriteInfo);

            // 2.扫描prefab并进行替换
            ReplaceSprites();

            LogFlush();

            EditorUtility.DisplayDialog("提示", "处理完毕", "OK");
        }

        private static SpriteInfo GetSpriteInfo(string file)
        {
            if (!File.Exists(file))
                return null;

            SpriteInfo spriteInfo = null;

            string targetFileName = Path.GetFileName(file);

            var spritePaths = GetSpritesPath();
            for(int i = 0; i < spritePaths.Count; i++)
            {
                if (!Directory.Exists(spritePaths[i]))
                    continue;

                TraverseFolder(spritePaths[i], targetFileName, file, ref spriteInfo);
            }

            return spriteInfo;
        }

        private static void TraverseFolder(string folder, string targetFileName, string orginFile, ref SpriteInfo spriteInfo)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(folder);
            var fileInfos = directoryInfo.GetFiles("*.png");
            if (null != fileInfos && fileInfos.Length > 0)
            {
                foreach (var item in fileInfos)
                {
                    if (targetFileName == item.Name 
                        && !item.FullName.Replace("\\", "/").Contains(orginFile))
                    {
                        if (null == spriteInfo)
                            spriteInfo = new SpriteInfo(item.FullName);
                        else
                            spriteInfo.Add(item.FullName);

                        break;
                    }
                }
            }

            DirectoryInfo[] subDirectoryInfos = directoryInfo.GetDirectories();
            for (int i = 0; i < subDirectoryInfos.Length; i++)
            {
                TraverseFolder(subDirectoryInfos[i].FullName, targetFileName, orginFile, ref spriteInfo);
            }
        }

        private static bool CheckAssets(string file)
        {
            if (!file.EndsWith(".png"))
            {
                EditorUtility.DisplayDialog("提示", "请选中一张PNG图片!", "OK");
                return false;
            }

            if (!file.Contains("Assets/Main/Arts/Sprites"))
            {
                EditorUtility.DisplayDialog("提示", "请选择Assets/Main/Arts/Sprites目录下的图片!", "OK");
                return false;
            }

            return true;
        }

        #endregion

        private static List<string> GetSpritesPath()
        {
            List<string> paths = new List<string>();
            paths.Add("Assets/Art/Sprites");
            // paths.Add("Assets/Main/ArtsDyre/Sprites");

            return paths;
        }

        private static List<string> GetPrefabsPath()
        {
            List<string> paths = new List<string>();
            paths.Add("Assets/Main/Arts/Prefabs");

            return paths;
        }

        /// <summary>
        /// 打开文件夹
        /// </summary>
        /// <param name="folder"></param>
        static void InternalOpenFolder(string folder)
        {
            folder = string.Format("\"{0}\"", folder);
            switch (Application.platform)
            {
                case RuntimePlatform.WindowsEditor:
                    Process.Start("Explorer.exe", folder.Replace('/', '\\'));
                    break;
                case RuntimePlatform.OSXEditor:
                    Process.Start("open", folder);
                    break;
                default:
                    throw new Exception(string.Format("Not support open folder on '{0}' platform.", Application.platform.ToString()));
            }
        }

        #region 处理日志

        private static StreamWriter sw = null;

        private static void SetLogFile(string file = null)
        {
            string logPath = "D:/output";

            if (!Directory.Exists(logPath))
                Directory.CreateDirectory(logPath);

            string logFileName = string.Format("{0}_{1}.log", string.IsNullOrEmpty(file) ? "handle_image" : file, DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-ms"));
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
    }

    #region 辅助类

    public class SpriteInfo
    {
        // 图片名称
        private string spriteName;

        public List<SpriteItem> spriteItemList = new List<SpriteItem>();

        public SpriteInfo(string fileFullName)
        {
            spriteName = Path.GetFileNameWithoutExtension(fileFullName);

            this.Add(fileFullName);
        }

        public void Add(string fileFullName)
        {
            if(spriteItemList.Count > 0)
            {
                var find = spriteItemList.Find(o => o.GetSpriteFile() == fileFullName);
                if(null == find)
                    spriteItemList.Add(new SpriteItem(fileFullName));
            }
            else
            {
                spriteItemList.Add(new SpriteItem(fileFullName));
            }
        }

        public void Insert(string fileFullName, int index)
        {
            if (spriteItemList.Count > 0 && index >= 0 && index < spriteItemList.Count)
            {
                var find = spriteItemList.Find(o => o.GetSpriteFile() == fileFullName);
                if (null == find)
                    spriteItemList.Insert(index, new SpriteItem(fileFullName));
            }
            else
            {
                spriteItemList.Add(new SpriteItem(fileFullName));
            }
        }

        public string GetSpriteName()
        {
            return this.spriteName;
        }
    }

    public class SpriteItem
    {
        private string spriteFile;
        private string spriteGuid;

        private string guidFlag = "guid:";

        public SpriteItem(string fileName)
        {
            this.spriteFile = fileName;
        }

        public string GetSpriteFile() 
        {
            return this.spriteFile;
        }
        
        public string GetSpriteGuid()
        {
            if (string.IsNullOrEmpty(this.spriteGuid))
            {
                string metaFile = this.spriteFile + ".meta";
                if (!File.Exists(metaFile))
                    return null;

                try
                {
                    using (FileStream fileStream = new FileStream(metaFile, FileMode.Open))
                    {
                        using (StreamReader reader = new StreamReader(fileStream))
                        {
                            string line;
                            while (!reader.EndOfStream)
                            {
                                line = reader.ReadLine();

                                if (!line.Contains(this.guidFlag))
                                    continue;

                                this.spriteGuid = line.Substring(line.IndexOf(this.guidFlag) + this.guidFlag.Length).Trim();

                                break;
                            }

                            reader.Close();
                            reader.Dispose();
                        }

                        fileStream.Close();
                        fileStream.Dispose();
                    }
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogErrorFormat("Read file throw exception,error:{0},file:{1}", e.Message, metaFile);

                    return null;
                }
            }
            
            return this.spriteGuid;
        }

        /// <summary>
        /// 获取文件的MD5码
        /// </summary>
        /// <returns></returns>
        public string GetSpriteMD5()
        {
            try
            {
                FileStream file = new FileStream(this.spriteFile, FileMode.Open);
                MD5 md5 = new MD5CryptoServiceProvider();
                byte[] retVal = md5.ComputeHash(file);
                file.Close();

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++)
                    sb.Append(retVal[i].ToString("x2"));

                return sb.ToString();
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogErrorFormat("get sprite's MD5 fail, file:{0}, error:{1}", this.spriteFile, ex.Message);
            }

            return null;
        }
    }

    #endregion
}