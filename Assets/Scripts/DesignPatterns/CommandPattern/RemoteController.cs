using System.Collections.Generic;

namespace DesignPatterns.CommandPattern
{
    public class RemoteController
    {
        private readonly List<Command> _commandList;
        private Command _lastCommand;

        public RemoteController()
        {
            _commandList = new List<Command>();
           
        }

        public void SetCommand(Command command )
        {
            _commandList.Add(command);
        }
        public void ClickCommand(int index)
        {
            _commandList[index].ExecuteCommand();
            _lastCommand = _commandList[index];
        }

        public void UndoCommand()
        {
            _lastCommand.UndoCommand();
        }

    }
}