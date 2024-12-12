using System.IO;
using Framework.UI;
using LS;
using UnityEngine;
using XLua;
#if UNITY_EDITOR
using UnityEditor;
#endif

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

[LuaCallCSharp]
public class UILuaForm : UIFormLogic
{
    #region 定义回调接口

    [CSharpCallLua]
    public delegate void delLuaOnCreateHandler(string modName, LuaTable uiComponents);
    private delLuaOnCreateHandler onCreate;

    [CSharpCallLua]
    public delegate void delLuaOnInitHandler(Transform transform, object userData);
    private delLuaOnInitHandler onInit;

    [CSharpCallLua]
    public delegate void delLuaOnOpenHandler(object userData);
    private delLuaOnOpenHandler onOpen;

    [CSharpCallLua]
    public delegate void delLuaOnCloseHandler(object userData);
    private delLuaOnCloseHandler onClose;

    [CSharpCallLua]
    public delegate void delLuaOnUpdateHandler(float elapseSeconds, float realElapseSeconds);
    private delLuaOnUpdateHandler onUpdate;

    [CSharpCallLua]
    public delegate void delLuaOnDestroyHandler();
    private delLuaOnDestroyHandler onDestroy;

    [CSharpCallLua]
    public delegate bool delLuaOnBackHandler();
    private delLuaOnBackHandler onBack;

    [CSharpCallLua]
    public delegate void delLuaOnApplicationPauseHandler(bool userData);
    private delLuaOnApplicationPauseHandler onApplicationPause;

    [CSharpCallLua]
    public delegate bool delCommonTopHelpBtn();
    private delCommonTopHelpBtn onLuaCommonTopHelpBtn;

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

    public int SerialId;

    // Lua文件名
    // [BlackList]
    public string LuaClassName;

    // Lua文件名
    [BlackList] public string LuaModName;

    // Lua文件完整路径
    [BlackList] public string LuaFilePath;

    private GameObject _nullObject = null;

    private LuaTable scriptEnv = null;
    private LuaTable uiComponents = null;

    #region 框架接口

    public override void OnInit(object userData)
    {
        base.OnInit(userData);

        SerialId = UIForm.SerialId;

        if (null == LuaModule.Instance.Env)
            return;

        if (null == scriptEnv)
        {
            var luaEnv = LuaModule.Instance.Env;

            byte[] luaChunk = LuaFileLoader.Instance.GetLuaString(Constant.Lua.UILuaBridgeFile);
            if (null == luaChunk || luaChunk.Length <= 0)
            {
                Debug.LogError($"lua file {Constant.Lua.UILuaBridgeFile} is error, please check lua config...");
                return;
            }

            scriptEnv = luaEnv.NewTable();

            LuaTable meta = luaEnv.NewTable();
            meta.Set("__index", luaEnv.Global);
            scriptEnv.SetMetaTable(meta);
            meta.Dispose();

            scriptEnv.Set("self", scriptEnv);

            SetLuaComs();

            luaEnv.DoString(luaChunk, "UILuaBridge", scriptEnv);

            onCreate = scriptEnv.Get<delLuaOnCreateHandler>("OnCreate");
            onInit = scriptEnv.Get<delLuaOnInitHandler>("OnInit");
            onOpen = scriptEnv.Get<delLuaOnOpenHandler>("OnOpen");
            onClose = scriptEnv.Get<delLuaOnCloseHandler>("OnClose");
            onDestroy = scriptEnv.Get<delLuaOnDestroyHandler>("OnDestroy");
            onBack = scriptEnv.Get<delLuaOnBackHandler>("OnBack");
            onApplicationPause = scriptEnv.Get<delLuaOnApplicationPauseHandler>("OnApplicationPause");

            onLuaCommonTopHelpBtn = scriptEnv.Get<delCommonTopHelpBtn>("onCommonTopHelpBtn");

            onCreate?.Invoke(LuaModName, this.uiComponents);
        }

        onInit?.Invoke(transform, userData);
    }

    public override void OnOpen(object userData)
    {
        base.OnOpen(userData);
        onOpen?.Invoke(userData);
    }

