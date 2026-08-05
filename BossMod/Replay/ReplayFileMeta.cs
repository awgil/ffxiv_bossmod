using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace BossMod;

// minimum replay data for culling so you don't have to full parse thousands of replays
public sealed record class ReplayFileMeta(FileInfo File, string DutyKey, TimeSpan Duration)
{
    private static readonly Regex TimestampSuffix = new(@"_(\d{4}_\d{2}_\d{2}_\d{2}_\d{2}_\d{2})$", RegexOptions.Compiled);
    private static readonly Regex JobLevel = BuildJobLevelRegex();
    private static readonly string[] FlagSuffixes = ["_U", "_LS", "_MI", "_NE"];

    public static List<ReplayFileMeta> Scan(DirectoryInfo dir)
    {
        var result = new List<ReplayFileMeta>();
        if (!dir.Exists)
            return result;

        foreach (var fi in dir.EnumerateFiles("*.log"))
        {
            if (TryParse(fi, out var meta))
                result.Add(meta);
        }
        return result;
    }

    public static bool TryParse(FileInfo fi, out ReplayFileMeta meta)
    {
        meta = null!;
        var stem = Path.GetFileNameWithoutExtension(fi.Name);
        if (stem.Length == 0)
            return false;

        DateTime? start = null;
        var m = TimestampSuffix.Match(stem);
        if (m.Success && DateTime.TryParseExact(m.Groups[1].Value, "yyyy_MM_dd_HH_mm_ss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var ts))
        {
            start = ts;
            stem = stem[..m.Index];
        }

        while (true)
        {
            var stripped = false;
            foreach (var flag in FlagSuffixes)
            {
                if (stem.EndsWith(flag, StringComparison.Ordinal))
                {
                    stem = stem[..^flag.Length];
                    stripped = true;
                }
            }
            if (!stripped)
                break;
        }

        var duty = stem;
        var jobMatch = JobLevel.Match(stem);
        if (jobMatch.Success)
            duty = stem[..jobMatch.Index];
        if (duty.Length == 0)
            duty = "(unknown)";

        var duration = start != null ? fi.LastWriteTime - start.Value : fi.LastWriteTime - fi.CreationTime;
        if (duration < TimeSpan.Zero)
            duration = TimeSpan.Zero;

        meta = new(fi, duty, duration);
        return true;
    }

    private static Regex BuildJobLevelRegex()
    {
        var names = Enum.GetNames<Class>().Where(n => n != nameof(Class.None));
        return new($@"_({string.Join('|', names)})\d{{1,3}}_", RegexOptions.Compiled);
    }
}
