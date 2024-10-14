using FFXIVClientStructs.FFXIV.Client.Game.Gauge;

namespace BossMod.ReplayVisualization;

public abstract class ColumnPlayerGauge : Timeline.ColumnGroup, IToggleableColumn
{
    public abstract bool Visible { get; set; }
    protected Replay Replay;
    protected Replay.Encounter Encounter;

    public static ColumnPlayerGauge? Create(Timeline timeline, StateMachineTree tree, List<int> phaseBranches, Replay replay, Replay.Encounter enc, Replay.Participant player, Class playerClass) => playerClass switch
    {
        Class.WAR => new ColumnPlayerGaugeWAR(timeline, tree, phaseBranches, replay, enc, player),
        Class.BRD => new ColumnPlayerGaugeBRD(timeline, tree, phaseBranches, replay, enc, player),
        _ => null
    };

    protected ColumnPlayerGauge(Timeline timeline, StateMachineTree tree, List<int> phaseBranches, Replay replay, Replay.Encounter enc, Replay.Participant player)
        : base(timeline)
    {
        Name = "G";
        Replay = replay;
        Encounter = enc;
    }

    protected DateTime MinTime() => Encounter.Time.Start.AddSeconds(Timeline.MinTime);

    protected IEnumerable<(DateTime time, T gauge)> EnumerateGauge<T>() where T : unmanaged
    {
        var minTime = MinTime();
        foreach (var frame in Replay.Ops.SkipWhile(op => op.Timestamp < minTime).TakeWhile(op => op.Timestamp <= Encounter.Time.End).OfType<WorldState.OpFrameStart>())
            yield return (frame.Timestamp, ClientState.GetGauge<T>(frame.GaugePayload));
    }
}

public class ColumnPlayerGaugeWAR : ColumnPlayerGauge
{
    private readonly ColumnGenericHistory _gauge;

    public override bool Visible
    {
        get => _gauge.Width > 0;
        set => _gauge.Width = value ? ColumnGenericHistory.DefaultWidth : 0;
    }

    public ColumnPlayerGaugeWAR(Timeline timeline, StateMachineTree tree, List<int> phaseBranches, Replay replay, Replay.Encounter enc, Replay.Participant player)
        : base(timeline, tree, phaseBranches, replay, enc, player)
    {
        _gauge = Add(new ColumnGenericHistory(timeline, tree, phaseBranches));

        int prevGauge = 0;
        var prevTime = MinTime();
        foreach (var (time, gauge) in EnumerateGauge<WarriorGauge>())
        {
            if (gauge.BeastGauge != prevGauge)
            {
                AddGaugeRange(prevTime, time, prevGauge);
                prevGauge = gauge.BeastGauge;
                prevTime = time;
            }

            // TODO: add combo/FC/inf actions?..
        }
        AddGaugeRange(prevTime, enc.Time.End, prevGauge);
    }

    private void AddGaugeRange(DateTime from, DateTime to, int gauge)
    {
        if (gauge != 0 && to > from)
        {
            _gauge.AddHistoryEntryRange(Encounter.Time.Start, from, to, $"{gauge} gauge", 0x80808080, gauge * 0.01f);
        }
    }
}

public class ColumnPlayerGaugeBRD : ColumnPlayerGauge
{
    private readonly ColorConfig _colors = Service.Config.Get<ColorConfig>();
    private readonly ColumnGenericHistory _songs;
    private readonly ColumnGenericHistory _soul;

    public override bool Visible
    {
        get => _songs.Width > 0;
        set => _songs.Width = _soul.Width = value ? ColumnGenericHistory.DefaultWidth : 0;
    }

    public ColumnPlayerGaugeBRD(Timeline timeline, StateMachineTree tree, List<int> phaseBranches, Replay replay, Replay.Encounter enc, Replay.Participant player)
        : base(timeline, tree, phaseBranches, replay, enc, player)
    {
        _songs = Add(new ColumnGenericHistory(timeline, tree, phaseBranches));
        _soul = Add(new ColumnGenericHistory(timeline, tree, phaseBranches));

        var prevSong = (default(SongFlags), 0);
        var prevSoul = 0;
        var prevSongTime = MinTime();
        var prevSoulTime = prevSongTime;
        var prevSongStartTimer = 0.0f;
        foreach (var (time, gauge) in EnumerateGauge<BardGauge>())
        {
            if ((gauge.SongFlags, gauge.Repertoire) != prevSong)
            {
                AddSongRange(prevSongTime, time, prevSong.Item1, prevSong.Item2, prevSongStartTimer);
                prevSong = (gauge.SongFlags, gauge.Repertoire);
                prevSongTime = time;
                prevSongStartTimer = gauge.SongTimer * 0.001f;
            }
            if (gauge.SoulVoice != prevSoul)
            {
                AddSoulRange(prevSoulTime, time, prevSoul);
                prevSoul = gauge.SoulVoice;
                prevSoulTime = time;
            }
        }
        AddSongRange(prevSongTime, enc.Time.End, prevSong.Item1, prevSong.Item2, prevSongStartTimer);
        AddSoulRange(prevSoulTime, enc.Time.End, prevSoul);
    }

    private void AddSongRange(DateTime from, DateTime to, SongFlags song, int repertoire, float timer)
    {
        if (song != SongFlags.None && to > from)
        {
            var (color, scale) = (song & SongFlags.WanderersMinuet) switch
            {
                SongFlags.MagesBallad => (_colors.PlannerWindow[0], 1),
                SongFlags.ArmysPaeon => (_colors.PlannerWindow[1], 0.2f),
                SongFlags.WanderersMinuet => (_colors.PlannerWindow[2], 0.25f),
                _ => (new(0x80808080), 1),
            };
            var e = _songs.AddHistoryEntryRange(Encounter.Time.Start, from, to, $"{song}, {repertoire} rep, {timer:f3} at start", color.ABGR, (1 + repertoire) * scale);
            e.TooltipExtra = (res, t) =>
            {
                var delta = t - (from - Encounter.Time.Start).TotalSeconds;
                var remaining = timer - delta;
                res.Add($"- time since start: {45 - remaining:f3}");
                res.Add($"- remaining: {remaining:f3}");
            };
        }
    }

    private void AddSoulRange(DateTime from, DateTime to, int soulVoice)
    {
        if (soulVoice != 0 && to > from)
        {
            _soul.AddHistoryEntryRange(Encounter.Time.Start, from, to, $"{soulVoice} voice", 0x80808080, soulVoice * 0.01f);
        }
    }
}
