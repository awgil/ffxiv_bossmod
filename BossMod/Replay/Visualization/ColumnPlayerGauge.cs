using FFXIVClientStructs.FFXIV.Client.Game.Gauge;

namespace BossMod.ReplayVisualization;

public abstract class ColumnPlayerGauge : Timeline.ColumnGroup, IToggleableColumn
{
    public abstract bool Visible { get; set; }

    protected ColumnPlayerGauge(Timeline timeline, StateMachineTree tree, List<int> phaseBranches, Replay replay, Replay.Encounter enc, Replay.Participant player)
        : base(timeline)
    {
        Name = "G";
    }

    public static ColumnPlayerGauge? Create(Timeline timeline, StateMachineTree tree, List<int> phaseBranches, Replay replay, Replay.Encounter enc, Replay.Participant player, Class playerClass)
    {
        return playerClass switch
        {
            Class.WAR => new ColumnPlayerGaugeWAR(timeline, tree, phaseBranches, replay, enc, player),
            _ => (ColumnPlayerGauge?)null // TODO: remove this cast as soon as we introduce other gauge type here...
        };
    }

    // TODO: reconsider...
    protected unsafe T GetGauge<T>(WorldState.OpFrameStart op) where T : unmanaged
    {
        T res = default;
        ((ulong*)&res)[1] = op.GaugePayload.Low;
        if (sizeof(T) > 16)
            ((ulong*)&res)[2] = op.GaugePayload.High;
        return res;
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

        var minTime = enc.Time.Start.AddSeconds(timeline.MinTime);
        int prevGauge = 0;
        var prevTime = minTime;
        foreach (var frame in replay.Ops.SkipWhile(op => op.Timestamp < minTime).TakeWhile(op => op.Timestamp <= enc.Time.End).OfType<WorldState.OpFrameStart>())
        {
            var gauge = GetGauge<WarriorGauge>(frame);
            if (gauge.BeastGauge != prevGauge)
            {
                if (prevGauge != 0)
                {
                    _gauge.AddHistoryEntryRange(enc.Time.Start, prevTime, frame.Timestamp, $"{prevGauge} gauge", 0x80808080, prevGauge * 0.01f);
                }

                prevGauge = gauge.BeastGauge;
                prevTime = frame.Timestamp;
            }

            // TODO: add combo/FC/inf actions?..
        }
    }
}
