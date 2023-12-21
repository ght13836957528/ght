using System;
using System.Collections;
using UnityEngine;

namespace DesignPatterns.IteratorPattern
{
    public class CoroutineTest: MonoBehaviour
    {
        private bool _ifPrint;
        private void Start()
        {
            // StartCoroutine(TwoTime());
            Test();
            _ifPrint = true;
        }

        private IEnumerator OneTime()
        {
            Debug.Log("first");
            yield return 1;
            _ifPrint = false;
            Debug.Log("second");
            yield return 2;
            Debug.Log("second");
            yield return "2";
        }
        
        private IEnumerator TwoTime()
        {
            Debug.Log("first");
            yield return new WaitForSeconds(1);
            Debug.Log("wait");
        }
        
        private void Test()
        {
            IEnumerator it  = OneTime();
            it.MoveNext();
            Debug.Log(it.Current);
            it.MoveNext();
            Debug.Log(it.Current);
            it.MoveNext();
            Debug.Log(it.Current);

        }
    }
}