using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UW.HUD
{
    public class HUDRenderMesh
    {
        public Mesh RealMesh;
        public Material RealMaterial;
        public BetterList<Vector3> Verts = new BetterList<Vector3>();
        public BetterList<Vector2> Offset = new BetterList<Vector2>();
        public BetterList<Vector2> Uvs = new BetterList<Vector2>();
        public BetterList<Color32> Cols = new BetterList<Color32>();
        public BetterList<int> Indices = new BetterList<int>();
        BetterList<HUDVertexInfo> SpriteVertex = new BetterList<HUDVertexInfo>();
        bool m_dirty = false;
        public bool MeshDirty
        {
            get { return m_dirty; }
            set { m_dirty = value; }
        }
        bool m_haveNullVertex = false;
        public int AtlasId = 0;

        public void Init(Texture texture, Shader shader)
        {
            RealMaterial = new Material(shader);
            RealMaterial.SetTexture("_MainTex", texture);
        }
        public void Release()
        {
	        Object.Destroy(RealMaterial);
            RealMaterial = null;
            RealMesh = null;
            Verts.Release();
            Offset.Release();
            Uvs.Release();
            Cols.Release();
            Indices.Release();
            SpriteVertex.Release();
        }
        public void ClearNullItem()
        {
            int nRealCount = 0;
            var buffer = SpriteVertex.buffer;
            var size = SpriteVertex.size;
            for (int i = 0, iMax = size; i < iMax; ++i)
            {
                if (buffer[i] != null)
                {
                    buffer[i].Index = nRealCount;
                    if (nRealCount != i)
                        buffer[nRealCount++] = buffer[i];
                    else
                        nRealCount++;
                }
            }
            // 删除的节点要清零
            for (int i = nRealCount; i < size; ++i) buffer[i] = null;
            SpriteVertex.size = nRealCount;
        }
        public void PushHUDVertex(HUDVertexInfo v)
        {
            int nIndex = v.Index;
            if (nIndex >= 0 && nIndex < SpriteVertex.size)
            {
                if (SpriteVertex[nIndex] != null)
                    return;
            }
            m_dirty = true;
            v.Index = SpriteVertex.size;
            SpriteVertex.Add(v);
        }

        public void EraseHUDVertex(HUDVertexInfo v)
        {
            int nIndex = v.Index;
            if (nIndex >= 0 && nIndex < SpriteVertex.size)
            {
                if (SpriteVertex[nIndex] != null)
                {
                    m_dirty = true;
                    m_haveNullVertex = true;
                    SpriteVertex[nIndex] = null;
                    v.Index = -1;

                    return;
                }
            }

        }
        public void UpdateLogic()
        {
            if (!m_dirty)
                return;
            m_dirty = false;

            if (m_haveNullVertex)
            {
                m_haveNullVertex = false;
                ClearNullItem();
            }
            UpdateMesh();
        }

        void PrepareWrite(int nVertexNumb)
        {
            Verts.CleanPreWrite(nVertexNumb);
            Offset.CleanPreWrite(nVertexNumb);
            Uvs.CleanPreWrite(nVertexNumb);
            Cols.CleanPreWrite(nVertexNumb);
        }
        void FillVertex()
        {
            PrepareWrite(SpriteVertex.size * 4);
            Vector2 vOffset = Vector2.zero;

            for (int i = 0, nSize = SpriteVertex.size; i < nSize; ++i)
            {
                HUDVertexInfo v = SpriteVertex[i];
                Verts.Add(v.WorldPos);
                Verts.Add(v.WorldPos);
                Verts.Add(v.WorldPos);
                Verts.Add(v.WorldPos);

                vOffset = v.uv2RU;
                vOffset += v.Offset;
                vOffset *= v.Scale;
                vOffset += v.Move;
                Offset.Add(vOffset);

                vOffset = v.uv2RD;
                vOffset += v.Offset;
                vOffset *= v.Scale;
                vOffset += v.Move;
                Offset.Add(vOffset);

                vOffset = v.uv2LD;
                vOffset += v.Offset;
                vOffset *= v.Scale;
                vOffset += v.Move;
                Offset.Add(vOffset);

                vOffset = v.uv2LU;
                vOffset += v.Offset;
                vOffset *= v.Scale;
                vOffset += v.Move;
                Offset.Add(vOffset);

                Uvs.Add(v.uvRU);
                Uvs.Add(v.uvRD);
                Uvs.Add(v.uvLD);
                Uvs.Add(v.uvLU);

                Cols.Add(v.clrRD);
                Cols.Add(v.clrRU);
                Cols.Add(v.clrLU);
                Cols.Add(v.clrLD);
            }
        }

        void RebuildIndices(int vertexCount)
        {
            Indices.CleanPreWrite(vertexCount / 4 * 6);
            // 填充多余的
            int nMaxCount = Indices.buffer.Length;

            int index = 0;
            int i = 0;
            for (; i < vertexCount; i += 4)
            {
                Indices[index++] = i;
                Indices[index++] = i + 1;
                Indices[index++] = i + 2;

                Indices[index++] = i + 2;
                Indices[index++] = i + 3;
                Indices[index++] = i;
            }
            int nLast = vertexCount - 1;
            for (; index < nMaxCount;)
            {
                Indices[index++] = nLast;
                Indices[index++] = nLast;
                Indices[index++] = nLast;
                Indices[index++] = nLast;
                Indices[index++] = nLast;
                Indices[index++] = nLast;
            }
            Indices.size = index;
        }

        void UpdateMesh()
        {
            int nOldVertexCount = Verts.size;
            FillVertex();
            int nLast = Verts.size - 1;
            int nExSize = Verts.buffer.Length;
            int nVertexCount = Verts.size;
            if (nLast >= 0)
            {
                Vector3[] vers = Verts.buffer;
                Vector2[] uv1s = Uvs.buffer;
                Vector2[] offs = Offset.buffer;
                Color32[] cols = Cols.buffer;
                for (int i = Verts.size, iMax = Verts.buffer.Length; i < iMax; ++i)
                {
                    vers[i] = vers[nLast];
                    uv1s[i] = uv1s[nLast];
                    offs[i] = offs[nLast];
                    cols[i] = cols[nLast];
                }
            }
            Verts.size = nExSize;
            Uvs.size = nExSize;
            Cols.size = nExSize;
            Offset.size = nExSize;

            // 更新索引数据
            bool rebuildIndices = nOldVertexCount != nExSize;
            if (rebuildIndices)
                RebuildIndices(nVertexCount);

            if (RealMesh == null)
            {
                RealMesh = new Mesh
                {
                    hideFlags = HideFlags.DontSave,
                    name = "hud_mesh"
                };
                RealMesh.MarkDynamic();
                m_dirty = true;
            }
            else if (rebuildIndices || RealMesh.vertexCount != Verts.size || SpriteVertex.size==0)
            {
                RealMesh.Clear();
                m_dirty = true;
            }

            if (RealMesh != null && SpriteVertex.size>0)
            {
                RealMesh.vertices = Verts.buffer;
                RealMesh.uv = Uvs.buffer;
                RealMesh.uv2 = Offset.buffer;
                RealMesh.colors32 = Cols.buffer;
                RealMesh.triangles = Indices.buffer;
            }
        }
    }

}
