using System;
using UnityEngine;

namespace DesignPatterns.IteratorPattern
{
    public class IteratorPatternInt : MonoBehaviour
    {
        private ContainerTest _containerTest;

        private void Start()
        {
            _containerTest = new ContainerTest();
            _containerTest.Add(1);
            _containerTest.Add(2);
            _containerTest.Add(3);
            _containerTest.Add(4);
            _containerTest.ForEach(Print);
        }

        private void Print(int number)
        {
            Debug.Log("number ==="+ number);
        }
    }
}