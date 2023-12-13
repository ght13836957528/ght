namespace SyntheticBigWatermelon
{
    public static class FruitConst
    {
        public enum FruitType
        {
            Default = -1,
            Cherry,
            Grape,
            Lemon,
            Orange,
            AnyCombine,
        }

        public enum FruitState
        {
            Default,
            StandBy,
            Dropping,
            Collision,
        }
        
        public enum FruitGenerateType
        {
            Default,
            Generate,
            Combine,
        }

    }
}