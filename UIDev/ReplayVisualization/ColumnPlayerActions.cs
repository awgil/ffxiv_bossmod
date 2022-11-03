using BossMod;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UIDev
{
    public class ColumnPlayerActions : Timeline.ColumnGroup
    {
        private GenericHistoryColumn _autoAttacks;
        private GenericHistoryColumn _animLocks;
        private Dictionary<int, GenericHistoryColumn> _cdGroups = new();

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
                DateTime nextBarFrom;
                float cdDelta = 0; // cooldown starts from cast start rather than cast end
                if (iCast < player.Casts.Count && player.Casts[iCast].Time.Start < a.Timestamp && player.Casts[iCast].ID == a.ID)
                {
                    // casted action
                    var cast = player.Casts[iCast++];
                    nextBarFrom = cast.Time.End;
                    cdDelta = cast.Time.Duration;

                    var castName = $"{cast.ID} -> {ReplayUtils.ParticipantString(cast.Target)}";
                    _animLocks.AddHistoryEntryRange(enc.Time.Start, cast.Time, castName, 0x80ffffff).AddCastTooltip(cast);
                    if (actionDef != null)
                        GetCooldownColumn(actionDef.CooldownGroup).AddHistoryEntryRange(enc.Time.Start, cast.Time, castName, 0x80ffffff).AddCastTooltip(cast);
                }
                else
                {
                    nextBarFrom = a.Timestamp;
                }

                AddAnimationLock(_animLocks, a, enc.Time.Start, nextBarFrom, actionName);
                _animLocks.AddHistoryEntryDot(enc.Time.Start, a.Timestamp, actionName, actionDef != null ? 0xffffffff : 0xff0000ff).AddActionTooltip(a);

                if (actionDef != null)
                {
                    var col = GetCooldownColumn(actionDef.CooldownGroup);
                    // TODO: effect should be extended by action-to-effectresult delay?..
                    if (actionDef.EffectDuration > 0)
                    {
                        col.AddHistoryEntryRange(enc.Time.Start, nextBarFrom, actionDef.EffectDuration, actionName, 0x8000ff00).TooltipExtra.Add($"- effect: {actionDef.EffectDuration:f1}s");
                        nextBarFrom = nextBarFrom.AddSeconds(actionDef.EffectDuration);
                        cdDelta += actionDef.EffectDuration;
                    }
                    // TODO: support abilities with charges
                    // TODO: support cooldown reduction effects
                    if (actionDef.Cooldown > cdDelta)
                    {
                        col.AddHistoryEntryRange(enc.Time.Start, nextBarFrom, actionDef.Cooldown - cdDelta, actionName, 0x80808080).TooltipExtra.Add($"- cooldown: {actionDef.Cooldown:f1}s");
                    }
                    col.AddHistoryEntryDot(enc.Time.Start, a.Timestamp, actionName, 0xffffffff).AddActionTooltip(a);
                }
            }

            // add remaining unfinished casts
            while (iCast < player.Casts.Count)
            {
                AddUnfinishedCast(player.Casts[iCast++], enc.Time.Start, classDef);
            }
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
                GetCooldownColumn(castActionDef.CooldownGroup).AddHistoryEntryRange(encStart, cast.Time, name, 0x800000ff).AddCastTooltip(cast);
        }

        private void AddAnimationLock(GenericHistoryColumn col, Replay.Action action, DateTime encStart, DateTime lockStart, string name)
        {
            if (action.AnimationLock > 0)
                col.AddHistoryEntryRange(encStart, lockStart, action.AnimationLock, name, 0x80808080).TooltipExtra.Add($"- anim lock: {action.AnimationLock:f2}");
        }

        private GenericHistoryColumn GetCooldownColumn(int cooldownGroup)
        {
            var col = _cdGroups.GetValueOrDefault(cooldownGroup);
            if (col == null)
                col = _cdGroups[cooldownGroup] = Add<GenericHistoryColumn>(new(Timeline, _autoAttacks.Tree, _autoAttacks.PhaseBranches));
            return col;
        }
    }
}
