using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitProblemItem : MonoBehaviour
{
    // Start is called before the first frame update
    public Scroll scrollItem;
    private Scroll scroll;
    private List<string> _testList;

    private void Awake()
    {
        Debug.LogError("InitProblemItem Awake");
    }

    void Start()
    {
        Debug.LogError("InitProblemItem start");
        _testList = new List<string>();
    }
    

    public void Init()
    {
        Debug.Log("InitProblemItem Init");
        // if (scroll == null)
        // {
        //     Debug.LogError("scroll is null");
        //     return;
        // }
        //
        // scroll.Test();
        // Debug.LogError("InitProblemItem Init" + scroll.GetInstanceID());
        if (_testList == null)
        {
            Debug.LogError("_testList is null");
        }
        
    }
}
