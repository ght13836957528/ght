using UnityEngine;


//#if ODIN_INSPECTOR
//using Sirenix.OdinInspector;
//#endif

namespace Framework.UI
{
    /// <summary>
    /// 界面。
    /// </summary>
    public sealed class UIForm : MonoBehaviour
    {
        private int m_SerialId;
        private string m_UIFormAssetName;
        private UIGroup m_UIGroup;
        private int m_DepthInUIGroup;
        private bool m_PauseCoveredUIForm;
        private UIFormLogic m_UIFormLogic;
		private bool m_Inited;
		private RectTransform rectTransform;

        /// <summary>
        /// 获取界面序列编号。
        /// </summary>
        public int SerialId
        {
            get
            {
                return m_SerialId;
            }
        }

        /// <summary>
        /// 获取界面资源名称。
        /// </summary>
        public string UIFormAssetName
        {
            get
            {
                return m_UIFormAssetName;
            }
        }

        /// <summary>
        /// 获取界面实例。
        /// </summary>
        public object Handle
        {
            get
            {
                return gameObject;
            }
        }

        /// <summary>
        /// 获取界面所属的界面组。
        /// </summary>
        public UIGroup UIGroup
        {
            get
            {
                return m_UIGroup;
            }
        }

        /// <summary>
        /// 获取界面深度。
        /// </summary>
        public int DepthInUIGroup
        {
            get
            {
                return m_DepthInUIGroup;
            }
        }

        /// <summary>
        /// 获取是否暂停被覆盖的界面。
        /// </summary>
        public bool PauseCoveredUIForm
        {
            get
            {
                return m_PauseCoveredUIForm;
            }
        }

        /// <summary>
        /// 获取界面逻辑。
        /// </summary>
        public UIFormLogic Logic
        {
            get
            {
                return m_UIFormLogic;
            }
        }

        public RectTransform GetRectTransform()
        {
            if (rectTransform == null)
            {
                rectTransform = GetComponent<RectTransform>();
            }
            return rectTransform;
        }

        private void Awake()
        {
            m_Inited = false;
        }

        /// <summary>
        /// 初始化界面。
        /// </summary>
        /// <param name="serialId">界面序列编号。</param>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <param name="uiGroup">界面所处的界面组。</param>
        /// <param name="pauseCoveredUIForm">是否暂停被覆盖的界面。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void OnInit(int serialId, string uiFormKey, string uiFormAssetName, UIGroup uiGroup, bool pauseCoveredUIForm, object userData)
        {
            rectTransform = GetComponent<RectTransform>();
			m_SerialId = serialId;
            m_UIFormAssetName = uiFormAssetName;

			if (!m_Inited)
            {
                m_UIGroup = uiGroup;
            }
            else if (m_UIGroup != uiGroup)
            {
                m_UIGroup = uiGroup;
                /*Debug.LogError("UI group is inconsistent for non-new-instance UI form.");
                return;*/
            }

            m_DepthInUIGroup = 0;
            m_PauseCoveredUIForm = pauseCoveredUIForm;

            if (m_Inited)
            {
                return;
            }
            m_Inited = true;
            transform.localPosition = Vector3.zero;
            m_UIFormLogic = GetComponent<UIFormLogic>();
            if (m_UIFormLogic == null)
            {
                Debug.LogError(string.Format("UI form '{0}' can not get UI form logic.",uiFormAssetName));
                return;
            }

            m_UIFormLogic.UIKey = uiFormKey;
            m_UIFormLogic.InternalOnInit(userData);
        }

        /// <summary>
        /// 界面回收。
        /// </summary>
        public void OnRecycle()
        {
            m_SerialId = 0;
            m_DepthInUIGroup = 0;
            m_PauseCoveredUIForm = true;

            //ResourceHelper.UnloadAssetWithPath<GameObject>(m_UIFormAssetName);
        }

        /// <summary>
        /// 界面打开。
        /// </summary>
        /// <param name="userData">用户自定义数据。</param>
        public void OnOpen(object userData)
        {
            m_UIFormLogic.InternalOnOpen(userData);
        }

        /// <summary>
        /// 界面关闭。
        /// </summary>
        /// <param name="userData">用户自定义数据。</param>
        public void OnClose(object userData)
        {
			m_UIFormLogic.InternalOnClose(userData);
        }

        /// <summary>
        /// 界面暂停。
        /// </summary>
        public void OnPause()
        {
            m_UIFormLogic.OnPause();
        }

        /// <summary>
        /// 界面暂停恢复。
        /// </summary>
        public void OnResume()
        {
            m_UIFormLogic.OnResume();
        }

        /// <summary>
        /// 界面遮挡。
        /// </summary>
        public void OnCover()
        {
            m_UIFormLogic.OnCover();
        }

        /// <summary>
        /// 界面遮挡恢复。
        /// </summary>
        public void OnReveal()
        {
            m_UIFormLogic.OnReveal();
        }

        /// <summary>
        /// 界面激活。
        /// </summary>
        /// <param name="userData">用户自定义数据。</param>
        public void OnRefocus(object userData)
        {
            m_UIFormLogic.OnRefocus(userData);
        }

        /// <summary>
        /// 界面轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
        public void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            m_UIFormLogic.OnUpdate(elapseSeconds, realElapseSeconds);
        }

        /// <summary>
        /// 界面深度改变。
        /// </summary>
        /// <param name="uiGroupDepth">界面组深度。</param>
        /// <param name="depthInUIGroup">界面在界面组中的深度。</param>
        public void OnDepthChanged(int uiGroupDepth, int depthInUIGroup)
        {
            m_DepthInUIGroup = depthInUIGroup;
            m_UIFormLogic.OnDepthChanged(uiGroupDepth, depthInUIGroup);
        }

		public void SetInvisible(bool isInvisible)
		{
			if (rectTransform == null)
			{
				return;
			}

			if (rectTransform.gameObject.activeSelf)
			{
				if (isInvisible)
				{
					rectTransform.localScale = Vector3.zero;
				}
				else
				{
					rectTransform.localScale = Vector3.one;
				}
			}

		}

        public bool OnBack()
        {
            return m_UIFormLogic.OnBack(); ;
        }


        public void FullCovered()
        {
            m_UIFormLogic.FullCovered();
        }

        public void FullReveal()
        {
            m_UIFormLogic.FullReveal();
        }

        public string GetUIKey()
        {
            return m_UIFormLogic.UIKey;
        }

        public int GetDepth()
        {
            return m_UIFormLogic.canvas.sortingOrder;
        }
    }
}
