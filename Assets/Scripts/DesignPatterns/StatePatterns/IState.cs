namespace DesignPatterns.StatePatterns
{
    public interface IState
    {
        public string StateName { get; }

        public void OnEnter();

        public void OnExit();

    }
}