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

        public FruitConst.FruitState FruitState
        {
            get; 
            private set;
        }
        
        public FruitConst.FruitGenerateType FruitGenerateType
        {
            get; 
            private set;
        }

        public void Init(FruitConst.FruitType fruitType, FruitConst.FruitGenerateType generateType )
        {
            _rigidbody2D = transform.GetComponent<Rigidbody2D>();
            _circleCollider2D = transform.GetComponent<CircleCollider2D>();
            _isDetected = true;
            FruitType = fruitType;
            FruitState = FruitConst.FruitState.StandBy;
            FruitGenerateType = generateType;
            SetSimulate(false);
            GameController.Instance.AddFruitToRecordList(this);
            OnInt();
        }
        
        public void Dispose()
        {
            bool succeed = GameController.Instance.RemoveFruitFromRecordList(this);
            if (succeed)
            {
                OnDispose();
                Destroy(gameObject);
            }
            else
            {
                Debug.LogError("remove fail");
            }
        }

        public void Fall()
        {
            SetSimulate(true);
            FruitState = FruitConst.FruitState.Dropping;
        }

        private void SetSimulate(bool simulate)
        {
            _rigidbody2D.simulated = simulate;
        }
        
        /// <summary>
        /// 设置是否进行碰撞检测
        /// </summary>
        /// <param name="detect"></param>
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
            
            FruitState = FruitConst.FruitState.Collision;
            GameController.Instance.UpdateFruitPosInRecordList(this);

            if (GetIfCanCombine(other , this))
            {
                GameController.Instance.OnFruitCollision(other,this);
            }

        }

        protected virtual void OnInt()
        {
            
        }

        protected virtual void OnDispose()
        {
            
        }
        
        protected virtual bool GetIfCanCombine(Collision2D other , FruitBase fruit )
        {
            return false;
        }

    }
}