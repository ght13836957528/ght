namespace Framework
{
    public abstract class GameBaseModule : IGameModule
    {
        /// <summary>
        /// 模块优先级, 值越低越前执行 
        /// </summary>
        /// <value></value>
        public virtual int Priority { get { return 0; } }

        public virtual bool Updatable => false;
        public virtual bool FixedUpdatable => false;
        public virtual bool LateUpdatable => false;

        public virtual void Update(float elapsedTime, float realElapsedTime)
        {
            throw new System.NotImplementedException("Module Updatable Trigger is Ture, But Update Function Not Override!");
        }
        public virtual void FixedUpdate()
        {
            throw new System.NotImplementedException("Module FixedUpdatable Trigger is Ture, But FixedUpdate Function Not Override!");
        }

        public virtual void LateUpdate()
        {
            throw new System.NotImplementedException("Module LateUpdatable Trigger is Ture, But LateUpdate Function Not Override!");
        }

        public virtual void Initialize() {}

        public int CompareTo(object other)
        {
            IGameModule otherModule = other as IGameModule;
            if (this.Priority == otherModule.Priority) 
                return 0;
            else 
                return this.Priority < otherModule.Priority ? 1 : -1;
        }

        public virtual void OnApplicationFocus(bool focus) { }
        public virtual void OnApplicationPause(bool paused) {}
        public virtual void OnApplicationQuit() { }
        public virtual void Shutdown() { }
    }
}