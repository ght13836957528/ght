using UnityEngine;

namespace SyntheticBigWatermelon
{
    public class FruitAnyCombine:FruitBase
    {
        protected override bool GetIfCanCombine(Collision2D other, FruitBase fruit)
        {
            FruitBase colliderFruit = other.transform.GetComponent<FruitBase>();
            if (colliderFruit != null && colliderFruit.FruitType == GameController.Instance.GetMaxFruitType())
                return false;
            bool result = colliderFruit != null;
            return result;
        }

        protected override void OnInt()
        {
            
        }
        
        protected override void OnDispose()
        {
            
        }
    }
}