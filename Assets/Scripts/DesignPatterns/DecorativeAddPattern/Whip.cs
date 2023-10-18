namespace Script.DecorativeAddPattern
{
    public class Whip : CondimentDecorator
    {
        
        public override string GetDescription()
        {
            return  " Whip ";
        }

        public override double Cost()
        {
            return 0.2;
        }
    }
}