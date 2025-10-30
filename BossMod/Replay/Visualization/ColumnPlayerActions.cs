using Dalamud.Bindings.ImGui;

namespace BossMod.ReplayVisualization;

public class ColumnPlayerActions : Timeline.ColumnGroup
{
    private struct CooldownGroup
    {
        public ColumnGenericHistory? Column;
        public DateTime Cursor;
        public DateTime ChargeCooldownEnd;
        public int ChargesOnCooldown;
        public int MaxCharges;
        public float ChargeCooldown;
        public ActionID CooldownAction;
    }

    private readonly ColumnGenericHistory _autoAttacks;
    private readonly ColumnGenericHistory _animLocks;
    private readonly CooldownGroup[] _cdGroups = new CooldownGroup[ClientState.NumCooldownGroups];
    private readonly ColumnSeparator _sep;
    private readonly Dictionary<ActionID, (int group, float cd)> _cooldownReductions = [];

    public ColumnPlayerActions(Timeline timeline, StateMachineTree tree, List<int> phaseBranches, Replay replay, Replay.Encounter enc, Replay.Participant player, Class playerClass)
        : base(timeline)
    {
        _autoAttacks = Add<ColumnGenericHistory>(new(timeline, tree, phaseBranches, "Auto attacks"));
        _animLocks = Add<ColumnGenericHistory>(new(timeline, tree, phaseBranches, "Abilities with animation locks"));
        _sep = Add(new ColumnSeparator(timeline));
        GetCooldownColumn(ActionDefinitions.GCDGroup, new()).Name = "GCD"; // make sure GCD column always exists and is before any others
        SetupClass(playerClass);

        int iCast = 0;
        var minTime = enc.Time.Start.AddSeconds(timeline.MinTime);
        foreach (var a in replay.Actions.SkipWhile(a => a.Timestamp < minTime).TakeWhile(a => a.Timestamp <= enc.Time.End).Where(a => a.Source == player))
        {
            // note: we assume autoattacks are never casted... in fact, I think only GCDs can be casted
            var actionName = $"{a.ID} -> {ReplayUtils.ParticipantString(a.MainTarget, a.Timestamp)} #{a.GlobalSequence}";
            if (a.ID == ActionDefinitions.IDAutoAttack || a.ID == ActionDefinitions.IDAutoShot)
            {
                AddAnimationLock(_autoAttacks, a, enc.Time.Start, a.Timestamp, actionName);
                _autoAttacks.AddHistoryEntryDot(enc.Time.Start, a.Timestamp, actionName, 0xffc0c0c0).AddActionTooltip(a);
                continue;
            }

            while (iCast < player.Casts.Count && player.Casts[iCast].Time.End < a.Timestamp)
            {
                // add cast without event (interrupted, cancelled, whatever)
                AddUnfinishedCast(player.Casts[iCast++], enc.Time.Start);
            }

            var actionDef = ActionDefinitions.Instance[a.ID];
            DateTime effectStart;
            if (iCast < player.Casts.Count && player.Casts[iCast].Time.Start < a.Timestamp && player.Casts[iCast].ID == a.ID)
            {
                // casted action
                var cast = player.Casts[iCast++];
                var castName = $"{cast.ID} -> {ReplayUtils.ParticipantString(cast.Target, cast.Time.Start)}";
                _animLocks.AddHistoryEntryRange(enc.Time.Start, cast.Time, castName, 0x80ffffff).AddCastTooltip(cast);
                if (actionDef?.MainCooldownGroup >= 0)
                {
                    StartCooldown(a.ID, actionDef, enc.Time.Start, cast.Time.Start);
                    GetCooldownColumn(actionDef.MainCooldownGroup, a.ID).AddHistoryEntryRange(enc.Time.Start, cast.Time, castName, 0x80ffffff).AddCastTooltip(cast);
                    AdvanceCooldown(actionDef.MainCooldownGroup, enc.Time.Start, cast.Time.End, false);
                }
                effectStart = cast.Time.End;
            }
            else
            {
                if (actionDef?.MainCooldownGroup >= 0)
                {
                    StartCooldown(a.ID, actionDef, enc.Time.Start, a.ClientAction != null ? a.ClientAction.Requested : a.Timestamp);
                }
                effectStart = a.ClientAction != null ? a.ClientAction.Requested : a.Timestamp; // TODO: different depending on config...
            }

            AddAnimationLock(_animLocks, a, enc.Time.Start, effectStart, actionName);
            _animLocks.AddHistoryEntryDot(enc.Time.Start, a.Timestamp, actionName, actionDef != null ? (ActionConfirmed(a) ? 0xffffffff : 0xff00ffff) : 0xff0000ff).AddActionTooltip(a);

            if (actionDef?.MainCooldownGroup >= 0)
            {
                var col = GetCooldownColumn(actionDef.MainCooldownGroup, a.ID);
                float effectDuration = 0;
                List<string> effectTooltip = [];
                foreach (var t in a.Targets)
                {
                    foreach (var eff in t.Effects.Where(eff => eff.Type is ActionEffectType.ApplyStatusEffectTarget or ActionEffectType.ApplyStatusEffectSource))
                    {
                        var source = eff.FromTarget ? t.Target : a.Source;
                        var target = eff.Type == ActionEffectType.ApplyStatusEffectTarget && !eff.AtSource ? t.Target : a.Source;
                        var status = replay.Statuses.FirstOrDefault(s => s.ID == eff.Value && s.Target == target && s.Source == source);
                        var duration = (float)((status?.Time.End ?? a.Timestamp) - a.Timestamp).TotalSeconds;
                        var delay = ((status?.Time.Start ?? a.Timestamp) - a.Timestamp).TotalSeconds;
                        effectDuration = Math.Max(effectDuration, duration);
                        effectTooltip.Add($"- effect: {Utils.StatusString(eff.Value)}, duration={(status != null ? status.Time : "???")}s, start-delay={delay:f3}s");
                    }
                }
                if (actionDef.MainCooldownGroup == ActionDefinitions.GCDGroup)
                    effectDuration = Math.Min(effectDuration, 0.6f); // TODO: this is a hack, reconsider... the problem is that sometimes actions apply statuses that are then refreshed, that usually happens for gcds...
                if (effectDuration > 0)
                {
                    var e = col.AddHistoryEntryRange(enc.Time.Start, effectStart, effectDuration, actionName, 0x8000ff00);
                    e.TooltipExtra = (res, _) => res.AddRange(effectTooltip);
                    AdvanceCooldown(actionDef.MainCooldownGroup, enc.Time.Start, effectStart.AddSeconds(effectDuration), false);
                }
                col.AddHistoryEntryDot(enc.Time.Start, a.Timestamp, actionName, 0xffffffff).AddActionTooltip(a);
            }

            // cooldown reduction
            if (_cooldownReductions.TryGetValue(a.ID, out var cdr))
            {
                AdvanceCooldown(cdr.group, enc.Time.Start, a.Timestamp, true);
                ref var data = ref _cdGroups[cdr.group];
                float actualReduction = 0;
                if (data.ChargesOnCooldown > 0)
                {
                    actualReduction = cdr.cd;
                    data.ChargeCooldownEnd = data.ChargeCooldownEnd.AddSeconds(-cdr.cd);
                    if (a.Timestamp >= data.ChargeCooldownEnd)
                    {
                        if (--data.ChargesOnCooldown > 0)
                        {
                            data.Column?.AddHistoryEntryLine(enc.Time.Start, data.Cursor, "", 0xffffffff);
                            data.ChargeCooldownEnd = data.ChargeCooldownEnd.AddSeconds(data.ChargeCooldown);
                        }
                        else
                        {
                            actualReduction -= (float)(a.Timestamp - data.ChargeCooldownEnd).TotalSeconds;
                        }
                    }
                }
                data.Column?.AddHistoryEntryDot(enc.Time.Start, a.Timestamp, $"CD reduced by {actualReduction:f1}/{cdr.cd:f1}s: {actionName}", actualReduction < cdr.cd ? 0xff00ffff : 0xffffff00).AddActionTooltip(a);
            }
        }

        // add remaining unfinished casts
        while (iCast < player.Casts.Count)
        {
            AddUnfinishedCast(player.Casts[iCast++], enc.Time.Start);
        }

        // add unfinished cooldowns
        for (int i = 0; i < _cdGroups.Length; ++i)
            if (_cdGroups[i].ChargesOnCooldown > 0)
                AdvanceCooldown(i, enc.Time.Start, DateTime.MaxValue, true);
    }

