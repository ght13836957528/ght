using System.IO;
using UnityEngine;
using XLua;
#if UNITY_EDITOR
using UnityEditor;
using LS;
#endif

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

[LuaCallCSharp]
public class UILuaItem : MonoBehaviour
{
    #region 定义回调接口

    [CSharpCallLua]
    public delegate void delLuaOnAwakeHandler(LuaTable self);

    private delLuaOnAwakeHandler onAwake;

    [CSharpCallLua]
    public delegate void delLuaOnUpdateHandler(LuaTable self);

    private delLuaOnUpdateHandler onUpdate;

    [CSharpCallLua]
    public delegate void delLuaOnFixUpdateHandler(LuaTable self);

    private delLuaOnFixUpdateHandler onFixUpdate;

    [CSharpCallLua]
    public delegate void delLuaOnEnableHandler(LuaTable self);

    private delLuaOnEnableHandler onEnable;

    [CSharpCallLua]
    public delegate void delLuaOnDisableHandler(LuaTable self);

    private delLuaOnDisableHandler onDisable;

    [CSharpCallLua]
    public delegate void delLuaOnDestroyHandler(LuaTable self);

    private delLuaOnDestroyHandler onDestroy;

    [CSharpCallLua]
    public delegate void delLuaOnApplicationPauseHandler(LuaTable self, bool userData);

    private delLuaOnApplicationPauseHandler onApplicationPause;

    #endregion

#if UNITY_EDITOR
#if ODIN_INSPECTOR
    [OnValueChanged("OnSelectedTarget")]
#endif
    [Header("将选择的组件拖到下方搜索组件的绑定信息")]
    [SerializeField]
    private GameObject SearchTarget;

    private string AssetsFlag = "Assets";
#endif

    [BlackList] [Header("Lua组件分组")] public LuaComGroup[] LuaComGroups;

    // Lua文件名
    [BlackList] public string LuaClassName;

    // Lua文件完整路径
    [BlackList] public string LuaFilePath;

    private GameObject _nullObject = null;

    private LuaTable scriptEnv = null;

    #region MonoBehaviour框架接口

    protected void Update()
    {
        onUpdate?.Invoke(this.scriptEnv);
    }

    private void FixedUpdate()
    {
        onFixUpdate?.Invoke(this.scriptEnv);
    }

    private void OnEnable()
    {
        onEnable?.Invoke(this.scriptEnv);
    }

    private void OnDisable()
    {
        onDisable?.Invoke(this.scriptEnv);
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        onApplicationPause?.Invoke(this.scriptEnv, pauseStatus);
    }

    private void OnDestroy()
    {
        onDestroy?.Invoke(this.scriptEnv);

        onAwake = null;
        onUpdate = null;
        onFixUpdate = null;
        onEnable = null;
        onDisable = null;
        onDestroy = null;
        onApplicationPause = null;

        if (null != scriptEnv)
        {
            scriptEnv.Set("gameObject", _nullObject);
            scriptEnv.Set("transform", _nullObject);
            //scriptEnv.Set("self", _nullObject);
        }

        //LuaManager.Instance.LuaEnv.Global.Set(LuaFilePath, _nullObject);

        ClearLuaComs();

        if (null != scriptEnv)
        {
            scriptEnv.Dispose();
            scriptEnv = null;
        }
    }

    #endregion

    /// <summary>
    /// 获得绑定的组件
    /// </summary>
    /// <returns></returns>
    public LuaTable GetLuaItem()
    {
        return this.scriptEnv;
    }

    /// <summary>
    /// 绑定UI组件
    /// </summary>
    /// <returns></returns>
    public void Bind(LuaTable table, object param)
    {
        if (null == table)
            return;

        if (null == LuaModule.Instance.Env)
            return;

        if (null != scriptEnv)
        {
            this.OnDestroy();
        }

        scriptEnv = table;

        scriptEnv.Set("gameObject", gameObject);
        scriptEnv.Set("transform", transform);

        int len = LuaComGroups.Length;
        for (int i = 0; i < len; i++)
        {
            LuaComGroup group = LuaComGroups[i];
            int lenCom = group.LuaComs.Length;
            for (int j = 0; j < lenCom; j++)
            {
                LuaCom com = group.LuaComs[j];
                scriptEnv.Set(com.Name, com.ComObj);
            }
        }

        onAwake = scriptEnv.Get<delLuaOnAwakeHandler>("Awake");
        onUpdate = scriptEnv.Get<delLuaOnUpdateHandler>("Update");
        onFixUpdate = scriptEnv.Get<delLuaOnFixUpdateHandler>("FixUpdate");
        onEnable = scriptEnv.Get<delLuaOnEnableHandler>("OnEnable");
        onDisable = scriptEnv.Get<delLuaOnDisableHandler>("OnDisable");
        onDestroy = scriptEnv.Get<delLuaOnDestroyHandler>("OnDestroy");
        onApplicationPause = scriptEnv.Get<delLuaOnApplicationPauseHandler>("OnApplicationPause");

        onAwake?.Invoke(scriptEnv);
    }

    /// <summary>
    /// 清理UI组件
    /// </summary>
    private void ClearLuaComs()
    {
        int len = LuaComGroups.Length;
        for (int i = 0; i < len; i++)
        {
            LuaComGroup group = LuaComGroups[i];
            int lenCom = group.LuaComs.Length;
            for (int j = 0; j < lenCom; j++)
            {
                LuaCom com = group.LuaComs[j];
                com.ComObj = null;

                if (null != scriptEnv)
                    scriptEnv.Set(com.Name, com.ComObj);

                com = null;
            }

            group = null;
        }
    }

