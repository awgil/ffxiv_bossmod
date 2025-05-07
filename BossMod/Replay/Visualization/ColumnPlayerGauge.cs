using FFXIVClientStructs.FFXIV.Client.Game.Gauge;

namespace BossMod.ReplayVisualization;

#region Base
public abstract class ColumnPlayerGauge : Timeline.ColumnGroup, IToggleableColumn
{
    public abstract bool Visible { get; set; }
    protected Replay Replay;
    protected Replay.Encounter Encounter;

    public static ColumnPlayerGauge? Create(Timeline timeline, StateMachineTree tree, List<int> phaseBranches, Replay replay, Replay.Encounter enc, Replay.Participant player, Class playerClass) => playerClass switch
    {
        //Class.PLD => new ColumnPlayerGaugePLD(timeline, tree, phaseBranches, replay, enc, player),
        Class.WAR => new ColumnPlayerGaugeWAR(timeline, tree, phaseBranches, replay, enc, player),
        //Class.DRK => new ColumnPlayerGaugeDRK(timeline, tree, phaseBranches, replay, enc, player),
        Class.GNB => new ColumnPlayerGaugeGNB(timeline, tree, phaseBranches, replay, enc, player),
        //Class.WHM => new ColumnPlayerGaugeWHM(timeline, tree, phaseBranches, replay, enc, player),
        //Class.SCH => new ColumnPlayerGaugeSCH(timeline, tree, phaseBranches, replay, enc, player),
        //Class.AST => new ColumnPlayerGaugeAST(timeline, tree, phaseBranches, replay, enc, player),
        //Class.SGE => new ColumnPlayerGaugeSGE(timeline, tree, phaseBranches, replay, enc, player),
        //Class.MNK => new ColumnPlayerGaugeMNK(timeline, tree, phaseBranches, replay, enc, player),
        //Class.DRG => new ColumnPlayerGaugeDRG(timeline, tree, phaseBranches, replay, enc, player),
        //Class.NIN => new ColumnPlayerGaugeNIN(timeline, tree, phaseBranches, replay, enc, player),
        //Class.SAM => new ColumnPlayerGaugeSAM(timeline, tree, phaseBranches, replay, enc, player),
        //Class.RPR => new ColumnPlayerGaugeRPR(timeline, tree, phaseBranches, replay, enc, player),
        //Class.VPR => new ColumnPlayerGaugeVPR(timeline, tree, phaseBranches, replay, enc, player),
        Class.BRD => new ColumnPlayerGaugeBRD(timeline, tree, phaseBranches, replay, enc, player),
        Class.MCH => new ColumnPlayerGaugeMCH(timeline, tree, phaseBranches, replay, enc, player),
        //Class.DNC => new ColumnPlayerGaugeDNC(timeline, tree, phaseBranches, replay, enc, player),
        //Class.BLM => new ColumnPlayerGaugeBLM(timeline, tree, phaseBranches, replay, enc, player),
        //Class.SMN => new ColumnPlayerGaugeSMN(timeline, tree, phaseBranches, replay, enc, player),
        //Class.RDM => new ColumnPlayerGaugeRDM(timeline, tree, phaseBranches, replay, enc, player),
        //Class.PCT => new ColumnPlayerGaugePCT(timeline, tree, phaseBranches, replay, enc, player),
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
#endregion

#region PLD
// TODO: add PLD gauge
#endregion

#region WAR
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
#endregion

#region DRK
// TODO: add DRK gauge
#endregion

#region GNB
public class ColumnPlayerGaugeGNB : ColumnPlayerGauge
{
    private readonly ColumnGenericHistory _gauge;
    private readonly ColorConfig _colors = Service.Config.Get<ColorConfig>();

    public override bool Visible
    {
        get => _gauge.Width > 0;
        set => _gauge.Width = value ? ColumnGenericHistory.DefaultWidth : 0;
    }

    public ColumnPlayerGaugeGNB(Timeline timeline, StateMachineTree tree, List<int> phaseBranches, Replay replay, Replay.Encounter enc, Replay.Participant player)
        : base(timeline, tree, phaseBranches, replay, enc, player)
    {
        _gauge = Add(new ColumnGenericHistory(timeline, tree, phaseBranches));
        _gauge.Name = "Ammo";

        var prevGauge = 0;
        var prevTime = MinTime();
        foreach (var (time, gauge) in EnumerateGauge<GunbreakerGauge>())
        {
            var count = gauge.Ammo;
            if (count != prevGauge)
            {
                AddGaugeRange(prevTime, time, prevGauge);
                prevGauge = count;
                prevTime = time;
            }
        }
        AddGaugeRange(prevTime, enc.Time.End, prevGauge);
    }

    private void AddGaugeRange(DateTime from, DateTime to, int gauge)
    {
        if (to > from)
        {
            var color = (gauge == 3) ? _colors.PlannerWindow[2] : (gauge == 2) ? new(0xFF90E0FF) : (gauge == 1) ? new(0xFFD6F5FF) : new(0x80808080);
            _gauge.AddHistoryEntryRange(Encounter.Time.Start, from, to, $"{gauge} Cartridge{(gauge == 1 ? "" : "s")}", color.ABGR, gauge * 0.31f);
        }
    }
}
#endregion

#region WHM
// TODO: add WHM gauge
#endregion

#region SCH
// TODO: add SCH gauge
#endregion

#region AST
// TODO: add AST gauge
#endregion

#region SGE
// TODO: add SGE gauge
#endregion

#region MNK
// TODO: add MNK gauge
#endregion

#region DRG
// TODO: add DRG gauge
#endregion

#region NIN
// TODO: add NIN gauge
#endregion

#region SAM
// TODO: add SAM gauge
#endregion

#region RPR
// TODO: add RPR gauge
#endregion

#region VPR
// TODO: add VPR gauge
#endregion

#region BRD
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
#endregion

#region MCH
public class ColumnPlayerGaugeMCH : ColumnPlayerGauge
{
    private readonly ColorConfig _colors = Service.Config.Get<ColorConfig>();
    private readonly ColumnGenericHistory _heat;
    private readonly ColumnGenericHistory _battery;

    public override bool Visible
    {
        get => _heat.Width > 0 || _battery.Width > 0;
        set
        {
            var width = value ? ColumnGenericHistory.DefaultWidth : 0;
            _heat.Width = width;
            _battery.Width = width;
        }
    }

    public ColumnPlayerGaugeMCH(Timeline timeline, StateMachineTree tree, List<int> phaseBranches, Replay replay, Replay.Encounter enc, Replay.Participant player)
        : base(timeline, tree, phaseBranches, replay, enc, player)
    {
        _heat = Add(new ColumnGenericHistory(timeline, tree, phaseBranches));
        _heat.Name = "Heat";
        _battery = Add(new ColumnGenericHistory(timeline, tree, phaseBranches));
        _battery.Name = "Battery";

        var prevHeat = 0;
        var prevBattery = 0;
        var prevHeatTime = MinTime();
        var prevBatteryTime = MinTime();
        foreach (var (time, gauge) in EnumerateGauge<MachinistGauge>())
        {
            if (gauge.Heat != prevHeat)
            {
                AddGaugeRange(_heat, prevHeatTime, time, prevHeat, _heat.Name);
                prevHeat = gauge.Heat;
                prevHeatTime = time;
            }
            if (gauge.Battery != prevBattery)
            {
                AddGaugeRange(_battery, prevBatteryTime, time, prevBattery, _battery.Name);
                prevBattery = gauge.Battery;
                prevBatteryTime = time;
            }
        }
        AddGaugeRange(_heat, prevHeatTime, enc.Time.End, prevHeat, _heat.Name);
        AddGaugeRange(_battery, prevBatteryTime, enc.Time.End, prevBattery, _battery.Name);
    }

    private void AddGaugeRange(ColumnGenericHistory col, DateTime from, DateTime to, int gauge, string label)
    {
        if (to > from)
        {
            var color = (gauge == 100) ? _colors.PlannerWindow[2] : label switch
            {
                "Heat" => new(0xFF90E0FF),
                "Battery" => new(0xFFFFA500),
                _ => new(0x08080808)
            };
            var width = gauge < 10 ? gauge * 0.02f : gauge * 0.01f; //if Heat = 5, it will not show on Visualizer, instead showing as 0.. so here's a small hack-around to make it visible
            col.AddHistoryEntryRange(Encounter.Time.Start, from, to, $"{label}: {gauge}", color.ABGR, width);
        }
    }
}
#endregion

#region DNC
// TODO: add DNC gauge
#endregion

#region BLM
// TODO: add BLM gauge
#endregion

#region SMN
// TODO: add SMN gauge
#endregion

#region RDM
// TODO: add RDM gauge
#endregion

#region PCT
// TODO: add PCT gauge
#endregion
