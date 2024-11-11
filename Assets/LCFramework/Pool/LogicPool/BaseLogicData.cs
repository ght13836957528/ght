// -------------------------------------------------------------------------------------------
// @说      明: 业务逻辑对象池数据基类
// @作      者: zhoumingfeng
// @版  本  号: V1.00
// @创建时间: 2024.05.29
// -------------------------------------------------------------------------------------------

namespace Framework.Pool
{
    public abstract class BaseLogicData : ILogicData
    {
        public abstract void Clear();

        public virtual void SetDirty()
        {
            this.Clear();

            LogicPoolManager.Instance.Recycle(this);
        }
    }
}