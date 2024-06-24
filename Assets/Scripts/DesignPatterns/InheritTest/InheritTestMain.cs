using System;
using Unity.VisualScripting;
using UnityEngine;

namespace DesignPatterns.InheritTest
{
    public class InheritTestMain : MonoBehaviour
    {
        private void Start()
        {
            Animal baseAni = new Animal("111");
            baseAni.Shout();
            Dog dog1 = new Dog();
            dog1.Shout();
            Dog dog = new Dog("222");
            dog.Shout();
        }
    }
}