using System;

namespace Script.DecorativeAddPattern
{
    public abstract class Beverage
    {
        public abstract string GetDescription();
        
        public abstract double Cost();

        public abstract void AddCondiment(CondimentDecorator decorator);
    }
}