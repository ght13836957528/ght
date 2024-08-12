using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Tools.LuaCombine
{
    public class UIForm : MonoBehaviour
    {
        
        [Header("Lua组件分组")] public LuaComGroup[] LuaComGroups;
        
        public string origionalLuaFilePath;
        
#if ODIN_INSPECTOR
        [Button(ButtonSizes.Large)]
#endif
        private void AutoBindCompontent()
        {
            var groups = UILuaComBinding.AutoBindCompontent(transform, origionalLuaFilePath, LuaComGroups);
            EditorUtility.SetDirty(this);
        }
    }
}