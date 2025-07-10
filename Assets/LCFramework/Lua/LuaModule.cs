using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using AssetBundles;
using Framework;
using LCFramework.Lua;
using UnityEngine;
using XLua;

public class LuaModule : GameBaseSingletonModule<LuaModule>
{
    
    public const string AssetsLuaPath = "Assets/Main/Lua";
    public const string AssetsLuaExt = ".lua.txt";

    
    [SerializeField]
    public bool m_Ready;

    // private const string AssetsLuaPath = "Assets/Main/Lua";

    // private const string dLUA_ROOT = "Lua";
    
 
    
    private LuaEnv m_LuaEnv;

    private LuaTable m_JsonUtil;
    private LuaFunction m_JsonDecode;

    [CSharpCallLua] public delegate void LuaAction();
    [CSharpCallLua] public delegate void LuaAction1(string msg);
    [CSharpCallLua] public delegate void LuaActionOnNetHandle(LuaTable table);
    [CSharpCallLua] public delegate void LuaActionOnWebSocketHandle(string msg);
    [CSharpCallLua] public delegate void LuaActionUpdate(float deltaTime, float unscaledDeltaTime);
    [CSharpCallLua] public delegate void LuaActionFixedUpdate(float fixedDeltaTime);
    [CSharpCallLua] public delegate void LuaActionOnApplicationFocus(bool focus);

    private LuaActionOnNetHandle m_OnNetHandle;
    private LuaActionOnWebSocketHandle m_OnSocketHandle;
    private LuaAction1 m_OnNetRest;
    private LuaActionUpdate m_Update;
    private LuaAction m_LateUpdate;
    private LuaActionFixedUpdate m_FixedUpdate;
    private LuaActionOnApplicationFocus m_OnApplicationFocus;
    private LuaAction m_OnApplicationQuit;

    private LuaAction m_OnLaunchStateEnter;
    private LuaAction m_OnLaunchStateUpdate;
    private LuaAction m_OnLaunchStateExit;

    private LuaAction m_OnClearNecessaryCache;
    
    public LuaAction OnLaunchStateEnter => m_OnLaunchStateEnter;
    public LuaAction OnLaunchStateUpdate => m_OnLaunchStateUpdate;
    public LuaAction OnLaunchStateExit => m_OnLaunchStateExit;

    public int Memory
    {
        get
        {
            return (int)m_LuaEnv?.Memory;
        }
    }

    public LuaGame luaGame
    {
        get;
        private set;
    }

    #region  Module Life Circle
    // 打开更新逻辑
    public override bool Updatable => true;
    public override bool LateUpdatable => true;
    public override bool FixedUpdatable => true;

    public override void Update(float elapsedTime, float realElapsedTime)
    {
        luaGame?.Update();
        m_Update?.Invoke(elapsedTime, realElapsedTime);
    }

    public override void LateUpdate()
    {
        luaGame?.LateUpdate();
        m_LateUpdate?.Invoke();
    }

    public override void FixedUpdate()
    {
        luaGame?.FixedUpdate();
        m_FixedUpdate?.Invoke(Time.fixedDeltaTime);
    }

    public override void OnApplicationFocus(bool focus)
    {
        luaGame?.OnApplicationFocus(focus);
        m_OnApplicationFocus?.Invoke(focus);
    }

    public override void OnApplicationQuit()
    {
        luaGame?.OnApplicationQuit();
        m_OnApplicationQuit?.Invoke();
    }

    #endregion

    #region XLua functions
    public LuaEnv Env
    {
        get
        {
            return m_LuaEnv;
        }
    }

    public LuaFunction GetFunction(string name)
    {
        return m_LuaEnv.Global.Get<LuaFunction>(name);
    }

    public LuaTable GetTable(string name)
    {
        return m_LuaEnv.Global.Get<LuaTable>(name);
    }

    #endregion
    
    // private float m_CostTime;


  

    private byte[] MyLoader(ref string filePath)
    {
#if UNITY_EDITOR
        if (filePath == "emmy_core")
        {
            Debug.Log("pass lua debug" + filePath);
            return null;
        }
        if (filePath == "LuaDebuggee")
        {
            Debug.Log("pass lua debug" + filePath);
            return null;
        }
#endif
        string fullPath = filePath.Replace(".", "/");
        fullPath = $"{AssetsLuaPath}/{fullPath}.lua.txt";;
        byte[] ret;
#if UNITY_EDITOR
        ret = LuaFileLoader.Instance.GetLuaString(fullPath);
#else
        ret = LuaFileLoader.Instance.GetLuaString(fullPath.ToLower());
#endif
        return ret;
    }

