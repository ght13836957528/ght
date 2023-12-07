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
        private FruitBase _fruitInScene;

        public void Init()
        {
            _fruitList = new List<SaveFruit>();
        }

        public void InitFruitFactory(List<GameObject> fruitGameObject , GameObject fruitParent , GameObject startPoint, GameObject recoverParent)
        {
            _factory = new FruitFactory(fruitGameObject , fruitParent , startPoint );
        }

        public FruitBase GetFruitInScene()
        {
            return _fruitInScene;
        }

        public void GenerateFruitInScene(FruitConst.FruitType type = FruitConst.FruitType.Default)
        {
            if (type == FruitConst.FruitType.AnyCombine)
            {
                GameObject.Destroy(_fruitInScene.gameObject);
            }

            _fruitInScene = GenerateFruit(type);
        }

        public FruitBase GenerateFruit(FruitConst.FruitType type )
        {
            FruitBase fruit = _factory.GenerateFruit(default,type);
            return fruit;
        }
        
        public void OnFruitCollision(Collision2D other , FruitBase fruit)
        {
            FruitBase colliderFruit = other.transform.GetComponent<FruitBase>();
            fruit.SetDetected(false);
            colliderFruit.SetDetected(false);
            FruitConst.FruitType generateType = colliderFruit.FruitType + 1;
            if (generateType > FruitConst.FruitType.Orange)
            {
                return;
            }
            FruitBase generateFruit = _factory.GenerateFruit(other.transform.position, generateType);
            generateFruit.SetSimulate(true);
            GameObject.Destroy(other.gameObject);
            GameObject.Destroy(fruit.gameObject);
            
        }

        public void AddForce()
        {
            List<FruitBase> fruitList = _factory.GetFruitList();
            foreach (var fruit in fruitList)
            {
                Rigidbody2D rigidbody2D = fruit.GetRigidbody2D();
                float x = UnityEngine.Random.Range(-1, 1);
                float y = UnityEngine.Random.Range(0, 1);
                rigidbody2D.AddForce(Vector2.up * 500);
            }
        }

        

    }
}