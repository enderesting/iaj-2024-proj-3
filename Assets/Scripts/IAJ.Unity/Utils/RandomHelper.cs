using UnityEngine;
using System.Collections.Generic;

namespace Assets.Scripts.IAJ.Unity.Utils
{
    public static class RandomHelper
    {
        public static float RandomBinomial(float max)
        {
            return Random.Range(0, max) - Random.Range(0, max);
        }

        public static float RandomBinomial()
        {
            return Random.Range(0, 1.0f) - Random.Range(0, 1.0f);
        }

        public static int RollD20()
        {
            return Random.Range(1, 21);
        }

        public static int RollD12()
        {
            return Random.Range(1, 13);
        }

        public static int RollD10()
        {
            return Random.Range(1, 11);
        }

        public static int RollD8()
        {
            return Random.Range(1, 9);
        }

        public static int RollD6()
        {
            return Random.Range(1, 7);
        }

        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = Random.Range(1, n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}
