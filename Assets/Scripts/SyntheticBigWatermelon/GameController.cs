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
        private Fruit _fruitInScene;

        public void Init()
        {
            _fruitList = new List<SaveFruit>();
        }

        public void InitFruitFactory(List<GameObject> fruitGameObject , GameObject fruitParent , GameObject startPoint, GameObject recoverParent)
        {
            _factory = new FruitFactory(fruitGameObject , fruitParent , startPoint );
        }

        public Fruit GetFruitInScene()
        {
            return _fruitInScene;
        }

        public void GenerateFruitInScene()
        {
            _fruitInScene = GenerateFruit();
        }

        public Fruit GenerateFruit()
        {
            Fruit fruit = _factory.GenerateFruit();
            return fruit;
        }
        
        public void OnFruitCollision(Collision2D other , Fruit fruit)
        {
            Fruit colliderFruit = other.transform.GetComponent<Fruit>();
            if (colliderFruit != null && colliderFruit.FruitType == fruit.FruitType)
            {
                fruit.SetDetected(false);
                colliderFruit.SetDetected(false);
                FruitConst.FruitType generateType = fruit.FruitType + 1;
                if (generateType > FruitConst.FruitType.Orange)
                {
                    return;
                }
                Debug.Log("OnFruitCollision other=="+other.transform.name);
                Fruit generateFruit = _factory.GenerateFruit(other.transform.position,generateType );
                generateFruit.SetSimulate(true);
                GameObject.Destroy(other.gameObject);
                GameObject.Destroy(fruit.gameObject);
            }
        }
    }
}