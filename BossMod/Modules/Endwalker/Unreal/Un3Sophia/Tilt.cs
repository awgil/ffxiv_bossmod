using System;
using System.Collections.Generic;

namespace BossMod.Endwalker.Unreal.Un3Sophia
{
    class Tilt : Components.Knockback
    {
        public static float DistanceShort = 28;
        public static float DistanceLong = 37;

        public float Distance;
        public Angle Direction;
        public DateTime Activation;

        public Tilt() : base(ActionID.MakeSpell(AID.QuasarTilt)) { }

        public override IEnumerable<Source> Sources(BossModule module, int slot, Actor actor)
        {
            if (Distance > 0)
                yield return new(new(), Distance, Activation, null, Direction, Kind.DirForward);
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            base.OnEventCast(module, caster, spell);
            if (spell.Action == WatchedAction)
                Distance = 0;
        }
    }

    class ScalesOfWisdom : Tilt
    {
        public bool RaidwideDone;

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            base.OnEventCast(module, caster, spell);
            switch ((AID)spell.Action.ID)
            {
                case AID.ScalesOfWisdomStart:
                    // prepare for first tilt
                    Distance = DistanceShort;
                    Direction = -90.Degrees();
                    Activation = module.WorldState.CurrentTime.AddSeconds(8);
                    break;
                case AID.QuasarTilt:
                    if (NumCasts == 1)
                    {
                        // prepare for second tilt
                        Distance = DistanceShort;
                        Direction = 90.Degrees();
                        Activation = module.WorldState.CurrentTime.AddSeconds(4.9f);
                    }
                    break;
                case AID.ScalesOfWisdomRaidwide:
                    RaidwideDone = true;
                    break;
            }
        }
    }

    class Quasar : Tilt
    {
        public int WeightLeft;
        public int WeightRight;

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            var weight = (AID)spell.Action.ID switch
            {
                AID.QuasarLight => 1,
                AID.QuasarHeavy => 3,
                _ => 0
            };
            if (weight != 0)
            {
                if (caster.Position.X < 0)
                    WeightLeft += weight;
                else
                    WeightRight += weight;

                Distance = (WeightLeft - WeightRight) switch
                {
                    0 => 0,
                    1 or -1 => DistanceShort,
                    _ => DistanceLong
                };
                Direction = (WeightLeft > WeightRight ? -90 : 90).Degrees();
                Activation = spell.FinishAt.AddSeconds(0.7f);
            }
        }
    }
}
