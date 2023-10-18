using System.Collections.Generic;
using AssetBundles;
using FrameWork.UI;
using GameManager;

public class GameEntry
{
    private static GameEntry _instance;

    public static GameEntry Instance()
    {
        if (_instance == null)
        {
            _instance = new GameEntry();
        }

        return _instance;
    }

    private List<GameBaseModule> _gameModulesList; 
    
    private GameEntry()
    {
        _gameModulesList = new List<GameBaseModule>(); 
    }
    

    private UIComponent _uiComponent;

    public UIComponent UIComponent
    {
        get;
    }
    

    public void Init()
    {
        string localPath = Utility.GetStreamingAssetsDirectory();
        ResourceManager.Instance.Initialize(localPath,null,null,AssetBundleManager.LoadMode.Local); // 暂时写到这里，后续添加启动状态机之后，挪到状态机里
        
        _uiComponent = new UIComponent(_gameModulesList);
  
        foreach (var item in _gameModulesList)
        {
            item.Init();
        }
    }

    public void Update()
    {
        foreach (var item in _gameModulesList)
        {
            item.Update();
        }
    }

    public void Dispose()
    {
        foreach (var item in _gameModulesList)
        {
            item.Dispose();
        }
        
    }
}