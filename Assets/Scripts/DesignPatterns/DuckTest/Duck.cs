using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Duck
{
    protected FlyBehavior _flyBehavior;
    protected QuackBehavior _quackBehavior;
    
    public void Fly()
    {
        _flyBehavior.Fly();
    }

    public void Quack()
    {
        _quackBehavior.Quack();
    }
}
