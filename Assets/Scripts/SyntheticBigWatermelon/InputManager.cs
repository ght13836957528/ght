using UnityEngine;

namespace SyntheticBigWatermelon
{
    public class InputManager : MonoBehaviour
    {
        private bool hasItBeenGenerated = false;//定义是否已在游戏中生成物体
        private Fruit fruitInTheScene;//定义用来保存场景中未落下的水果
        private float time = 0;//计时

        // Update is called once per frame
        void Update()
        {

            //用作延迟生成物体
            if (time < 0.3f)
            {
                time += Time.deltaTime;
            }
            else
            {
                //判断是否完成点击
                if (Input.GetMouseButtonUp(0))
                {
                    Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);//获取点击位置
                    fruitInTheScene = GameController.Instance.GenerateFruit();
                    fruitInTheScene.SetSimulate(true);
                }
            }
        }
    }
}