using System;
using UnityEngine;

namespace SyntheticBigWatermelon
{
    public class InputManager : MonoBehaviour
    {
       
        private FruitBase _fruitInTheScene;//定义用来保存场景中未落下的水果
        public GameObject left;
        public GameObject right;
        private bool _isInt;
        

        // Update is called once per frame
        void Update()
        {
            if (!_isInt)
            {
                return;
            }
            //判断是否完成点击
            if (Input.GetMouseButton(0))
            {
                Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition); //获取点击位置
                if (mousePosition.x < left.transform.position.x)
                {
                    return;
                }
                _fruitInTheScene = GameController.Instance.GetFruitNext();
                _fruitInTheScene.transform.position = new Vector3(mousePosition.x, _fruitInTheScene.transform.position.y);
            }

            if (Input.GetMouseButtonUp(0))
            {
                Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition); //获取点击位置
                if (mousePosition.x < left.transform.position.x)
                {
                    return;
                }

                _fruitInTheScene = GameController.Instance.GetFruitNext();
                if (_fruitInTheScene != null)
                {
                    _fruitInTheScene.transform.position = new Vector3(mousePosition.x, _fruitInTheScene.transform.position.y);
                    _fruitInTheScene.Fall();
                    GameController.Instance.GenerateFruitNext(FruitConst.FruitType.Default);
                }

            }
        }

        public void Init()
        {
            Debug.Log("_isInt");
            _isInt = true;
        }
    }
}