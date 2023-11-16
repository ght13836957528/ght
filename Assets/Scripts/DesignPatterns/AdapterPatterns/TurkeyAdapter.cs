namespace DesignPatterns.AdapterPatterns
{
    public class TurkeyAdapter: IDuck
    {
        private WildTurkey _turkey;

        public TurkeyAdapter(WildTurkey wildTurkey)
        {
            _turkey = wildTurkey;
        }

        public void Quack()
        {
            _turkey.Gobble();
        }
        public void Fly()
        {
            _turkey.Fly();
        }
    }
}