    public void DrawConfig(UITree tree)
    {
        if (ImGui.Button("Show all"))
            foreach (var col in Columns)
                col.Width = ColumnGenericHistory.DefaultWidth;
        ImGui.SameLine();
        if (ImGui.Button("Hide all"))
            foreach (var col in Columns)
                col.Width = 0;

        foreach (var col in Columns)
        {
            bool visible = col.Width > 0;
            if (ImGui.Checkbox(col.Name, ref visible))
            {
                col.Width = visible ? ColumnGenericHistory.DefaultWidth : 0;
            }
        }

        _sep.Width = Columns.Any(c => c.Width > 0) ? 1 : 0;
    }

    private void SetupClass(Class playerClass)
    {
        switch (playerClass)
        {
            case Class.WAR:
                // make sure important damage cooldowns are in consistent order
                GetCooldownColumn(ActionID.MakeSpell(WAR.AID.Infuriate));
                GetCooldownColumn(ActionID.MakeSpell(WAR.AID.InnerRelease));
                GetCooldownColumn(ActionID.MakeSpell(WAR.AID.Upheaval));
                GetCooldownColumn(ActionID.MakeSpell(WAR.AID.Onslaught));
                // infuriate cooldown reductions
                var infCDG = ActionDefinitions.Instance.Spell(WAR.AID.Infuriate)!.MainCooldownGroup;
                _cooldownReductions[ActionID.MakeSpell(WAR.AID.InnerBeast)] = (infCDG, 5);
                _cooldownReductions[ActionID.MakeSpell(WAR.AID.FellCleave)] = (infCDG, 5);
                _cooldownReductions[ActionID.MakeSpell(WAR.AID.InnerChaos)] = (infCDG, 5);
                _cooldownReductions[ActionID.MakeSpell(WAR.AID.SteelCyclone)] = (infCDG, 5);
                _cooldownReductions[ActionID.MakeSpell(WAR.AID.Decimate)] = (infCDG, 5);
                _cooldownReductions[ActionID.MakeSpell(WAR.AID.ChaoticCyclone)] = (infCDG, 5);
                break;
        }
    }

