using System.Collections.Generic;

namespace Script.DecorativeAddPattern
{
    public class Espresso : Beverage
    {
        private List<CondimentDecorator> _decoratorsList;

        public Espresso()
        {
            _decoratorsList = new List<CondimentDecorator>();
        }

        public override string GetDescription()
        {
            string result = "Espresso";
            foreach (var item in _decoratorsList)
            {
                result = result + item.GetDescription();
            }
            return result;
        }

        public override double Cost()
        {
            double result = 1.0;
            foreach (var item in _decoratorsList)
            {
                result = result + item.Cost();
            }
            return result;
        }

        public override void AddCondiment(CondimentDecorator decorator)
        {
            _decoratorsList.Add(decorator);
        }
    }
}