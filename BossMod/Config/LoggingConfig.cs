using Newtonsoft.Json;
using System.IO;

namespace BossMod
{
    [ConfigDisplay(Name = "Logging settings", Order = 0)]
    public class LoggingConfig : ConfigNode
    {
        public enum LogFormat
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

        [JsonIgnore]
        [PropertyDisplay("Write event log")]
        public bool DumpWorldStateEvents = false;

        [PropertyDisplay("Log format")]
        public LogFormat WorldLogFormat = LogFormat.BinaryCompressed;

        [PropertyDisplay("Store server packets in the event log")]
        public bool DumpServerPackets = false;

        [PropertyDisplay("Store client packets in the event log")]
        public bool DumpClientPackets = false;

        [JsonIgnore]
        public DirectoryInfo? TargetDirectory;

        [JsonIgnore]
        public string LogPrefix = "World";
    }
}
