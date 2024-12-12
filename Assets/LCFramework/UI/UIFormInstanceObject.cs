using Framework.Pool;
using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Framework.UI
{
    /// <summary>
    /// 界面实例对象。
    /// </summary>
    public class UIFormInstanceObject : ObjectBase
    {
        private readonly object m_UIFormAsset;

        public UIFormInstanceObject(string name, object uiFormAsset) 
            : base(name, Object.Instantiate((Object)uiFormAsset))
        {
            if (uiFormAsset == null)
            {
                throw new Exception("UI form asset is invalid.");
            }

            m_UIFormAsset = uiFormAsset;
        }

        protected internal override void Release(bool isShutdown)
        {
            var obj = Target as GameObject;
            if(null != obj) Object.Destroy(obj);

            ResourceHelper.UnloadAssetWithObject(m_UIFormAsset, true);
        }
    }
}
