using UnityEngine;

// Rand関数拡張
namespace RandExtension {
    public static class RandomEx {

        // min以上float未満のInt型を返す。
        public static int RangeforInt(int min, int max) {
            float rand;
            do {
                rand = Random.Range(min, max);
            } while(rand == max);
            return Mathf.FloorToInt(rand);
        }
    }
}