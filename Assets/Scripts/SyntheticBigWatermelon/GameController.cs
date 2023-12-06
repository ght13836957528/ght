using System.Collections.Generic;
using GameManager;
using UnityEngine;

namespace SyntheticBigWatermelon
{
    public class GameController : Singleton<GameController>
    {
        private FruitFactory _factory;

        public void InitFruitFactory(List<GameObject> fruitGameObject , GameObject fruitParent , GameObject startPoint)
        {
            _factory = new FruitFactory(fruitGameObject , fruitParent , startPoint);
        }

        public Fruit GenerateFruit()
        {
            if (_factory != null)
            {
                return _factory.GenerateFruit();
            }

            return null;
        }
    }
}