using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    
    {
        // Debug.LogError("AnchoredPosition===" + this.transform.anchoredPosition);
        // Debug.Log("localPosition==="+GetComponent<RectTransform>().localPosition);
        Debug.Log("anchoredPosition==="+GetComponent<RectTransform>().anchoredPosition);
        //GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
