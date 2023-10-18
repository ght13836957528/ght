using System;

namespace Script.DecorativeAddPattern
{
    public class CondimentDecorator
    {
        private string _description = String.Empty;
        private double _costNum = 0.0;
        
        public  virtual string GetDescription()
        {
            return _description;
        }

        public  virtual double Cost()
        {
            return _costNum;
        }

    }
}