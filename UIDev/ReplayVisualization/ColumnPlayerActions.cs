using BossMod;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UIDev
{
    public class ColumnPlayerActions : Timeline.ColumnGroup
    {
        private struct CooldownGroup
        {
            public GenericHistoryColumn? Column;
            public DateTime Cursor;
            public DateTime ChargeCooldownEnd;
            public int ChargesOnCooldown;
            public float ChargeCooldown;
            public ActionID CooldownAction;
        }

        private GenericHistoryColumn _autoAttacks;
        private GenericHistoryColumn _animLocks;
        private CooldownGroup[] _cdGroups = new CooldownGroup[80];

        public ColumnPlayerActions(Timeline timeline, StateMachineTree tree, List<int> phaseBranches, Replay replay, Replay.Encounter enc, Replay.Participant player, Class playerClass)
            : base(timeline)
        {
            _autoAttacks = Add<GenericHistoryColumn>(new(timeline, tree, phaseBranches));
            _animLocks = Add<GenericHistoryColumn>(new(timeline, tree, phaseBranches));
            GetCooldownColumn(CommonDefinitions.GCDGroup); // make sure GCD column always exists and is before any others

            var classDef = AbilityDefinitions.Classes.GetValueOrDefault(playerClass);
            int iCast = 0;
            foreach (var a in replay.EncounterActions(enc).Where(a => a.Source == player))
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

                var actionDef = classDef?.Abilities.GetValueOrDefault(a.ID)?.Definition;
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
                        GetCooldownColumn(actionDef.CooldownGroup).AddHistoryEntryRange(enc.Time.Start, cast.Time, castName, 0x80ffffff).AddCastTooltip(cast);
                        AdvanceCooldown(actionDef.CooldownGroup, enc.Time.Start, cast.Time.End, false);
                    }
                    effectStart = cast.Time.End;
                }
                else
                {
                    if (actionDef != null)
                    {
                        StartCooldown(a.ID, actionDef, enc.Time.Start, a.Timestamp);
                    }
                    effectStart = a.Timestamp;
                }

                AddAnimationLock(_animLocks, a, enc.Time.Start, effectStart, actionName);
                _animLocks.AddHistoryEntryDot(enc.Time.Start, a.Timestamp, actionName, actionDef != null ? 0xffffffff : 0xff0000ff).AddActionTooltip(a);

                if (actionDef != null)
                {
                    var col = GetCooldownColumn(actionDef.CooldownGroup);
                    // TODO: effect should be extended by action-to-effectresult delay?..
                    if (actionDef.EffectDuration > 0)
                    {
                        col.AddHistoryEntryRange(enc.Time.Start, effectStart, actionDef.EffectDuration, actionName, 0x8000ff00).TooltipExtra.Add($"- effect: {actionDef.EffectDuration:f1}s");
                        AdvanceCooldown(actionDef.CooldownGroup, enc.Time.Start, effectStart.AddSeconds(actionDef.EffectDuration), false);
                    }
                    // TODO: support cooldown reduction effects
                    col.AddHistoryEntryDot(enc.Time.Start, a.Timestamp, actionName, 0xffffffff).AddActionTooltip(a);
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

        }

        private void AddUnfinishedCast(Replay.Cast cast, DateTime encStart, AbilityDefinitions.Class? classDef)
        {
            var name = $"[unfinished] {cast.ID} -> {ReplayUtils.ParticipantString(cast.Target)}";
            _animLocks.AddHistoryEntryRange(encStart, cast.Time, name, 0x800000ff).AddCastTooltip(cast);

            var castActionDef = classDef?.Abilities.GetValueOrDefault(cast.ID)?.Definition;
            if (castActionDef != null)
            {
                AdvanceCooldown(castActionDef.CooldownGroup, encStart, cast.Time.Start, true);
                GetCooldownColumn(castActionDef.CooldownGroup).AddHistoryEntryRange(encStart, cast.Time, name, 0x800000ff).AddCastTooltip(cast);
                AdvanceCooldown(castActionDef.CooldownGroup, encStart, cast.Time.Start, false); // consider cooldown reset instead?..
            }
        }

        private void AddAnimationLock(GenericHistoryColumn col, Replay.Action action, DateTime encStart, DateTime lockStart, string name)
        {
            if (action.AnimationLock > 0)
                col.AddHistoryEntryRange(encStart, lockStart, action.AnimationLock, name, 0x80808080).TooltipExtra.Add($"- anim lock: {action.AnimationLock:f2}");
        }

        private GenericHistoryColumn GetCooldownColumn(int cooldownGroup)
        {
            var col = _cdGroups[cooldownGroup].Column;
            if (col == null)
                col = _cdGroups[cooldownGroup].Column = Add<GenericHistoryColumn>(new(Timeline, _autoAttacks.Tree, _autoAttacks.PhaseBranches));
            return col;
        }

        private void AdvanceCooldown(int cdGroup, DateTime encStart, DateTime timestamp, bool addRanges)
        {
            var col = GetCooldownColumn(cdGroup); // ensure column exists
            ref var data = ref _cdGroups[cdGroup];

            while (data.ChargesOnCooldown > 0 && timestamp >= data.ChargeCooldownEnd)
            {
                // next charge is fully finished
                if (addRanges)
                    col.AddHistoryEntryRange(encStart, data.Cursor, data.ChargeCooldownEnd, data.CooldownAction.ToString(), 0x80808080).TooltipExtra.Add($"- cooldown: {data.ChargeCooldown:f1}s ({data.ChargesOnCooldown} charges) ");
                data.Cursor = data.ChargeCooldownEnd;
                if (--data.ChargesOnCooldown > 0)
                {
                    col.AddHistoryEntryLine(encStart, data.Cursor, "", 0xffffffff);
                    data.ChargeCooldownEnd = data.Cursor.AddSeconds(data.ChargeCooldown);
                }
            }

            // add partially finished cooldown: we're either just spending another charge, or we're starting new cooldown slightly earlier than expected (due to high sks for gcd, or due to client-server timing differences)
            if (data.ChargesOnCooldown > 0)
            {
                // assertion: timestamp < data.ChargeCooldownEnd
                if (addRanges)
                    col.AddHistoryEntryRange(encStart, data.Cursor, timestamp, data.CooldownAction.ToString(), 0x80808080).TooltipExtra.Add($"- cooldown: {data.ChargeCooldown:f1}s ({data.ChargesOnCooldown} charges) ");
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
    }
}
