using System.Collections;

namespace DesignPatterns.IteratorPattern
{
    public class ContainerTest : IEnumerable
    {
         
        private object[] _numberArray;
        private int _curIndex;
        private readonly int _defaultLength = 4;
        public delegate void CallBack(int number);
        public ContainerTest()
        {
            _curIndex = 0;
            _numberArray = new object[_defaultLength];
        }

        public bool Add(int number)
        {
            _numberArray[_curIndex] = number;
            _curIndex ++;
            return true;
        }

        public IEnumerator GetEnumerator()
        {
            return new ContainerItem(_numberArray);
        }
        
        /// <summary>
        /// 猜测foreach遍历与该函数逻辑类似
        /// </summary>
        /// <param name="callBack"></param>
        public void ForEach(CallBack callBack)
        {
            IEnumerator iEnumerator = GetEnumerator();
            iEnumerator.Reset();
            while (iEnumerator.MoveNext())
            {
                if (iEnumerator.Current != null)
                {
                    int number = (int)iEnumerator.Current;
                    callBack(number);
                }
            } 
           
        }
    }
}