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
        private List<FruitBase> _fruitBaseList;
        public FruitFactory(List<GameObject> fruitLIst ,GameObject parent, GameObject startPoint)
        {
            _fruitList = fruitLIst;
            _fruitParent = parent;
            _fruitStartPoint = startPoint;
            _fruitBaseList = new List<FruitBase>();
        }

        private FruitConst.FruitType GetGenerateType( FruitConst.FruitType type)
        {
            FruitConst.FruitType generateType;
            if (type == FruitConst.FruitType.Default)
            {
                if (_fruitList.Count >= 4)//判断总水果是否大于4个
                {
                    generateType= (FruitConst.FruitType)Random.Range(0, 4);
                }
                else
                {
                    generateType = (FruitConst.FruitType)Random.Range(0, _fruitList.Count);
                }
            }
            else
            {
                generateType = type;
            }

            return generateType;
        }

        public FruitBase GenerateFruit(Vector3 pos = default,FruitConst.FruitType type = FruitConst.FruitType.Default)
        {
            FruitConst.FruitType generateType = GetGenerateType(type);
            GameObject i = _fruitList[(int)generateType];
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
            fruit.Init(generateType);
            fruit.SetSimulate(false);
            _fruitBaseList.Add(fruit);
            return fruit;
        }

        public List<FruitBase> GetFruitList()
        {
            return _fruitBaseList;
        }

    }
}