using UnityEngine;

namespace SyntheticBigWatermelon
{
    public class ForceTest: MonoBehaviour
    {
        public void CLickForceTest()
        {
            Rigidbody2D rigidbody2D = GetComponent<Rigidbody2D>();
            rigidbody2D.AddForce(Vector2.up * 500);
        }

    }
}