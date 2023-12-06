using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterMelon : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject fruit;
    public GameObject parent;
    public GameObject startPoint;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            GameObject spawnFruit =Instantiate(fruit,parent.transform);
            spawnFruit.transform.position = startPoint.transform.position;

        }
    }
}
