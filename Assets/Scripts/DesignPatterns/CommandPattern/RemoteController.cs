using System.Collections.Generic;

namespace DesignPatterns.CommandPattern
{
    public class RemoteController
    {
        private readonly List<Command> _commandList;
       

        public RemoteController()
        {
            _commandList = new List<Command>();
           
        }

        public void SetCommand(int index ,Command onCommand , Command offCommand)
        {
            if (onCommand != null)
            {
                _commandList.Add(onCommand);
            }
            else if (offCommand != null)
            {
                _commandList.Add(offCommand); ;
            }
        }

        public void ClickCommand(int index)
        {
            _commandList[index].ExecuteCommand();
        }
        
      
    }
}