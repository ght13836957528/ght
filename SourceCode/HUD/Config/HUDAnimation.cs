using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UW.HUD
{
    [System.Serializable]
    public struct AnimationAttribute
    {
        public string name;
        public AnimationCurve AlphaCurve;
        public AnimationCurve ScaleCurve;
        public AnimationCurve MoveXCurve;
        public AnimationCurve MoveYCurve;
    }
    public class HUDAnimation : MonoBehaviour
    {
        public AnimationAttribute DefaultAnimation;
        public AnimationAttribute[] AnimationAttributes;
        public float AnimationTime;
        public Dictionary<string, AnimationAttribute> AnimationConfig = new Dictionary<string, AnimationAttribute>();
        public void Init()
        {
            for(int i=0; i<AnimationAttributes.Length; ++i)
            {
                AnimationConfig.Add(AnimationAttributes[i].name, AnimationAttributes[i]);
            }
        }
        public void Release()
        {
            AnimationConfig.Clear();
        }

    }

}
