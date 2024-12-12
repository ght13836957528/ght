using System;
using System.Collections;
using Framework;
using System.Collections.Generic;
using System.IO;
using Framework.Resource;
using LS;
using UnityEngine;
using UnityEngine.Assertions;

public class LuaFileLoader : Singleton<LuaFileLoader>
{
    private AssetBundle m_luaBundle;
    // private AssetBundle m_luaBundleHotFix;
    private const int MAX_LOAD_LUA_FRAME = 700;
    private TextAsset scriptText;
    // private Dictionary<string, byte[]> luaBytes = new Dictionary<string, byte[]>();
    private Dictionary<string, byte[]> luaFiles = new Dictionary<string, byte[]>();
    private float m_LastTime;
    public bool OpenHotFix = true;
    public void LoadLuaBundle(Action callback)
    {
        // GameLauncher.Instance.StartCoroutine(InitLoader(callback));
    }

    private IEnumerator InitLoader(Action callback)
    {
        m_LastTime = Time.realtimeSinceStartup;
       
        InitBundle();
        int index = 0;
        string[] assetNames = m_luaBundle.GetAllAssetNames();
        
      
        foreach (var name in assetNames)
        {
            scriptText = m_luaBundle.LoadAsset<TextAsset>(name);
            luaFiles.Add(name, scriptText?.bytes);
            // Debug.LogError($"luaName {name}");
            index++;
            if (index > MAX_LOAD_LUA_FRAME)
            {
                index -= MAX_LOAD_LUA_FRAME;
                yield return null;
            }
        }

        yield return null;
        Debug.LogWarningFormat("load lua cost total time1 : {0} ", Time.realtimeSinceStartup - m_LastTime);
        callback?.Invoke();
        Debug.LogWarningFormat("load lua cost total time2 : {0} ", Time.realtimeSinceStartup - m_LastTime);
    }


    public static string GetInternalDir()
    {
        return Application.persistentDataPath;
    }
    private void InitBundle(bool reload = false)
    {
        if (reload || m_luaBundle == null)
        {
            if (OpenHotFix)
            {
                string sanboxBundlePath = Path.Combine(GetInternalDir(),
                    Constant.Lua.AssetsLuaBundlePath);
                if (File.Exists(sanboxBundlePath)) 
                    m_luaBundle = AssetBundle.LoadFromFile(sanboxBundlePath);
                if (m_luaBundle != null) return;
            }
            string innerBundlePath = Path.Combine(Utility.GetStreamingAssetsDirectory(),
                Constant.Lua.AssetsLuaBundlePath);
            m_luaBundle = AssetBundle.LoadFromFile(innerBundlePath);
            Assert.IsNotNull(m_luaBundle, "Lua bundle init error");
        }
    }
    
    public byte[] GetLuaString(string luaName)
    {
        byte[] luaBytes = null;
        if (luaFiles.TryGetValue(luaName, out luaBytes))
            return luaBytes;
        
#if UNITY_EDITOR 
        luaBytes = System.Text.Encoding.UTF8.GetBytes(File.ReadAllText(luaName));
        // 增加了缓存， 支持lua界面即时刷新需要重新处理
        luaFiles.Add(luaName, luaBytes);
        return luaBytes;
#else
        // 这里基本没啥用 - -
        scriptText = null;
        scriptText = m_luaBundle.LoadAsset<TextAsset>(luaName);
        if (scriptText == null)  {
            luaName = Path.GetFileNameWithoutExtension(luaName);
            scriptText = m_luaBundle.LoadAsset<TextAsset>(luaName);
        }
        if (scriptText == null) return null;
        luaBytes = scriptText.bytes;
        luaFiles.Add(luaName, luaBytes);
        return luaBytes;
#endif
    }
}
