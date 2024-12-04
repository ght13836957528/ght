

namespace LS.Controller.NoviceGuide.Trigger
{
    public class NoviceGuideTriggerParam
    {
        public NoviceGuideDefine.NoviceGuideTriggerType Type;
        public bool ParamBool { get; set; }
        public int ParamInt { get; set; }
        public long ParamLong { get; set; }
        public float ParamFloat { get; set; }
        public string ParamString { get; set; }

        public NoviceGuideTriggerParam(NoviceGuideDefine.NoviceGuideTriggerType type, bool param)
        {
            Type = type;
            ParamBool = param;
        }
        public NoviceGuideTriggerParam(NoviceGuideDefine.NoviceGuideTriggerType type, int param)
        {
            Type = type;
            ParamInt = param;
        }
        public NoviceGuideTriggerParam(NoviceGuideDefine.NoviceGuideTriggerType type, long param)
        {
            Type = type;
            ParamLong = param;
        }
        
        public NoviceGuideTriggerParam(NoviceGuideDefine.NoviceGuideTriggerType type, float param)
        {
            Type = type;
            ParamFloat = param;
        }
        public NoviceGuideTriggerParam(NoviceGuideDefine.NoviceGuideTriggerType type, string param)
        {
            Type = type;
            ParamString = param;
        }

    }
}