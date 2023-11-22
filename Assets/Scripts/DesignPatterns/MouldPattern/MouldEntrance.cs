using System;
using UnityEngine;

namespace DesignPatterns.MouldPattern
{
    /// <summary>
    /// 模板模式测试入口类
    /// </summary>
    public class MouldEntrance : MonoBehaviour
    {
        private void Start()
        {
            CoffeePrepare coffeePrepare = new CoffeePrepare();

            Duck[] ducks =
            {
                new Duck("aaa", 5),
                new Duck("bbb", 2.3f),
                new Duck("cc", 10),
                new Duck("uuu", 7.0f),
            };
            
            Array.Sort(ducks);

            foreach (var duck in ducks)
            {
                Debug.Log("duck name==" + duck.GetName() + " weight===" + duck.GetWeight());
            }
        }
    }
}