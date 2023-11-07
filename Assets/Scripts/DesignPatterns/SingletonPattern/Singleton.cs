namespace DesignPatterns.SingletonPattern
{
    public class Singleton
    {
        private static Singleton _singleton;

        public static Singleton GetInstance()
        {
            if (_singleton == null)
            {
                _singleton = new Singleton(); // lazy instantiation 延迟实例化
            }

            return _singleton;
        }
    }
}