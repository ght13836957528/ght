using UnityEngine;

namespace DesignPatterns.MouldPattern
{
    public class CoffeePrepare : BeveragePrepare
    {
        protected override void AddResource()
        {
            Debug.Log("add coffee bean");
        }

        protected override void AddSpices()
        {
            Debug.Log("add cream");
        }

        protected override bool IfAddSpices()
        {
            return true;
        }
    }
}