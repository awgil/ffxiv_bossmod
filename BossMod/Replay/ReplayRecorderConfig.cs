using Newtonsoft.Json;
using System.IO;

namespace BossMod
{
    [ConfigDisplay(Name = "Replay recorder settings", Order = 0)]
    public class ReplayRecorderConfig : ConfigNode
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

        [PropertyDisplay("Show recorder UI")]
        public bool ShowUI = false;

        [PropertyDisplay("Store server packets in the replay")]
        public bool DumpServerPackets = false;

        [PropertyDisplay("Store client packets in the replay")]
        public bool DumpClientPackets = false;

        [PropertyDisplay("Log format")]
        public LogFormat WorldLogFormat = LogFormat.BinaryCompressed;

        [JsonIgnore]
        public DirectoryInfo? TargetDirectory;

        [JsonIgnore]
        public string LogPrefix = "World";
    }
}
