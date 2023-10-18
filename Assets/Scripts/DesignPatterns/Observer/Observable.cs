using System.Collections.Generic;
/// <summary>
/// 观察者模式中的被观察者类
/// </summary>

public class Observable
{
    private List<Observer> _observerList;
    private bool _mChanged = false;

    protected Observable()
    {
        _observerList = new List<Observer>();
    }

    public void RegisterObserver(Observer observer)
    {
        _observerList.Add(observer);
    }

    public void RemoveObserver(Observer observer)
    {
        _observerList.Remove(observer);
    }

    private void NotifyObservers()
    {
        if (_mChanged)
        {
            foreach (var item in _observerList)
            {
                item.Update();
            }
        }
    }
    
    protected void SetChanged()
    {
        _mChanged = true;
        NotifyObservers();
    }
}