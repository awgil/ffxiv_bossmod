using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Savage.P12S1Athena
{
    // note: no idea how pair targets are selected, assume same role as POV
    abstract class SuperchainTheory : BossComponent
    {
        public enum Shape { Unknown, Circle, Donut, Spread, Pairs }

        public struct Chain
        {
            public Actor Origin;
            public Actor Moving;
            public Shape Shape;
            public DateTime Activation;

            public Chain(Actor origin, Actor moving, Shape shape, DateTime activation)
            {
                Origin = origin;
                Moving = moving;
                Shape = shape;
                Activation = activation;
            }
        }

        public List<Chain> Chains = new();
        public int NumCasts { get; private set; }
        private List<Actor> _pendingTethers = new(); // unfortunately, sometimes tether targets are created after tether events - recheck such tethers every frame

        private static AOEShapeCircle _shapeCircle = new(7);
        private static AOEShapeDonut _shapeDonut = new(6, 70);
        private static AOEShapeCone _shapeSpread = new(100, 15.Degrees()); // TODO: verify angle
        private static AOEShapeCone _shapePair = new(100, 20.Degrees()); // TODO: verify angle

        public IEnumerable<Chain> ImminentChains()
        {
            var threshold = Chains.FirstOrDefault().Activation.AddSeconds(1);
            return Chains.TakeWhile(c => c.Activation < threshold);
        }

        public abstract float ActivationDelay(float distance);

        public override void Update(BossModule module)
        {
            for (int i = 0; i < _pendingTethers.Count; ++i)
            {
                var source = _pendingTethers[i];
                var shape = (TetherID)source.Tether.ID switch
                {
                    TetherID.SuperchainCircle => Shape.Circle,
                    TetherID.SuperchainDonut => Shape.Donut,
                    TetherID.SuperchainSpread => Shape.Spread,
                    TetherID.SuperchainPairs => Shape.Pairs,
                    _ => Shape.Unknown
                };

                if (shape == Shape.Unknown)
                {
                    // irrelevant, remove
                    _pendingTethers.RemoveAt(i--);
                }
                else if (module.WorldState.Actors.Find(source.Tether.Target) is var origin && origin != null)
                {
                    Chains.Add(new(origin, source, shape, module.WorldState.CurrentTime.AddSeconds(ActivationDelay((source.Position - origin.Position).Length()))));
                    Chains.SortBy(c => c.Activation);
                    _pendingTethers.RemoveAt(i--);
                }
            }
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            foreach (var c in ImminentChains())
            {
                switch (c.Shape)
                {
                    case Shape.Circle:
                        if (_shapeCircle.Check(actor.Position, c.Origin))
                            hints.Add("GTFO from aoe!");
                        break;
                    case Shape.Donut:
                        if (_shapeDonut.Check(actor.Position, c.Origin))
                            hints.Add("GTFO from aoe!");
                        break;
                    case Shape.Spread:
                        hints.Add("Spread!", module.Raid.WithoutSlot().Exclude(actor).InShape(_shapeSpread, c.Origin.Position, Angle.FromDirection(actor.Position - c.Origin.Position)).Any());
                        break;
                    case Shape.Pairs:
                        bool actorIsSupport = actor.Class.IsSupport();
                        int sameRole = 0, diffRole = 0;
                        foreach (var p in module.Raid.WithoutSlot().Exclude(actor).InShape(_shapePair, c.Origin.Position, Angle.FromDirection(actor.Position - c.Origin.Position)))
                            if (p.Class.IsSupport() == actorIsSupport)
                                ++sameRole;
                            else
                                ++diffRole;
                        hints.Add("Stack in pairs!", sameRole != 0 || diffRole != 1);
                        break;
                }
            }
        }

        public override PlayerPriority CalcPriority(BossModule module, int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
        {
            return PlayerPriority.Normal;
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var c in ImminentChains())
            {
                switch (c.Shape)
                {
                    case Shape.Circle:
                        _shapeCircle.Draw(arena, c.Origin);
                        break;
                    case Shape.Donut:
                        _shapeDonut.Draw(arena, c.Origin);
                        break;
                    case Shape.Spread:
                        foreach (var p in module.Raid.WithoutSlot().Exclude(pc))
                            _shapeSpread.Draw(arena, c.Origin.Position, Angle.FromDirection(p.Position - c.Origin.Position));
                        break;
                    case Shape.Pairs:
                        bool pcIsSupport = pc.Class.IsSupport();
                        foreach (var p in module.Raid.WithoutSlot().Where(p => p != pc && p.Class.IsSupport() == pcIsSupport))
                            _shapePair.Draw(arena, c.Origin.Position, Angle.FromDirection(p.Position - c.Origin.Position));
                        break;
                }
            }
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var c in ImminentChains())
            {
                switch (c.Shape)
                {
                    case Shape.Spread:
                        _shapeSpread.Outline(arena, c.Origin.Position, Angle.FromDirection(pc.Position - c.Origin.Position));
                        break;
                    case Shape.Pairs:
                        _shapePair.Outline(arena, c.Origin.Position, Angle.FromDirection(pc.Position - c.Origin.Position), ArenaColor.Safe);
                        break;
                }
            }
        }

        public override void OnTethered(BossModule module, Actor source, ActorTetherInfo tether)
        {
            _pendingTethers.Add(source);
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            var shape = (AID)spell.Action.ID switch
            {
                AID.SuperchainBurst => Shape.Circle,
                AID.SuperchainCoil => Shape.Donut,
                AID.SuperchainRadiation => Shape.Spread,
                AID.SuperchainEmission => Shape.Pairs,
                _ => Shape.Unknown
            };
            if (shape != Shape.Unknown && Chains.FindIndex(c => c.Shape == shape && c.Origin.Position.AlmostEqual(caster.Position, 1) && c.Origin.Position.AlmostEqual(c.Moving.Position, 3)) is var index && index >= 0)
            {
                ++NumCasts;
                Chains.RemoveAt(index);
            }
        }
    }

    class SuperchainTheory1 : SuperchainTheory
    {
        public override float ActivationDelay(float distance)
        {
            return distance switch
            {
                < 7 => 10.7f, // first circle/donut + spread/pair are at distance 6
                < 16 => 12.7f, // second circle + donut are at distance 15
                < 19 => 14.6f, // third circle/donut is at distance 18
                _ => 16.6f, // fourth circle/donut is at distance 24
            };
        }
    }

    class SuperchainTheory2A : SuperchainTheory
    {
        public override float ActivationDelay(float distance)
        {
            return distance switch
            {
                < 10 => 11.9f, // first 2 circles + pairs are at distance 9
                < 18 => 14.3f, // second donut is at distance 16.5
                < 25 => 16.9f, // third circle is at distance 24
                _ => 20.3f, // fourth circle + spread/pairs are at distance 34.5
            };
        }
    }

    class SuperchainTheory2B : SuperchainTheory
    {
        public override float ActivationDelay(float distance)
        {
            return distance switch
            {
                < 10 => 11.7f, // first circle + donut are at distance 9
                < 20 => 15.1f, // second circle + spread/pairs area at distance 18
                _ => 19.6f, // third 2 circles + spread/pairs are at distance 33
            };
        }
    }
}
