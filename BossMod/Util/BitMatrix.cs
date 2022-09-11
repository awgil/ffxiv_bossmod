namespace BossMod
{
    // 8x8 matrix with 1-bit elements, stored as 64-bit int
    // typically used for per-player-pair flags in normal raids
    public struct BitMatrix
    {
        public ulong Raw;

        public BitMatrix(ulong raw = 0) { Raw = raw; }

        // one-dimensional indexer works with consecutive 8-bit masks; higher bits are zero/ignored
        public BitMask this[int row]
        {
            get => new((uint)row < 8 ? ((Raw >> (8 * row)) & 0xFFul) : 0);
            set
            {
                if ((uint)row < 8)
                {
                    Raw &= ~(0xFFul << (8 * row));
                    Raw |= (value.Raw & 0xFF) << (8 * row);
                }
            }
        }

        // two-dimensional indexer works with single bits
        public bool this[int row, int col]
        {
            get => (uint)row < 8 && (uint)col < 8 ? (Raw & (1ul << (8 * row + col))) != 0 : false;
            set
            {
                if ((uint)row < 8 && (uint)col < 8)
                {
                    if (value)
                        Raw |= 1ul << (8 * row + col);
                    else
                        Raw &= ~(1ul << (8 * row + col));
                }
            }
        }

        // check if any bit in column is set
        public bool AnyBitInColumn(int col) => (Raw & (0x0101010101010101ul << col)) != 0;

        public void Reset() => Raw = 0;
    }
}
