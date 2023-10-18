namespace Script.DecorativePattern
{
    public class Mocha : CondimentDecorator
    {
        private Beverage _beverage;
        public Mocha(Beverage beverage)
        {
            _beverage = beverage;
        }

        public override string GetDescription()
        {
            return _beverage.GetDescription() + " Mocha";
        }

        public override double Cost()
        {
            return _beverage.Cost() + 0.2;
        }
    }
}