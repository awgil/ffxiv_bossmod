using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Components
{
    // generic component that shows arbitrary shapes representing avoidable aoes
    public abstract class GenericAOEs : CastCounter
    {
        public struct AOEInstance
        {
            public AOEShape Shape;
            public WPos Origin;
            public Angle Rotation;
            public DateTime Activation;
            public uint Color;
            public bool Risky;

            public AOEInstance(AOEShape shape, WPos origin, Angle rotation = new(), DateTime activation = new(), uint color = ArenaColor.AOE, bool risky = true)
            {
                Shape = shape;
                Origin = origin;
                Rotation = rotation;
                Activation = activation;
                Color = color;
                Risky = risky;
            }

            public bool Check(WPos pos) => Shape.Check(pos, Origin, Rotation);
        }

        private string _warningText;

        public GenericAOEs(ActionID aid = new(), string warningText = "GTFO from aoe!") : base(aid)
        {
            _warningText = warningText;
        }

        public abstract IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor);

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (ActiveAOEs(module, slot, actor).Any(c => c.Risky && c.Check(actor.Position)))
                hints.Add(_warningText);
        }

        public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            foreach (var c in ActiveAOEs(module, slot, actor))
                if (c.Risky)
                    hints.AddForbiddenZone(c.Shape, c.Origin, c.Rotation, c.Activation);
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var c in ActiveAOEs(module, pcSlot, pc))
                c.Shape.Draw(arena, c.Origin, c.Rotation, c.Color);
        }
    }

    // self-targeted aoe that happens at the end of the cast
    public class SelfTargetedAOEs : GenericAOEs
    {
        public AOEShape Shape { get; private init; }
        public int MaxCasts { get; private init; } // used for staggered aoes, when showing all active would be pointless
        public uint Color = ArenaColor.AOE; // can be customized if needed
        public bool Risky = true; // can be customized if needed
        private List<Actor> _casters = new();
        public IReadOnlyList<Actor> Casters => _casters;
        public IEnumerable<Actor> ActiveCasters => _casters.Take(MaxCasts);

        public SelfTargetedAOEs(ActionID aid, AOEShape shape, int maxCasts = int.MaxValue) : base(aid)
        {
            Shape = shape;
            MaxCasts = maxCasts;
        }

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            return ActiveCasters.Select(c => new AOEInstance(Shape, c.Position, c.CastInfo!.Rotation, c.CastInfo.FinishAt, Color, Risky));
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

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            return ActiveCasters.Select(c => new AOEInstance(Shape, c.Position, c.Rotation, c.CastInfo!.FinishAt));
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
        public int MaxCasts { get; private init; } // used for staggered aoes, when showing all active would be pointless
        public uint Color = ArenaColor.AOE; // can be customized if needed
        public bool Risky = true; // can be customized if needed
        private List<Actor> _casters = new();
        public IReadOnlyList<Actor> Casters => _casters;
        public IEnumerable<Actor> ActiveCasters => _casters.Take(MaxCasts);

        public LocationTargetedAOEs(ActionID aid, float radius, string warningText = "GTFO from puddle!", int maxCasts = int.MaxValue) : base(aid, warningText)
        {
            Shape = new(radius);
            MaxCasts = maxCasts;
        }

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            foreach (var c in ActiveCasters)
                yield return new(Shape, c.CastInfo!.LocXZ, c.CastInfo.Rotation, c.CastInfo.FinishAt, Color, Risky);
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
        public float HalfWidth { get; private init; }
        private List<(Actor caster, AOEShape shape, Angle direction)> _casters = new();
        public IReadOnlyList<(Actor caster, AOEShape shape, Angle direction)> Casters => _casters;

        public ChargeAOEs(ActionID aid, float halfWidth) : base(aid)
        {
            HalfWidth = halfWidth;
        }

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            return Casters.Select(csr => new AOEInstance(csr.shape, csr.caster.Position, csr.direction, csr.caster.CastInfo!.FinishAt));
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
                _casters.RemoveAll(e => e.caster == caster);
        }
    }
}
