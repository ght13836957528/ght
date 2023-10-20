using UnityEngine;

namespace DesignPatterns.FactoryPatterns
{
    public class NYPizzaStore : PizzaStore
    {
        public override Pizza CreatePizza(Pizza.PizzaType type)
        {
            Pizza pizza = null;
            switch (type)
            {
                case Pizza.PizzaType.Cheese:
                {
                    pizza = PizzaFactory.CreatePizza(Pizza.PizzaType.Cheese);
                    break;
                }
                case Pizza.PizzaType.Beef:
                {
                    pizza = PizzaFactory.CreatePizza(Pizza.PizzaType.Beef);
                    break;
                }
            }
            return pizza;
        }

        public override void AddLocalSource()
        {
            Debug.Log("add NY source");
        }
    }
}