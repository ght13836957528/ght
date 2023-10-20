using UnityEngine;

namespace DesignPatterns.FactoryPatterns
{
    public class CheesePizza : Pizza
    {
        public override void Cook()
        {
            Debug.Log("cook cheese");
        }

        public override void Box()
        {
            Debug.Log("box cheese");
        }
    }
}