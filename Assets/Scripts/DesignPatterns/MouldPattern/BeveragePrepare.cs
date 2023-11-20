using UnityEngine;

namespace DesignPatterns.MouldPattern
{
    public abstract class BeveragePrepare
    {
        protected BeveragePrepare()
        {
            AddResource();
            BoilWater();
            PourIntoBottle();
            if (IfAddSpices())
                AddSpices();
        }

        protected abstract void AddResource();

        protected abstract void AddSpices();

        private void BoilWater()
        {
            Debug.Log("Boil Water");
        }

        private void PourIntoBottle()
        {
            Debug.Log("Pour Into Bottle");
        }

        protected abstract bool IfAddSpices();

    }
}