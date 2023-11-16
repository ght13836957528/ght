using System;

namespace Script.DecorativeAddPattern
{
    /// <summary>
    /// 饮料抽象基类
    /// </summary>
    public abstract class Beverage
    {
        public abstract string GetDescription();
        
        public abstract double Cost();

        public abstract void AddCondiment(CondimentDecorator decorator);
    }
}