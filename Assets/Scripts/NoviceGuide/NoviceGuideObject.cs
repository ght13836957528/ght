using LS.Controller.NoviceGuide.StepAction;
using Main.Scripts.LSLogic.Model.NoviceGuide;
using UnityEngine;

namespace LS.Controller.NoviceGuide
{
    public class NoviceGuideObject
    {
        private NoviceGuideInfo _guideInfo;
        private NoviceGuideStepBaseAction _curStepAction;
        public NoviceGuideObject(NoviceGuideInfo guideInfo)
        {
            _guideInfo = guideInfo;
        }

        public void Init()
        {
            AddEventListener();
        }

        public void Dispose()
        {
            _guideInfo = null;
            if (_curStepAction != null)
            {
                _curStepAction.Exit();
                _curStepAction.Dispose();
            }
            OnGuideOver();
            RemoveEventListener();
        }

        private void AddEventListener()
        {
            
        }
        
        private void RemoveEventListener()
        {
            
        }

        public void Update()
        {
            if (_curStepAction != null)
            {
                _curStepAction.Execute();
            }
        }

        /// <summary>
        /// 从指定的index开始执行引导
        /// </summary>
        /// <param name="index">引导id</param>
        public void BeginGuide(int index)
        {
            _guideInfo.CurGuideIndex = index;
            if (_guideInfo.IsGuideOver())
            {
               Debug.Log("Guide is Over");
                return;
            }

            var guideInfo = _guideInfo.GetCurGuideInfo();
            DoActionStep(guideInfo);
        }

        /// <summary>
        /// 执行某一步操作
        /// </summary>
        /// <param name="stepInfo"></param>
        private void DoActionStep(NoviceGuideStepBaseInfo stepInfo)
        {
            if (stepInfo == null)
            {
                Debug.LogError("step info is null");
                return;
            }

            if (_curStepAction != null)
            {
                _curStepAction.Exit();
                _curStepAction.Dispose();
                _curStepAction = null;
            }

            var stepAction = NoviceGuideFactory.CreateGuideStepAction(stepInfo);
            if (_curStepAction == null)
            {
                Debug.LogError("create action is null,type==" + stepInfo.GetGuideType());
                return;
            }
            
            _curStepAction = stepAction;
            _curStepAction.Enter();
        }

        public void MoveNext()
        {
            if (_curStepAction == null)
            {
                Debug.LogError("curStepAction is null");
                return;
            }
            _curStepAction.Exit();
            _curStepAction.Dispose();
            _curStepAction = null;
            
            _guideInfo.NextStep();

            if (_guideInfo.IsGuideOver())
            {
                Debug.Log("Guide is over");
                OnGuideOver();
            }
            else
            {
                var curGuideInfo = _guideInfo.GetCurGuideInfo();
                DoActionStep(curGuideInfo);
            }
        }
        //引导结束回调
        private void OnGuideOver()
        {
            
        }
        
        
    }
}