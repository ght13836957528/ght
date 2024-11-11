using UnityEngine;

namespace Framework
{
    /// <summary>
    /// 游戏框架组件抽象类。
    /// </summary>
    public abstract class FrameworkComponent : MonoBehaviour
    {
        /// <summary>
        /// 游戏框架组件初始化。
        /// </summary>
        protected virtual void Awake()
        {
            // GameEntry.RegisterComponent(this);
        }
    }
}
