namespace LCFramework.Lua
{
    public interface LuaGame
    {
        // void OnPrepare(SceneLogicBase container);
        void OnEnter();
        void Update();
        void LateUpdate();
        void FixedUpdate();
        void OnApplicationFocus(bool focus);
        void OnApplicationQuit();
    }
}