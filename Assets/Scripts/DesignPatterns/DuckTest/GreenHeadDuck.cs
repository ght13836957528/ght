
    public class GreenHeadDuck : Duck
    {
        public GreenHeadDuck()
        {
            _flyBehavior = new FlyNoWings();
            _quackBehavior = new QuackLoudly();
        }

        public void SetFlyBehaviour(FlyBehavior flyBehavior)
        {
            _flyBehavior = flyBehavior;
        }
    }
