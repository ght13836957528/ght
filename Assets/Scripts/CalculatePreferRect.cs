using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CalculatePreferRect : MonoBehaviour
{
    // Start is called before the first frame update
    public TextMeshProUGUI tmp;
    public Text _text;
    void Start()
    {
       // IfPopBox("test");
        _text.text = "test test";
        float height = _text.preferredHeight;
       
    }

    // Update is called once per frame
    void Update()
    {
        // float tmpWidth = tmp.rectTransform.rect.width;
        //
        // float tmpHeight = tmp.rectTransform.rect.height;
        //
        // Debug.LogError("tmpWidth=="+ tmpWidth+ "  tmpHeight=="+tmpHeight);
    }

    private bool IfPopBox( string text)
    {
        tmp.text = text;
        float preHeight = tmp.preferredHeight;
        Debug.LogError("pre height=="+ preHeight);
        
        float preWidth = tmp.preferredWidth;
        Debug.LogError("pre width=="+ preWidth);

        float tmpWidth = tmp.rectTransform.rect.width;
        
        float tmpHeight = tmp.rectTransform.rect.height;

        // Debug.LogError("tmpWidth=="+ tmpWidth+ "  tmpHeight=="+tmpHeight);
        return false;

      
    }
}
