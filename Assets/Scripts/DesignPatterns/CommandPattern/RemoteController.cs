using System.Collections.Generic;

namespace DesignPatterns.CommandPattern
{
    public class RemoteController
    {
        private readonly List<Command> _onCommandList;
        private readonly List<Command> _offCommandList;
        const int ControllerCount = 2;

        public RemoteController()
        {
            _onCommandList = new List<Command>();
            _offCommandList = new List<Command>();
        }

        public void SetCommand(int index ,Command onCommand , Command offCommand)
        {
            if (onCommand != null)
            {
                _onCommandList.Add(onCommand);
            }
            else if (offCommand != null)
            {
                _offCommandList.Add(offCommand); ;
            }
        }

        public void ClickCommand(int index)
        {
            _onCommandList[index].ExecuteCommand();
        }
        
      
    }
}