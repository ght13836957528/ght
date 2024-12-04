namespace LS.Controller.NoviceGuide
{
    public class NoviceGuideDefine
    {
        public enum NoviceGuideType
        {
            CameraMove, // 镜头移动
            Plot, // 对话
            
        }

        public enum NoviceGuideActionState
        {
            Init, 
            Enter,
            Update,
            Exit, 
        }


        public enum NoviceGuideTriggerType
        {
            Unknown, 
            CityBuildingUpgrade,
        }
    }
}