using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceTest : MonoBehaviour
{
    // Start is called before the first frame update
    public Rigidbody2D[] objList;
    public int Times = 1;

    private float time;
    private bool _roll;
    void Start()
    {
        time = 0.5f;
        _roll = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (_roll)
        {
            time -= Time.deltaTime;
            if (time <= 0)
            {
                AddForce();
                time = 1.0f;
            }
        }
    }

    public void BeginRoll()
    {
        _roll = true;
    }

    public void AddForce()
    {
        foreach (var obj in objList)
        {
            float x = Random.Range(-1.0f, 1.0f);
            float y = Random.Range(0.0f, 1.0f);
            obj.AddForce(new Vector3(x, y * Times, 0), ForceMode2D.Impulse);
        
        }
    }
}
