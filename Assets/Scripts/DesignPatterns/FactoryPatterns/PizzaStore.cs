namespace DesignPatterns.FactoryPatterns
{
    public class PizzaStore
    {
        public void OrderPizza(Pizza.PizzaType type)
        {
            Pizza orderPizza = PizzaFactory.CreatePizza(type);
            orderPizza.Cook();
            orderPizza.Box();
        }
    }
}