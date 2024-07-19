using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UITest : MonoBehaviour
{
    private EventTriggerListener _eventTrigger;
    public GameObject game;
    private void Start()
    {
        // _eventTrigger = GetComponent<EventTriggerListener>();
        // _eventTrigger.OnBeginDragHandle = OnBeginDragHandle;
        // _eventTrigger.OnDragHandle = OnDragHandle;
        // _eventTrigger.OnEndDragHandle = OnEndDragHandle;
        // _eventTrigger.OnDownHandle = OnDownHandle;
        game.GetComponent<RectTransform>().offsetMin = Vector2.zero;
        game.GetComponent<RectTransform>().offsetMax = Vector2.zero;
        GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 50);
    }

    private void OnDragHandle(PointerEventData eventdata)
    {
        Debug.Log("OnDragHandle");
    }
    
    private void OnBeginDragHandle(PointerEventData eventdata)
    {
        Debug.Log("OnBeginDragHandle");
    }
    private void OnEndDragHandle(PointerEventData eventdata)
    {
        Debug.Log("OnEndDragHandle");
    }
    
    private void OnDownHandle(GameObject obj)
    {
        Debug.Log("OnDownHandle");
    }
    

    private void OnEnable()
    {
       
    }

   

}