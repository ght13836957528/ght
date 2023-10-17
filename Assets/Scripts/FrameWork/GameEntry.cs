using System.Collections.Generic;
using FrameWork.ResourceManager;
using FrameWork.UI;

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
    
    private ResourceManager _resourceManager;

    public ResourceManager ResourceManager
    {
        get;
    }

    public void Init()
    {
        _uiComponent = new UIComponent(_gameModulesList);
        _resourceManager = new ResourceManager(_gameModulesList);
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