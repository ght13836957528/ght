namespace Framework
{
    public interface IGameModule : System.IComparable, IGameLifeCircle
    {
        int Priority { get; }
        bool Updatable { get; }
        bool FixedUpdatable { get; }
        bool LateUpdatable { get; }
        void Initialize();
    }
}