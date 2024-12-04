

namespace LS.Controller.NoviceGuide.Trigger
{
    public class NoviceGuideStepTriggerCondition
    {
        private int _id;
        private NoviceGuideDefine.NoviceGuideTriggerType _type;
        private int _jumpId ;
        private string _triggerParam ;

        public NoviceGuideStepTriggerCondition()
        {
            _id = -1;
            _type = NoviceGuideDefine.NoviceGuideTriggerType.Unknown;
            _jumpId = -1;
            _triggerParam = "";
        }

        public void Parse()
        {
            
        }
        
        public void Dispose()
        {
            
            
        }
        
        public virtual bool Equals(NoviceGuideTriggerParam param)
        {
            return false;
        }
        
        protected virtual void OnDispose()
        {
          
        }

        public int GetJumpId()
        {
            return _jumpId;
        }

    }
}