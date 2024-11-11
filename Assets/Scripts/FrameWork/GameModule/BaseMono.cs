using UnityEngine;

namespace Framework
{
    public class BaseMono : MonoBehaviour
    {
        // 是否为池对象
        public bool isPoolObject = false;

        // 是否为实例化对象
        public bool isInstanceObject = false;

        // 是否激活过
        protected bool _isHaveActive = false;
        
        
        public virtual void OnAddEventListener()
        {
        }

        public virtual void OnRemoveEventListener()
        {
        }
        
        
        #region 回池处理

        /// <summary>
        /// 对象从池中取出
        /// 对象从池中取出时需要处理的逻辑可继承此接口
        /// </summary>
        public virtual void OnSpawn()
        {
            //OnAddEventListener();
        }

        /// <summary>
        /// 对象回池处理
        /// 对象回池前需要做处理(数据清理、还原等)可继承此接口
        /// </summary>
        public virtual void OnRecycle()
        {
            if(isPoolObject)
            {
                // var form = this as PopupBaseView;
                // if (null != form)
                //     form.OnClose(null);
            }

            //OnRemoveEventListener();
        }

        private void Unspawn(bool isImmediate = false)
        {
            if (!isPoolObject)
                return;

            var instance = InstanceObjectManager.Instance.GetInstanceObject(this.gameObject);
            if (null == instance || !instance.IsInUse)
                return;

            InstanceObjectManager.Instance.Unspawn(this.gameObject, isImmediate);
        }

        /// <summary>
        /// 释放对象
        /// </summary>
        /// <param name="isImmediate">是否立即释放</param>
        public virtual void Release(bool isImmediate = false)
        {
            if (isPoolObject)
            {
                Unspawn(isImmediate);
            }
            else
            {
                if (!isImmediate)
                    Destroy(gameObject);
                else
                    DestroyImmediate(gameObject);
            }
        }

        #endregion
    }
}