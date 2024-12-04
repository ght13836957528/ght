namespace LS.Controller.NoviceGuide
{
    public class NoviceGuideVersion
    {
        
        private enum NoviceGuideType
        {
            None,
            k1,
            k2,
        }

        private NoviceGuideType _curGuideType;
        
        public NoviceGuideVersion()
        {
            _curGuideType = NoviceGuideType.None;
        }

        public void Init()
        {
            
        }

        public void Dispose()
        {
            
        }

        public int GetFirstStepId()
        {
            return 0;
        }
    }
}