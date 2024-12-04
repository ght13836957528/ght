using System.Collections.Generic;
using Framework;
using LS.Controller.NoviceGuide.Trigger;
using UnityEngine;

namespace LS.Controller.NoviceGuide
{
    /// <summary>
    /// 触发引导的逻辑处理，仅为引导的开始，等待操作的类似逻辑，需要在自己定义的action中进行处理
    /// </summary>
    public class NoviceGuideTrigger
    {
        private Dictionary<int, List<NoviceGuideStepTriggerCondition>> _conditions;

        public NoviceGuideTrigger()
        {
            _conditions = new Dictionary<int, List<NoviceGuideStepTriggerCondition>>();
        }

        public void Init()
        {
            AddEventListener();
            ParseGuideTable();
        }

        public void Dispose()
        {
            RemoveEventListener();
            _conditions.Clear();
            _conditions = null;
        }

        private void AddEventListener()
        {
            // GameEntry.Event.Subscribe(EventId.NoviceGuideEventTrigger, OnTrigger);
        }

        private void RemoveEventListener()
        {
            // GameEntry.Event.Unsubscribe(EventId.NoviceGuideEventTrigger, OnTrigger);
        }

        private void ParseGuideTable()
        {
            
        }

        private void OnTrigger(object sender)
        {
            // CommonEventArgs args = e as CommonEventArgs;
            // if (args == null)
            // {
            //     return;
            // }
            //
            // var triggerParam = args.UserData as NoviceGuideTriggerParam;
            // if (triggerParam == null)
            // {
            //     Debug.LogError("triggerParam is null");
            //     return;
            // }
            //
            // var triggerType = (int)triggerParam.Type;
            // if (!_conditions.ContainsKey(triggerType))
            // {
            //     Debug.LogError("_conditions not ContainsKey" + (NoviceGuideDefine.NoviceGuideTriggerType)triggerType);
            //     return;
            // }
            //
            // var conditionList = _conditions[triggerType];
            // foreach (var condition in conditionList)
            // {
            //     if (condition.Equals(triggerParam))
            //     {
            //         var jumpId = condition.GetJumpId();
            //         // NoviceGuideController.getInstance().Play(jumpId);
            //         break;
            //     }
            // }
        }
    }
}