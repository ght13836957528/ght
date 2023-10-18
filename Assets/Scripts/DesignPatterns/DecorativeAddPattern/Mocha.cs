namespace Script.DecorativeAddPattern
{
    public class Mocha : CondimentDecorator
    {

        public override string GetDescription()
        {
            return " Mocha";
        }

        public override double Cost()
        {
            return 0.2;
        }
    }
}