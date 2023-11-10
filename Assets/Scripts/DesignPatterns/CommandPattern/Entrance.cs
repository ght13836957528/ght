using System;
using UnityEngine;

namespace DesignPatterns.CommandPattern
{
    public class Entrance : MonoBehaviour
    {
        private void Start()
        {
            RemoteController remoteController = new RemoteController();
            LightOffCommand lightOffCommand = new LightOffCommand();
            Light curLight = new Light();
            lightOffCommand.SetLight(curLight);
            
            LightOnCommand lightOnCommand = new LightOnCommand();
            lightOnCommand.SetLight(curLight);
            
            remoteController.SetCommand(0,null,lightOffCommand);
            remoteController.SetCommand(0,lightOnCommand,null);

            remoteController.ClickCommand(0);
            remoteController.ClickCommand(1);
            

        }
    }
}