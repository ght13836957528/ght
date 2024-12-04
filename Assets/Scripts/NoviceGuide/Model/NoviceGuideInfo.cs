using System.Collections.Generic;
using UnityEngine;

namespace Main.Scripts.LSLogic.Model.NoviceGuide
{
    public class NoviceGuideInfo
    {
        private Dictionary<int,NoviceGuideStepBaseInfo> _stepInfos;
        
        public int CurGuideIndex
        {
            get;
            set;
        }
        
        public void Init()
        {
            InitGuideStepTable();
        }
        
        public void Dispose()
        {
            foreach (var item in _stepInfos)
            {
                item.Value.Dispose();
            }
            _stepInfos.Clear();
            _stepInfos = null;
        }

        private void InitGuideStepTable()
        {
            //解析Guide表数据,填充_stepInfos
        }

        /// <summary>
        /// 引导是否结束
        /// </summary>
        /// <returns></returns>
        public bool IsGuideOver()
        {
            return CurGuideIndex == -1 || _stepInfos == null || _stepInfos?[CurGuideIndex] != null;
        }

        public NoviceGuideStepBaseInfo GetCurGuideInfo()
        {
            return _stepInfos[CurGuideIndex];
        }

        /// <summary>
        /// 数据层下一步
        /// </summary>
        public void NextStep()
        {
            var stepInfo = GetCurGuideInfo();
            if (stepInfo == null)
            {
                Debug.LogError("cur guide info is null");
                return;
            }

            CurGuideIndex = stepInfo.GetNextId();
        }
        
       
    }
}