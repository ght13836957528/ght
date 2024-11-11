using System;

namespace Framework
{
    public class GameBaseSingletonModule<T> : GameBaseModule where T : new()
    {
        protected static T _instance;
        private static readonly object _lock;

        static GameBaseSingletonModule()
        {
            _lock = new object();
        }

        protected GameBaseSingletonModule() { }

        public static T Instance
        {
            get
            {
                // Double-Checked Locking
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = default(T) == null ? Activator.CreateInstance<T>() : default;
                        }
                    }
                }

                return _instance;
            }

            protected set
            {
                if (_instance != null)
                {
                    throw new Exception("Instance already exists.");
                }
                _instance = value;
            }
        }
    }
}