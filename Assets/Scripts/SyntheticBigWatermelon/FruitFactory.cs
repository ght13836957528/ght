using System.Collections.Generic;
using GameManager;
using UnityEngine;

namespace SyntheticBigWatermelon
{
    public class FruitFactory
    {
        private List<GameObject> _fruitList;
        private GameObject _fruitParent;
        private GameObject _fruitStartPoint;
        private GameObject _recoverParent;
        
        public FruitFactory(List<GameObject> fruitLIst ,GameObject parent, GameObject startPoint)
        {
            _fruitList = fruitLIst;
            _fruitParent = parent;
            _fruitStartPoint = startPoint;
           
        }

        private FruitConst.FruitType GetGenerateType( FruitConst.FruitType type)
        {
            FruitConst.FruitType generateType;
            if (type == FruitConst.FruitType.Default)
            {
                generateType= (FruitConst.FruitType)Random.Range(0, 2);
            }
            else
            {
                generateType = type;
            }

            return generateType;
        }

        public FruitBase GenerateFruit(FruitConst.FruitGenerateType generateType,Vector3 pos = default,FruitConst.FruitType type = FruitConst.FruitType.Default )
        {
            FruitConst.FruitType fruitType = GetGenerateType(type);
            GameObject i = _fruitList[(int)fruitType];
            GameObject fruitObj = GameObject.Instantiate(i,_fruitParent.transform);//实例化物体
            if (pos != default)
            {
                fruitObj.transform.position = pos;
            }
            else
            {
                fruitObj.transform.position = _fruitStartPoint.transform.position;
            }
            FruitBase fruit = fruitObj.GetComponent<FruitBase>();
            fruit.Init(fruitType,generateType);
            return fruit;
        }

       

    }
}