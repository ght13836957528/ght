namespace Script.DecorativeAddPattern
{
    public class DoubleMochaAndWhipEspresso : Beverage
    {
        private Beverage _beverage;
        public DoubleMochaAndWhipEspresso()
        {
            _beverage = new Espresso();
            CondimentDecorator mocha1 = new Mocha();
            CondimentDecorator mocha2 = new Mocha();
            CondimentDecorator whip1 = new Whip();
            _beverage.AddCondiment(mocha1);
            _beverage.AddCondiment(whip1);
            _beverage.AddCondiment(mocha2);
        }

        public override double Cost()
        {
            return _beverage.Cost();
        }

        public override string GetDescription()
        {
            return _beverage.GetDescription();
        }

        public override void AddCondiment (CondimentDecorator decorator)
        {
            
        }
    }
}