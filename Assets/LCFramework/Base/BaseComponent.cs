using Framework;
using Unity.VisualScripting;
using UnityEngine;

namespace LCFramework.Base
{

    /// <summary>
    /// 基础组件。
    /// </summary>
    [DisallowMultipleComponent]
    public class BaseComponent : FrameworkComponent
    {
        private const int DefaultDpi = 96;  // default windows dpi

        private string m_GameVersion = string.Empty;
        private int m_InternalApplicationVersion = 0;
        private float m_GameSpeedBeforePause = 1f;

        [SerializeField]
        private bool m_EditorResourceMode = true;

        // [SerializeField]
        // private Icons.Language m_EditorLanguage = Icons.Language.Unspecified;

        [SerializeField]
        private int m_FrameRate = 30;

        [SerializeField]
        private float m_GameSpeed = 1f;

        [SerializeField]
        private bool m_RunInBackground = true;

        [SerializeField]
        private bool m_NeverSleep = true;

        /// <summary>
        /// 获取或设置游戏版本号。
        /// </summary>
        public string GameVersion
        {
            get
            {
                return m_GameVersion;
            }
            set
            {
                m_GameVersion = value;
            }
        }

        /// <summary>
        /// 获取或设置应用程序内部版本号。
        /// </summary>
        public int InternalApplicationVersion
        {
            get
            {
                return m_InternalApplicationVersion;
            }
            set
            {
                m_InternalApplicationVersion = value;
            }
        }

        /// <summary>
        /// 获取或设置是否使用编辑器资源模式（仅编辑器内有效）。
        /// </summary>
        public bool EditorResourceMode
        {
            get
            {
                return m_EditorResourceMode;
            }
            set
            {
                m_EditorResourceMode = value;
            }
        }

        // /// <summary>
        // /// 获取或设置编辑器语言（仅编辑器内有效）。
        // /// </summary>
        // public Icons.Language EditorLanguage
        // {
        //     get
        //     {
        //         return m_EditorLanguage;
        //     }
        //     set
        //     {
        //         m_EditorLanguage = value;
        //     }
        // }

        /// <summary>
        /// 获取或设置编辑器资源辅助器。
        /// </summary>
        // public IResourceManager EditorResourceHelper
        // {
        //     get;
        //     set;
        // }

        /// <summary>
        /// 获取或设置游戏帧率。
        /// </summary>
        public int FrameRate
        {
            get
            {
                return m_FrameRate;
            }
            set
            {
                Application.targetFrameRate = m_FrameRate = value;
            }
        }

        /// <summary>
        /// 获取或设置游戏速度。
        /// </summary>
        public float GameSpeed
        {
            get
            {
                return m_GameSpeed;
            }
            set
            {
                Time.timeScale = m_GameSpeed = (value >= 0f ? value : 0f);
            }
        }

        /// <summary>
        /// 获取游戏是否暂停。
        /// </summary>
        public bool IsGamePaused
        {
            get
            {
                return m_GameSpeed <= 0f;
            }
        }

        /// <summary>
        /// 获取是否正常游戏速度。
        /// </summary>
        public bool IsNormalGameSpeed
        {
            get
            {
                return m_GameSpeed == 1f;
            }
        }

        /// <summary>
        /// 获取或设置是否允许后台运行。
        /// </summary>
        public bool RunInBackground
        {
            get
            {
                return m_RunInBackground;
            }
            set
            {
                Application.runInBackground = m_RunInBackground = value;
            }
        }

        /// <summary>
        /// 获取或设置是否禁止休眠。
        /// </summary>
        public bool NeverSleep
        {
            get
            {
                return m_NeverSleep;
            }
            set
            {
                m_NeverSleep = value;
                Screen.sleepTimeout = value ? SleepTimeout.NeverSleep : SleepTimeout.SystemSetting;
            }
        }

        /// <summary>
        /// 游戏框架组件初始化。
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            //Log.InfoFormat("Game Framework version is {0}. Unity Game Framework version is {1}.", FrameworkEntry.Version, GameEntry.Version);

#if UNITY_5_3_OR_NEWER || UNITY_5_3

            m_EditorResourceMode &= Application.isEditor;
            if (m_EditorResourceMode)
            {
                Debug.Log("During this run, Game Framework will use editor resource files, which you should validate first.");
            }

            Application.targetFrameRate = m_FrameRate;
            Time.timeScale = m_GameSpeed;
            Application.runInBackground = m_RunInBackground;
            Screen.sleepTimeout = m_NeverSleep ? SleepTimeout.NeverSleep : SleepTimeout.SystemSetting;
#else
            Log.Error("Game Framework only applies with Unity 5.3 and above, but current Unity version is {0}.", Application.unityVersion);
            GameEntry.Shutdown(ShutdownType.Quit);
#endif
#if UNITY_5_6_OR_NEWER
            Application.lowMemory += OnLowMemory;
#endif
        }

        private void Start()
        {

        }

        private void Update()
        {
            // FrameworkEntry.Update(Time.deltaTime, Time.unscaledDeltaTime);
        }

        private void OnDestroy()
        {
#if UNITY_5_6_OR_NEWER
            Application.lowMemory -= OnLowMemory;
#endif
            // FrameworkEntry.Shutdown();
        }

        /// <summary>
        /// 暂停游戏。
        /// </summary>
        public void PauseGame()
        {
            if (IsGamePaused)
            {
                return;
            }

            m_GameSpeedBeforePause = GameSpeed;
            GameSpeed = 0f;
        }

        /// <summary>
        /// 恢复游戏。
        /// </summary>
        public void ResumeGame()
        {
            if (!IsGamePaused)
            {
                return;
            }

            GameSpeed = m_GameSpeedBeforePause;
        }

        /// <summary>
        /// 重置为正常游戏速度。
        /// </summary>
        public void ResetNormalGameSpeed()
        {
            if (IsNormalGameSpeed)
            {
                return;
            }

            GameSpeed = 1f;
        }

        internal void Shutdown()
        {
            Destroy(gameObject);
        }

        private void OnLowMemory()
        {
            Debug.Log("Low memory reported...");
        }
    }
}
