using System.Collections.Generic;
using UnityEngine;

namespace SyntheticBigWatermelon
{
    public class FruitRecordParam
    {
        public FruitConst.FruitType type;
        public Vector3 pos;
    }

    public class FruitRecord
    {
        private Dictionary<int, FruitRecordParam> _fruitRecordParamsDic;

        public FruitRecord()
        {
            _fruitRecordParamsDic = new Dictionary<int, FruitRecordParam>();
        }

        public void AddFruitToRecordList(FruitBase fruit)
        {
            FruitRecordParam paramTest = new FruitRecordParam();
            bool ifExist = _fruitRecordParamsDic.TryGetValue(fruit.GetHashCode(), out paramTest);
            if (ifExist)
                return;
            FruitRecordParam param = new FruitRecordParam();
            param.type = fruit.FruitType;
            param.pos = fruit.gameObject.transform.position;
            int index = fruit.GetHashCode();
            _fruitRecordParamsDic[index] = param;
            Record();
        }

        public void UpdateFruitPosInRecordList(FruitBase fruit)
        {
            FruitRecordParam fruitParam = _fruitRecordParamsDic[fruit.GetHashCode()];
            if (fruitParam == null)
            {
                Debug.LogError("fruitParam is null" + fruit.transform.name);
                return;
            }

            fruitParam.pos = fruit.transform.position;
        }

        public bool RemoveFruitFromRecordList(FruitBase fruit)
        {
            int hashCode = fruit.GetHashCode();
            bool result =  _fruitRecordParamsDic.Remove(hashCode);
            Record();
            return result;
        }

        private void Record()
        {
            Debug.Log("dic length = "+_fruitRecordParamsDic.Count );
            if (IfRecord())
            {
            }
        }

        private bool IfRecord()
        {
            return _fruitRecordParamsDic.Count >= 5;
        }
    }
}