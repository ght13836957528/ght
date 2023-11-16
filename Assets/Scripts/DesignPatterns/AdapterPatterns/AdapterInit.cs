using System;
using UnityEngine;

namespace DesignPatterns.AdapterPatterns
{
    public class AdapterInit: MonoBehaviour
    {
        private void Start()
        {
            MallardDuck duck = new MallardDuck();
            duck.Fly();
            duck.Quack();

            WildTurkey turkey = new WildTurkey();
            IDuck adapter = new TurkeyAdapter(turkey);
            
            adapter.Fly();
            adapter.Quack();
            
            ITurkey duckAdapter = new DuckAdapter(duck);
            duckAdapter.Fly();
            duckAdapter.Gobble();
            

        }
    }
}