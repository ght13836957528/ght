using System;
using UnityEngine;

namespace DesignPatterns.FactoryPatterns
{
    public class FactoryInit : MonoBehaviour
    {
        private void Start()
        {
            PizzaStore store = new PizzaStore();
            store.OrderPizza(Pizza.PizzaType.Cheese);
        }
    }
}