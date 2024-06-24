using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitProblemMain : MonoBehaviour
{
    [SerializeField] private InitProblemItem item;
    [SerializeField] private Transform content;

    private GameObject itemInstance;
    // Start is called before the first frame update
    void Start()
    {
        itemInstance = GameObject.Instantiate(item.gameObject, content);
        itemInstance.SetActive(true);
        StartCoroutine(Init());


    }

    public IEnumerator Init()
    {
        yield return null; 
        itemInstance.transform.GetComponent<InitProblemItem>().Init();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
