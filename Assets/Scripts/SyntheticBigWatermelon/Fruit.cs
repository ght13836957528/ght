using System;
using Unity.VisualScripting;
using UnityEngine;

namespace SyntheticBigWatermelon
{
    public class Fruit: MonoBehaviour
    {
        private Rigidbody2D _rigidbody2D;
        private CircleCollider2D _circleCollider2D;
        private bool _isDetected;

        public FruitConst.FruitType FruitType
        {
            get; set;
        }
        void OnCollisionEnter2D(Collision2D other) //碰撞检测
        {
           
        }

        public void Init()
        {
            _rigidbody2D = transform.GetComponent<Rigidbody2D>();
            _circleCollider2D = transform.GetComponent<CircleCollider2D>();
        }

        public void SetSimulate(bool simulate)
        {
            _rigidbody2D.simulated = simulate;
        }
        
        public void SetDetected(bool detect)
        {
            _isDetected = detect;
        }
        

    }
}