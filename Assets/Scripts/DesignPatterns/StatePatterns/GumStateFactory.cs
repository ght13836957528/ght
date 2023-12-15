namespace DesignPatterns.StatePatterns
{
    public class GumStateFactory
    {
        public static IState GenerateGumState(GumStateEnum.StateType type)
        {
            IState state = new DefaultGumState();
            switch (type)
            {
                case GumStateEnum.StateType.Empty:
                    state = new EmptyState();
                    break;
                case GumStateEnum.StateType.InsertedCoins:
                    state = new InsertedCoinsState();
                    break;
                case GumStateEnum.StateType.SellGum:
                    state = new SellGumState();
                    break;
                case GumStateEnum.StateType.SellOut:
                    state = new SellOutState();
                    break;
            }
            return state;
        }
    }
}