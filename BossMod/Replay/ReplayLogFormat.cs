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
