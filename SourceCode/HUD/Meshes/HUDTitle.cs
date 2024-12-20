using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UW.HUD
{
    public class HUDTitle
    {
        public Vector3 WorldPos;
        public Vector3 ScreenPos;
        public float OffsetY;
        public float OffsetX;
        public Transform FollowTarget;
        public BetterList<HUDVertexInfo> VertexList = new BetterList<HUDVertexInfo>();
        public HUDVertexInfo BloodVertex = null;
        public bool Show = false;
        public float HpRate = 0;
        public int TitleId = -1;
        public int Width = 0;
        public int Height = 0;
        public float ShowTime;

        public void Reset()
        {
            VertexList.ClearAnSetNull();
        }
    }

    public class HUDJumpWorldTitle:HUDTitle
    {
        public float BeginTime;
        public JumpWorldType JPType;
    }
}