    public override void OnClose(object userData)
    {
        base.OnClose(userData);
        onClose?.Invoke(userData);
    }

    protected internal override void OnUpdate(float elapseSeconds, float realElapseSeconds)
    {
        base.OnUpdate(elapseSeconds, realElapseSeconds);
        onUpdate?.Invoke(elapseSeconds, realElapseSeconds);
    }

    protected internal override bool OnBack()
    {
        if (null == onBack)
            return base.OnBack();
        else
            return onBack.Invoke();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        onApplicationPause?.Invoke(pauseStatus);
    }
    

    private void OnDestroy()
    {
        onDestroy?.Invoke();

        onInit = null;
        onOpen = null;
        onClose = null;
        onDestroy = null;
        onBack = null;
        onApplicationPause = null;
        onLuaCommonTopHelpBtn = null;

        scriptEnv.Set("self", _nullObject);

        ClearLuaComs();

        if (scriptEnv != null)
        {
            scriptEnv.Dispose();
            scriptEnv = null;
        }
    }

    #endregion

    /// <summary>
    /// 绑定UI组件
    /// </summary>
    /// <returns></returns>
    private void SetLuaComs()
    {
        uiComponents = LuaModule.Instance.Env.NewTable();
        uiComponents.Set("gameObject", gameObject);
        uiComponents.Set("transform", transform);
        uiComponents.Set("mono", this);

        int len = LuaComGroups.Length;
        for (int i = 0; i < len; i++)
        {
            LuaComGroup group = LuaComGroups[i];
            int lenCom = group.LuaComs.Length;
            for (int j = 0; j < lenCom; j++)
            {
                LuaCom com = group.LuaComs[j];
                uiComponents.Set(com.Name, com.ComObj);
            }
        }
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
            //luaFile = this.LuaFilePath.Replace(".lua", ".txt");
        }
        else
        {
            string file = Path.Combine(Constant.Lua.LuaRootPath, this.LuaFilePath);
            if (!File.Exists(file))
            {
                Debug.LogError($"文件不存在: {file}");
            }
            else
            {
                luaFile = file;
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
    private void OutputLuaFile()
    {
        this.outputLuaFile();
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
    private void outputLuaFile()
    {
        string luaFileName = getLuaFileName();
        if (string.IsNullOrEmpty(luaFileName))
        {
            EditorUtility.DisplayDialog("提示", "请切换到编辑模式再保存!", "OK");
            return;
        }

        // 用户自己选择输出路径，默认在指定lua目录下
        string title = "导出Lua文件";
        string luaDir = Path.Combine(Application.dataPath, Constant.Lua.UILuaExportPath);

        // 如果文件存在,那么覆盖
        if (!string.IsNullOrEmpty(this.LuaFilePath))
        {
            var assetFile = Path.Combine(Constant.Lua.LuaRootPath, this.LuaFilePath);
            if (File.Exists(assetFile))
            {
                int index = assetFile.LastIndexOf("/");
                luaDir = assetFile.Substring(0, index);

                title = "增量覆盖已有文件";
            }
        }
        else
        {
            var lastOpenPath = PlayerPrefs.GetString(Constant.Keys.OpenLuaGenerateCodePath);
            if (!string.IsNullOrEmpty(lastOpenPath))
                luaDir = lastOpenPath;
        }

        luaFileName = luaFileName + (".lua.txt");
        string path = EditorUtility.SaveFilePanel(title, luaDir, luaFileName, Constant.Lua.LuaFileExtensionName);
        if (path.Length > 0)
        {
            var assetFilePath = this.GetAssetsFile(path);
            this.LuaFilePath = assetFilePath.Replace(Constant.Lua.LuaRootPath, "");
            this.LuaModName = this.LuaFilePath.Replace("/", ".").Replace($".{Constant.Lua.LuaFileExtensionName}", "");
            this.LuaClassName = Path.GetFileNameWithoutExtension(path);
            this.LuaClassName = Path.GetFileNameWithoutExtension(this.LuaClassName);

            if (File.Exists(path))
            {
                LuaCodeOverwrite over = new LuaCodeOverwrite();
                over.generate(this, path);
            }
            else
            {
                LuaCodeGen gen = new LuaCodeGen();
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
