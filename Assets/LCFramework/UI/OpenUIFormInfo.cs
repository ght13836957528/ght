using System;

namespace Framework.UI
{
    public class OpenUIFormInfo
    {
        private readonly int m_SerialId;
        private readonly string m_UIKey;
        private readonly UIGroup m_UIGroup;
        private readonly bool m_PauseCoveredUIForm;
        private readonly object m_UserData;
        private readonly Action<UIForm> m_onLoadFormSuccessAction;

        public OpenUIFormInfo(int serialId, string uiKey, UIGroup uiGroup, bool pauseCoveredUIForm, object userData, Action<UIForm> onLoadFromSuccessAction)
        {
            m_SerialId = serialId;
            m_UIKey = uiKey;
            m_UIGroup = uiGroup;
            m_PauseCoveredUIForm = pauseCoveredUIForm;
            m_UserData = userData;
            m_onLoadFormSuccessAction = onLoadFromSuccessAction;

        }

        public int SerialId
        {
            get
            {
                return m_SerialId;
            }
        }

        public string UIKey
        {
            get
            {
                return m_UIKey;
            }
        }

        public UIGroup UIGroup
        {
            get
            {
                return m_UIGroup;
            }
        }

        public bool PauseCoveredUIForm
        {
            get
            {
                return m_PauseCoveredUIForm;
            }
        }

        public object UserData
        {
            get
            {
                return m_UserData;
            }
        }

        public Action<UIForm> LoadFormSuccessAction
        {
            get
            {
                return m_onLoadFormSuccessAction;
            }
        }
    }
}
