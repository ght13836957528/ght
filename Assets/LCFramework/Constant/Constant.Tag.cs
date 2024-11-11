
namespace LS
{
    public static partial class Constant
    {
        /// <summary>
        /// 标签常量定义类
        /// </summary>
        public static class Tag
        {
            /// <summary>
            /// 默认资源加载优先级。
            /// </summary>
            internal const int DefaultPriority = 0;
              
            /// <summary>
            /// 同步资源加载优先级。
            /// </summary>
            internal const int SyncPriority = 99;
            // 动态加载图片标记
            public const string Name_Dynamic_load_Sprite = "_Dynamic_Load_Sprite_";
            public const string Name_Default = "Name_Default";
            public const string Name_Loading = "Name_Loading";

            public const string File_Extension_PNG = ".png";

            public const string Wildcard_PercentSign_CPP = "%d";
            public const string Wildcard_PercentSign_CSharp = "{0}";
        }
    }
}
