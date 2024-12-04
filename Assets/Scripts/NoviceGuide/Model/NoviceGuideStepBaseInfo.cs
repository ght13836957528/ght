using LS.Controller.NoviceGuide;

namespace Main.Scripts.LSLogic.Model.NoviceGuide
{
    public class NoviceGuideStepBaseInfo 
    {
        private int _id;
        private int _nextId;
        protected NoviceGuideDefine.NoviceGuideType _type;

        public NoviceGuideStepBaseInfo()
        {
            Parse();
        }

        public void Dispose()
        {
            OnDispose();
        }

        public NoviceGuideDefine.NoviceGuideType GetGuideType()
        {
            return _type;
        }

        public int GetNextId()
        {
            return _nextId;
        }

        public void Parse()
        {
            // 解析公共部分字段；
            OnParse();
        }

        
        protected virtual void OnParse()
        {
        }

        protected virtual void OnDispose()
        {
        }
        
    }
}