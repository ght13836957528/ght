using System;

namespace Script.DecorativePattern
{
    public class CondimentDecorator : Beverage
    {
        private string _description = String.Empty;
        private double _costNum = 0.0;
        
        public override string GetDescription()
        {
            return _description;
        }

        public override double Cost()
        {
            return _costNum;
        }

    }
}