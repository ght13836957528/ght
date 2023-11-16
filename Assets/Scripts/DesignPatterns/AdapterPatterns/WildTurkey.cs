using UnityEngine;

namespace DesignPatterns.AdapterPatterns
{
    public class WildTurkey : ITurkey
    {
        public void Gobble()
        {
            Debug.Log("WildTurkey Gobble");
        }

        public void Fly()
        {
            Debug.Log("WildTurkey Fly");
        }
    }
}