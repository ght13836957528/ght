using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Animal 
{
    public Animal()
    {
        Debug.Log("new animal");
    }

    public Animal(string name)
    {
        Debug.Log("new animal=="+ name);
    }

    public virtual void Shout()
    {
        Debug.Log("animal shout");
    }
}
