using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Game的启动入口
/// </summary>
public class Launch : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GameEntry.Instance().Init();
    }

    // Update is called once per frame
    void Update()
    {
        GameEntry.Instance().Update();
    }

    private void OnDestroy()
    {
        GameEntry.Instance().Dispose();
    }
}
