using BossMod;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UIDev
{
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

        private ColumnGenericHistory _autoAttacks;
        private ColumnGenericHistory _animLocks;
        private CooldownGroup[] _cdGroups = new CooldownGroup[80];
        private ColumnSeparator _sep;
        private Dictionary<ActionID, (int group, float cd)> _cooldownReductions = new();

        public ColumnPlayerActions(Timeline timeline, StateMachineTree tree, List<int> phaseBranches, Replay replay, Replay.Encounter enc, Replay.Participant player, Class playerClass)
            : base(timeline)
        {
            _autoAttacks = Add<ColumnGenericHistory>(new(timeline, tree, phaseBranches, "Auto attacks"));
            _animLocks = Add<ColumnGenericHistory>(new(timeline, tree, phaseBranches, "Abilities with animation locks"));
            _sep = Add(new ColumnSeparator(timeline));
            GetCooldownColumn(CommonDefinitions.GCDGroup, new()).Name = "GCD"; // make sure GCD column always exists and is before any others
            SetupClass(playerClass);

            var classDef = PlanDefinitions.Classes.GetValueOrDefault(playerClass);
            int iCast = 0;
            var minTime = enc.Time.Start.AddSeconds(timeline.MinTime);
            foreach (var a in replay.Actions.SkipWhile(a => a.Timestamp < minTime).TakeWhile(a => a.Timestamp <= enc.Time.End).Where(a => a.Source == player))
            {
                // note: we assume autoattacks are never casted... in fact, I think only GCDs can be casted
                var actionName = $"{a.ID} -> {ReplayUtils.ParticipantString(a.MainTarget)} #{a.GlobalSequence}";
                if (a.ID == CommonDefinitions.IDAutoAttack || a.ID == CommonDefinitions.IDAutoShot)
                {
                    AddAnimationLock(_autoAttacks, a, enc.Time.Start, a.Timestamp, actionName);
                    _autoAttacks.AddHistoryEntryDot(enc.Time.Start, a.Timestamp, actionName, 0xffc0c0c0).AddActionTooltip(a);
                    continue;
                }

                while (iCast < player.Casts.Count && player.Casts[iCast].Time.End < a.Timestamp)
                {
                    // add cast without event (interrupted, cancelled, whatever)
                    AddUnfinishedCast(player.Casts[iCast++], enc.Time.Start, classDef);
                }

                var actionDef = classDef?.Abilities.GetValueOrDefault(a.ID);
                DateTime effectStart;
                if (iCast < player.Casts.Count && player.Casts[iCast].Time.Start < a.Timestamp && player.Casts[iCast].ID == a.ID)
                {
                    // casted action
                    var cast = player.Casts[iCast++];
                    var castName = $"{cast.ID} -> {ReplayUtils.ParticipantString(cast.Target)}";
                    _animLocks.AddHistoryEntryRange(enc.Time.Start, cast.Time, castName, 0x80ffffff).AddCastTooltip(cast);
                    if (actionDef != null)
                    {
                        StartCooldown(a.ID, actionDef, enc.Time.Start, cast.Time.Start);
                        GetCooldownColumn(actionDef.CooldownGroup, a.ID).AddHistoryEntryRange(enc.Time.Start, cast.Time, castName, 0x80ffffff).AddCastTooltip(cast);
                        AdvanceCooldown(actionDef.CooldownGroup, enc.Time.Start, cast.Time.End, false);
                    }
                    effectStart = cast.Time.End;
                }
                else
                {
                    if (actionDef != null)
                    {
                        StartCooldown(a.ID, actionDef, enc.Time.Start, a.ClientAction != null ? a.ClientAction.Requested : a.Timestamp);
                    }
                    effectStart = a.ClientAction != null ? a.ClientAction.Requested : a.Timestamp; // TODO: different depending on config...
                }

                AddAnimationLock(_animLocks, a, enc.Time.Start, effectStart, actionName);
                _animLocks.AddHistoryEntryDot(enc.Time.Start, a.Timestamp, actionName, actionDef != null ? (ActionConfirmed(a) ? 0xffffffff : 0xff00ffff) : 0xff0000ff).AddActionTooltip(a);

                if (actionDef != null)
                {
                    var col = GetCooldownColumn(actionDef.CooldownGroup, a.ID);
                    // TODO: effect should be extended by action-to-effectresult delay?..
                    if (actionDef.EffectDuration > 0)
                    {
                        col.AddHistoryEntryRange(enc.Time.Start, effectStart, actionDef.EffectDuration, actionName, 0x8000ff00).TooltipExtra.Add($"- effect: {actionDef.EffectDuration:f1}s");
                        AdvanceCooldown(actionDef.CooldownGroup, enc.Time.Start, effectStart.AddSeconds(actionDef.EffectDuration), false);
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
                AddUnfinishedCast(player.Casts[iCast++], enc.Time.Start, classDef);
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
                    GetCooldownColumn((int)BossMod.WAR.CDGroup.Infuriate, ActionID.MakeSpell(BossMod.WAR.AID.Infuriate));
                    GetCooldownColumn((int)BossMod.WAR.CDGroup.InnerRelease, ActionID.MakeSpell(BossMod.WAR.AID.InnerRelease));
                    GetCooldownColumn((int)BossMod.WAR.CDGroup.Upheaval, ActionID.MakeSpell(BossMod.WAR.AID.Upheaval));
                    GetCooldownColumn((int)BossMod.WAR.CDGroup.Onslaught, ActionID.MakeSpell(BossMod.WAR.AID.Onslaught));
                    // infuriate cooldown reductions
                    _cooldownReductions[ActionID.MakeSpell(BossMod.WAR.AID.InnerBeast)] = ((int)BossMod.WAR.CDGroup.Infuriate, 5);
                    _cooldownReductions[ActionID.MakeSpell(BossMod.WAR.AID.FellCleave)] = ((int)BossMod.WAR.CDGroup.Infuriate, 5);
                    _cooldownReductions[ActionID.MakeSpell(BossMod.WAR.AID.InnerChaos)] = ((int)BossMod.WAR.CDGroup.Infuriate, 5);
                    _cooldownReductions[ActionID.MakeSpell(BossMod.WAR.AID.SteelCyclone)] = ((int)BossMod.WAR.CDGroup.Infuriate, 5);
                    _cooldownReductions[ActionID.MakeSpell(BossMod.WAR.AID.Decimate)] = ((int)BossMod.WAR.CDGroup.Infuriate, 5);
                    _cooldownReductions[ActionID.MakeSpell(BossMod.WAR.AID.ChaoticCyclone)] = ((int)BossMod.WAR.CDGroup.Infuriate, 5);
                    break;
            }
        }

        private void AddUnfinishedCast(Replay.Cast cast, DateTime encStart, PlanDefinitions.ClassData? classDef)
        {
            var name = $"[unfinished] {cast.ID} -> {ReplayUtils.ParticipantString(cast.Target)}";
            _animLocks.AddHistoryEntryRange(encStart, cast.Time, name, 0x800000ff).AddCastTooltip(cast);

            var castActionDef = classDef?.Abilities.GetValueOrDefault(cast.ID);
            if (castActionDef != null)
            {
                AdvanceCooldown(castActionDef.CooldownGroup, encStart, cast.Time.Start, true);
                GetCooldownColumn(castActionDef.CooldownGroup, cast.ID).AddHistoryEntryRange(encStart, cast.Time, name, 0x800000ff).AddCastTooltip(cast);
                AdvanceCooldown(castActionDef.CooldownGroup, encStart, cast.Time.End, false); // consider cooldown reset instead?..
            }
        }

        private void AddAnimationLock(ColumnGenericHistory col, Replay.Action action, DateTime encStart, DateTime lockStart, string name)
        {
            if (action.AnimationLock > 0)
                col.AddHistoryEntryRange(encStart, lockStart, action.AnimationLock, name, 0x80808080).TooltipExtra.Add($"- anim lock: {action.AnimationLock:f2}");
        }

        private ColumnGenericHistory GetCooldownColumn(int cooldownGroup, ActionID defaultAction)
        {
            var col = _cdGroups[cooldownGroup].Column;
            if (col == null)
                col = _cdGroups[cooldownGroup].Column = AddBefore<ColumnGenericHistory>(new(Timeline, _autoAttacks.Tree, _autoAttacks.PhaseBranches, defaultAction.ToString()), _sep);
            return col;
        }

        private void AddCooldownRange(ref CooldownGroup data, DateTime encStart, DateTime rangeEnd)
        {
            if (data.Column == null)
                return;
            var width = data.MaxCharges > 0 ? (float)data.ChargesOnCooldown / data.MaxCharges : 1;
            var e = data.Column.AddHistoryEntryRange(encStart, data.Cursor, rangeEnd, data.CooldownAction.ToString(), 0x80808080, width);
            e.TooltipExtra.Add($"- charges remaining: {data.MaxCharges - data.ChargesOnCooldown}/{data.MaxCharges}");
            e.TooltipExtra.Add($"- start CD: {(data.ChargeCooldownEnd - data.Cursor).TotalSeconds:f3}s");
            e.TooltipExtra.Add($"- end CD: {(data.ChargeCooldownEnd - rangeEnd).TotalSeconds:f3}s");
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
            AdvanceCooldown(actionDef.CooldownGroup, encStart, timestamp, true);
            ref var data = ref _cdGroups[actionDef.CooldownGroup];
            if (data.ChargesOnCooldown == 0)
            {
                // off cd -> cd
                data.ChargeCooldownEnd = data.Cursor.AddSeconds(actionDef.Cooldown);
                data.ChargesOnCooldown = 1;
                data.MaxCharges = actionDef.MaxChargesAtCap;
            }
            else if (data.ChargesOnCooldown < actionDef.MaxChargesAtCap)
            {
                // just spend new charge
                ++data.ChargesOnCooldown;
            }
            else
            {
                // already at max charges, assume previous cooldown is slightly smaller than expected
                data.Column!.AddHistoryEntryLine(encStart, timestamp, aid.ToString(), 0xffffffff).TooltipExtra.Add($"- cooldown {(data.ChargeCooldownEnd - data.Cursor).TotalSeconds:f1}s smaller than expected");
                data.ChargeCooldownEnd = data.Cursor.AddSeconds(actionDef.Cooldown);
            }
            data.ChargeCooldown = actionDef.Cooldown;
            data.CooldownAction = aid;
        }

        private bool ActionConfirmed(Replay.Action a) => a.Targets.Count == 0 || a.Targets.Any(t => t.ConfirmationSource != default || t.ConfirmationTarget != default);
    }
}
