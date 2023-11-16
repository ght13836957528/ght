using UnityEngine;

namespace DesignPatterns.AdapterPatterns
{
    public class MallardDuck : IDuck
    {
        public void Fly()
        {
            Debug.Log("MallardDuck fly");
        }

        public void Quack()
        {
            Debug.Log("MallardDuck Quack");
        }
    }
}