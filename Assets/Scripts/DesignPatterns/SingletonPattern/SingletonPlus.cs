namespace DesignPatterns.SingletonPattern
{
    public class SingletonPlus
    {
        private static SingletonPlus _singletonPlus;
        private static readonly object _lock = new object();

        public static SingletonPlus Instance()
        {
            if (_singletonPlus == null)
            {
                lock (_lock) //避免多线程时实例化多个对象
                {
                    if (_singletonPlus == null)
                    {
                        _singletonPlus = new SingletonPlus();
                    }
                }
            }
            return _singletonPlus;
        }

    }
}