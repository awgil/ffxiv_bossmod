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
        Class.MNK => new ColumnPlayerGaugeMNK(timeline, tree, phaseBranches, replay, enc, player),
        //Class.DRG => new ColumnPlayerGaugeDRG(timeline, tree, phaseBranches, replay, enc, player),
        //Class.NIN => new ColumnPlayerGaugeNIN(timeline, tree, phaseBranches, replay, enc, player),
        Class.SAM => new ColumnPlayerGaugeSAM(timeline, tree, phaseBranches, replay, enc, player),
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
public class ColumnPlayerGaugeMNK : ColumnPlayerGauge
{
    private readonly ColorConfig _colors = Service.Config.Get<ColorConfig>();
    private readonly ColumnGenericHistory _chakras;
    private readonly ColumnGenericHistory _beast1;
    private readonly ColumnGenericHistory _beast2;
    private readonly ColumnGenericHistory _beast3;
    private readonly ColumnGenericHistory _nadi;

    public override bool Visible
    {
        get => _chakras.Width > 0 || _beast1.Width > 0 || _beast2.Width > 0 || _beast3.Width > 0 || _nadi.Width > 0;
        set
        {
            var width = value ? ColumnGenericHistory.DefaultWidth : 0;
            _chakras.Width = width;
            _beast1.Width = width;
            _beast2.Width = width;
            _beast3.Width = width;
            _nadi.Width = width;
        }
    }
    public ColumnPlayerGaugeMNK(Timeline timeline, StateMachineTree tree, List<int> phaseBranches, Replay replay, Replay.Encounter enc, Replay.Participant player)
        : base(timeline, tree, phaseBranches, replay, enc, player)
    {
        _chakras = Add(new ColumnGenericHistory(timeline, tree, phaseBranches));
        _chakras.Name = "Chakras";
        _beast1 = Add(new ColumnGenericHistory(timeline, tree, phaseBranches));
        _beast1.Name = "Beast Chakra 1";
        _beast2 = Add(new ColumnGenericHistory(timeline, tree, phaseBranches));
        _beast2.Name = "Beast Chakra 2";
        _beast3 = Add(new ColumnGenericHistory(timeline, tree, phaseBranches));
        _beast3.Name = "Beast Chakra 3";
        _nadi = Add(new ColumnGenericHistory(timeline, tree, phaseBranches));
        _nadi.Name = "Nadi";
        var prevChakras = 0;
        var prevBeast1 = default(BeastChakraType);
        var prevBeast2 = default(BeastChakraType);
        var prevBeast3 = default(BeastChakraType);
        var prevNadi = default(NadiFlags);
        var prevChakraTime = MinTime();
        var prevBeastChakra1Time = MinTime();
        var prevBeastChakra2Time = MinTime();
        var prevBeastChakra3Time = MinTime();
        var prevNadiTime = MinTime();
        foreach (var (time, gauge) in EnumerateGauge<MonkGauge>())
        {
            if (gauge.Chakra != prevChakras)
            {
                AddChakraRange(_chakras, prevChakraTime, time, prevChakras, _chakras.Name);
                prevChakras = gauge.Chakra;
                prevChakraTime = time;
            }
            if (gauge.BeastChakra1 != prevBeast1)
            {
                AddBeastChakraRange(_beast1, prevBeastChakra1Time, time, prevBeast1, _beast1.Name);
                prevBeast1 = gauge.BeastChakra1;
                prevBeastChakra1Time = time;
            }
            if (gauge.BeastChakra2 != prevBeast2)
            {
                AddBeastChakraRange(_beast2, prevBeastChakra2Time, time, prevBeast2, _beast2.Name);
                prevBeast2 = gauge.BeastChakra2;
                prevBeastChakra2Time = time;
            }
            if (gauge.BeastChakra3 != prevBeast3)
            {
                AddBeastChakraRange(_beast3, prevBeastChakra3Time, time, prevBeast3, _beast3.Name);
                prevBeast3 = gauge.BeastChakra3;
                prevBeastChakra3Time = time;
            }
            if (gauge.Nadi != prevNadi)
            {
                AddNadiRange(_nadi, prevNadiTime, time, prevNadi, _nadi.Name);
                prevNadi = gauge.Nadi;
                prevNadiTime = time;
            }
        }
        AddChakraRange(_chakras, prevChakraTime, enc.Time.End, prevChakras, _chakras.Name);
        AddBeastChakraRange(_beast1, prevBeastChakra1Time, enc.Time.End, prevBeast1, _beast1.Name);
        AddBeastChakraRange(_beast2, prevBeastChakra2Time, enc.Time.End, prevBeast2, _beast2.Name);
        AddBeastChakraRange(_beast3, prevBeastChakra3Time, enc.Time.End, prevBeast3, _beast3.Name);
        AddNadiRange(_nadi, prevNadiTime, enc.Time.End, prevNadi, _nadi.Name);
    }
    private void AddChakraRange(ColumnGenericHistory col, DateTime from, DateTime to, int gauge, string label)
    {
        if (to > from)
        {
            var color = gauge switch
            {
                5 => _colors.PlannerWindow[2],
                4 => new(0xFFE8FAFF),
                3 => new(0xFFBEEFFF),
                2 => new(0xFF90E0FF),
                1 => new(0xFFB0E0E6),
                _ => new(0x80808080)
            };
            col.AddHistoryEntryRange(Encounter.Time.Start, from, to, $"{label}: {gauge}", color.ABGR, gauge * 0.2f);
        }
    }
    private void AddBeastChakraRange(ColumnGenericHistory col, DateTime from, DateTime to, BeastChakraType count, string label)
    {
        if (count != BeastChakraType.None && to > from)
        {
            var color = count == BeastChakraType.None ? _colors.PlannerWindow[2] : new(0xFF0066FF);
            col.AddHistoryEntryRange(Encounter.Time.Start, from, to, $"{label}: {count}", color.ABGR);
        }
    }
    private void AddNadiRange(ColumnGenericHistory col, DateTime from, DateTime to, NadiFlags nadi, string label)
    {
        if (to > from)
        {
            var color = nadi switch
            {
                NadiFlags.Solar => new(0xFF90E0FF),
                NadiFlags.Lunar => new(0xFFFFA500),
                0 => new(0x80808080),
                _ => _colors.PlannerWindow[2]
            };
            col.AddHistoryEntryRange(Encounter.Time.Start, from, to, $"{label}: {nadi}", color.ABGR, 100);
        }
    }
}
#endregion

