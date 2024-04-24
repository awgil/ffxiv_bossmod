namespace BossMod.ReplayVisualization;

public class ColumnActorHP : Timeline.ColumnGroup, IToggleableColumn
{
    private readonly ColumnGenericHistory _hpBase;
    private readonly ColumnGenericHistory _hpExtended;
    private readonly ColumnGenericHistory _shield;

    public bool Visible
    {
        get => _hpBase.Width > 0;
        set => _hpBase.Width = _hpExtended.Width = _shield.Width = value ? ColumnGenericHistory.DefaultWidth : 0;
    }

    public ColumnActorHP(Timeline timeline, StateMachineTree tree, List<int> phaseBranches, Replay replay, Replay.Encounter enc, Replay.Participant actor)
        : base(timeline)
    {
        Name = "HP";
        _hpBase = Add<ColumnGenericHistory>(new(timeline, tree, phaseBranches, "Base"));
        _hpExtended = Add<ColumnGenericHistory>(new(timeline, tree, phaseBranches, "Bonus"));
        _shield = Add<ColumnGenericHistory>(new(timeline, tree, phaseBranches, "Shield"));

        var initial = actor.HPMPAt(enc.Time.Start);
        DateTime prevTime = enc.Time.Start;
        var prevHPMP = initial;
        foreach (var h in actor.HPMPHistory.SkipWhile(e => e.Key <= enc.Time.Start).TakeWhile(e => e.Key < enc.Time.End))
        {
            AddRange(enc.Time.Start, prevTime, h.Key, initial.MaxHP, prevHPMP);
            prevTime = h.Key;
            prevHPMP = h.Value;
        }
        AddRange(enc.Time.Start, prevTime, enc.Time.End, initial.MaxHP, actor.HPMPAt(enc.Time.End));

        foreach (var a in replay.EncounterActions(enc))
        {
            int damage = 0;
            int heal = 0;
            foreach (var t in a.Targets)
            {
                foreach (var e in t.Effects)
                {
                    var effTarget = e.AtSource ? a.Source : t.Target;
                    if (effTarget == actor)
                    {
                        if (e.Type is ActionEffectType.Damage or ActionEffectType.BlockedDamage or ActionEffectType.ParriedDamage)
                            damage += e.DamageHealValue;
                        else if (e.Type == ActionEffectType.Heal)
                            heal += e.DamageHealValue;
                    }
                }
            }

            if (damage != 0 || heal != 0)
            {
                var name = $"-{damage} +{heal}: {a.ID} {ReplayUtils.ParticipantString(a.Source, a.Timestamp)} -> {ReplayUtils.ParticipantString(a.MainTarget, a.Timestamp)} #{a.GlobalSequence}";
                var color = damage == 0 ? 0xff00ff00 : heal == 0 ? 0xff00ffff : 0xffff00ff;
                _hpBase.AddHistoryEntryDot(enc.Time.Start, a.Timestamp, name, color).AddActionTooltip(a);
            }
        }
    }

    private void AddRange(DateTime encStart, DateTime rangeStart, DateTime rangeEnd, uint initialMax, ActorHPMP hpmp)
    {
        if (hpmp.CurHP == 0)
            return;

        var pctFromInitial = (float)hpmp.CurHP / initialMax;
        var text = $"{hpmp.CurHP}+{hpmp.Shield}/{hpmp.MaxHP} ({pctFromInitial * 100:f2}%)";
        _hpBase.AddHistoryEntryRange(encStart, rangeStart, rangeEnd, text, 0x80808080, Math.Min(pctFromInitial, 1));
        if (pctFromInitial > 1)
        {
            _hpExtended.AddHistoryEntryRange(encStart, rangeStart, rangeEnd, text, 0x8080ff80, Math.Min(pctFromInitial - 1, 1));
        }
        if (hpmp.Shield > 0)
        {
            _shield.AddHistoryEntryRange(encStart, rangeStart, rangeEnd, text, 0x80008080, Math.Min((float)hpmp.Shield / initialMax, 1));
        }
    }
}
