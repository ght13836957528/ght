namespace DesignPatterns.CommandPattern
{
    public class LightOffCommand : Command
    {
        private Light _light;

        public void SetLight(Light Light)
        {
            _light = Light;
        }

        public override void ExecuteCommand()
        {
            _light.TurnOff();
        }

        public override void UndoCommand()
        {
            _light.TurnOn();
        }
    }
}