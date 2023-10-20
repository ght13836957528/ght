using UnityEngine;

namespace DesignPatterns.FactoryPatterns
{
    public class BeefPizza : Pizza
    {
        public override void Box()
        {
             Debug.Log("box beef");
        }

        public override void Cook()
        {
            Debug.Log("cook beef");
        }
    }
}