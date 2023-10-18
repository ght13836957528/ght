using System;
using UnityEngine;

namespace Script.DecorativePattern
{
    public class InitEntrance : MonoBehaviour
    {
        private void Start()
        {
            DoubleMochaAndWhipEspresso _b = new DoubleMochaAndWhipEspresso();
            Debug.Log("cost==="+_b.Cost());
            Debug.Log("dec==="+_b.GetDescription());
        }
    }
}