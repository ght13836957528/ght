using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterTest : MonoBehaviour
{
    // Start is called before the first frame update
    public Animator animator;
    private int time = 0;
    void Start()
    {
        animator.Play("standby");

        
    }

    // Update is called once per frame
    void Update()
    {
        if (time > 100)
        {
            animator.Play("move");
        }
        else
        {
            time++;
        }
    }

    private void Plus(ref int a)
    {
       
        
    }
    
    
}
