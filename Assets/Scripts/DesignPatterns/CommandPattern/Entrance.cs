using System;
using UnityEngine;

namespace DesignPatterns.CommandPattern
{
    public class Entrance : MonoBehaviour
    {
        private void Start()
        {
            RemoteController remoteController = new RemoteController();
            LightCommand lightCommand = new LightCommand();
            Light curLight = new Light();
            lightCommand.SetLight(curLight);
            
            remoteController.SetCommand(lightCommand);
            remoteController.Click();
        }
    }
}