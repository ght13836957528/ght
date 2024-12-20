using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace UW.HUD
{
    public struct JumpWorldData
    {
       public Transform target;
        public int number;
        public float offsetY;

        public JumpWorldData(Transform target, int number, float offsetY)
        {
            this.target = target;
            this.number = number;
            this.offsetY = offsetY;
        }
    }
    public class HUDJumpWorld
    {
        float disTime = 1f;
        int eneityId;
        Dictionary<int,int> jumpQueue = new Dictionary<int, int>();
        Dictionary<int, float> jumpTimes = new Dictionary<int, float>();
        JumpWorldContext worldContext = null;
        int maxCount = (int)JumpWorldType.Max - 1;
        public HUDJumpWorld(JumpWorldContext worldContext,int eneityId)
        {
            this.worldContext = worldContext;
            this.eneityId = eneityId;
            for (int i = 1; i <= maxCount; i++)
            {
                jumpTimes.Add(i, 0);
                jumpQueue.Add(i,0);
            }
        }
        public void Update()
        {
            for (int i = 1; i <= maxCount; i++)
            {
                float time;
                if (jumpTimes.TryGetValue(i,out time))
                {
                    if (time <= 0)
                    {
                        int index;
                        if (jumpQueue.TryGetValue(i, out index))
                        {
                            jumpQueue[i] = 0;
                        }
                    }
                    else
                    {
                        jumpTimes[i] -= Time.deltaTime;
                    }
                }
            }
        }
        public void ShowJumpWorld(JumpWorldType nType, Transform target = null, int number = 0, float offsetY = 2)
        {
            worldContext.ShowJumpWorld(nType, target, number, offsetY);
        }
        public void TryShowJumpWorld(int nType, Transform target = null, int number = 0, float offsetY = 2)
        {
            float time;
            if (jumpTimes.TryGetValue(nType, out time))
            {
                if (time > 0)
                {
                    int index;
                    if (jumpQueue.TryGetValue(nType, out index))
                    {
                        jumpQueue[nType]++;
                    }
                }
                else
                {
                    jumpTimes[nType] = disTime;
                }
            }
            else
            {
                jumpTimes.Add(nType, 0);
            }
            int off_index;
            JumpWorldType jType = (JumpWorldType)nType;
            if (jumpQueue.TryGetValue(nType, out off_index))
            {
                ShowJumpWorld(jType, target, number, offsetY + GetOffDelY(jType, off_index));
            }
            else
            {
                jumpQueue.Add(nType, 0);
                ShowJumpWorld(jType, target, number, offsetY);
            }
            
        }

        float GetOffDelY(JumpWorldType jType,int off_index)
        {
            float result = off_index * 0f;
            return result;
            switch (jType) { 
                case JumpWorldType.Damage: return -result;
                case JumpWorldType.Crit: return -result;
                case JumpWorldType.ShieldMinus: return -result;
                case JumpWorldType.AngerMinus: return -result;
                case JumpWorldType.ShieldAdd: return -result;
                case JumpWorldType.Cure: return -result;
                case JumpWorldType.AngerAdd: return -result;
                default: return result;
            };
        }
        public void Release()
        {
            jumpTimes.Clear();
            jumpQueue.Clear();
        }
    }
}
