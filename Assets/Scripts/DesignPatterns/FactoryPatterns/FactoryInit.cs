using System;
using UnityEngine;

namespace DesignPatterns.FactoryPatterns
{
    public class FactoryInit : MonoBehaviour
    {
        private void Start()
        {
            PizzaStore nyStore = new NYPizzaStore();
            nyStore.OrderPizza(Pizza.PizzaType.Cheese);
        }
    }
}