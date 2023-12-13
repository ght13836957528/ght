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
            InitInputManager();

            GameController.Instance.GenerateFirstFruit();
        }

        private void InitGameController()
        {
            GameController.Instance.Init(fruitList,fruitParent,startPoint);
        }
        
        private void InitInputManager()
        {
            GetComponent<InputManager>().Init();
        }

        public void UseAnyCombine()
        {
           GameController.Instance.GenerateFruitNext(FruitConst.FruitType.AnyCombine);
        }
        
       

    }
}