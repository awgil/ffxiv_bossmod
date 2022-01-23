namespace BossMod
{
    // we use 'bit vectors' and 'bit matrices' to represent some things like player-player range checks etc
    public class BitVector
    {
        public static bool IsVector8BitSet(byte vector, int i)
        {
            return (vector & (1 << i)) != 0;
        }

        public static void SetVector8Bit(ref byte vector, int i)
        {
            vector |= (byte)(1 << i);
        }

        public static void ClearVector8Bit(ref byte vector, int i)
        {
            vector &= (byte)~(1 << i);
        }

        public static void ModifyVector8Bit(ref byte vector, int i, bool v)
        {
            if (v)
                SetVector8Bit(ref vector, i);
            else
                ClearVector8Bit(ref vector, i);
        }

        public static bool IsVector64BitSet(ulong vector, int i)
        {
            return (vector & (1ul << i)) != 0;
        }

        public static void SetVector64Bit(ref ulong vector, int i)
        {
            vector |= (byte)(1ul << i);
        }

        public static void ClearVector64Bit(ref ulong vector, int i)
        {
            vector &= (byte)~(1ul << i);
        }

        public static void ModifyVector64Bit(ref ulong vector, int i, bool v)
        {
            if (v)
                SetVector64Bit(ref vector, i);
            else
                ClearVector64Bit(ref vector, i);
        }

        public static bool IsMatrix8x8BitSet(ulong matrix, int i, int j)
        {
            return (matrix & (1ul << (8 * i + j))) != 0;
        }

        public static void SetMatrix8x8Bit(ref ulong matrix, int i, int j, bool v)
        {
            if (v)
                matrix |= 1ul << (8 * i + j);
            else
                matrix &= ~(1ul << (8 * i + j));
        }

        public static byte ExtractVectorFromMatrix8x8(ulong matrix, int i)
        {
            return (byte)(matrix >> (i * 8));
        }

        public static void SetMatrix8x8Vector(ref ulong matrix, int i, byte vector)
        {
            matrix |= ((ulong)vector) << (8 * i);
        }
    }
}
