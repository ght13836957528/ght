namespace DesignPatterns.FactoryPatterns
{
    public class PizzaFactory
    {
        public static Pizza CreatePizza(Pizza.PizzaType type)
        {
            Pizza orderPizza = null;
            switch (type)
            {
                case Pizza.PizzaType.Cheese:
                {
                    orderPizza = new CheesePizza();
                    break;
                }

                case Pizza.PizzaType.Beef:
                {
                    orderPizza = new BeefPizza();
                    break;
                }

            }

            return orderPizza;
        }
    }
}