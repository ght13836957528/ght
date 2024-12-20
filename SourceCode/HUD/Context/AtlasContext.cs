using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UW.HUD
{
   
    public class AtlasContext
    {
        public Texture AtlasTex
        {
            get { return AtlasManager.AtlasPng; }
        }
        public Shader AtlasShader
        {
            get { return AtlasManager.HudSprite; }
        }
        public HUDAnimation HUDSetting;
        public AtlasManager AtlasManager;
        private Dictionary<string, HUDSpriteInfo> m_AllSprite = new Dictionary<string, HUDSpriteInfo>();   // 所有的精灵对象
        private Dictionary<string, HUDTexAtlas> m_TexAtlas = new Dictionary<string, HUDTexAtlas>();    // 材质对象
        protected CMyArray<HUDSpriteInfo> m_SpritePtr = new CMyArray<HUDSpriteInfo>();
        protected CMyArray<HUDTexAtlas> m_AtlasPtr = new CMyArray<HUDTexAtlas>();
        protected int m_nFileVersion = 0;
        public void Init(AtlasManager atlasManager, HUDAnimation hudSetting)
        {
            hudSetting.Init();
            AtlasManager = atlasManager;
            HUDSetting = hudSetting;
            InitCfg(atlasManager.AtlasConfig.bytes);
            InitSpriteId();
        }
        public void Release()
        {
            AtlasManager.Release();
            HUDSetting.Release();
            AtlasManager = null;
            HUDSetting = null;
            m_AllSprite.Clear();
            m_TexAtlas.Clear();
            m_SpritePtr.Clear();
            m_AtlasPtr.Clear();
        }

        private void InitCfg(byte[] fileData)
        {
            if (fileData != null && m_TexAtlas.Count == 0)
            {
                HUDCSerialize arRead = new HUDCSerialize(SerializeType.read, fileData, fileData.Length);
                Serialize(arRead);
                MakeSpriteAtlasID();
            }
        }
        public void InitSpriteId()
        {
            AtlasManager.BloodBk.SpriteId = SpriteNameToID(AtlasManager.BloodBk.SpriteName);
            AtlasManager.BloodBlue.SpriteId = SpriteNameToID(AtlasManager.BloodBlue.SpriteName);
            AtlasManager.BloodRed.SpriteId = SpriteNameToID(AtlasManager.BloodRed.SpriteName);
            AtlasManager.InitJumpWordSpriteConfig(this);
        }

        protected void Serialize(HUDCSerialize ar)
        {
            byte yVersion = 4;
            ar.ReadWriteValue(ref yVersion);
            ar.SetVersion(yVersion);
            if (yVersion > 2)
                ar.ReadWriteValue(ref m_nFileVersion);
            ar.SerializeDictionary<string, HUDSpriteInfo>(ref m_AllSprite, SerializeIterator);
            ar.SerializeDictionary<string, HUDTexAtlas>(ref m_TexAtlas, SerializeIterator);
        }
        void SerializeIterator(HUDCSerialize ar, ref string key, ref HUDSpriteInfo value)
        {
            if (key == null) key = string.Empty;
            if (value == null) value = new HUDSpriteInfo();
            ar.ReadWriteValue(ref key);
            value.Serailize(ref ar);
        }
        void SerializeIterator(HUDCSerialize ar, ref string key, ref HUDTexAtlas value)
        {
            if (key == null) key = string.Empty;
            if (value == null) value = new HUDTexAtlas();
            ar.ReadWriteValue(ref key);
            value.Serailize(ref ar);
        }
        protected bool MakeSpriteAtlasID()
        {
            m_SpritePtr.Clear();
            m_AtlasPtr.Clear();
            m_SpritePtr.reserve(m_AllSprite.Count);
            m_AtlasPtr.reserve(m_TexAtlas.Count);

            bool bDirty = false;
            CMyArray<HUDTexAtlas> newAtlas = new CMyArray<HUDTexAtlas>();

            Dictionary<string, HUDTexAtlas>.Enumerator itAtlas = m_TexAtlas.GetEnumerator();
            int nMaxAtlasID = m_TexAtlas.Count + 1;
            while (itAtlas.MoveNext())
            {
                HUDTexAtlas atlas = itAtlas.Current.Value;
                if (atlas.m_nAtlasID > 0 && atlas.m_nAtlasID <= nMaxAtlasID)
                {
                    if (m_AtlasPtr.IsValid(atlas.m_nAtlasID - 1) && m_AtlasPtr[atlas.m_nAtlasID - 1] != null)
                    {
                        newAtlas.push_back(atlas);
                    }
                    else
                    {
                        m_AtlasPtr.GrowSet(atlas.m_nAtlasID - 1, atlas);
                    }
                }
                else
                {
                    newAtlas.push_back(atlas);
                }
            }
            if (newAtlas.size() > 0)
                bDirty = true;
            int nStartPos = m_AtlasPtr.FindNextNull(0);
            for (int i = 0; i < newAtlas.size(); ++i)
            {
                HUDTexAtlas atlas = newAtlas[i];
                atlas.m_nAtlasID = m_AtlasPtr.FindNextNull(nStartPos) + 1;
                nStartPos = atlas.m_nAtlasID;
                m_AtlasPtr.GrowSet(atlas.m_nAtlasID - 1, atlas);
            }

            CMyArray<HUDSpriteInfo> newSprite = new CMyArray<HUDSpriteInfo>();

            Dictionary<string, HUDSpriteInfo>.Enumerator itSprite = m_AllSprite.GetEnumerator();
            int nMaxID = m_AllSprite.Count + 1;
            while (itSprite.MoveNext())
            {
                HUDSpriteInfo sp = itSprite.Current.Value;
                sp.m_nAtlasID = AtlasNameToID(sp.m_szAtlasName);
                if (sp.m_nNameID > 0 && sp.m_nNameID <= nMaxID)
                {
                    if (m_SpritePtr.IsValid(sp.m_nNameID - 1) && m_SpritePtr[sp.m_nNameID - 1] != null)
                    {
                        // 重复的ID
                        newSprite.push_back(sp);
                    }
                    else
                    {
                        m_SpritePtr.GrowSet(sp.m_nNameID - 1, sp);
                    }
                }
                else
                {
                    newSprite.push_back(sp);
                }
            }
            nStartPos = m_SpritePtr.FindNextNull(0);
            int nNewSpriteCount = newSprite.size();
            for (int i = 0; i < nNewSpriteCount; ++i)
            {
                HUDSpriteInfo sp = newSprite[i];
                sp.m_nNameID = m_SpritePtr.FindNextNull(nStartPos) + 1;
                nStartPos = sp.m_nNameID;
                m_SpritePtr.GrowSet(sp.m_nNameID - 1, sp);
            }
            if (newSprite.size() > 0)
                bDirty = true;
            return bDirty;
        }
        public int AtlasNameToID(string szAtlasName)
        {
            if (string.IsNullOrEmpty(szAtlasName))
                return 0;
            HUDTexAtlas atlas = null;
            if (m_TexAtlas.TryGetValue(szAtlasName, out atlas))
            {
                return atlas.m_nAtlasID;
            }
            return 0;
        }
        public int SpriteNameToID(string szSpriteName)
        {
            if (string.IsNullOrEmpty(szSpriteName))
                return 0;
            HUDSpriteInfo sprite = null;
            if (m_AllSprite.TryGetValue(szSpriteName, out sprite))
            {
                return sprite.m_nNameID;
            }
            return 0;
        }
        public HUDTexAtlas GetAtlasByID(int nAtlasID)
        {
            if (m_AtlasPtr.IsValid(nAtlasID - 1))
                return m_AtlasPtr[nAtlasID - 1];
            return null;
        }
        public HUDSpriteInfo GetSpriteByID(int nSpriteID)
        {
            if (m_SpritePtr.IsValid(nSpriteID - 1))
                return m_SpritePtr[nSpriteID - 1];
            return null;
        }
        public Rect ConvertToTexCoords(Rect rect, int width, int height)
        {
            Rect final = rect;
            if(width > 0 && height > 0)
            {
                final.xMin = rect.xMin / width;
                final.xMax = rect.xMax / width;
                final.yMin = 1-rect.yMax / height;
                final.yMax = 1-rect.yMin / height;
            }
            return final;
        }

        public SpriteConfig GetSpriteConfig(JumpWorldType type, out bool bExist)
        {
            bExist = AtlasManager.DicJumpWord.TryGetValue(type, out SpriteConfig sprite);
            return bExist ? sprite : default;
        }
        public AnimationAttribute GetAnimation(string name)
        {
            var bHave = HUDSetting.AnimationConfig.TryGetValue(name, out AnimationAttribute config);
            return bHave ? config : HUDSetting.DefaultAnimation;
        }
    }
}

