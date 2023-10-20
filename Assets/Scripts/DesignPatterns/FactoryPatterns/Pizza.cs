namespace DesignPatterns.FactoryPatterns
{
    public abstract class Pizza
    {
        public enum PizzaType
        {
            Cheese,
            Durian,
            Beef,
        }

        public abstract void Cook();

        public abstract void Box();
        
        
    }
}