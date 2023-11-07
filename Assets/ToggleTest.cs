using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleTest : MonoBehaviour
{
    // Start is called before the first frame update
    public Toggle toggle;
    void Start()
    {
        toggle.isOn = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void change()
    {
        Debug.Log("change");
    }
}
