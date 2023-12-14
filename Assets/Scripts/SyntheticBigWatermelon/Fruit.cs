using DG.Tweening;
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
            if (FruitGenerateType == FruitConst.FruitGenerateType.Combine)
            {
                Vector3 beginSize = new Vector3(0.67f, 0.67f, 1);
                transform.localScale = beginSize;
                Sequence s = DOTween.Sequence();
                Vector3 bigSize = new Vector3(1.3f, 1.3f, 1);
                s.Append(transform.DOScale(bigSize,0.1f));
                Vector3 smallSize = new Vector3(1.0f, 1.0f, 1);
                s.Append(transform.DOScale(smallSize,0.1f));
            }
        }
        
        protected override void OnDispose()
        {
            
        }
    }
}