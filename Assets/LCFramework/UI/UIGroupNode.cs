using System;
using UnityEngine;

namespace Framework.UI
{
    [Serializable]
    public class UIGroupNode
    {
        [SerializeField]
        private string m_Name = null;

        [SerializeField]
        private int m_Depth = 0;

        public string Name
        {
            get
            {
                return m_Name;
            }
        }

        public int Depth
        {
            get
            {
                return m_Depth;
            }
        }
    }
}
