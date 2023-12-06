using System;
using System.Collections.Generic;
using UnityEngine;

namespace SyntheticBigWatermelon
{
    public class Game : MonoBehaviour
    {
        [SerializeField] private List<GameObject> fruitList;
        [SerializeField] private GameObject fruitParent;
        [SerializeField] private GameObject startPoint;
        
        private void Start()
        {
            Debug.Log("Game Start");
            InitFruitFactory();
        }

        private void InitFruitFactory()
        {
            GameController.Instance.InitFruitFactory(fruitList,fruitParent,startPoint);
        }
    }
}