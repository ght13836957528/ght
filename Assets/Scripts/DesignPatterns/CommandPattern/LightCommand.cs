namespace DesignPatterns.CommandPattern
{
    public class LightCommand : ICommand
    {
        private Light _light;

        public void SetLight(Light Light)
        {
            _light = Light;
        }

        public void ExecuteCommand()
        {
            _light.Pull();
        }

        
    }
}