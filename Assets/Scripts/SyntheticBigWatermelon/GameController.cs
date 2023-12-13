using System;
using System.Collections.Generic;
using GameManager;
using UnityEngine;


namespace SyntheticBigWatermelon
{
    public class GameController : Singleton<GameController>
    {
        private FruitFactory _factory;
        private FruitBase _fruitNext;
        private FruitRecord _record;
        
        public GameController()
        {
            
        }

        public void Init(List<GameObject> fruitGameObject , GameObject fruitParent , GameObject startPoint)
        {
            InitRecord();
            InitFruitFactory(fruitGameObject, fruitParent, startPoint);
        }

        private void InitFruitFactory(List<GameObject> fruitGameObject , GameObject fruitParent , GameObject startPoint)
        {
            _factory = new FruitFactory(fruitGameObject , fruitParent , startPoint );
        }

        private void InitRecord()
        {
            _record = new FruitRecord();
        }

        public void Reload()
        {
            
        }

        public FruitBase GetFruitNext()
        {
            return _fruitNext;
        }
       
        public void GenerateFirstFruit()
        {
            GenerateFruitNext();
        }
        
        /// <summary>
        /// 生成下一个水果
        /// </summary>
        /// <param name="type"></param>
        public void GenerateFruitNext(FruitConst.FruitType type = default)
        {
            _fruitNext = GenerateFruit(type);
        }
        
        private FruitBase GenerateFruit(FruitConst.FruitType type = default )
        {
            FruitBase fruit = _factory.GenerateFruit(FruitConst.FruitGenerateType.Generate,default,type);
            return fruit;
        }
        
        public void OnFruitCollision(Collision2D other , FruitBase fruit)
        {
            FruitBase colliderFruit = other.transform.GetComponent<FruitBase>();
            fruit.SetDetected(false);
            colliderFruit.SetDetected(false);
            FruitConst.FruitType generateType = colliderFruit.FruitType + 1;
            FruitBase generateFruit = _factory.GenerateFruit(FruitConst.FruitGenerateType.Combine, other.transform.position, generateType );
            
            
            
            generateFruit.Fall();
            colliderFruit.Dispose();
            fruit.Dispose();
        }
        
        public void AddFruitToRecordList(FruitBase fruitBase)
        {
            _record.AddFruitToRecordList(fruitBase);
        }
        
        public bool RemoveFruitFromRecordList(FruitBase fruitBase)
        {
            bool succeed = _record.RemoveFruitFromRecordList(fruitBase);
            return succeed;
        }

        public void UpdateFruitPosInRecordList(FruitBase fruitBase)
        {
             _record.UpdateFruitPosInRecordList(fruitBase);
        }

        public FruitConst.FruitType GetMaxFruitType()
        {
            return FruitConst.FruitType.Orange;
        }



    }
}