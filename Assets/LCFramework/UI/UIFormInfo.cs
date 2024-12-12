using System;

namespace Framework.UI
{
    /// <summary>
    /// 界面组界面信息。
    /// </summary>
    public sealed class UIFormInfo
    {
        private readonly UIForm m_UIForm;
        private bool m_Paused;
        private bool m_Covered;

        /// <summary>
        /// 初始化界面组界面信息的新实例。
        /// </summary>
        /// <param name="uiForm">界面。</param>
        public UIFormInfo(UIForm uiForm)
        {
            if (uiForm == null)
            {
                throw new Exception("UI form is invalid.");
            }

            m_UIForm = uiForm;
            m_Paused = true;
            m_Covered = true;
        }

        /// <summary>
        /// 获取界面。
        /// </summary>
        public UIForm UIForm
        {
            get
            {
                return m_UIForm;
            }
        }

        /// <summary>
        /// 获取或设置界面是否暂停。
        /// </summary>
        public bool Paused
        {
            get
            {
                return m_Paused;
            }
            set
            {
                m_Paused = value;
            }
        }

        /// <summary>
        /// 获取或设置界面是否遮挡。
        /// </summary>
        public bool Covered
        {
            get
            {
                return m_Covered;
            }
            set
            {
                m_Covered = value;
            }
        }
    }
}
