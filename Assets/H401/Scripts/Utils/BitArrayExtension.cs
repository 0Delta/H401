// BitArray型を拡張
using System.Collections;

namespace BitArrayExtension
{
    public static class Extension
    {
        // 既存の演算は全部上書きなんで、自身を書き換えない関数を作った。
        public static BitArray retOr(this BitArray Bit, BitArray BitA)
        {
            return new BitArray(Bit).Or(BitA);
        }
        public static BitArray retAnd(this BitArray Bit,BitArray BitA)
        {
            return new BitArray(Bit).And(BitA);
        }
        public static BitArray retXor(this BitArray Bit,BitArray BitA)
        {
            return new BitArray(Bit).Xor(BitA);
        }
        public static BitArray retNot(this BitArray Bit)
        {
            return new BitArray(Bit).Not();
        }

        public static bool isZero(this BitArray Bit)
        {
            foreach (bool it in Bit)
            {
                if (it) return false;   // 全走査して一個でも立ってるならfalse
            }
            return true;
        }
        public static bool isNotZero(this BitArray Bit)
        {
            foreach (bool it in Bit)
            {
                if (it) return true;    // 全走査して一個でも立ってるならfalse
            }
            return false;
        }
        public static bool isEqual(this BitArray Bit,BitArray BitA)
        {
            return Bit.Xor(BitA).isZero();  // XORして0なら合致
        }

        public static string ToStringEx(this BitArray array) {
            string[] str = new string[1];
            for(int i = 0; i < array.Count; i++)
                str[0] += array[i] ? "1" : "0";

            return str[0];
        }
    }
}
