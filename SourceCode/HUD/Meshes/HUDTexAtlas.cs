using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace UW.HUD
{
    public class HUDTexAtlas
    {
        public enum Coordinates
        {
            Pixels,
            TexCoords,
        }

        public string m_szAtlasName = "Input name"; // 材质名字
        public string m_szTexName = "";   // 纹理名字
        public string m_szShaderName = ""; // shader名字
        public Material m_material;    // 材质
        public Texture m_MainAlpha;    // 主贴图的通道图
        public int m_nAtlasID = 0;        // 材质ID

        Coordinates m_Coordinates = Coordinates.Pixels;

        // Size in pixels for the sake of MakePixelPerfect functions.
        int m_PixelSize = 1;

        // Whether the atlas is using a pre-multiplied alpha material. -1 = not checked. 0 = no. 1 = yes.
        int m_PMA = -1;

        int m_nTexWidth = 1;   // 纹理的宽度
        int m_nTexHeight = 1;  // 纹理的高度

        bool m_bCanLOD = false; // 是不是可以LOD缩放

        // -----------------------------------------------------------
        // 以下时临时变量
        public int m_nSpriteNumb;  // 精寻对象数量
        public float m_fReleaseTime; // 释放时间
        public bool m_bLoading = false;      // 是不是正在加载中
        public bool m_bAddAssetBundleRef = false; // 是不是添加了AssetBundle的引用计数
        public string m_szMainResName;
        public string m_szAlphaResName;
        public float m_fLoadingTime = 0.0f; // 上一次加载的时间
        public AssetBundle m_mainBundle;
        public AssetBundle m_mainAlphaBundle;

        public delegate void OnLoadAtlas();
        public OnLoadAtlas m_lpOnLoadAtlas; // 加载纹理成功后的事件，因为是异步的操作
                                            // -----------------------------------------------------------

        public void CopyFromSetting(HUDTexAtlas from)
        {
            m_Coordinates = from.m_Coordinates;
            m_PixelSize = from.m_PixelSize;
            m_PMA = from.m_PMA;
        }
        public void SetTextureSizeByMaterial(Material mat)
        {
            Texture tex = mat != null ? mat.mainTexture : null;
            SetTextureSizeByTexture(tex);
            if (mat != null && mat.shader != null)
                m_szShaderName = mat.shader.name;
        }
        public void SetTextureSizeByTexture(Texture tex)
        {
            if (tex != null)
            {
                m_nTexWidth = tex.width;
                m_nTexHeight = tex.height;
            }
            else
            {
                m_nTexWidth = m_nTexHeight = 1;
            }
        }
        public int texWidth
        {
            get { return m_nTexWidth; }
        }
        public int texHeight
        {
            get { return m_nTexHeight; }
        }

        public Coordinates coordinates
        {
            get
            {
                return m_Coordinates;
            }
            set
            {
                m_Coordinates = value;
            }
        }
        public int pixelSize
        {
            get
            {
                return m_PixelSize;
            }
            set
            {
                m_PixelSize = value;
            }
        }

        public bool premultipliedAlpha
        {
            get
            {
                if (m_PMA == -1)
                {
                    Material mat = m_material;
                    m_PMA = (mat != null && mat.shader != null && mat.shader.name.Contains("Premultiplied")) ? 1 : 0;
                }
                return (m_PMA == 1);
            }
        }
        public bool IsCanLOD()
        {
            return m_bCanLOD;
        }
        public void SetLODFlag(bool bCanLOD)
        {
            m_bCanLOD = bCanLOD;
        }

        public void AdjustAtlas(HUDTexAtlas other)
        {
            m_szAtlasName = other.m_szAtlasName;
            m_szTexName = other.m_szTexName;
            m_nAtlasID = other.m_nAtlasID;

            m_szShaderName = other.m_szShaderName;
            m_PixelSize = other.m_PixelSize;
            m_Coordinates = other.m_Coordinates;
            m_nTexWidth = other.m_nTexWidth;
            m_nTexHeight = other.m_nTexHeight;
            m_bCanLOD = other.m_bCanLOD;
        }

        public void Serailize(ref HUDCSerialize ar)
        {
            int nCoordinatesType = (int)m_Coordinates;
            ar.ReadWriteValue(ref m_szAtlasName);
            ar.ReadWriteValue(ref m_szTexName);
            ar.ReadWriteValue(ref nCoordinatesType);
            ar.ReadWriteValue(ref m_PixelSize);
            m_Coordinates = nCoordinatesType == (int)Coordinates.Pixels ? Coordinates.Pixels : Coordinates.TexCoords;
            ar.ReadWriteValue(ref m_nTexWidth);
            ar.ReadWriteValue(ref m_nTexHeight);
            if (ar.GetVersion() >= 1)
            {
                ar.ReadWriteValue(ref m_nAtlasID);
            }
            if (ar.GetVersion() >= 2)
            {
                ar.ReadWriteValue(ref m_szShaderName);
            }
            if (ar.GetVersion() >= 4)
            {
                ar.ReadWriteValue(ref m_bCanLOD);
            }
        }
    };
}

 
