using System;
using System.Collections.Generic;
using GameManager;
using UnityEngine;

namespace Framework.UI
{
    /// <summary>
    /// 界面组。
    /// </summary>
    public class UIGroup
    {
        private readonly string m_Name;
        private int m_Depth;
        private bool m_Pause;
        private UIGroupMono m_UIGroupMono;
        private LinkedList<UIFormInfo> m_UIFormInfos;
        private List<string> m_UsedFormAssets;

        /// <summary>
        /// 初始化界面组的新实例。
        /// </summary>
        /// <param name="name">界面组名称。</param>
        /// <param name="depth">界面组深度。</param>
        /// <param name="uiGroupHelper">界面组辅助器。</param>
        public UIGroup(string name, int depth, Transform parent)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new Exception("UI group name is invalid.");
            }

            m_Pause = false;
            m_UIFormInfos = new LinkedList<UIFormInfo>();
            m_UsedFormAssets = new List<string>();
            m_Name = name;
            Depth = depth;

            CreateGroupNode(parent);
        }

        /// <summary>
        /// 获取界面组名称。
        /// </summary>
        public string Name
        {
            get
            {
                return m_Name;
            }
        }

        /// <summary>
        /// 获取或设置界面组深度。
        /// </summary>
        public int Depth
        {
            get
            {
                return m_Depth;
            }
            set
            {
                if (m_Depth == value)
                {
                    return;
                }

                m_Depth = value;

                if(null != m_UIGroupMono)
                    m_UIGroupMono.SetDepth(m_Depth);
                Refresh();
            }
        }

        public void CreateGroupNode(Transform parent)
        {
            GameObject groupNode = new GameObject($"UI Group - {Name}");
            m_UIGroupMono = groupNode.GetOrAddComponent<UIGroupMono>();
            m_UIGroupMono.transform.SetParent(parent);
            m_UIGroupMono.SetDepth(m_Depth);
        }

        /// <summary>
        /// 获取或设置界面组是否暂停。
        /// </summary>
        public bool Pause
        {
            get
            {
                return m_Pause;
            }
            set
            {
                if (m_Pause == value)
                {
                    return;
                }

                m_Pause = value;
                Refresh();
            }
        }

        /// <summary>
        /// 获取界面组中界面数量。
        /// </summary>
        public int UIFormCount
        {
            get
            {
                return m_UIFormInfos.Count;
            }
        }

        /// <summary>
        /// 获取当前界面。
        /// </summary>
        public UIForm CurrentUIForm
        {
            get
            {
                return m_UIFormInfos.First != null ? m_UIFormInfos.First.Value.UIForm : null;
            }
        }

        /// <summary>
        /// 获取界面组辅助器。
        /// </summary>
        public UIGroupMono Mono
        {
            get
            {
                return m_UIGroupMono;
            }
        }

        /// <summary>
        /// 检查并添加被加载的资源到列表中
        /// </summary>
        /// <param name="assetPath">资源名称</param>
        public bool AddNewFormAsset(string path)
        {
            if (m_UsedFormAssets.Contains(path))
            {
                return false;
            }
            m_UsedFormAssets.Add(path);
            return true;
        }

        /// <summary>
        /// 界面组轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
        public void Update(float elapseSeconds, float realElapseSeconds)
        {
            LinkedListNode<UIFormInfo> current = m_UIFormInfos.First;
            while (current != null)
            {
                if (current.Value.Paused)
                {
                    break;
                }

                LinkedListNode<UIFormInfo> next = current.Next;
                current.Value.UIForm.OnUpdate(elapseSeconds, realElapseSeconds);
                current = next;
            }
        }

        /// <summary>
        /// 界面组中是否存在界面。
        /// </summary>
        /// <param name="serialId">界面序列编号。</param>
        /// <returns>界面组中是否存在界面。</returns>
        public bool HasUIForm(int serialId)
        {
            foreach (UIFormInfo uiFormInfo in m_UIFormInfos)
            {
                if (uiFormInfo.UIForm.SerialId == serialId)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 界面组中是否存在界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <returns>界面组中是否存在界面。</returns>
        public bool HasUIForm(string uiFormAssetName)
        {
            if (string.IsNullOrEmpty(uiFormAssetName))
            {
                throw new Exception("UI form asset name is invalid.");
            }

            foreach (UIFormInfo uiFormInfo in m_UIFormInfos)
            {
                if (uiFormInfo.UIForm.UIFormAssetName == uiFormAssetName)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 从界面组中获取界面。
        /// </summary>
        /// <param name="serialId">界面序列编号。</param>
        /// <returns>要获取的界面。</returns>
        public UIForm GetUIForm(int serialId)
        {
            foreach (UIFormInfo uiFormInfo in m_UIFormInfos)
            {
                if (uiFormInfo.UIForm.SerialId == serialId)
                {
                    return uiFormInfo.UIForm;
                }
            }

            return null;
        }

        /// <summary>
        /// 从界面组中获取界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <returns>要获取的界面。</returns>
        public UIForm GetUIForm(string uiFormAssetName)
        {
            if (string.IsNullOrEmpty(uiFormAssetName))
            {
                throw new Exception("UI form asset name is invalid.");
            }

            foreach (UIFormInfo uiFormInfo in m_UIFormInfos)
            {
                if (uiFormInfo.UIForm.UIFormAssetName == uiFormAssetName)
                {
                    return uiFormInfo.UIForm;
                }
            }

            return null;
        }

        /// <summary>
        /// 从界面组中获取界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <returns>要获取的界面。</returns>
        public UIForm[] GetUIForms(string uiFormAssetName)
        {
            if (string.IsNullOrEmpty(uiFormAssetName))
            {
                throw new Exception("UI form asset name is invalid.");
            }

            List<UIForm> uiForms = new List<UIForm>();
            foreach (UIFormInfo uiFormInfo in m_UIFormInfos)
            {
                if (uiFormInfo.UIForm.UIFormAssetName == uiFormAssetName)
                {
                    uiForms.Add(uiFormInfo.UIForm);
                }
            }

            return uiForms.ToArray();
        }

        /// <summary>
        /// 从界面组中获取所有界面。
        /// </summary>
        /// <returns>界面组中的所有界面。</returns>
        public UIForm[] GetAllUIForms()
        {
            List<UIForm> uiForms = new List<UIForm>();
            foreach (UIFormInfo uiFormInfo in m_UIFormInfos)
            {
                uiForms.Add(uiFormInfo.UIForm);
            }

            return uiForms.ToArray();
        }

        /// <summary>
        /// 往界面组增加界面。
        /// </summary>
        /// <param name="uiForm">要增加的界面。</param>
        public void AddUIForm(UIForm uiForm)
        {
            UIFormInfo uiFormInfo = new UIFormInfo(uiForm);
            m_UIFormInfos.AddFirst(uiFormInfo);
        }

        /// <summary>
        /// 从界面组移除界面。
        /// </summary>
        /// <param name="uiForm">要移除的界面。</param>
        public void RemoveUIForm(UIForm uiForm)
        {
            UIFormInfo uiFormInfo = GetUIFormInfo(uiForm);
            if (uiFormInfo == null)
            {
                Debug.LogError(string.Format("Can not find UI form info for serial id '{0}', UI form asset name is '{1}'.", uiForm.SerialId.ToString(), uiForm.UIFormAssetName));
                return;
            }

            if (!uiFormInfo.Covered)
            {
                uiFormInfo.Covered = true;
                uiForm.OnCover();
            }

            if (!uiFormInfo.Paused)
            {
                uiFormInfo.Paused = true;
                uiForm.OnPause();
            }

            m_UIFormInfos.Remove(uiFormInfo);
            m_UsedFormAssets.Remove(uiForm.UIFormAssetName);
        }

        /// <summary>
        /// 激活界面。
        /// </summary>
        /// <param name="uiForm">要激活的界面。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void RefocusUIForm(UIForm uiForm, object userData)
        {
            UIFormInfo uiFormInfo = GetUIFormInfo(uiForm);
            if (uiFormInfo == null)
            {
                throw new Exception("Can not find UI form info.");
            }

            m_UIFormInfos.Remove(uiFormInfo);
            m_UIFormInfos.AddFirst(uiFormInfo);
        }

        /// <summary>
        /// 刷新界面组。
        /// </summary>
        public void Refresh()
        {
            LinkedListNode<UIFormInfo> current = m_UIFormInfos.First;
            bool pause = m_Pause;
            bool cover = false;
            int depth = UIFormCount;
            while (current != null)
            {
                LinkedListNode<UIFormInfo> next = current.Next;
                current.Value.UIForm.OnDepthChanged(Depth, depth--);
                if (pause)
                {
                    if (!current.Value.Covered)
                    {
                        current.Value.Covered = true;
                        current.Value.UIForm.OnCover();
                    }

                    if (!current.Value.Paused)
                    {
                        current.Value.Paused = true;
                        current.Value.UIForm.OnPause();
                    }
                }
                else
                {
                    if (current.Value.Paused)
                    {
                        current.Value.Paused = false;
                        current.Value.UIForm.OnResume();
                    }

                    if (current.Value.UIForm.PauseCoveredUIForm)
                    {
                        pause = true;
                    }

                    if (cover)
                    {
                        if (!current.Value.Covered)
                        {
                            current.Value.Covered = true;
                            current.Value.UIForm.OnCover();
                        }
                    }
                    else
                    {
                        if (current.Value.Covered)
                        {
                            current.Value.Covered = false;
                            current.Value.UIForm.OnReveal();
                        }

                        cover = true;
                    }
                }

                current = next;
            }
        }

        public void RefreshInvisibleUI(UIForm uiForm, UIGroup uiGroup)
        {
            if (uiGroup.Depth >= Depth)
            {
                if (uiGroup.Name == Name)
                {
                    foreach (UIFormInfo uiFormInfo in m_UIFormInfos)
                    {
                        if (uiFormInfo.UIForm != uiForm)
                        {
                            uiFormInfo.UIForm.SetInvisible(true);
                        }
                    }
                }
                else
                {
                    foreach (UIFormInfo uiFormInfo in m_UIFormInfos)
                    {
                        uiFormInfo.UIForm.SetInvisible(true);
                    }
                }
            }
        }

        private UIFormInfo GetUIFormInfo(UIForm uiForm)
        {
            if (uiForm == null)
            {
                throw new Exception("UI form is invalid.");
            }

            foreach (UIFormInfo uiFormInfo in m_UIFormInfos)
            {
                if (uiFormInfo.UIForm == uiForm)
                {
                    return uiFormInfo;
                }
            }

            return null;
        }
    }
}
