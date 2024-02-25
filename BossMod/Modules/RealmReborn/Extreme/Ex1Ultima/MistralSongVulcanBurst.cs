using System;
using System.Collections.Generic;

namespace BossMod.RealmReborn.Extreme.Ex1Ultima
{
    class MistralSongVulcanBurst : Components.GenericAOEs
    {
        public bool Active { get; private set; }
        private Actor? _garuda; // non-null while mechanic is active
        private DateTime _resolve;
        private bool _burstImminent;
        private static AOEShapeCone _shape = new(23.4f, 75.Degrees());

        public MistralSongVulcanBurst() : base(ActionID.MakeSpell(AID.MistralSong)) { }

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            if (Active)
                yield return new(_shape, _garuda!.Position, _garuda!.Rotation, _resolve);
        }

        public override void Update(BossModule module)
        {
            Active = _garuda != null && (_resolve - module.WorldState.CurrentTime).TotalSeconds <= 6; // note that garuda continues rotating for next ~0.5s
        }

        public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            if (!Active)
                return;

            // we have custom shape before burst - we try to make it so that post-knockback position is safe
            if (_burstImminent)
            {
                var p1 = module.Bounds.Center + module.Bounds.HalfSize * (_garuda!.Rotation + _shape.HalfAngle).ToDirection();
                var p2 = module.Bounds.Center + module.Bounds.HalfSize * (_garuda!.Rotation - _shape.HalfAngle).ToDirection();
                var a1 = Angle.FromDirection(p1 - module.PrimaryActor.Position);
                var a2 = Angle.FromDirection(p2 - module.PrimaryActor.Position);
                if (a2.Rad > a1.Rad)
                    a2 -= 360.Degrees();
                hints.AddForbiddenZone(ShapeDistance.Cone(module.PrimaryActor.Position, 40, (a1 + a2) / 2, (a1 - a2) / 2), _resolve);
            }
            else
            {
                hints.AddForbiddenZone(_shape.Distance(_garuda!.Position, _garuda.Rotation), _resolve);
            }
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            base.DrawArenaForeground(module, pcSlot, pc, arena);

            var adjPos = _burstImminent ? arena.Bounds.ClampToBounds(Components.Knockback.AwayFromSource(pc.Position, module.PrimaryActor, 30)) : pc.Position;
            Components.Knockback.DrawKnockback(pc, adjPos, arena);
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (spell.Action == WatchedAction)
            {
                _garuda = caster;
                _resolve = spell.NPCFinishAt;
                _burstImminent = true;
            }
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (spell.Action == WatchedAction)
                _garuda = null;
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            base.OnEventCast(module, caster, spell);
            if ((AID)spell.Action.ID == AID.VulcanBurst)
                _burstImminent = false;
        }

        public override void OnActorPlayActionTimelineEvent(BossModule module, Actor actor, ushort id)
        {
            if ((OID)actor.OID == OID.UltimaGaruda && id == 0x0588)
            {
                _garuda = actor;
                _resolve = module.WorldState.CurrentTime.AddSeconds(6.6f);
                _burstImminent = true;
            }
        }
    }
}
