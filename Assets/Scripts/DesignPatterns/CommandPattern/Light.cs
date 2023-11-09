using UnityEngine;

namespace DesignPatterns.CommandPattern
{
    public class Light
    {
        private bool _lightIsOn;
        
        public Light()
        {
            _lightIsOn = false;
        }
        
        private void TurnOnLight()
        {
            _lightIsOn = true;
            Debug.Log("turn on lights");
        }

        private void TurnOffLight()
        {
            _lightIsOn = false;
            Debug.Log("turn off lights");
        }

        private bool GetLightIsOn()
        {
            return _lightIsOn;
        }

        public void Pull()
        {
            if (GetLightIsOn())
            {
                TurnOffLight();
            }
            else
            {
                TurnOnLight();
            }
        }

    }
}