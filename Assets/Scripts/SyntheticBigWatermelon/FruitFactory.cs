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
        public FruitFactory(List<GameObject> fruitLIst ,GameObject parent, GameObject startPoint)
        {
            _fruitList = fruitLIst;
            _fruitParent = parent;
            _fruitStartPoint = startPoint;
        }

        public Fruit GenerateFruit()
        {
            GameObject i;
            if (_fruitList.Count >= 4)//判断总水果是否大于4个
            {
                int randomNumber = Random.Range(0, 4);
                 i = _fruitList[randomNumber];
            }
            else
            {
                int randomNumber = Random.Range(0, _fruitList.Count);
                 i = _fruitList[randomNumber];
            }
            GameObject fruitObj = GameObject.Instantiate(i,_fruitParent.transform);//实例化物体
            fruitObj.transform.position = _fruitStartPoint.transform.position;
            Fruit fruit = fruitObj.GetComponent<Fruit>();
            fruit.SetSimulate(false);
            return fruit;
        }
    }
}