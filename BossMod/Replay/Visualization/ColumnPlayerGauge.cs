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

        var ir = replay.EncounterStatuses(enc).Where(s => s.ID == (uint)WAR.SID.InnerRelease && s.Target == player).ToList();
        int prevGauge = 0;
        var prevTime = enc.Time.Start;
        foreach (var a in replay.EncounterActions(enc).Where(a => a.Source == player && a.ID.Type == ActionType.Spell))
        {
            var delta = (WAR.AID)a.ID.ID switch
            {
                WAR.AID.Maim => 10,
                WAR.AID.StormPath => 20,
                WAR.AID.StormEye => 10,
                WAR.AID.MythrilTempest => 20,
                WAR.AID.InnerBeast or WAR.AID.FellCleave or WAR.AID.SteelCyclone or WAR.AID.Decimate => ir.Any(s => s.Time.Contains(a.Timestamp)) ? 0 : -50,
                WAR.AID.InnerChaos or WAR.AID.ChaoticCyclone => -50,
                WAR.AID.Infuriate => 50,
                _ => -1,
            };
            if (delta == -1)
                continue;

            if (prevGauge > 0)
                _gauge.AddHistoryEntryRange(enc.Time.Start, prevTime, a.Timestamp, $"{prevGauge} gauge", 0x80808080, prevGauge * 0.01f);

            var newGauge = Math.Clamp(prevGauge + delta, 0, 100);
            var actionName = $"{delta} gauge: {a.ID} -> {ReplayUtils.ParticipantString(a.MainTarget, a.Timestamp)} #{a.GlobalSequence}";
            _gauge.AddHistoryEntryDot(enc.Time.Start, a.Timestamp, actionName, delta == 0 ? 0xffc0c0c0 : newGauge == prevGauge + delta ? 0xffffffff : 0xff0000ff).AddActionTooltip(a);

            prevGauge = newGauge;
            prevTime = a.Timestamp;
        }
    }
}
