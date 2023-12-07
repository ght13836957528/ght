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
        [SerializeField] private GameObject recoverParent;
        
        private void Start()
        {
            Debug.Log("Game Start");
            InitGameController();
            InitFruitFactory();
        }

        private void InitGameController()
        {
            GameController.Instance.Init();
        }

        private void InitFruitFactory()
        {
            GameController.Instance.InitFruitFactory(fruitList,fruitParent,startPoint,recoverParent);
        }

        public void Recovery()
        {
            Debug.Log("Recovery");
            GameController.Instance.Recovery();
        }

        public void SaveToList()
        {
            Debug.Log("SaveToList");
            GameController.Instance.SaveToList();
        }
    }
}