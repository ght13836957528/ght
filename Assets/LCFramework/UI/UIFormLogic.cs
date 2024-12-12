using LS;
using System;
using GameManager;
using UnityEngine;
using UnityEngine.UI;

namespace Framework.UI
{
    /// <summary>
    /// 界面逻辑基类。
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(GraphicRaycaster))]
    [RequireComponent(typeof(UIForm))]
    public abstract class UIFormLogic : MonoBehaviour
    {
        public const int DepthFactor = 100;
        private int m_OriginalLayer = 0;
        private string uiKey = string.Empty;

        public Canvas canvas { get; private set; }
        public CanvasGroup canvasGroup { get; private set; }
        public RectTransform recrTransform { get; private set; }

        /// <summary>
        /// 获取界面。
        /// </summary>
        public UIForm UIForm
        {
            get
            {
                return GetComponent<UIForm>();
            }
        }

        /// <summary>
        /// 获取或设置界面名称。
        /// </summary>
        public string Name
        {
            get
            {
                return gameObject.name;
            }
            set
            {
                gameObject.name = value;
            }
        }

        public string UIKey
        {
            get { return this.uiKey; }
            set { this.uiKey = value; }
        }

        /// <summary>
        /// 获取界面是否可用。
        /// </summary>
        public bool IsAvailable
        {
            get
            {
                return gameObject.activeSelf;
            }
        }

        /// <summary>
        /// 获取已缓存的 Transform。
        /// </summary>
        public Transform CachedTransform
        {
            get;
            private set;
        }

        #region 框架接口

        #region 框架内部接口

        protected internal virtual void InternalOnInit(object userData)
        {
            if (CachedTransform == null)
            {
                CachedTransform = transform;
            }

            m_OriginalLayer = gameObject.layer;

            canvasGroup = gameObject.GetOrAddComponent<CanvasGroup>();
            canvasGroup.alpha = 1;

            canvas = gameObject.GetOrAddComponent<Canvas>();
            canvas.sortingOrder = 0;
            canvas.overrideSorting = true;
            // canvas.worldCamera = GameEntry.UI.GetCamera();

            gameObject.GetOrAddComponent<GraphicRaycaster>();

            recrTransform = GetComponent<RectTransform>();

            OnInit(userData);
        }

        protected internal virtual void InternalOnOpen(object userData)
        {
            ResetUIParams();

            gameObject.SetActive(true);

            OnAfterOpenUI();
        }

        protected internal virtual void InternalOnClose(object userData)
        {
            // gameObject.SetLayerRecursively(m_OriginalLayer);

            OnBeforeCloseUI();
        }

        #endregion

        #region 框架对外接口

        /// <summary>
        /// 界面初始化。
        /// </summary>
        /// <param name="userData">用户自定义数据。</param>
        public virtual void OnInit(object userData)
        {

        }

        /// <summary>
        /// 界面打开。
        /// </summary>
        /// <param name="userData">用户自定义数据。</param>
        public virtual void OnOpen(object userData)
        {

        }

        /// <summary>
        /// 界面关闭。
        /// </summary>
        /// <param name="userData">用户自定义数据。</param>
        public virtual void OnClose(object userData)
        {

        }

        /// <summary>
        /// 界面暂停。
        /// </summary>
        protected internal virtual void OnPause()
        {
            gameObject.SetActive(false);
        }

        /// <summary>
        /// 界面暂停恢复。
        /// </summary>
        protected internal virtual void OnResume()
        {
            gameObject.SetActive(true);
        }

        /// <summary>
        /// 界面遮挡。
        /// </summary>
        protected internal virtual void OnCover()
        {

        }

        /// <summary>
        /// 界面遮挡恢复。
        /// </summary>
        protected internal virtual void OnReveal()
        {

        }


        /// <summary>
        /// 界面被全屏遮挡
        /// </summary>
        protected internal virtual void OnFullCovered()
        {

        }

        /// <summary>
        /// 界面被全屏遮挡后恢复
        /// </summary>
        protected internal virtual void OnFullReveal()
        {

        }

        /// <summary>
        /// 界面激活。
        /// </summary>
        /// <param name="userData">用户自定义数据。</param>
        protected internal virtual void OnRefocus(object userData)
        {

        }

        /// <summary>
        /// 界面轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
        protected internal virtual void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {

        }

        /// <summary>
        /// 界面深度改变。
        /// </summary>
        /// <param name="uiGroupDepth">界面组深度。</param>
        /// <param name="depthInUIGroup">界面在界面组中的深度。</param>
        protected internal virtual void OnDepthChanged(int uiGroupDepth, int depthInUIGroup)
        {
            int deltaDepth = UIGroupMono.DepthFactor * uiGroupDepth + DepthFactor * depthInUIGroup;

            if (null == this.canvas)
                this.canvas = gameObject.GetOrAddComponent<Canvas>();

            this.canvas.sortingOrder = deltaDepth;

            // var childUpdateOrders = this.GetComponentsInChildren<IForceUpdateOrder>(true);
            // if (null != childUpdateOrders)
            // {
            //     for (int i = 0; i < childUpdateOrders.Length; i++)
            //         childUpdateOrders[i].UpdateSortingOrder(deltaDepth, "Default");
            // }
        }

        protected internal virtual bool OnBack()
        {
            Debug.Log(name + " OnBack");
            return false;
        }

        #endregion

        #endregion

        #region 公有接口

        /// <summary>
        /// 获得当前界面的层级
        /// </summary>
        /// <returns></returns>
        public int GetSortingOrder()
        {
            return this.canvas.sortingOrder;
        }

        #endregion

        protected internal void ResetUIParams()
        {
            if (recrTransform != null)
            {
                recrTransform.localScale = Vector3.one;
                recrTransform.offsetMin = Vector3.zero;
                recrTransform.offsetMax = Vector3.zero;
                recrTransform.anchorMin = Vector2.zero;
                recrTransform.anchorMax = Vector2.one;
                recrTransform.pivot = new Vector2(0.5f, 0.5f);
                recrTransform.sizeDelta = Vector2.zero;
                recrTransform.anchoredPosition = Vector2.zero;
                recrTransform.SetAsLastSibling();

                int uiLayerIndex = LayerMask.NameToLayer("UI");
                if (gameObject.layer != uiLayerIndex)
                    gameObject.layer = uiLayerIndex;
            }
        }


        public void OnAfterOpenUI()
        {
            this.CheckAndOpenSenceCameraRender(false);
        }

        public void OnBeforeCloseUI()
        {
            this.CheckAndOpenSenceCameraRender(true);
        }

        /// <summary>
        /// 界面被全屏遮挡
        /// </summary>
        public void FullCovered()
        {
            if (null == canvasGroup)
                return;

            canvasGroup.alpha = 0;
            canvasGroup.blocksRaycasts = false;

            this.OnFullCovered();
        }

        /// <summary>
        /// 界面被全屏遮挡恢复
        /// </summary>
        /// <param name="notify">是否通知UI触发恢复逻辑</param>
        public void FullReveal(bool notify = true)
        {
            if (null == canvasGroup)
                return;

            canvasGroup.alpha = 1;
            canvasGroup.blocksRaycasts = true;

            if (notify)
                this.OnFullReveal();
        }


        #region 全屏界面关闭场景相机渲染处理逻辑

        public static int g_CloseSenceCameraCount = 0;

        /// <summary>
        /// 还原场景相机状态
        /// </summary>
        public void RevertSenceCameraRender()
        {
            g_CloseSenceCameraCount = 0;
            this.ToggleBackgroundCamera(true);
        }

        /// <summary>
        /// 检查并打开或者关闭场景相机的渲染
        /// </summary>
        /// <param name="isOpen">true:打开 false:关闭</param>
        protected void CheckAndOpenSenceCameraRender(bool isOpen)
        {
            if (string.IsNullOrEmpty(this.uiKey))
                return;

            // var datarow = GameEntry.Table.GetDataRow<LS.UiDataRow>(this.uiKey);
            // if (null == datarow || (!datarow.IsFullScreen /*&& !datarow.IsCaptureSceneScreenshot*/))
            //     return;
            //
            // if (!isOpen)
            // {
            //     g_CloseSenceCameraCount++;
            //
            //     this.ToggleBackgroundCamera(false);
            // }
            // else
            // {
            //     g_CloseSenceCameraCount--;
            //
            //     if (g_CloseSenceCameraCount < 0)
            //         g_CloseSenceCameraCount = 0;
            //
            //     if (g_CloseSenceCameraCount <= 0)
            //         this.ToggleBackgroundCamera(true);
            // }
        }

        private void ToggleBackgroundCamera(bool bSet)
        {
            // ScenesManager.Instance.ToggleCamera(bSet);
        }

        #endregion
    }
}
