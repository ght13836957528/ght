using System.Collections.Generic;

public class GameBaseModule
{
    private List<GameBaseModule> _moduleList;
    public GameBaseModule(List<GameBaseModule> moduleList)
    {
        _moduleList = moduleList;
    }

    public void Init()
    {
        _moduleList.Add(this);
    }


    public void Update()
    {
        
    }

    public void Dispose()
    {
        
    }
}