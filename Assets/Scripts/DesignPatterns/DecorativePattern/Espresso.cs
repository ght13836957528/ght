namespace Script.DecorativePattern
{
    public class Espresso : Beverage
    {
        private int _size;
        public override string GetDescription()
        {
            return "Espresso";
        }

        public override double Cost()
        {
            return  1.0;
        }
    }
}