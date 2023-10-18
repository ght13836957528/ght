using System;
using UnityEngine;

public class InitEntrance : MonoBehaviour
{
    private GreenHeadDuck _greenHeadDuck;

    private void Start()
    {
      
        _greenHeadDuck = new GreenHeadDuck( );
        FlyBehavior flyBehavior = new FlyNoWings();
        _greenHeadDuck.SetFlyBehaviour(flyBehavior);
        _greenHeadDuck.Fly();
        _greenHeadDuck.Quack();
    }
}