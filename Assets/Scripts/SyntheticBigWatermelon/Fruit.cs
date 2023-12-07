﻿using UnityEngine;

namespace SyntheticBigWatermelon
{
    public class Fruit: FruitBase
    {
        protected override bool GetIfCanCombine(Collision2D other, FruitBase fruit)
        {
            FruitBase colliderFruit = other.transform.GetComponent<FruitBase>();
            return (colliderFruit != null && colliderFruit.FruitType == fruit.FruitType);
        }
    }
}