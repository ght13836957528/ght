namespace DesignPatterns.CommandPattern
{
    public class LightOnCommand : Command
    {
        private Light _light;

        public void SetLight(Light Light)
        {
            _light = Light;
        }

        public override void ExecuteCommand()
        {
            _light.TurnOn();
        }

        
    }
}