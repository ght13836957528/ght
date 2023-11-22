using System;

namespace DesignPatterns.MouldPattern
{
    public class Duck : IComparable
    {
        private string _name;
        private float _weight;

        public Duck(string name, float weight)
        {
            _name = name;
            _weight = weight;
        }

        /// <summary>
        /// 钩子使用
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int CompareTo(object obj) 
        {
            Duck duck = (Duck)obj;
            int result = 1;
            if (_weight > duck._weight)
                result = -1;
            else if (_weight.Equals(duck._weight))
                result = 0;
            else if (_weight < duck._weight)
                result = 1;
            return result;
        }

        public string GetName()
        {
            return _name;
        }

        public float GetWeight()
        {
            return _weight;
        }
    }
}