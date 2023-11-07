using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleClip : MonoBehaviour
{
    private ParticleSystem _system;
    private Material _material;
    public RectTransform rectTransform;
    public CanvasRenderer canvasRenderer;

    void Start()
    {
        _material = GetComponent<ParticleSystem>().GetComponent<Renderer>().material;
        Debug.Log("material Name==" + _material.name );
        
    }

    void Update()
    {
        canvasRenderer.EnableRectClipping(rectTransform.rect);
    }
    
}
  
