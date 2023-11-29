using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class ParticleClip : MonoBehaviour
{
    
    [SerializeField] private RectTransform cullArea;
    [SerializeField] private ParticleSystemRenderer[] objArr;
    [SerializeField] private GameObject left;
    [SerializeField] private GameObject right;
    private Vector3[] _corners = new Vector3[4];
    private Vector3[] _cornerArr = new Vector3[4];
    private float leftMinX;
    private float leftMinY;
    
    private float rightMaxX;
    private float rightMaxY;

    void Start()
    {
        SetCorner();
        SetMaterialChipRect();
    }

    void SetCorner()
    {
        cullArea.GetWorldCorners(_cornerArr);
        leftMinX = _cornerArr[0].x;
        leftMinY = _cornerArr[0].y;

        rightMaxX = _cornerArr[2].x;
        rightMaxY = _cornerArr[2].y;
        
        left.transform.position = new Vector3(leftMinX,leftMinY,0);
        right.transform.position = new Vector3(rightMaxX,rightMaxY,0);
    }

    void SetMaterialChipRect()
    {
        foreach (var obj in objArr)
        {
            float num = 0f;
            var mat = obj.material;
            mat.SetFloat("_MinX", leftMinX + num);
            mat.SetFloat("_MinY", leftMinY + num );
            mat.SetFloat("_MaxX", rightMaxX - num);
            mat.SetFloat("_MaxY", rightMaxY - num);
        }
    }
    
}
  
