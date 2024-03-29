using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Components
{
    // voidzone (circle aoe that stays active for some time) centered at each existing object with specified OID, assumed to be persistent voidzone center
    public class PersistentVoidzone : GenericAOEs
    {
        public AOEShapeCircle Shape { get; private init; }
        public Func<BossModule, IEnumerable<Actor>> Sources { get; private init; }

        public PersistentVoidzone(float radius, Func<BossModule, IEnumerable<Actor>> sources) : base(new(), "GTFO from voidzone!")
        {
            Shape = new(radius);
            Sources = sources;
        }

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            foreach (var s in Sources(module))
                yield return new(Shape, s.Position);
        }
    }

    // voidzone that appears with some delay at cast target
    // note that if voidzone is predicted by cast start rather than cast event, we have to account for possibility of cast finishing without event (e.g. if actor dies before cast finish)
    public class PersistentVoidzoneAtCastTarget : GenericAOEs
    {
        public AOEShapeCircle Shape { get; private init; }
        public Func<BossModule, IEnumerable<Actor>> Sources { get; private init; }
        public float CastEventToSpawn { get; private init; }
        private List<(WPos pos, DateTime time)> _predictedByEvent = new();
        private List<(Actor caster, DateTime time)> _predictedByCast = new();

        public bool HaveCasters => _predictedByCast.Count > 0;

        public PersistentVoidzoneAtCastTarget(float radius, ActionID aid, Func<BossModule, IEnumerable<Actor>> sources, float castEventToSpawn) : base(aid, "GTFO from voidzone!")
        {
            Shape = new(radius);
            Sources = sources;
            CastEventToSpawn = castEventToSpawn;
        }

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            foreach (var p in _predictedByEvent)
                yield return new(Shape, p.pos, activation: p.time);
            foreach (var p in _predictedByCast)
                yield return new(Shape, module.WorldState.Actors.Find(p.caster.CastInfo!.TargetID)?.Position ?? p.caster.CastInfo.LocXZ, activation: p.time);
            foreach (var z in Sources(module))
                yield return new(Shape, z.Position);
        }

        public override void Update(BossModule module)
        {
            if (_predictedByEvent.Count > 0)
                foreach (var s in Sources(module))
                    _predictedByEvent.RemoveAll(p => p.pos.InCircle(s.Position, 3));
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (spell.Action == WatchedAction)
                _predictedByCast.Add((caster, spell.NPCFinishAt.AddSeconds(CastEventToSpawn)));
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (spell.Action == WatchedAction)
                _predictedByCast.RemoveAll(p => p.caster == caster);
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            base.OnEventCast(module, caster, spell);
            if (spell.Action == WatchedAction)
                _predictedByEvent.Add((module.WorldState.Actors.Find(spell.MainTargetID)?.Position ?? spell.TargetXZ, module.WorldState.CurrentTime.AddSeconds(CastEventToSpawn)));
        }
    }

    // voidzone (circle aoe that stays active for some time) centered at each existing object with specified OID, assumed to be persistent voidzone center
    // inverts from dangerous to safe when a specific AID is being casted
    public class PersistentInvertibleVoidzone : GenericAOEs
    {
        public AOEShapeCircle Shape { get; private init; }
        public Func<BossModule, IEnumerable<Actor>> Sources { get; private init; }
        private bool inverting;
        private DateTime _activation;
        private float Radius;

        public PersistentInvertibleVoidzone(float radius, ActionID aid, Func<BossModule, IEnumerable<Actor>> sources) : base(aid, "GTFO from voidzone!")
        {
            Shape = new(radius);
            Sources = sources;
            Radius = radius;
        }

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            if (!inverting)
                foreach (var s in Sources(module))
                    yield return new(Shape, s.Position);
            if (inverting)
                foreach (var s in Sources(module))
                    yield return new(Shape, s.Position, activation: _activation, color: ArenaColor.SafeFromAOE, risky: false);
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (spell.Action == WatchedAction)
            {
                inverting = true;
                _activation = spell.NPCFinishAt;
            }
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (spell.Action == WatchedAction)
                inverting = false;
        }

        public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            var shapes = new List<Func<WPos, float>>();
            foreach (var c in ActiveAOEs(module, slot, actor))
            {
                if (c.Risky)
                    hints.AddForbiddenZone(c.Shape, c.Origin, c.Rotation, c.Activation);
                if (inverting)
                    shapes.Add(ShapeDistance.InvertedCircle(c.Origin, Radius));
            }
            if (shapes.Count > 0)
                hints.AddForbiddenZone(p => shapes.Select(f => f(p)).Max(), _activation);
            if (shapes.Count > 0 && !inverting)
                shapes.Clear();
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            base.AddHints(module, slot, actor, hints, movementHints);
            if (ActiveAOEs(module, slot, actor).Any(c => c.Check(actor.Position)) && inverting)
                hints.Add("Wait in puddle until mechanic resolves!", false);
            if (!ActiveAOEs(module, slot, actor).Any(c => c.Check(actor.Position)) && inverting)
                hints.Add("Go into puddle until mechanic resolves!");
        }
    }
}
