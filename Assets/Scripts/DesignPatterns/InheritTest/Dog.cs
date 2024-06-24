using UnityEngine;

namespace DesignPatterns.InheritTest
{
    public class Dog : Animal
    {
        public Dog()
        {
            Debug.Log("New dog");
        }

        public Dog(string name) : base(name)
        {
            Debug.Log("New dog,name=="+name);
        }

        public override void Shout()
        {
            Debug.Log("dog bark");
        }
    }
}