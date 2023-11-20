using UnityEngine;

namespace DesignPatterns.MouldPattern
{
    public class TeaPrepare : BeveragePrepare
    {
        protected override void AddResource()
        {
            Debug.Log("add Tea");
        }
        
        protected override void AddSpices()
        {
            Debug.Log("add Lemon");
        }

        protected override bool IfAddSpices()
        {
            return false;
        }
    }
}