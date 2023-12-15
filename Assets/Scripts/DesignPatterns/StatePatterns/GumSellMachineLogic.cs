namespace DesignPatterns.StatePatterns
{
    public class GumSellMachineLogic
    {
        private StateMachine _gumStateMachine;

        public GumSellMachineLogic()
        {
            InitStateMachine();
        }

        private void InitStateMachine()
        {
            _gumStateMachine = new StateMachine();
            // _gumStateMachine.AddState();
        }
    }
}