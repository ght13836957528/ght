// -------------------------------------------------------------------------------------------
// @说      明: 业务逻辑对象池数据基类
// @作      者: zhoumingfeng
// @版  本  号: V1.00
// @创建时间: 2024.05.30
// -------------------------------------------------------------------------------------------

namespace Framework.Pool
{
    public abstract class LogicData<T> : BaseLogicData where T : BaseLogicData, new()
    {
        public static T Create()
        {
            return LogicPoolManager.Instance.Spawn<T>();
        }
    }
}