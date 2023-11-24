using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

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
            _curIndex = -1;
            _numberArray = new object[_defaultLength];
        }

        public bool Add(int number)
        {
            _curIndex ++;
            _numberArray[_curIndex] = number;
            return true;
        }

        public IEnumerator GetEnumerator()
        {
            return new ContainerItem(_numberArray);
        }
        
        public void ForEach(CallBack callBack)
        {
            IEnumerator iEnumerator = GetEnumerator();
            iEnumerator.Reset();
            do
            {
                if (iEnumerator.Current != null)
                {
                    int number = (int)iEnumerator.Current;
                    callBack(number);
                }
            } while (iEnumerator.MoveNext());
           
        }
    }
}