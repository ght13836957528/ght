using System;
using UnityEngine;

namespace DesignPatterns.AdapterPatterns
{
    
    /// <summary>
    /// 适配模式
    /// 用于将一个类的接口转换成为另一个类的接口，下面例子声明了两个类的实例 MallardDuck的duck， WildTurkey的turkey
    /// 若希望将turkey作为参数，还需要调用Quack方法，则需要一个适配器类，调用适配器的Quack方法，适配器内部调用turkey的Gobble方法即可
    /// </summary>
    public class AdapterInit: MonoBehaviour
    {
        private void Start()
        {
            MallardDuck duck = new MallardDuck();
            duck.Fly();
            duck.Quack();

            WildTurkey turkey = new WildTurkey();
            turkey.Fly();
            turkey.Gobble();
            
            IDuck adapter = new TurkeyAdapter(turkey);
            adapter.Fly();
            adapter.Quack();
            
            ITurkey duckAdapter = new DuckAdapter(duck);
            duckAdapter.Fly();
            duckAdapter.Gobble();
            

        }
    }
}