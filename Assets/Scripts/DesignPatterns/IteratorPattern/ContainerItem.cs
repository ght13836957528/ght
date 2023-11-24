using System.Collections;

namespace DesignPatterns.IteratorPattern
{
    public class ContainerItem : IEnumerator
    {
        private object[] _numberArray;
        private int _curIndex;
        public ContainerItem(object[] numberArray)
        {
            _numberArray = numberArray;
            _curIndex = -1;
        }

        public object Current 
        {
            get { return _numberArray[_curIndex]; }
        }

        public bool MoveNext()
        {
            _curIndex++;
            return _numberArray.Length > _curIndex ;
        }
        
        public void Reset()
        {
            _curIndex = 0;
        }
        

    }
}