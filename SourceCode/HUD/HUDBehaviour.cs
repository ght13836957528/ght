using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UIBattleRenderPassFeature;

namespace UW.HUD
{
    public enum HUDHpType
    {
        None = 0,
        SmallBlueHp,
        SmallRedHp,
    }
    public class HUDBehaviour : MonoBehaviour
    {
        private HpContext m_hpContext;
        public HpContext HPContext { get => m_hpContext; }
        //public HUDRenderMesh HpMesh { get => hpMesh; set => hpMesh = value; }
        //public HUDRenderMesh JumpWorldMesh { get => jumpWorldMesh; set => jumpWorldMesh = value; }
       
        private JumpWorldContext m_jpContext;
        private AtlasContext m_atlasContext;
        public static Camera s_HUDCamera;
        public static Camera s_HUDUICamera;

        public bool isDebug = false;
        public Transform target = null;
        //private HUDRenderMesh hpMesh = null;
        //private HUDRenderMesh jumpWorldMesh = null;
        //CommandBuffer cmd;
        private Dictionary<int, HUDJumpWorld> m_jumpWorldDic = new Dictionary<int, HUDJumpWorld>();
        private List<int> m_jumpWorldKeys = new List<int>();
        private bool m_IsInit = false;

       
        public void Init()
        {
            var atlasMng = gameObject.GetComponent<AtlasManager>();
            var hudSetting = gameObject.GetComponent<HUDAnimation>();

            m_atlasContext = new AtlasContext();
            m_atlasContext.Init(atlasMng, hudSetting);

            m_hpContext = new HpContext();
            m_hpContext.Init(m_atlasContext);

            m_jpContext = new JumpWorldContext();
            m_jpContext.Init(m_atlasContext);

            m_IsInit = true;

          
        }
        void Update()
        {
            if (!m_IsInit)
            {
                return;
            }
            if (GetHUDMainCamera() == null)
                return;
            m_hpContext.UpdateMesh();
            m_jpContext.UpdateMesh();
            UpdateJumpWorld();
            if (isDebug)
               DebugTest();
        }
        void UpdateJumpWorld()
        {
            for (int i = 0; i < m_jumpWorldKeys.Count; i++)
            {
                HUDJumpWorld hUDJumpWorld;
                if (m_jumpWorldDic.TryGetValue(m_jumpWorldKeys[i], out hUDJumpWorld))
                {
                    hUDJumpWorld.Update();
                }
            }
        }

        private void DebugTest()
        {
#if UNITY_EDITOR
            if (target == null)
                return;

            if (Input.GetKeyDown(KeyCode.Keypad0))
            {
                ShowJumpWorld(1,(int)JumpWorldType.Damage, target.transform, -1000);
            }
            if (Input.GetKeyDown(KeyCode.Keypad1))
            {
                ShowJumpWorld(1, (int)JumpWorldType.Cure, target.transform, -999);
            }
            if (Input.GetKeyDown(KeyCode.Keypad2))
            {
                ShowJumpWorld(1, (int)JumpWorldType.Crit, target.transform, -888);
            }
            if (Input.GetKeyDown(KeyCode.Keypad3))
            {
                ShowJumpWorld(1, (int)JumpWorldType.AngerAdd, target.transform, -777);
            }
            if (Input.GetKeyDown(KeyCode.Keypad4))
            {
                ShowJumpWorld(1, (int)JumpWorldType.AngerMinus, target.transform, -666);
            }
            if (Input.GetKeyDown(KeyCode.Keypad5))
            {
                ShowJumpWorld(1, (int)JumpWorldType.Miss, target.transform, -555);
            }
            if (Input.GetKeyDown(KeyCode.Keypad6))
            {
                ShowJumpWorld(1, (int)JumpWorldType.ShieldMinus, target.transform, -444);
            }
            if (Input.GetKeyDown(KeyCode.Keypad7))
            {
                ShowJumpWorld(1, (int)JumpWorldType.ShieldAdd, target.transform, -333);
            }
            if (Input.GetKeyDown(KeyCode.Keypad8))
            {
                ShowJumpWorld(1, (int)JumpWorldType.PhysicsDefAdd, target.transform, -222);
            }
            if (Input.GetKeyDown(KeyCode.Keypad9))
            {
                ShowJumpWorld(1, (int)JumpWorldType.PhysicsDefSubRight, target.transform, -111);
            }

#endif
        }
        //private void LateUpdate()
        //{
        //    ExecuteMesh();
        //}
        public void ExecuteMesh()
        {
            //if (HpMesh == null && JumpWorldMesh == null)
            //    return;
            //if (cmd == null)
            //{
            //    cmd = new CommandBuffer();
            //}
            //Camera camera = GetHUDUICamera();
            //if (camera != null)
            //{
            //    camera.RemoveCommandBuffer(CameraEvent.AfterImageEffects, cmd);
            //}
            //cmd.Clear();
            //if (HpMesh != null)
            //{
            //    cmd.DrawMesh(HpMesh.RealMesh, Matrix4x4.identity, HpMesh.RealMaterial);
            //}
            //if (JumpWorldMesh != null)
            //{
               
            //    cmd.DrawMesh(JumpWorldMesh.RealMesh, Matrix4x4.identity, JumpWorldMesh.RealMaterial);
            //}
            //if (camera != null)
            //{
            //    camera.AddCommandBuffer(CameraEvent.AfterImageEffects, cmd);
            //}
         
        }

