using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scroll : MonoBehaviour
{
    // Start is called before the first frame update
    private GameObject go;
    private void Awake()
    {
        Debug.LogError("Scroll Awake" );
        // var scrollRectTransform = this.GetComponent<RectTransform>();
        // go = new GameObject("Container", typeof(RectTransform));
        // go.transform.SetParent(scrollRectTransform);
    }

    void Start()
    {
        Init();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Init()
    {
        // Debug.LogError("Scroll Init" + GetInstanceID());
        // if (go == null)
        // {
        //     Debug.LogError("go is null");
        //     return;
        // }
        // Debug.LogError("go name is " + go.transform.name);
    }

    public void Test()
    {
        Debug.LogError("Scroll Test" + GetInstanceID());
    }
}
