using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UW.HUD;

namespace UW.HUD
{
    public class HUDSpriteInfo
    {
        public string name = "Unity Bug";   // 对象的名字
        public Rect outer = new Rect(0f, 0f, 1f, 1f);     // 外框，精灵的实际大小（在纹理的像素坐标)
        public Rect inner = new Rect(0f, 0f, 1f, 1f);     // 内框，用来做填充模式时的像素坐标，这个必须是在外框之内的
        public bool rotated = false;

        // Padding is needed for trimmed sprites and is relative to sprite width and height
        public float paddingLeft = 0f;   // 用来做精灵图层选择时扩展选择框范围的东东，没有实际意义
        public float paddingRight = 0f;
        public float paddingTop = 0f;
        public float paddingBottom = 0f;

        // 下面是扩展属性
        public int m_nNameID;   // 精灵ID
        public int m_nAtlasID;  // 材质ID
        public string m_szAtlasName;  // 对应的材质名字

        public bool hasPadding { get { return paddingLeft != 0f || paddingRight != 0f || paddingTop != 0f || paddingBottom != 0f; } }

        public HUDSpriteInfo Clone()
        {
            HUDSpriteInfo p = new HUDSpriteInfo();
            p.Copy(this);
            return p;
        }

        // 功能：拷贝对象 
        public void Copy(HUDSpriteInfo src)
        {
            name = src.name.Clone() as string;
            outer = new Rect(src.outer.xMin, src.outer.yMin, src.outer.width, src.outer.height);
            inner = new Rect(src.inner.xMin, src.inner.yMin, src.inner.width, src.inner.height);
            rotated = src.rotated;
            paddingLeft = src.paddingLeft;
            paddingRight = src.paddingRight;
            paddingTop = src.paddingTop;
            paddingBottom = src.paddingBottom;
            m_nNameID = src.m_nNameID;
            m_nAtlasID = src.m_nAtlasID;
            m_szAtlasName = src.m_szAtlasName.Clone() as string;
        }
        public void Serailize(ref HUDCSerialize ar)
        {
            ar.ReadWriteValue(ref name);
            ar.ReadWriteValue(ref outer);
            ar.ReadWriteValue(ref inner);
            ar.ReadWriteValue(ref rotated);
            ar.ReadWriteValue(ref paddingLeft);
            ar.ReadWriteValue(ref paddingRight);
            ar.ReadWriteValue(ref paddingTop);
            ar.ReadWriteValue(ref paddingBottom);
            ar.ReadWriteValue(ref m_szAtlasName);
            if (ar.GetVersion() >= 1)
            {
                ar.ReadWriteValue(ref m_nNameID);
                ar.ReadWriteValue(ref m_nAtlasID);
            }
        }
    }
}

   
