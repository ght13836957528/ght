using XLua;

[LuaCallCSharp]
public static class UILuaExtend
{
    /// <summary>
    /// 提供给lua检查gameObject是否为null
    /// </summary>
    /// <param name="o"></param>
    /// <returns></returns>
    public static bool IsNull(this UnityEngine.Object o)
    {
        return null == o;
    }

    public static bool IsNotNull(this UnityEngine.Object o)
    {
        return o;
    }
}