    /// <summary>
    /// 获得Lua文件名
    /// </summary>
    /// <returns></returns>
    private string GetLuaFileName()
    {
        string luaFile = string.Empty;

        if (!Application.isEditor)
        {
            luaFile = this.LuaClassName;
        }
        else
        {
            //string file = Path.Combine(Application.dataPath, this.LuaFilePath.Replace(AssetsFlag, "")).Replace("\\", "/");
            if (!File.Exists(this.LuaFilePath))
            {
                var fileName = string.IsNullOrEmpty(this.LuaFilePath) ? this.LuaClassName : this.LuaFilePath;
                Debug.LogError($"The file {fileName} is not find, please check...");
            }
            else
            {
                luaFile = this.LuaFilePath;
            }
        }

        return luaFile;
    }

#if UNITY_EDITOR

    #region UNITY_EDITOR 模式下的代码

    [BlackList] public string uiComment;

#if ODIN_INSPECTOR
    [Button(ButtonSizes.Large)]
#endif
    private void OutputLuaUIFile()
    {
        this.outputLuaUIFile();
    }

    [BlackList] public string origionalLuaFilePath;
#if ODIN_INSPECTOR
    [Button(ButtonSizes.Large)]
#endif
    private void AutoBindCompontent()
    {
        string luaFilePath = origionalLuaFilePath;
        if (string.IsNullOrEmpty(luaFilePath))
        {
            luaFilePath = Path.Combine(Application.dataPath, "Main/Lua" ,LuaFilePath);
        }
        LuaComGroups = UILuaComBinding.AutoBindCompontent(transform, luaFilePath, LuaComGroups);
        EditorUtility.SetDirty(this);
    }


    // 输出UI文件
    private void outputLuaUIFile()
    {
        string luaFileName = getLuaFileName();
        if (string.IsNullOrEmpty(luaFileName))
        {
            EditorUtility.DisplayDialog("提示", "请切换到编辑模式再保存!", "OK");
            return;
        }

        luaFileName = Selection.activeObject.name;

        // 用户自己选择输出路径，默认在指定lua目录下
        string title = "导出Lua文件";
        string luaDir = Path.Combine(Application.dataPath, Constant.Lua.UILuaExportPath);

        // 如果文件存在,那么覆盖
        if (!string.IsNullOrEmpty(this.LuaFilePath) && File.Exists(this.LuaFilePath))
        {
            int index = this.LuaFilePath.LastIndexOf("/");
            luaDir = this.LuaFilePath.Substring(0, index);

            title = "增量覆盖已有文件";
        }
        else
        {
            var lastOpenPath = PlayerPrefs.GetString(Constant.Keys.OpenLuaGenerateCodePath);
            if (!string.IsNullOrEmpty(lastOpenPath))
                luaDir = lastOpenPath;
        }

        string path = EditorUtility.SaveFilePanel(title, luaDir, luaFileName, Constant.Lua.LuaFileExtensionName);
        if (path.Length > 0)
        {
            var assetFilePath = this.GetAssetsFile(path);
            this.LuaFilePath = assetFilePath.Replace(Constant.Lua.LuaRootPath, "");
            this.LuaClassName = Path.GetFileNameWithoutExtension(path);
            this.LuaClassName = Path.GetFileNameWithoutExtension(this.LuaClassName);

            if (File.Exists(path))
            {
                LuaItemCodeOverwrite over = new LuaItemCodeOverwrite();
                over.generate(this, path);
            }
            else
            {
                LuaItemCodeGen gen = new LuaItemCodeGen();
                gen.generate(this, path);
            }

            int index = path.LastIndexOf("/");
            var lastPath = path.Substring(0, index);
            PlayerPrefs.SetString(Constant.Keys.OpenLuaGenerateCodePath, lastPath);
            PlayerPrefs.Save();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }

    // 获取lua输出路径
    private string getLuaFileName()
    {
        string luaFileName = string.Empty;

        // 用ab的获取路径
        UnityEditor.SceneManagement.PrefabStage stage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
        if (stage != null)
            luaFileName = Path.GetFileNameWithoutExtension(stage.assetPath);

        return luaFileName;
    }

    /// <summary>
    /// 选择目标
    /// </summary>
    private void OnSelectedTarget()
    {
        if (null == SearchTarget)
            return;

        if (null == this.LuaComGroups || this.LuaComGroups.Length <= 0)
            return;

        // 目标实例ID;
        int targetInstanceId = SearchTarget.GetInstanceID();

        bool isFinded = false;
        int tempInstanceId = 0;

        for (int i = 0; i < this.LuaComGroups.Length; i++)
        {
            for (int j = 0; j < this.LuaComGroups[i].LuaComs.Length; j++)
            {
                tempInstanceId = this.LuaComGroups[i].LuaComs[j].GetGameObjectInstanceId();

                if (tempInstanceId != -1 && tempInstanceId == targetInstanceId)
                {
                    isFinded = true;
                    Debug.LogErrorFormat("Group:" + this.LuaComGroups[i].Name + ", Name:" + this.LuaComGroups[i].LuaComs[j].Name + ", Type:" +
                                         this.LuaComGroups[i].LuaComs[j].Type.ToString());
                }
            }
        }

        if (!isFinded)
            Debug.LogError("此组件没有绑定...");
    }

    private string GetAssetsFile(string fileName)
    {
        int index = fileName.IndexOf(AssetsFlag);
        if (index > 0)
            return fileName.Substring(index);

        return fileName;
    }

#if ODIN_INSPECTOR
    [Button(ButtonSizes.Large)]
#endif
    private void OpenInCodeEditor()
    {
        // UILuaComBinding.OpenInCodeEditor(LuaFilePath);
    }
    
    #endregion

#endif
}
