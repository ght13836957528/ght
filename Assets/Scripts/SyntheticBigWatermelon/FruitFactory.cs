﻿using System.Collections.Generic;
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

        public Fruit GenerateFruit(Vector3 pos = default,FruitConst.FruitType type = FruitConst.FruitType.Default)
        {
            GameObject i;
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

            i = _fruitList[(int)generateType];
            GameObject fruitObj = GameObject.Instantiate(i,_fruitParent.transform);//实例化物体
            if (pos != default)
            {
                fruitObj.transform.position = pos;
            }
            else
            {
                fruitObj.transform.position = _fruitStartPoint.transform.position;
            }
            Fruit fruit = fruitObj.GetComponent<Fruit>();
            fruit.Init();
            fruit.SetSimulate(false);
            fruit.FruitType = generateType;
            return fruit;
        }
        
    }
}