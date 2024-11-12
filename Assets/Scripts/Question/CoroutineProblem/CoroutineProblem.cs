using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoroutineProblem : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var iEnumerable = GetNumbers(5);
     
        var enumerator = iEnumerable.GetEnumerator();
        while (enumerator.MoveNext())
        {
            var item = enumerator.Current;
            Debug.Log("item=="+item);
        }
        
        foreach (var num in iEnumerable)
        {
            Debug.Log("num=="+ num);
        }
    }
    
    
    IEnumerable GetNumbers(int count)
    {
        // for (int i = 1; i <= count; i++)
        // {
            yield return 1;
        // }
    }
    
  
}


