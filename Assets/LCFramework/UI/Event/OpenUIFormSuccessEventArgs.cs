using System;

namespace Framework.UI
{
    /// <summary>
    /// 打开界面成功事件。
    /// </summary>
    public sealed class OpenUIFormSuccessEventArgs : EventArgs
    {
        /// <summary>
        /// 初始化打开界面成功事件的新实例。
        /// </summary>
        /// <param name="uiForm">加载成功的界面。</param>
        /// <param name="duration">加载持续时间。</param>
        /// <param name="userData">用户自定义数据。</param>
        public OpenUIFormSuccessEventArgs(UIForm uiForm, float duration, object userData)
        {
            UIForm = uiForm;
            Duration = duration;
            UserData = userData;
        }

        /// <summary>
        /// 获取打开成功的界面。
        /// </summary>
        public UIForm UIForm
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取加载持续时间。
        /// </summary>
        public float Duration
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取用户自定义数据。
        /// </summary>
        public object UserData
        {
            get;
            private set;
        }
    }
}
