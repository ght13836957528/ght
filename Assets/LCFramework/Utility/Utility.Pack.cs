using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Framework
{
    public static partial class Utility
    {
        // 这个方法只能够在打包时使用，android包内资源要用www
        public static string GetCustomSaveFileValue(string key)
        {
            string pathInStreaming = Application.streamingAssetsPath + "/CUSTOMSAVE.txt";
            if (File.Exists(pathInStreaming) == false)
                return string.Empty;

            var content = File.ReadAllText(pathInStreaming);
            if (string.IsNullOrEmpty(content))
                return string.Empty;
                
            content = content.Trim('\r', '\n');
            var configs = new Dictionary<string, string>();
            string[] args = content.Split('|');
            for (int i = 0; i < args.Length; ++i)
            {
                string[] temp = args[i].Split(',');
                if (temp.Length == 2)
                {
                    //获取服务器配置键值对
                    string _key = temp[0];
                    string _val = temp[1];
                    configs[_key] = _val.Trim('\r');
                }
            }

            if (configs.TryGetValue(key, out var value))
                return value;
            else
                return string.Empty;
        }
        public static void CopyDirectory(string srcPath, string desPath, Func<string, bool> fliter = null)
        {
            string[] filenames = Directory.GetFileSystemEntries(srcPath);
            foreach (string file in filenames)
            {
                if (Directory.Exists(file))
                {
                    string desSubPath = Path.Combine(desPath, Path.GetFileNameWithoutExtension(file));
                    if (!Directory.Exists(desSubPath))
                        Directory.CreateDirectory(desSubPath);

                    CopyDirectory(file, desSubPath, fliter);
                }
                else
                {
                    if (!Directory.Exists(desPath))
                        Directory.CreateDirectory(desPath);
                    if(fliter != null && !fliter.Invoke(file)) continue;
                    string desFile = Path.Combine(desPath, Path.GetFileName(file));
                    if(File.Exists(desFile)) File.Delete(desFile);
                    File.Copy(file, desFile);
                }
            }
        }

        private static string StreamAssetPath = Utility.GetStreamingAssetsDirectory();
        public static byte[] ReadBytesInStreamAssetSync(string filepath)
        {
            #if UNITY_ANDROID
            if (filepath.IndexOf(StreamAssetPath, StringComparison.Ordinal) != -1)
            {
                WWW loadWWW = new WWW(filepath);
                while (!loadWWW.isDone)
                {

                }
                if(!loadWWW.error.IsNullOrEmpty())
                    return null;
                return loadWWW.bytes;
            }
            #endif
            if (File.Exists(filepath))
                return File.ReadAllBytes(filepath);
            return null;
        }
        
        public static string ReadAllTextInStreamAssetSync(string filepath)
        {
#if UNITY_ANDROID
            if (filepath.IndexOf(StreamAssetPath, StringComparison.Ordinal) != -1)
            {
                WWW loadWWW = new WWW(filepath);
                while (!loadWWW.isDone) { }
                if(!loadWWW.error.IsNullOrEmpty())
                    return "";
                return loadWWW.text;
            }
#endif
            if (File.Exists(filepath))
                return File.ReadAllText(filepath);
            return "";
        }
    }
}