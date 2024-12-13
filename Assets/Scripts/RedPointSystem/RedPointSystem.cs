using System.Collections.Generic;
using UnityEngine;

namespace RedPointSystem
{
    public class RedPointSystem
    {
        
        public class RedPointConst
        {
            public const string Main = "Main";
            public const string Mail = "Main.Mail";
            public const string MailAlliance = "Main.Mail.Alliance";
            public const string MailSystem = "Main.Mail.System";
        }
        
        public delegate void OnPointNumChange(RedPointNode node);

        private RedPointNode mRootNode;

        private List<string> redPointTreeList = new List<string>
        {
            RedPointConst.Main,
            RedPointConst.Mail,
            RedPointConst.MailAlliance,
            RedPointConst.MailSystem,
        };

        public void InitRedPointTreeNode()
        {
            mRootNode = new RedPointNode();
            mRootNode.NodeName = RedPointConst.Main;
        }

        public void Register(string nodeName,Transform parent)
        {
            
            foreach (var s in redPointTreeList)
            {
                var node = mRootNode;
                var treeNodeArr = nodeName.Split('.');
                if (treeNodeArr[0] != mRootNode.NodeName)
                {
                    Debug.LogError("fail");
                    continue;
                }

                if (treeNodeArr.Length > 1)
                {
                    for (int i = 0; i < treeNodeArr.Length; i++)
                    {
                        if (!node.dicChilds.ContainsKey(treeNodeArr[i]))
                        {
                            node.dicChilds.Add(treeNodeArr[i],new RedPointNode());
                        }

                        node.dicChilds[treeNodeArr[i]].NodeName = treeNodeArr[i];
                        node.dicChilds[treeNodeArr[i]].Parent = node;

                        node = node.dicChilds[treeNodeArr[i]];
                    }
                }
            }
        }

        public void SetRedPointNodeCallBack(string strNode, OnPointNumChange callback)
        {
            var nodeList = strNode.Split('.');
            if (nodeList.Length == 1)
            {
                if (nodeList[0] != RedPointConst.Main)
                {
                    Debug.LogError("fail");
                    return;
                }
            }
            var node = mRootNode;
            for (int i = 0; i < nodeList.Length; i++)
            {
                if (!node.dicChilds.ContainsKey(nodeList[i]))
                {
                    Debug.LogError("fail");
                    return;
                }

                node = node.dicChilds[nodeList[i]];
                if (nodeList.Length - 1 == i)
                {
                    node.numChangeFunc = callback;
                    return;
                }
            }
        }

        public void SetInvoke(string strNode, int rpNum)
        {
            var nodeList = strNode.Split('.');
            if (nodeList.Length == 1 && nodeList[0] != RedPointConst.Main)
            {
                Debug.LogError("fail");
                return;
            }

            var node = mRootNode;
            for (int i = 1; i < nodeList.Length; i++)
            {
                if (!node.dicChilds.ContainsKey(nodeList[i]))
                {
                    Debug.LogError("fail");
                    return;
                }

                node = node.dicChilds[nodeList[i]];

                if (i == nodeList.Length - 1)
                {
                    node.SetRedPointNum(rpNum);
                }
            }
        }


    }
}