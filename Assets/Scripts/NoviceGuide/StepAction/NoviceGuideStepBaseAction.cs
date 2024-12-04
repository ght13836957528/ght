using Main.Scripts.LSLogic.Model.NoviceGuide;
using UnityEngine;

namespace LS.Controller.NoviceGuide.StepAction
{
    public class NoviceGuideStepBaseAction
    {
        protected NoviceGuideStepBaseInfo BaseInfo;
        private NoviceGuideDefine.NoviceGuideActionState _actionState;

        public NoviceGuideStepBaseAction(NoviceGuideStepBaseInfo stepInfo)
        {
            Init(stepInfo);
        }

        private void Init(NoviceGuideStepBaseInfo stepInfo)
        {
            OnAddEventListener();
            if (BaseInfo == null)
            {
                Debug.LogError("stepInfo is null");
                return;
            }

            BaseInfo = stepInfo;
            _actionState = NoviceGuideDefine.NoviceGuideActionState.Init;
            OnInit();
        }


        public void Enter()
        {
            //可添加基类公共字段统一逻辑的处理，例如关闭所有弹出页面的逻辑；lag逻辑等
            _actionState = NoviceGuideDefine.NoviceGuideActionState.Enter;
            OnEnter();
        }

        public void Execute()
        {
            _actionState = NoviceGuideDefine.NoviceGuideActionState.Update;
            OnExecute();
        }

        public void Exit()
        {
            _actionState = NoviceGuideDefine.NoviceGuideActionState.Exit;
            OnExit();
        }

        public void Dispose()
        {
            OnRemoveEventListener();
            OnDispose();
        }
        
        /// <summary>
        /// 该步骤完成，执行下一步
        /// </summary>
        protected void MoveNext()
        {
            // NoviceGuideController.getInstance().MoveNext();
        }

       

        protected virtual void OnAddEventListener()
        {
        }
        
        protected virtual void OnRemoveEventListener()
        {
        }

        protected virtual void OnInit()
        {
        }

        protected virtual void OnEnter()
        {
        }

        protected virtual void OnExecute()
        {
        }

        protected virtual void OnExit()
        {
        }

        protected virtual void OnDispose()
        {
        }
        
      
      
    }
}