    private void AddUnfinishedCast(Replay.Cast cast, DateTime encStart)
    {
        var name = $"[unfinished] {cast.ID} -> {ReplayUtils.ParticipantString(cast.Target, cast.Time.Start)}";
        _animLocks.AddHistoryEntryRange(encStart, cast.Time, name, 0x800000ff).AddCastTooltip(cast);

        var castActionDef = ActionDefinitions.Instance[cast.ID];
        if (castActionDef?.MainCooldownGroup >= 0)
        {
            AdvanceCooldown(castActionDef.MainCooldownGroup, encStart, cast.Time.Start, true);
            GetCooldownColumn(castActionDef.MainCooldownGroup, cast.ID).AddHistoryEntryRange(encStart, cast.Time, name, 0x800000ff).AddCastTooltip(cast);
            AdvanceCooldown(castActionDef.MainCooldownGroup, encStart, cast.Time.End, false); // consider cooldown reset instead?..
        }
    }

    private void AddAnimationLock(ColumnGenericHistory col, Replay.Action action, DateTime encStart, DateTime lockStart, string name)
    {
        if (action.AnimationLock > 0)
        {
            var e = col.AddHistoryEntryRange(encStart, lockStart, action.AnimationLock, name, 0x80808080);
            e.TooltipExtra = (res, t) =>
            {
                var elapsed = t - (lockStart - encStart).TotalSeconds;
                res.Add($"- anim lock: {action.AnimationLock - elapsed:f3} / {action.AnimationLock:f3}");
            };
        }
    }

    private ColumnGenericHistory GetCooldownColumn(int cooldownGroup, ActionID defaultAction)
        => _cdGroups[cooldownGroup].Column ??= AddBefore<ColumnGenericHistory>(new(Timeline, _autoAttacks.Tree, _autoAttacks.PhaseBranches, defaultAction.ToString()), _sep);
    private ColumnGenericHistory GetCooldownColumn(ActionID action) => GetCooldownColumn(ActionDefinitions.Instance[action]!.MainCooldownGroup, action);