    private string GetLastPathName(string path)
    {
        if (path.LastIndexOf('.') == -1)
        {
            return path;
        }

        return path.Substring(path.LastIndexOf('.') + 1);
    }

    private void ResetDelegates()
    {
        m_OnNetHandle = null;
        m_OnSocketHandle = null;
        m_OnNetRest = null;
        m_Update = null;
        m_LateUpdate = null;
        m_FixedUpdate = null;
        m_OnApplicationFocus = null;
        m_OnApplicationQuit = null;

        m_OnLaunchStateEnter = null;
        m_OnLaunchStateUpdate = null;
        m_OnLaunchStateExit = null;
        
        m_OnClearNecessaryCache = null;
    }
    
    public override void Initialize()
    {
        if (m_LuaEnv != null)
        {
            ResetDelegates();
            m_LuaEnv.Dispose();
        }
        m_LuaEnv = new LuaEnv();
        
#if UNITY_EDITOR
        // m_LuaEnv.translator.debugDelegateBridgeRelease = true;
#endif
        
        m_LuaEnv.AddLoader(MyLoader);
    }

    private Action complete;
    public void LoadLuaScripts(Action complete = null)
    {
         this.complete = complete;
#if UNITY_EDITOR
        LoadLuaScriptsEnd();
#else
        LuaFileLoader.Instance.LoadLuaBundle(LoadLuaScriptsEnd);
#endif
    }

    private void LoadLuaScriptsEnd()
    {
        startLua();
        complete?.Invoke();
    }
    
    private void startLua()
    {
        m_Ready = true;
        m_LuaEnv.DoString("require 'Main'");
        m_LuaEnv.Global.Get<Action>("Start").Invoke();

        m_LuaEnv.Global.Get("Update", out m_Update);
        m_LuaEnv.Global.Get("LateUpdate", out m_LateUpdate);
        m_LuaEnv.Global.Get("FixedUpdate", out m_FixedUpdate);
        m_LuaEnv.Global.Get("OnNetHandleMessage", out m_OnNetHandle);
        m_LuaEnv.Global.Get("OnWebSocketHandleMessage", out m_OnSocketHandle);
        m_LuaEnv.Global.Get("OnNetReset", out m_OnNetRest);
        m_LuaEnv.Global.Get("OnApplicationFocus", out m_OnApplicationFocus);
        m_LuaEnv.Global.Get("OnApplicationQuit", out m_OnApplicationQuit);

        m_LuaEnv.Global.Get("OnLaunchStateEnter", out m_OnLaunchStateEnter);
        m_LuaEnv.Global.Get("OnLaunchStateUpdate", out m_OnLaunchStateUpdate);
        m_LuaEnv.Global.Get("OnLaunchStateExit", out m_OnLaunchStateExit);
    
        m_LuaEnv.Global.Get("OnClearNecessaryCache", out m_OnClearNecessaryCache);
        
        luaGame = m_LuaEnv.Global.Get<LuaGame>("Game");
    }

    // todo: 随后添加获取lua全局方法，各个功能模块缓存调用。
    public void handleWebSocketMessage(string msg) // 聊天消息
    {
        if (m_OnSocketHandle != null)
        {

            m_OnSocketHandle(msg);
            return;
        }

        Debug.LogError("m_OnSocketHandle null");
    }

    public void NetReset(string netType) //网络断开，lua的cmd列表清空
    {
        if (m_OnNetRest != null)
        {
            m_OnNetRest(netType);
            return;
        }

        Debug.Log("netReset null");
    }

    public void HandleNetMessage(LuaTable tbl) // 正常把消息抛给lua
    {
        if (m_OnNetHandle != null)
        {
            m_OnNetHandle(tbl);
            return;
        }
        Debug.LogError("eventOnNetHandleMessage null");
    }

    public void handleClearNecessaryCache()
    {
        if (m_OnClearNecessaryCache != null)
        {

            m_OnClearNecessaryCache();
            return;
        }

        Debug.LogError("m_OnClearNecessaryCache null");
    }
    
}
