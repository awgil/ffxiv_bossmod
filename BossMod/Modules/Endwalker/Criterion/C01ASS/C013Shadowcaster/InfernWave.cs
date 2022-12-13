using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Criterion.C01ASS.C013Shadowcaster
{
    class InfernWave : Components.CastCounter
    {
        private class Beacon
        {
            public Actor Source;
            public DateTime Activation;
            public List<(Actor target, Angle dir)> Targets = new();

            public Beacon(Actor source, DateTime activation)
            {
                Source = source;
                Activation = activation;
            }
        }

        public bool ShowHints = true;
        private int _aoesPerBeacon;
        private int _maxActive;
        private List<Beacon> _beacons = new();

        private static AOEShapeCone _shape = new(60, 45.Degrees());

        public InfernWave(int aoesPerBeacon, int maxActive) : base(ActionID.MakeSpell(AID.InfernWaveAOE))
        {
            _aoesPerBeacon = aoesPerBeacon;
            _maxActive = maxActive;
        }

        public override void Update(BossModule module)
        {
            // create entries for newly activated beacons
            foreach (var s in module.Enemies(OID.Beacon).Where(s => s.ModelState.AnimState1 == 1 && !_beacons.Any(b => b.Source == s)))
            {
                _beacons.Add(new(s, module.WorldState.CurrentTime.AddSeconds(17.1f)));
            }

            // update beacon targets
            if (ShowHints)
            {
                foreach (var b in ActiveBeacons())
                {
                    b.Targets.Clear();
                    foreach (var t in module.Raid.WithoutSlot().SortedByRange(b.Source.Position).Take(_aoesPerBeacon))
                        b.Targets.Add((t, Angle.FromDirection(t.Position - b.Source.Position)));
                }
            }
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (!ShowHints)
                return;

            bool clipping = false, clipped = false;
            int numBaits = 0;
            foreach (var b in ActiveBeacons())
            {
                foreach (var t in b.Targets)
                {
                    if (t.target == actor)
                    {
                        ++numBaits;
                        clipping |= module.Raid.WithoutSlot().Exclude(actor).InShape(_shape, b.Source.Position, t.dir).Any();
                    }
                    else
                    {
                        clipped |= _shape.Check(actor.Position, b.Source.Position, t.dir);
                    }
                }
            }

            if (numBaits > 1)
                hints.Add("Baiting mulitple cones!");
            if (clipping)
                hints.Add("GTFO from raid!");
            if (clipped)
                hints.Add("GTFO from other bait!");
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (!ShowHints)
                return;

            foreach (var b in ActiveBeacons())
                foreach (var t in b.Targets)
                    _shape.Draw(arena, b.Source.Position, t.dir);
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var b in ActiveBeacons())
                arena.Actor(b.Source, ArenaColor.Object, true);
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            base.OnEventCast(module, caster, spell);
            if (spell.Action == WatchedAction)
            {
                var beacon = _beacons.Find(b => b.Source.Position.AlmostEqual(caster.Position, 1));
                if (beacon != null)
                    beacon.Activation = new();
            }
        }

        private IEnumerable<Beacon> ActiveBeacons() => _beacons.Where(b => b.Activation != new DateTime()).Take(_maxActive);
    }

    class InfernWave1 : InfernWave
    {
        public InfernWave1() : base(2, 2)
        {
            ShowHints = false;
        }
    }

    class InfernWave2 : InfernWave
    {
        public InfernWave2() : base(2, 1) { }
    }
}
