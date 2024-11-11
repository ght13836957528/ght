using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshTest : MonoBehaviour
{
    // Start is called before the first frame update
    private void Awake()
    {
        Application.targetFrameRate = 30;
    }

    void Start()
    {
        var meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null)
            return;
        var mesh = meshFilter.mesh;
        var normals = mesh.normals;
        // foreach (var normal in normals)
        // {
        //     Debug.LogError("normal=="+normal);
        // }

        var vertices = mesh.vertices;
        foreach (var vertex in vertices)
        {
            Debug.LogError("vertex=="+vertex);
        }

       
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
