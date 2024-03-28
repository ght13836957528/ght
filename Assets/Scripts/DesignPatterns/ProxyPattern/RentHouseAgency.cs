using UnityEngine;

namespace DesignPatterns.ProxyPattern
{
    public class RentHouseAgency : IRentHouse
    {
        public void RentHouse()
        {
            Debug.Log("Rent House From Proxy");
        }
    }
}