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
            
            remoteController.SetCommand(lightOffCommand);
            remoteController.SetCommand(lightOnCommand);

            remoteController.ClickCommand(0);
            remoteController.ClickCommand(1);
            
            remoteController.UndoCommand();
        }
    }
}