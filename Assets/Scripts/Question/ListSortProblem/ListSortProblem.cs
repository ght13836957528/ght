using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Question.ListSortProblem
{
    public class SelfInfo
    {

        public string Name;
        public int Age;

        public SelfInfo(string name,int age)
        {
            Name = name;
            Age = age;
        }
    }

    public class ListSortProblem : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            Comparison<SelfInfo> compareFun = ((info1, info2) =>
            {
                if (info1.Age > info2.Age)
                {
                    return 1;
                }
        
                return -1;
            });
            
            List<SelfInfo> infos = new List<SelfInfo>();
            SelfInfo info1 = new SelfInfo("aaa", 126);
            SelfInfo info2 = new SelfInfo("bbb", 122);
            SelfInfo info3 = new SelfInfo("ccc", 189);
            infos.Add(info1);
            infos.Add(info2);
            infos.Add(info3);
            // infos.Sort(delegate(SelfInfo info1, SelfInfo info2)
            // {
            //     if (info1.Age < info2.Age)
            //     {
            //         return 1;
            //     }
            //
            //     return -1;
            //
            // });
            infos.Sort((info1, info2) =>
            {
                return compareFun(info1, info2);
            });
            foreach (var item in infos)
            {
                Debug.LogError("name=="+item.Name+" age=="+ item.Age);
            }
        }
        
        
    }
    
  
}