using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UW.HUD
{
    public class HUDVertexInfo
    {
        public Vector2 uv2RU; // 右上角
        public Vector2 uv2RD; // 右下角
        public Vector2 uv2LD; // 左下角
        public Vector2 uv2LU; // 左上角

        public Vector2 uvRD;  // 右上角
        public Vector2 uvRU;
        public Vector2 uvLU;
        public Vector2 uvLD;

        public Color32 clrRD;
        public Color32 clrRU;
        public Color32 clrLU;
        public Color32 clrLD;

        public Vector3 WorldPos;
        public Vector2 ScreenPos; // 屏幕坐标
        public Vector2 Move;    // 当前移动量(变动值)
        public int Index = -1;
        public int SpriteId;
        public Vector2 Offset;
        public int Width;
        public int Height;
        public float Scale = 1.0f;

        public void Reset()
        {
            Scale = 1.0f;
            Index = -1;
        }
    }

}
