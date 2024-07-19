using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AlphaProblem : MonoBehaviour
{
    // Start is called before the first frame update
    public Image Image;
    void Start()
    {
        var color = Image.color;
        color.a = 1;
        Image.color = color;
    }

    // Update is called once per frame
    void Update()
    {
      

    }
    
}
