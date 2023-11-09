namespace DesignPatterns.CommandPattern
{
    public class RemoteController
    {
        private ICommand _command;
        
        public void SetCommand(ICommand command)
        {
            _command = command;
        }

        public void Click()
        {
            _command.ExecuteCommand();
        }
    }
}