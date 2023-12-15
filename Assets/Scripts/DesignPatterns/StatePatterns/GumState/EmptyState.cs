namespace DesignPatterns.StatePatterns
{
    public class EmptyState : IState
    {
        public string StateName {
            get
            {
                return "EmptyState";
            }
        }

        public void OnEnter()
        {
            
        }

        public void OnExit()
        {
            
        }
    }
}