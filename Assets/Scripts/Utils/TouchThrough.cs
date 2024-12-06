using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TouchThrough : MonoBehaviour, IPointerClickHandler
    {

        [SerializeField] private bool _passClick = true;

        private void PassEvent<T> (PointerEventData data, ExecuteEvents.EventFunction<T> function)
            where T : IEventSystemHandler
        {
            Debug.Log("Depend Obj: {0}"+gameObject.name);
            var results = new List<RaycastResult> ();

            //查了一下源码和api 获取到的这个 List -> results
            //是按照射线检测顺序排序过的
            EventSystem.current.RaycastAll (data, results);

            if (results.Count < 1)
                return;

            var source = data.pointerCurrentRaycast.gameObject;

            var lowerThanCurrent = false;

            foreach (var result in results)
            {
                Debug.Log("ResultGO: {0}"+ result.gameObject.name);


                #region 层级检测

                // 这部分检测是为了只把事件透传给层级低于该对象的对象
                if (result.gameObject == gameObject)
                {
                    lowerThanCurrent = true;
                    continue;
                }

                if (!lowerThanCurrent)
                    continue;

                #endregion

                //避免事件传递产生死循环，理论上这个判断永远为 false
                if (result.gameObject == source)
                    continue;

                if (ExecuteEvents.Execute (result.gameObject, data, function))
                {
#if UNITY_EDITOR
                    Debug.Log("Pass to: " + result.gameObject.name + " Depend obj: " + gameObject.name);
#endif
                    break;
                }
            }

            //若射线检测列表里没有该脚本所在的gameobject
            //则会走到这
            //说明射线检测时该go已经关闭，那么直接将事件传递给第一个对象
            if (!lowerThanCurrent)
            {
                ExecuteEvents.Execute (results[0].gameObject, data, function);
            }

            results.Clear ();
        }

        public void OnPointerClick (PointerEventData eventData)
        {
            if (!_passClick)
                return;
            PassEvent (eventData, ExecuteEvents.pointerClickHandler);
        }
    }

///其实就是继承了接口IPointerClickHandler，重写方法OnPointerClick。具体做法就是，射线照射，
/// 然后从得到的结果中，找当前脚本挂的gameObject，然后如果找到了，那么就取它下面第一个gameObjct，然后对他执行ExecuteEvents.Execute方法
