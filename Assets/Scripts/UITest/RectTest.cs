using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NewBehaviourScript : MonoBehaviour
{
    [SerializeField]private Mask parentMask;
    // Start is called before the first frame update
    void Start()
    {
        Material material = parentMask.graphic.materialForRendering;
        Debug.Log("material name===" + material.name);
        var sten = material.GetFloat("_Stencil");
        Debug.Log("sten==" + sten);
        
        ParticleSystemRenderer psr = gameObject.GetComponent<ParticleSystemRenderer>();
        Material arrMat = psr.sharedMaterial;
        arrMat.SetFloat("_Stencil",sten);
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Play()
    {
        
    }
    
    
}
