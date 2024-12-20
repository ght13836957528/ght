using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UW.HUD
{
    public class BaseContext
    {
        protected AtlasContext m_atlasContext;
        protected HUDRenderMesh m_mesh;
        protected Dictionary<int, HUDTitle> m_titles;
      
        public Dictionary<int, HUDTitle> GetTitles()
        {
            return m_titles;
        }
       
        public virtual void Init(AtlasContext atlasContext)
        {
            m_titles = new Dictionary<int, HUDTitle>(10);
            m_atlasContext = atlasContext;
            m_mesh = new HUDRenderMesh();
            m_mesh.Init(m_atlasContext.AtlasTex, m_atlasContext.AtlasShader);
        }
        public virtual void Release()
        {
            m_atlasContext = null;
            m_mesh.Release();
            m_titles.Clear();
        }
        protected void AlignCenter(HUDVertexInfo vertex)
        {
            vertex.Offset.Set(-vertex.Width / 2, 0);
        }
        protected void RefreshVertex(ref HUDVertexInfo vertex, int spriteId, int nWidth = -1, int nHeight = -1)
        {
            var spriteInfo = m_atlasContext.GetSpriteByID(spriteId);
            if (spriteInfo == null)
                return;
            vertex.WorldPos = Vector3.zero;
            vertex.ScreenPos = Vector3.zero;
            int width = nWidth > -1 ? nWidth : (short)(spriteInfo.outer.width + 0.5f);
            int height = nHeight > -1 ? nHeight : (short)(spriteInfo.outer.height + 0.5f);
            vertex.Width = width;
            vertex.Height = height;
            Rect outerUV = spriteInfo.outer;
            var texAtlas = m_atlasContext.GetAtlasByID(spriteInfo.m_nAtlasID);
            if (texAtlas != null)
            {
                outerUV = m_atlasContext.ConvertToTexCoords(outerUV, texAtlas.texWidth, texAtlas.texHeight);
            }
            float fL = 0;
            float fT = 0.0f;
            float fR = width;
            float fB = height;

            vertex.uv2RU.Set(fR, fT);
            vertex.uv2RD.Set(fR, fB);
            vertex.uv2LD.Set(fL, fB);
            vertex.uv2LU.Set(fL, fT);

            float uvR = outerUV.xMax;
            float uvL = outerUV.xMin;
            float uvB = outerUV.yMin;
            float uvT = outerUV.yMax;

            vertex.uvRU.Set(uvR, uvB);
            vertex.uvRD.Set(uvR, uvT);
            vertex.uvLD.Set(uvL, uvT);
            vertex.uvLU.Set(uvL, uvB);

            vertex.clrLD = vertex.clrLU = vertex.clrRD = vertex.clrRU = Color.white;
            AlignCenter(vertex);
        }
        protected void UpdateVertex()
        {
            foreach (var title in m_titles)
            {
                if (title.Value.Show)
                {
                    for (int i = 0; i < title.Value.VertexList.size; ++i)
                    {
                        m_mesh.PushHUDVertex(title.Value.VertexList[i]);
                    }
                }
            }
        }
        protected void SyncPos(HUDTitle title)
        {
            var camUI = HUDBehaviour.GetHUDUICamera();
            var camMain = HUDBehaviour.GetHUDMainCamera();
            var worldPos = title.FollowTarget == null ? title.WorldPos : title.FollowTarget.position;
            worldPos.y += title.OffsetY;
            worldPos.x += title.OffsetX;
            var screenPos = camMain.WorldToScreenPoint(worldPos);
            worldPos = camUI.ScreenToWorldPoint(screenPos);
            title.WorldPos = worldPos;
            title.ScreenPos = screenPos;
            HUDVertexInfo vertex = null;
            for (int i = 0; i < title.VertexList.size; ++i)
            {
                vertex = title.VertexList[i];
                vertex.ScreenPos = screenPos;
                vertex.WorldPos = worldPos;
            }
        }
        protected void UpdateTitlePos()
        {
            bool bSync = false;
            foreach(var title in m_titles)
            {
                if (title.Value.Show)
                {
                    SyncPos(title.Value);
                    bSync = true;
                }
            }
            if (bSync)
                m_mesh.MeshDirty = true;
        }
        public virtual void UpdateMesh()
        {
            UpdateVertex();
            UpdateTitlePos();
            m_mesh.UpdateLogic();
        }

    }

}
