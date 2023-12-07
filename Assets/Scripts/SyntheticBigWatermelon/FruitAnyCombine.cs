using UnityEngine;

namespace SyntheticBigWatermelon
{
    public class FruitAnyCombine:FruitBase
    {
        protected override bool GetIfCanCombine(Collision2D other, FruitBase fruit)
        {
            Debug.Log("AbyCombine other==="+ other.transform.name);
            FruitBase colliderFruit = other.transform.GetComponent<FruitBase>();
            bool result = colliderFruit != null;
            Debug.Log("AbyCombine result==="+ result);
            return result;
        }
    }
}