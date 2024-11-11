public interface IGameLifeCircle
{
    void Update(float elapsedTime, float realElapsedTime);
    void FixedUpdate();
    void LateUpdate();
    void OnApplicationFocus(bool focus);
    void OnApplicationPause(bool paused);
    void OnApplicationQuit();
}