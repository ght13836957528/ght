using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace UW.HUD
{
    public class HpContext:BaseContext
    {
        //private HUDBehaviour m_hUDBehaviour;
        private const float ShowTitleTime = 1.5f;
        //public HpContext(HUDBehaviour hUDBehaviour)
        //{
        //    m_hUDBehaviour = hUDBehaviour;

        //}
        public override void Init(AtlasContext atlasContext)
        {
            base.Init(atlasContext);
        }
        private void RefreshAllTitles()
        {
            foreach(var title in m_titles)
            {
                if(title.Value.Show)
                {
                    title.Value.ShowTime -= Time.deltaTime;
                    if (title.Value.ShowTime < 0)
                    {
                        title.Value.Show = false;
                        for (int i = 0; i < title.Value.VertexList.size; ++i)
                        {
                            m_mesh.EraseHUDVertex(title.Value.VertexList[i]);
                        }
                    }
                }
            }
        }
        public override void UpdateMesh()
        {
            RefreshAllTitles();
            base.UpdateMesh();
            if (m_mesh.MeshDirty)
                UIBattleRenderPassFeature.UIBattleHudPass.HpMesh = m_mesh;
        }

        public override void Release()
        {
            base.Release();
        }
        
        private void PushSprite(HUDTitle title, HUDHpType hpType)
        {
            int nBkWidth = m_atlasContext.AtlasManager.BloodBk.Width;
            int nBkHeight = m_atlasContext.AtlasManager.BloodBk.Height;
            int nBkSpriteId = m_atlasContext.AtlasManager.BloodBk.SpriteId;
            
            int spriteId = 0;
            int nBloodWidth = 0;
            int nBloodHight = 0;
            if(hpType == HUDHpType.SmallBlueHp)
            {
                spriteId = m_atlasContext.AtlasManager.BloodBlue.SpriteId;
                nBloodWidth = m_atlasContext.AtlasManager.BloodBlue.Width;
                nBloodHight = m_atlasContext.AtlasManager.BloodBlue.Height;
            }
            else if(hpType == HUDHpType.SmallRedHp)
            {
                spriteId = m_atlasContext.AtlasManager.BloodRed.SpriteId;
                nBloodWidth = m_atlasContext.AtlasManager.BloodRed.Width;
                nBloodHight = m_atlasContext.AtlasManager.BloodRed.Height;
            }
            HUDVertexInfo bkVertex = new HUDVertexInfo();
            HUDVertexInfo bloodVertex = new HUDVertexInfo();
            RefreshVertex(ref bkVertex, nBkSpriteId, nBkWidth, nBkHeight);
            RefreshVertex(ref bloodVertex, spriteId, nBloodWidth, nBloodHight);
            if(bkVertex!=null)
                title.VertexList.Add(bkVertex);
            if (bloodVertex != null)
            {
                title.VertexList.Add(bloodVertex);
                title.BloodVertex = bloodVertex;
                title.Width = nBloodWidth;
                title.Height = nBloodHight;
                bloodVertex.SpriteId = spriteId;
            }
        }

        public void CreateHpTitle(HUDHpType hpType, int entityId, float offsetY, Transform target = null)
        {
            HUDTitle title = new HUDTitle();
            title.WorldPos = Vector3.zero;
            title.ScreenPos = Vector3.zero;
            title.TitleId = entityId;
            title.OffsetY = offsetY;
            title.OffsetX = 0;
            title.HpRate = 1;
            title.FollowTarget = target;
            title.Show = false;
            PushSprite(title, hpType);
            m_titles.Add(entityId, title);
        }

        public void ShowHpTitle(int titleId, bool bShow)
        {
            HUDTitle title;
            m_titles.TryGetValue(titleId, out title);
            if(title != null)
            {
                title.Show = bShow;
                if (!bShow)
                {
                    for(int i=0; i<title.VertexList.size;++i)
                    {
                        m_mesh.EraseHUDVertex(title.VertexList[i]);
                    }
                }
                else
                {
                    title.ShowTime = ShowTitleTime;
                }
            }
            
        }

        public void SetHpRate(int titleId, float hpRate)
        {
            HUDTitle title;
            m_titles.TryGetValue(titleId, out title);
            if (title != null && title.BloodVertex!=null)
            {
                title.HpRate = hpRate;
                int width = (int)(title.Width * hpRate + 0.5);
                RefreshVertex(ref title.BloodVertex, title.BloodVertex.SpriteId, width, title.Height);
                title.BloodVertex.Offset.Set(-title.Width / 2, 0);
            }
        }
    }
}
