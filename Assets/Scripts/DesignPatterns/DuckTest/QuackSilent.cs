using UnityEngine;

namespace Script.DuckTest
{
    public class QuackSilent : QuackBehavior
    {
        public void Quack()
        {
            Debug.Log("QuackSilent");
        }
    }
}