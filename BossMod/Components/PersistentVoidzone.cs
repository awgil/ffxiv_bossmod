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

        public override IEnumerable<(AOEShape shape, WPos origin, Angle rotation, DateTime time)> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            foreach (var s in Sources(module))
                yield return (Shape, s.Position, new(), new());
        }
    }

    // voidzone that appears with some delay at cast target
    public class PersistentVoidzoneAtCastTarget : GenericAOEs
    {
        public AOEShapeCircle Shape { get; private init; }
        public Func<BossModule, IEnumerable<Actor>> Sources { get; private init; }
        public float SpawnDelay { get; private init; }
        public bool PredictFromCastStart { get; private init; } // if true, predicted spawn is defined by cast-start rather than cast-event
        private List<(WPos pos, DateTime time)> _predictedSpawns = new();

        public PersistentVoidzoneAtCastTarget(float radius, ActionID aid, Func<BossModule, IEnumerable<Actor>> sources, float spawnDelay, bool predictFromCastStart) : base(aid, "GTFO from voidzone!")
        {
            Shape = new(radius);
            Sources = sources;
            SpawnDelay = spawnDelay;
            PredictFromCastStart = predictFromCastStart;
        }

        public override IEnumerable<(AOEShape shape, WPos origin, Angle rotation, DateTime time)> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            foreach (var p in _predictedSpawns)
                yield return (Shape, p.pos, new(), p.time);
            foreach (var z in Sources(module))
                yield return (Shape, z.Position, new(), new());
        }

        public override void Update(BossModule module)
        {
            if (_predictedSpawns.Count > 0)
                foreach (var s in Sources(module))
                    _predictedSpawns.RemoveAll(p => p.pos.InCircle(s.Position, 2));
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (PredictFromCastStart && spell.Action == WatchedAction)
                _predictedSpawns.Add((spell.LocXZ, module.WorldState.CurrentTime.AddSeconds(SpawnDelay)));
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            base.OnEventCast(module, caster, spell);
            if (!PredictFromCastStart && spell.Action == WatchedAction)
                _predictedSpawns.Add((spell.TargetXZ, module.WorldState.CurrentTime.AddSeconds(SpawnDelay)));
        }
    }
}
