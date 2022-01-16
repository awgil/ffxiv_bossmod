namespace BossMod
{
    // we use 'bit vectors' and 'bit matrices' to represent some things like player-player range checks etc
    public class BitVector
    {
        public static bool IsVector8BitSet(byte vector, int i)
        {
            return (vector & (1 << i)) != 0;
        }

        public static void SetVector8Bit(ref byte vector, int i, bool v)
        {
            if (v)
                vector |= (byte)(1 << i);
            else
                vector &= (byte)~(1 << i);
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
