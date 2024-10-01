using UnityEngine;

namespace StellaCircles.Utils
{
    public static class MathUtils
    {
        public static int GetLCM(int a, int b)
        {
            return Mathf.Abs(a * b) / GetGCD(a, b);
        }

        public static int GetGCD(int a, int b)
        {
            while (b != 0)
            {
                int temp = b;
                b = a % b;
                a = temp;
            }
            return a;
        }
    }
}
