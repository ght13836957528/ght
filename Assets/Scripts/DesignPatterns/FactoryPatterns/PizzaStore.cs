namespace DesignPatterns.FactoryPatterns
{
    public abstract class PizzaStore
    {
        public void OrderPizza(Pizza.PizzaType type)
        {
            Pizza orderPizza = CreatePizza(type);
            AddLocalSource();
            orderPizza.Cook();
            orderPizza.Box();
        }
        public abstract Pizza CreatePizza(Pizza.PizzaType type);

        public abstract void AddLocalSource();
    }
}