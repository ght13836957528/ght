using Framework.Pool;
using System;
using GameManager;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Framework
{
    /// <summary>
    /// GameObject实例对象
    /// </summary>
    public class InstanceObject : ObjectBase
    {
        private readonly GameObject _originAsset;

        private GameObject _owner = null;
        private bool _hasOwner = false;
        private BaseMono _mono = null;

        public InstanceObject(string name, object objectAsset)
            : base(name, Object.Instantiate((Object)objectAsset))
        {
            if (objectAsset == null)
            {
                throw new Exception("asset is invalid.");
            }

            _originAsset = objectAsset as GameObject;
        }

        public GameObject Instance => Target as GameObject;
        public GameObject Owner => _owner;
        public bool HasOwner => _hasOwner;

        public void ResetParams()
        {
            if (null != this.Instance)
            {
                this.Instance.transform.localPosition = Vector3.zero;
                this.Instance.transform.localRotation = Quaternion.identity;
                this.Instance.transform.localScale = Vector3.one;
            }
        }

        public void SetParent(Transform parent, bool worldPositionStays = false)
        {
            if (null != parent)
            {
                this.SetOwner(parent.gameObject);

                if (null != this.Instance)
                    this.Instance.transform.SetParent(parent, worldPositionStays);
            }

            this.OnShow();
        }

        public void SetOwner(GameObject owner)
        {
            _hasOwner = (null != owner);
            _owner = owner;
        }

        public void ClearOwner()
        {
            _hasOwner = false;
            _owner = null;
        }

        /// <summary>
        /// 获取对象时的事件。
        /// </summary>
        protected internal override void OnSpawn()
        {
            if (null != Instance)
            {
                _mono = Instance.GetOrAddComponent<BaseMono>();
                _mono.isPoolObject = true;
                _mono.OnAddEventListener();
                _mono.OnSpawn();

                Instance.gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// 回收对象时的事件。
        /// </summary>
        protected internal override void OnUnspawn()
        {
            ClearOwner();

            if (null != Instance)
            {
                if(null == _mono)
                    _mono = Instance.GetComponent<BaseMono>();

                if (null != _mono)
                {
                    _mono.OnRemoveEventListener();
                    _mono.OnRecycle();
                }

                Instance.gameObject.SetActive(false);

                if(Application.isPlaying && null != Instance && null != Instance.transform 
                    && null != ResourceManager.Instance && null != ResourceManager.Instance.transform)
                    Instance.transform.SetParent(ResourceManager.Instance.transform);
            }
        }

        public override void OnShow()
        {
            //if (null != Instance)
            //{
            //    if (null == _mono)
            //        _mono = Instance.GetComponent<BaseMono>();

            //    if (null != _mono)
            //        _mono.onEnter();
            //}
        }

        public override void OnHide()
        {
            //if (null != Instance)
            //{
            //    if (null == _mono)
            //        _mono = Instance.GetComponent<BaseMono>();

            //    if (null != _mono)
            //        _mono.onExit();
            //}
        }

        protected internal override void Release(bool isShutdown)
        {
            if(null != Instance) Object.Destroy(Instance);

            ResourceHelper.UnloadAssetWithObject(_originAsset, true);
        }
    }
}
