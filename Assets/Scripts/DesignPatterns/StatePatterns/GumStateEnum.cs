using System;

namespace DesignPatterns.StatePatterns
{
    public static class GumStateEnum
    {
        public enum StateType
        {
            Default,
            Empty,
            InsertedCoins,
            SellGum,
            SellOut,
        }
    }
}