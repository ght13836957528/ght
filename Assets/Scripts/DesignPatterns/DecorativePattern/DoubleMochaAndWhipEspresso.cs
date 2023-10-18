namespace Script.DecorativePattern
{
    public class DoubleMochaAndWhipEspresso : Beverage
    {
        private Beverage _beverage;
        public DoubleMochaAndWhipEspresso()
        {
            _beverage = new Espresso();
            _beverage = new Mocha(_beverage);
            _beverage = new Mocha(_beverage);
            _beverage = new Whip(_beverage);
        }

        public override double Cost()
        {
            return _beverage.Cost();
        }

        public override string GetDescription()
        {
            return _beverage.GetDescription();
        }


    }
}