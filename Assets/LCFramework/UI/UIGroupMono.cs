using GameManager;
using UnityEngine;
using UnityEngine.UI;

namespace Framework.UI
{
    public class UIGroupMono : MonoBehaviour
    {
        public const int DepthFactor = 3000;

        private int _depth = 0;
        private Canvas _canvas = null;

        private void Awake()
        {
            _canvas = gameObject.GetOrAddComponent<Canvas>();
            gameObject.GetOrAddComponent<GraphicRaycaster>();
        }

        private void Start()
        {
            _canvas.overrideSorting = true;
            _canvas.sortingOrder = DepthFactor * _depth;

            RectTransform rt = this.gameObject.GetOrAddComponent<RectTransform>();
            rt.localScale = Vector3.one;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.anchoredPosition = Vector2.zero;
            rt.offsetMin = Vector3.zero;
            rt.offsetMax = Vector3.zero;
            rt.sizeDelta = Vector2.zero;

            this.transform.localScale = Vector3.one;

            this.gameObject.layer = LayerMask.NameToLayer("UI");
        }

        /// <summary>
        /// 设置界面组深度。
        /// </summary>
        /// <param name="depth">界面组深度。</param>
        public void SetDepth(int depth)
        {
            _depth = depth;
            _canvas.overrideSorting = true;
            _canvas.sortingOrder = DepthFactor * depth;
        }
    }
}
