using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Components
{
    // generic component that shows arbitrary shapes representing avoidable aoes
    public abstract class GenericAOEs : CastCounter
    {
        private string _warningText;

        public GenericAOEs(ActionID aid = new(), string warningText = "GTFO from aoe!") : base(aid)
        {
            _warningText = warningText;
        }

        public abstract IEnumerable<(AOEShape shape, WPos origin, Angle rotation, DateTime time)> ActiveAOEs(BossModule module, int slot, Actor actor);

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (ActiveAOEs(module, slot, actor).Any(c => c.shape.Check(actor.Position, c.origin, c.rotation)))
                hints.Add(_warningText);
        }

        public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            foreach (var c in ActiveAOEs(module, slot, actor))
                hints.AddForbiddenZone(c.shape, c.origin, c.rotation, c.time);
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var c in ActiveAOEs(module, pcSlot, pc))
                c.shape.Draw(arena, c.origin, c.rotation);
        }
    }

    // self-targeted aoe that happens at the end of the cast
    public class SelfTargetedAOEs : GenericAOEs
    {
        public AOEShape Shape { get; private init; }
        public int MaxCasts { get; private init; } // used for staggered aoes, when showing all active would be pointless
        private List<Actor> _casters = new();
        public IReadOnlyList<Actor> Casters => _casters;
        public IEnumerable<Actor> ActiveCasters => _casters.Take(MaxCasts);

        public SelfTargetedAOEs(ActionID aid, AOEShape shape, int maxCasts = int.MaxValue) : base(aid)
        {
            Shape = shape;
            MaxCasts = maxCasts;
        }

        public override IEnumerable<(AOEShape shape, WPos origin, Angle rotation, DateTime time)> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            return ActiveCasters.Select(c => (Shape, c.Position, c.CastInfo!.Rotation, c.CastInfo!.FinishAt));
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (spell.Action == WatchedAction)
                _casters.Add(caster);
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (spell.Action == WatchedAction)
                _casters.Remove(caster);
        }
    }

    // self-targeted aoe that uses current caster's rotation instead of rotation from cast-info - used by legacy modules written before i've reversed real cast rotation
    public class SelfTargetedLegacyRotationAOEs : GenericAOEs
    {
        public AOEShape Shape { get; private init; }
        public int MaxCasts { get; private init; } // used for staggered aoes, when showing all active would be pointless
        private List<Actor> _casters = new();
        public IReadOnlyList<Actor> Casters => _casters;
        public IEnumerable<Actor> ActiveCasters => _casters.Take(MaxCasts);

        public SelfTargetedLegacyRotationAOEs(ActionID aid, AOEShape shape, int maxCasts = int.MaxValue) : base(aid)
        {
            Shape = shape;
            MaxCasts = maxCasts;
        }

        public override IEnumerable<(AOEShape shape, WPos origin, Angle rotation, DateTime time)> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            return ActiveCasters.Select(c => (Shape, c.Position, c.Rotation, c.CastInfo!.FinishAt));
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (spell.Action == WatchedAction)
                _casters.Add(caster);
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (spell.Action == WatchedAction)
                _casters.Remove(caster);
        }
    }

    // location-targeted aoe that happens at the end of the cast
    public class LocationTargetedAOEs : GenericAOEs
    {
        public AOEShapeCircle Shape { get; private init; }
        private List<Actor> _casters = new();
        public IReadOnlyList<Actor> Casters => _casters;

        public LocationTargetedAOEs(ActionID aid, float radius) : base(aid, "GTFO from puddle!")
        {
            Shape = new(radius);
        }

        public override IEnumerable<(AOEShape shape, WPos origin, Angle rotation, DateTime time)> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            foreach (var c in _casters)
                yield return (Shape, c.CastInfo!.LocXZ, new(), c.CastInfo!.FinishAt);
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (spell.Action == WatchedAction)
                _casters.Add(caster);
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (spell.Action == WatchedAction)
                _casters.Remove(caster);
        }
    }

    // 'charge at location' aoes that happen at the end of the cast
    public class ChargeAOEs : GenericAOEs
    {
        public int HalfWidth { get; private init; }
        private List<(Actor caster, AOEShape shape, Angle direction)> _casters = new();
        public IReadOnlyList<(Actor caster, AOEShape shape, Angle direction)> Casters => _casters;

        public ChargeAOEs(ActionID aid, int halfWidth) : base(aid)
        {
            HalfWidth = halfWidth;
        }

        public override IEnumerable<(AOEShape shape, WPos origin, Angle rotation, DateTime time)> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            return Casters.Select(csr => (csr.shape, csr.caster.Position, csr.direction, csr.caster.CastInfo!.FinishAt));
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (spell.Action == WatchedAction)
            {
                var dir = spell.LocXZ - caster.Position;
                _casters.Add((caster, new AOEShapeRect(dir.Length(), HalfWidth), Angle.FromDirection(dir)));
            }
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (spell.Action == WatchedAction)
                _casters.RemoveAll(e => e.Item1 == caster);
        }
    }
}
