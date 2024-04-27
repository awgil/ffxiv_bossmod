namespace BossMod;

public enum ReplayLogFormat
{
    [PropertyDisplay("Compressed binary")]
    BinaryCompressed,

    [PropertyDisplay("Raw binary")]
    BinaryUncompressed,

    [PropertyDisplay("Condensed text")]
    TextCondensed,

    [PropertyDisplay("Verbose text")]
    TextVerbose,
}

public static class ReplayLogFormatMagic
{
    public static readonly FourCC CompressedBinary = new("BLCB"u8); // bossmod log compressed brotli
    public static readonly FourCC RawBinary = new("BLOG"u8); // bossmod log
}
