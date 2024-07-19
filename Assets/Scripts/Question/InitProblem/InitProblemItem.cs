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
        Debug.Log("InitProblemItem Awake");
        Debug.Log("Awake Frame=="+ InitProblemMain._frameNum);
    }

    public void Start()
    {
        Debug.LogError("InitProblemItem start");
        _testList = new List<string>();
        Debug.Log("start Frame=="+ InitProblemMain._frameNum);
    }
    

    public void Init()
    {
        Debug.Log("InitProblemItem Init");
        if (_testList == null)
        {
            Debug.LogError("_testList is null");
        }
        Debug.Log("Init Frame=="+ InitProblemMain._frameNum);
    }

   
}
