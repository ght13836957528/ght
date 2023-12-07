using System;
using UnityEngine;

namespace SyntheticBigWatermelon
{
    public class InputManager : MonoBehaviour
    {
       
        private Fruit _fruitInTheScene;//定义用来保存场景中未落下的水果
        public GameObject left;
        public GameObject right;
        private bool _isInt;

        private void Start()
        {
            _isInt = false;
        }

        // Update is called once per frame
        void Update()
        {
            if (!_isInt)
            {
                return;
            }
            //判断是否完成点击
            if (Input.GetMouseButtonUp(0))
            {
                Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition); //获取点击位置
                if (mousePosition.x < left.transform.position.x)
                {
                    return;
                }

                _fruitInTheScene = GameController.Instance.GetFruitInScene();
                if (_fruitInTheScene != null)
                {
                    _fruitInTheScene.transform.position = new Vector3(mousePosition.x, _fruitInTheScene.transform.position.y);
                    _fruitInTheScene.SetSimulate(true);
                    GameController.Instance.GenerateFruitInScene();
                }

            }
        }

        public void Init()
        {
            _isInt = true;
        }
    }
}