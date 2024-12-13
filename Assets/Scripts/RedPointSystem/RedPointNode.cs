using System.Collections;
using System.Collections.Generic;
using RedPointSystem;
using UnityEngine;

public class RedPointNode
{
   public string NodeName;
   public int PointNum;
   public RedPointNode Parent;
   public RedPointSystem.RedPointSystem.OnPointNumChange numChangeFunc;
   public Dictionary<string, RedPointNode> dicChilds = new Dictionary<string, RedPointNode>();
   private RedPointMono _pointMono;

   public void SetRedPointNum(int rpNum)
   {
      PointNum = rpNum;
      NotifyPointNumChange();
      if (Parent != null)
      {
         Parent.NotifyPointNumChange();
      }
   }

   private void NotifyPointNumChange()
   {
      numChangeFunc?.Invoke(this);
   }
}
