using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UITest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Color nowColor;
        ColorUtility.TryParseHtmlString("#FECEE1", out nowColor);
        transform.GetComponent<Image>().color = nowColor;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
