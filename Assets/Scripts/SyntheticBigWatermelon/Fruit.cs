using UnityEngine;

namespace SyntheticBigWatermelon
{
    public class Fruit: FruitBase
    {
        protected override bool GetIfCanCombine(Collision2D other, FruitBase fruit)
        {
            FruitBase colliderFruit = other.transform.GetComponent<FruitBase>();
            if (colliderFruit == null || colliderFruit.FruitType == GameController.Instance.GetMaxFruitType())
                return false;
            return (colliderFruit != null && colliderFruit.FruitType == fruit.FruitType);
        }
        
        protected override void OnInt()
        {
            
        }
        
        protected override void OnDispose()
        {
            
        }
    }
}