﻿namespace SyntheticBigWatermelon
{
    public static class FruitConst
    {
        public enum FruitType
        {
            Default = -1,
            Cherry = 0,
            Grape,
            Lemon,
            Orange,
        }

        public enum FruitState
        {
            Default,
            StandBy,
            Dropping,
            Collision,
        }

    }
}