    private void AddCooldownRange(ref CooldownGroup data, DateTime encStart, DateTime rangeEnd)
    {
        if (data.Column == null)
            return;
        var maxCharges = data.MaxCharges;
        var chargesOnCooldown = data.ChargesOnCooldown;
        var chargeCooldown = data.ChargeCooldown;
        var chargeCooldownEnd = data.ChargeCooldownEnd;
        var startCD = (chargeCooldownEnd - data.Cursor).TotalSeconds;
        var endCD = (chargeCooldownEnd - rangeEnd).TotalSeconds;
        var width = maxCharges > 0 ? (float)chargesOnCooldown / maxCharges : 1;
        var e = data.Column.AddHistoryEntryRange(encStart, data.Cursor, rangeEnd, data.CooldownAction.ToString(), 0x80808080, width);
        e.TooltipExtra = (res, t) =>
        {
            res.Add($"- remaining: {(chargeCooldownEnd - encStart).TotalSeconds - t:f3}s / {chargeCooldown:f3}s ({maxCharges - chargesOnCooldown}/{maxCharges} charges)");
            res.Add($"- start CD: {startCD:f3}s / {chargeCooldown:f3}s");
            res.Add($"- end CD: {endCD:f3}s / {chargeCooldown:f3}s");
        };
    }

    private void AdvanceCooldown(int cdGroup, DateTime encStart, DateTime timestamp, bool addRanges)
    {
        ref var data = ref _cdGroups[cdGroup];

        while (data.ChargesOnCooldown > 0 && timestamp >= data.ChargeCooldownEnd)
        {
            // next charge is fully finished
            if (addRanges)
                AddCooldownRange(ref data, encStart, data.ChargeCooldownEnd);
            data.Cursor = data.ChargeCooldownEnd;
            if (--data.ChargesOnCooldown > 0)
            {
                data.Column?.AddHistoryEntryLine(encStart, data.Cursor, "", 0xffffffff);
                data.ChargeCooldownEnd = data.Cursor.AddSeconds(data.ChargeCooldown);
            }
        }

        // add partially finished cooldown: we're either just spending another charge, or we're starting new cooldown slightly earlier than expected (due to high sks for gcd, or due to client-server timing differences)
        if (data.ChargesOnCooldown > 0)
        {
            // assertion: timestamp < data.ChargeCooldownEnd
            if (addRanges)
                AddCooldownRange(ref data, encStart, timestamp);
        }

        data.Cursor = timestamp;
    }

    private void StartCooldown(ActionID aid, ActionDefinition actionDef, DateTime encStart, DateTime timestamp)
    {
        AdvanceCooldown(actionDef.MainCooldownGroup, encStart, timestamp, true);
        ref var data = ref _cdGroups[actionDef.MainCooldownGroup];
        if (data.ChargesOnCooldown == 0)
        {
            // off cd -> cd
            data.ChargeCooldownEnd = data.Cursor.AddSeconds(actionDef.Cooldown);
            data.ChargesOnCooldown = 1;
            data.MaxCharges = actionDef.MaxChargesAtCap(); // TODO: at player level
        }
        else if (data.ChargesOnCooldown < actionDef.MaxChargesAtCap()) // TODO: at player level
        {
            // just spend new charge
            ++data.ChargesOnCooldown;
        }
        else
        {
            // already at max charges, assume previous cooldown is slightly smaller than expected
            var deficit = (data.ChargeCooldownEnd - data.Cursor).TotalSeconds;
            var e = data.Column!.AddHistoryEntryLine(encStart, timestamp, aid.ToString(), 0xffffffff);
            e.TooltipExtra = (res, _) => res.Add($"- cooldown {deficit:f1}s smaller than expected");
            data.ChargeCooldownEnd = data.Cursor.AddSeconds(actionDef.Cooldown);
        }
        data.ChargeCooldown = actionDef.Cooldown;
        data.CooldownAction = aid;
    }

    private bool ActionConfirmed(Replay.Action a) => a.Targets.Count == 0 || a.Targets.Any(t => t.ConfirmationSource != default || t.ConfirmationTarget != default);
}
