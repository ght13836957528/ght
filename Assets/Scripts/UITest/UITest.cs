using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UITest : MonoBehaviour
{
    private void OnEnable()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        Debug.Log("anchoredPos==" + rectTransform.anchoredPosition);
        Debug.Log("sizeDelta==" + rectTransform.sizeDelta);
        Debug.Log("offSetMax==" + rectTransform.offsetMax + "offSetMin===" + rectTransform.offsetMin);
        rectTransform.pivot = new Vector2(0.5f, 1);
        rectTransform.anchoredPosition = Vector2.zero;
        // transform.localPosition = Vector3.zero;
    }

   

}