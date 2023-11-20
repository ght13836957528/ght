using System;
using UnityEngine;

namespace DesignPatterns.MouldPattern
{
    public class MouldEntrance : MonoBehaviour
    {
        private void Start()
        {
            CoffeePrepare coffeePrepare = new CoffeePrepare();
        }
    }
}