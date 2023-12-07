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
            InitInputManager();

            GameController.Instance.GenerateFruitInScene();
        }

        private void InitGameController()
        {
            GameController.Instance.Init();
        }

        private void InitFruitFactory()
        {
            GameController.Instance.InitFruitFactory(fruitList,fruitParent,startPoint,recoverParent);
        }

        private void InitInputManager()
        {
            GetComponent<InputManager>().Init();
        }

    }
}