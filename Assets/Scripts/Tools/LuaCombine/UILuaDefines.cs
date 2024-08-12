using System;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

public enum LuaComType
{
    // 基础组件
    Unknown = 0,
    GameObject = 1,
    Transform = 2,
    RectTransform = 3,
    Toggle = 4,
    ToggleGroup = 5,
    Slider = 6,
    ParticleSystem = 7,
    RawImage = 8,
    ScrollRect = 9,
    InputField = 10,
    Material = 11,
    Animator = 12,
    Animation = 13,
    CanvasGroup = 14,
    TextMeshProUGUI = 15,
    TMP_InputField = 16,

    // 扩展的基础通用组件从100开始
    IMImage = 100,
    IMTextMeshProUGUI = 101,
    UIExpireTimer = 102,
    UIMultiScroller = 103,
    UIExtendLoader = 104,
    UIRedDot = 105,
    CDButton = 106,
    CheckBoxButton = 107,
    ToggleButton = 108,
    GroupButton = 109,
    SwitchButton = 110,
    SkeletonGraphic = 111,
    EnhancedScroller = 112,
    EnhancedScrollerCellView = 113,
    ToggleButtonGroup = 114,

    // 自定义通用业务组件从200开始
    UIItemIcon = 200,
}

[Serializable]
/// <summary>
/// Lua组件分组
/// </summary>
public class LuaComGroup
{
    /// <summary>
    /// 名称
    /// </summary>
    public string Name;

    /// <summary>
    /// Lua组件数组
    /// </summary>
    public LuaCom[] LuaComs;
}

[Serializable]
/// <summary>
/// Lua组件
/// </summary>
public class LuaCom
{
    /// <summary>
    /// 名称
    /// </summary>
    public string Name;

#if ODIN_INSPECTOR
    [OnValueChanged("OnTypeChange")]
#endif
    /// <summary>
    /// 类型
    /// </summary>
    public LuaComType Type;

#if ODIN_INSPECTOR
    [OnValueChanged("OnComObjChange")]
#endif
    public UnityEngine.Object ComObj;

    /// <summary>
    /// 原始物体的实例编号
    /// </summary>
    [HideInInspector] public int m_InstanceId;

    #region OnComObjChange 设置组件

   

    #endregion

    #region OnTypeChange 类型修改

    /// <summary>
    /// 类型修改
    /// </summary>
    private void OnTypeChange()
    {
#if UNITY_EDITOR
        if (ComObj == null)
        {
            Name = "";
            Type = LuaComType.Unknown;
            return;
        }

        LuaComType oldType = Type;

        Transform Trans = null;
        Transform[] arr = UnityEditor.Selection.transforms[0].GetComponentsInChildren<Transform>(true);
        if (null != arr)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i].gameObject.GetInstanceID() == m_InstanceId)
                {
                    Trans = arr[i];
                    break;
                }
            }
        }

        UnityEngine.Object tempObj = null;

        #region 设置类型

        switch (Type)
        {
           
            case LuaComType.InputField:
            {
                tempObj = Trans.GetComponent<InputField>();
                break;
            }
            case LuaComType.ScrollRect:
            {
                tempObj = Trans.GetComponent<ScrollRect>();
                break;
            }
            case LuaComType.Slider:
            {
                tempObj = Trans.GetComponent<Slider>();
                break;
            }
            case LuaComType.RectTransform:
            {
                tempObj = Trans.GetComponent<RectTransform>();
                break;
            }
            case LuaComType.Transform:
            {
                tempObj = Trans.GetComponent<Transform>();
                break;
            }
            case LuaComType.Toggle:
            {
                tempObj = Trans.GetComponent<Toggle>();
                break;
            }
            case LuaComType.ToggleGroup:
            {
                tempObj = Trans.GetComponent<ToggleGroup>();
                break;
            }
            case LuaComType.Animator:
            {
                tempObj = Trans.GetComponent<Animator>();
                break;
            }
            case LuaComType.GameObject:
            {
                tempObj = Trans.gameObject;
                break;
            }
    
            case LuaComType.TextMeshProUGUI:
            {
                tempObj = Trans.gameObject.GetComponent<TMPro.TextMeshProUGUI>();
                break;
            }

            case LuaComType.TMP_InputField:
            {
                tempObj = Trans.gameObject.GetComponent<TMPro.TMP_InputField>();
                break;
            }
      
            default:
                break;
        }

        #endregion

        if (tempObj != null)
        {
            ComObj = tempObj;
        }
        else
        {
            Type = oldType;
            Debug.LogError("您选择类型在当前组件上不存在 ComObj=" + ComObj.name);
        }
#endif
    }

    #endregion

    #region 获得组件的实例信息

    // 获得组件的GameObject
    public GameObject GetGameObject()
    {
        GameObject obj = null;

        switch (this.Type)
        {
            case LuaComType.Unknown:
            {
                break;
            }
            case LuaComType.GameObject:
            {
                obj = ComObj as GameObject;
                break;
            }
            case LuaComType.Transform:
            {
                Transform temp = ComObj as Transform;
                if (null != temp)
                    obj = temp.gameObject;

                break;
            }
            case LuaComType.Toggle:
            {
                Toggle temp = ComObj as Toggle;
                obj = temp?.gameObject;

                break;
            }
            case LuaComType.ToggleGroup:
            {
                ToggleGroup temp = ComObj as ToggleGroup;
                obj = temp?.gameObject;

                break;
            }
            case LuaComType.Slider:
            {
                Slider temp = ComObj as Slider;
                obj = temp?.gameObject;

                break;
            }
            case LuaComType.ParticleSystem:
            {
                ParticleSystem temp = ComObj as ParticleSystem;
                obj = temp?.gameObject;

                break;
            }
            case LuaComType.RawImage:
            {
                RawImage temp = ComObj as RawImage;
                obj = temp?.gameObject;

                break;
            }
            case LuaComType.ScrollRect:
            {
                ScrollRect temp = ComObj as ScrollRect;
                obj = temp?.gameObject;

                break;
            }
            case LuaComType.InputField:
            {
                InputField temp = ComObj as InputField;
                obj = temp?.gameObject;

                break;
            }
            case LuaComType.Material:
            {
                obj = null;

                break;
            }
            case LuaComType.Animator:
            {
                Animator temp = ComObj as Animator;
                obj = temp?.gameObject;

                break;
            }
            case LuaComType.Animation:
            {
                Animation temp = ComObj as Animation;
                obj = temp?.gameObject;

                break;
            }
            case LuaComType.CanvasGroup:
            {
                CanvasGroup temp = ComObj as CanvasGroup;
                obj = temp?.gameObject;

                break;
            }
          
            case LuaComType.TextMeshProUGUI:
            {
                TMPro.TextMeshProUGUI temp = ComObj as TMPro.TextMeshProUGUI;
                obj = temp?.gameObject;

                break;
            }
            case LuaComType.RectTransform:
            {
                RectTransform temp = ComObj as RectTransform;
                obj = temp?.gameObject;

                break;
            }
       
            default:
                break;
        }

        return obj;
    }

    // 获得绑定组件的实例ID;
    public int GetGameObjectInstanceId()
    {
        int instanceId = -1;

        GameObject temp = this.GetGameObject();
        if (null != temp)
            instanceId = temp.GetInstanceID();

        return instanceId;
    }

    #endregion
}
