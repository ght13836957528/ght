using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class InitProblemMain : MonoBehaviour
{
    [SerializeField] private InitProblemItem item;
    [SerializeField] private Transform content;

    private GameObject itemInstance;
    
    public static int _frameNum = 0;
    // Start is called before the first frame update
    void Start()
    {
        itemInstance = GameObject.Instantiate(item.gameObject, content);
        Debug.Log("_frameNum =="+ _frameNum);
        itemInstance.transform.GetComponent<InitProblemItem>().Init();
        itemInstance.SetActive(true);
        // StartCoroutine(Init());


    }

    public IEnumerator Init()
    {
        yield return null; 
        itemInstance.transform.GetComponent<InitProblemItem>().Init();
    }

    // Update is called once per frame
    void Update()
    {
        _frameNum++;
    }
}
