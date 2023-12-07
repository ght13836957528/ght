using UnityEngine;

namespace SyntheticBigWatermelon
{
    public class FruitBase : MonoBehaviour
    {
        private Rigidbody2D _rigidbody2D;
        private CircleCollider2D _circleCollider2D;
        private bool _isDetected;
        public FruitConst.FruitType FruitType
        {
            get; 
            private set;
        }
        
        protected virtual bool GetIfCanCombine(Collision2D other , FruitBase fruit )
        {
            return false;
        }
        
        public void Init(FruitConst.FruitType fruitType )
        {
            _rigidbody2D = transform.GetComponent<Rigidbody2D>();
            _circleCollider2D = transform.GetComponent<CircleCollider2D>();
            _isDetected = true;
            FruitType = fruitType;
        }

        public void SetSimulate(bool simulate)
        {
            _rigidbody2D.simulated = simulate;
        }
        
        public void SetDetected(bool detect)
        {
            _isDetected = detect;
        }
        
        void OnCollisionEnter2D(Collision2D other) //碰撞检测
        {
            if (!_isDetected)
            {
                return;
            }

            if (GetIfCanCombine(other , this))
            {
                GameController.Instance.OnFruitCollision(other,this);
            }
        }

        public Rigidbody2D GetRigidbody2D()
        {
            return _rigidbody2D;
        }

    }
}