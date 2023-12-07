using System;
using System.Collections.Generic;
using GameManager;
using UnityEngine;

namespace SyntheticBigWatermelon
{
    public class SaveFruit
    {
        public int fruitType;
        public Vector2 pos;
    }

    public class GameController : Singleton<GameController>
    {
        private FruitFactory _factory;
        private List<SaveFruit> _fruitList;
        private List<Fruit> _fruitPool;
        
        public void Init()
        {
            _fruitList = new List<SaveFruit>();
        }

        public void InitFruitFactory(List<GameObject> fruitGameObject , GameObject fruitParent , GameObject startPoint, GameObject recoverParent)
        {
            _factory = new FruitFactory(fruitGameObject , fruitParent , startPoint , recoverParent);
        }
        public Fruit GenerateFruit(Vector3 pos)
        {
            if (_fruitPool == null)
            {
                _fruitPool = new List<Fruit>();
            }

            if (_factory != null)
            {
                Fruit fruit = _factory.GenerateFruit(pos);
                _fruitPool.Add(fruit);
                return fruit;
            }
            return null;
        }

        public void SaveToList()
        {
            foreach (var fruit in _fruitPool)
            {
                SaveFruit aSaveFruit = new SaveFruit();
                aSaveFruit.fruitType = (int)fruit.FruitType;
                aSaveFruit.pos = fruit.transform.localPosition;
                _fruitList.Add(aSaveFruit);
            }
           
        }

        public void Recovery()
        {
            foreach (var fruit in _fruitList)
            {
                Vector2 pos = new Vector2(fruit.pos.x +400 ,fruit.pos.y);
                int type = fruit.fruitType;
                _factory.GenerateFruitByPos(pos, (FruitConst.FruitType)type);
            }
        }
    }
}