        public void Release()
        {
            if (m_hpContext != null)
            {
                m_hpContext.Release();
                m_hpContext = null;
            }
            if (m_jpContext != null)
            {
                m_jpContext.Release();
                m_jpContext = null;
            }
            if(m_atlasContext !=null)
            {
                m_atlasContext.Release();
                m_atlasContext = null;
            }
            s_HUDCamera = null;
            s_HUDUICamera = null;
            //hpMesh = null;
            //jumpWorldMesh = null;
            UIBattleHudPass.ClearMesh();
            m_IsInit = false;

            for (int i = 0; i < m_jumpWorldKeys.Count; i++)
            {
                HUDJumpWorld hUDJumpWorld;
                if (m_jumpWorldDic.TryGetValue(m_jumpWorldKeys[i], out hUDJumpWorld))
                {
                    hUDJumpWorld.Release();
                }
            }
            m_jumpWorldKeys.Clear();
            m_jumpWorldDic.Clear();
        }

        public static Camera GetHUDMainCamera()
        {
            if (s_HUDCamera == null)
            {
                var mainCam = GameObject.Find("BattleCamera");
                if (mainCam != null)
                {
                    s_HUDCamera = mainCam.GetComponent<Camera>();
                }
                else
                {
                    Debug.LogError("not find main_cam in scene");
                }
            }
            return s_HUDCamera;
        }

        public static Camera GetHUDUICamera()
        {
            if (s_HUDUICamera == null)
            {
                var camUI = GameObject.Find("GUICamera");
                if (camUI != null)
                {
                    s_HUDUICamera = camUI.GetComponent<Camera>();
                }
                else {
                    Debug.LogError("not find cameraUI in scene");
                }
            }
            return s_HUDUICamera;
        }
        public Dictionary<int, HUDTitle> GetHpTitles()
        {
            return m_hpContext.GetTitles();
        }

        public int CreateHpTitle(int hpType, int entityId, float offsetY=6, Transform target = null)
        {
            m_hpContext.CreateHpTitle((HUDHpType)hpType, entityId, offsetY, target);
            return entityId;
        }
        public void ShowHpTitle(int titleId, bool bShow)
        {
            m_hpContext.ShowHpTitle(titleId, bShow);
        }
        public void SetHpRate(int titleId, float hpRate)
        {
            m_hpContext.SetHpRate(titleId, hpRate);
        }
        public void ShowJumpWorld(int eneityId,int nType, Transform target=null, int number=0, float offsetY = 2)
        {
            HUDJumpWorld jumpWorld;
            if (!m_jumpWorldDic.TryGetValue(eneityId, out jumpWorld))
            {
                jumpWorld = new HUDJumpWorld(m_jpContext, eneityId);
                m_jumpWorldDic.Add(eneityId, jumpWorld);
                m_jumpWorldKeys.Add(eneityId);
            }
            jumpWorld.TryShowJumpWorld(nType, target, number, offsetY);
        }
    }
}
