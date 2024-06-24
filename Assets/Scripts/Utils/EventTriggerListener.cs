using UnityEngine;
using UnityEngine.EventSystems;

public class EventTriggerListener : EventTrigger
{
    public delegate void ClickDelegate(GameObject go);

    public delegate void DragDelegate(PointerEventData eventData);

    public DragDelegate OnBeginDragHandle;
    public DragDelegate OnEndDragHandle;
    public DragDelegate OnDragHandle;

    public ClickDelegate OnDownHandle;
    public ClickDelegate OnEnterHandle;
    public ClickDelegate OnExitHandle;
    public ClickDelegate OnUpHandle;

    public override void OnPointerDown(PointerEventData eventData)
    {
        if (OnDownHandle != null) OnDownHandle(gameObject);
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        if (OnEnterHandle != null) OnEnterHandle(gameObject);
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        if (OnExitHandle != null) OnExitHandle(gameObject);
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        if (OnUpHandle != null) OnUpHandle(gameObject);
    }

    public override void OnDrag(PointerEventData eventData)
    {
        if (OnDragHandle != null) OnDragHandle(eventData);
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        if (OnBeginDragHandle != null) OnBeginDragHandle(eventData);
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        if (OnEndDragHandle != null) OnEndDragHandle(eventData);
    }

}