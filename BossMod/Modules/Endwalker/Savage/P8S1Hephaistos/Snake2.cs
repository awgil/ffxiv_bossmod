using System;
using System.Collections.Generic;

namespace BossMod.Endwalker.Savage.P8S1Hephaistos
{
    class Gorgospit : Components.GenericAOEs
    {
        private List<(Actor caster, DateTime finish)> _casters = new();

        private static AOEShapeRect _shape = new(60, 5);

        public Gorgospit() : base(ActionID.MakeSpell(AID.Gorgospit)) { }

        public override IEnumerable<(AOEShape shape, WPos origin, Angle rotation, DateTime time)> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            foreach (var c in _casters)
            {
                if (c.caster.CastInfo == null)
                    yield return (_shape, c.caster.Position, c.caster.Rotation, c.finish);
                else
                    yield return (_shape, c.caster.Position, c.caster.CastInfo.Rotation, c.caster.CastInfo.FinishAt);
            }
        }

        public override void OnActorPlayActionTimelineEvent(BossModule module, Actor actor, ushort id)
        {
            if ((OID)actor.OID == OID.IllusoryHephaistosSnakes && id == 0x11D2)
                _casters.Add((actor, module.WorldState.CurrentTime.AddSeconds(8)));
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (spell.Action == WatchedAction)
                _casters.RemoveAll(c => c.caster == caster);
        }
    }

    // TODO: add various hints for gaze/explode
    // TODO: show circle around assigned snake
    class Snake2 : PetrifactionCommon
    {
        private BitMask _firstPetrify;
        private BitMask _debuffCrown;
        private BitMask _debuffBreath;

        private const float _breathRadius = 6;

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            base.AddHints(module, slot, actor, hints, movementHints);
            hints.Add($"Petrify order: {(_firstPetrify[slot] ? 1 : 2)}, {(_debuffCrown[slot] ? "hide behind snake" : "stack between snakes")}", false);
        }

        public override PlayerPriority CalcPriority(BossModule module, int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
        {
            return NumCrownCasts == 0 && _debuffCrown[playerSlot] ? PlayerPriority.Danger : PlayerPriority.Irrelevant;
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            base.DrawArenaForeground(module, pcSlot, pc, arena);

            if (NumEyeCasts < 8)
            {
                if (_firstPetrify[pcSlot] == NumEyeCasts < 4)
                    DrawPetrify(pc, NumCasts == 0, arena);
                else
                    DrawExplode(pc, NumCasts == 0, arena);
            }
            else if (NumBreathCasts == 0)
            {
                foreach (var p in module.Raid.WithSlot().IncludedInMask(_debuffBreath))
                    arena.AddCircle(p.Item2.Position, _breathRadius, ArenaColor.Safe);
            }
        }

        public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
        {
            switch ((SID)status.ID)
            {
                case SID.EyeOfTheGorgon:
                    if ((status.ExpireAt - module.WorldState.CurrentTime).TotalSeconds < 25)
                        _firstPetrify.Set(module.Raid.FindSlot(actor.InstanceID));
                    break;
                case SID.CrownOfTheGorgon:
                    _debuffCrown.Set(module.Raid.FindSlot(actor.InstanceID));
                    break;
                case SID.BreathOfTheGorgon:
                    _debuffBreath.Set(module.Raid.FindSlot(actor.InstanceID));
                    break;
            }
        }
    }
}
