using Framework;
using Main.Scripts.LSLogic.Model.NoviceGuide;
using UnityEngine;

namespace LS.Controller.NoviceGuide
{
    public class NoviceGuideController
    {
        private NoviceGuideObject _guideObject;
        private NoviceGuideInfo _guideInfo;
        private NoviceGuideVersion _version;
        private NoviceGuideTrigger _trigger;
        
        protected  void OnInit()
        {
            
            AddEventLister();
            Init();
        }

        protected  void OnDestroy()
        {
           
            RemoveEventListener();
            UnInit();
        }

        private void AddEventLister()
        {
            
        }

        private void RemoveEventListener()
        {
            
        }

        private void Init()
        {
            _guideInfo = new NoviceGuideInfo();
            _guideInfo.Init();
            
            _guideObject = new NoviceGuideObject(_guideInfo);
            _guideObject.Init();

            _version = new NoviceGuideVersion();
            _version.Init();

            _trigger = new NoviceGuideTrigger();
            _trigger.Init();
        }

        private void UnInit()
        {
            _guideInfo.Dispose();
            _guideInfo = null;
            
            _guideObject.Dispose();
            _guideObject = null;
            
            _version.Dispose();
            _version = null;
            
            _trigger.Dispose();
            _trigger = null;
        }

        public  void onEnterFrame(float dt) // 每秒执行，可能后续需要改成每帧执行
        {
            _guideObject.Update();
        }

        public void InitSeverData()
        {
            
        }

        /// <summary>
        /// 从第一步开始新手引导
        /// </summary>
        public void StartGuide()
        {
            if (_version == null)
            {
                Debug.LogError("version is null");
                return;
            }

            var firstStep = _version.GetFirstStepId();
            Play(firstStep);
        }

        /// <summary>
        ///  恢复引导
        /// </summary>
        private void Cover()
        {
            var coverId = GetCoverId();
            Play(coverId);
        }

        private int GetCoverId()
        {
            return -1;
        }

        public void Play(int guideId)
        {
            if (_guideObject == null)
            {
                Debug.LogError("guideObject is null");
                return;
            }
            _guideObject.BeginGuide(guideId);
        }


        public void MoveNext()
        {
            _guideObject.MoveNext();
        }


    }
}