#region DRG
// TODO: add DRG gauge
#endregion

#region NIN
// TODO: add NIN gauge
#endregion

#region SAM
public class ColumnPlayerGaugeSAM : ColumnPlayerGauge
{
    private readonly ColumnGenericHistory _kenki;
    private readonly ColumnGenericHistory _sen;
    private readonly ColumnGenericHistory _meditation;
    private readonly ColorConfig _colors = Service.Config.Get<ColorConfig>();

    public override bool Visible
    {
        get => _kenki.Width > 0 || _meditation.Width > 0 || _sen.Width > 0;
        set
        {
            var width = value ? ColumnGenericHistory.DefaultWidth : 0;
            _kenki.Width = width;
            _sen.Width = width;
            _meditation.Width = width;
        }
    }
    public ColumnPlayerGaugeSAM(Timeline timeline, StateMachineTree tree, List<int> phaseBranches, Replay replay, Replay.Encounter enc, Replay.Participant player)
        : base(timeline, tree, phaseBranches, replay, enc, player)
    {
        _kenki = Add(new ColumnGenericHistory(timeline, tree, phaseBranches));
        _sen = Add(new ColumnGenericHistory(timeline, tree, phaseBranches));
        _meditation = Add(new ColumnGenericHistory(timeline, tree, phaseBranches));

        var prevKenki = 0;
        var prevSen = default(SenFlags);
        var prevKa = default(SenFlags);
        var prevGetsu = default(SenFlags);
        var prevSetsu = default(SenFlags);
        var prevMeditation = 0;
        var prevKenkiTime = MinTime();
        var prevSenTime = MinTime();
        var prevKaTime = MinTime();
        var prevGetsuTime = MinTime();
        var prevSetsuTime = MinTime();
        var prevMeditationTime = MinTime();
        foreach (var (time, gauge) in EnumerateGauge<SamuraiGauge>())
        {
            if (gauge.Kenki != prevKenki)
            {
                AddKenkiRange(prevKenkiTime, time, prevKenki);
                prevKenki = gauge.Kenki;
                prevKenkiTime = time;
            }
            if (gauge.SenFlags != prevSen)
            {
                AddSenRange(prevSenTime, time, prevSen);
                prevSen = gauge.SenFlags;
                prevSenTime = time;
            }
            if (gauge.MeditationStacks != prevMeditation)
            {
                AddMeditationRange(prevMeditationTime, time, prevMeditation);
                prevMeditation = gauge.MeditationStacks;
                prevMeditationTime = time;
            }
        }
        AddKenkiRange(prevKenkiTime, enc.Time.End, prevKenki);
        AddSenRange(prevKaTime, enc.Time.End, prevKa);
        AddSenRange(prevGetsuTime, enc.Time.End, prevGetsu);
        AddSenRange(prevSetsuTime, enc.Time.End, prevSetsu);
        AddMeditationRange(prevMeditationTime, enc.Time.End, prevMeditation);
    }
    private void AddKenkiRange(DateTime from, DateTime to, int Kenki)
    {
        if (Kenki != 0 && to > from)
        {
            var color = Kenki == 100 ? _colors.PlannerWindow[2] : new(0xFF90E0FF);
            _kenki.AddHistoryEntryRange(Encounter.Time.Start, from, to, $"{Kenki} Kenki", color.ABGR, Kenki < 10 ? Kenki * 0.02f : Kenki * 0.01f);
        }
    }
    private int GetSenCount(SenFlags sen)
    {
        var senCount = 0;
        if (sen.HasFlag(SenFlags.Setsu))
            senCount++;
        if (sen.HasFlag(SenFlags.Getsu))
            senCount++;
        if (sen.HasFlag(SenFlags.Ka))
            senCount++;

        return senCount;
    }
    private void AddSenRange(DateTime from, DateTime to, SenFlags sen)
    {
        if (sen != SenFlags.None && to > from)
        {
            var color = GetSenCount(sen) == 3 ? _colors.PlannerWindow[2] : new(0xFFFFAACC);
            _sen.AddHistoryEntryRange(Encounter.Time.Start, from, to, $"{sen}", color.ABGR, GetSenCount(sen) == 3 ? 1f : GetSenCount(sen) == 2 ? 0.6f : GetSenCount(sen) == 1 ? 0.3f : 1f);
        }
    }
    private void AddMeditationRange(DateTime from, DateTime to, int mediStacks)
    {
        if (mediStacks != 0 && to > from)
        {
            var color = mediStacks == 3 ? _colors.PlannerWindow[2] : new(0xFF8080FF);
            _meditation.AddHistoryEntryRange(Encounter.Time.Start, from, to, $"{mediStacks} Meditation stack{(mediStacks == 1 ? "" : "s")}", color.ABGR, mediStacks * 0.31f);
        }
    }
}
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
