using System;
using System.Collections.Generic;

namespace BossMod.Endwalker.Extreme.Ex5Rubicante
{
    class Welts : Components.StackSpread
    {
        public enum Mechanic { StackFlare, Spreads, Done }

        public Mechanic NextMechanic;

        public Welts() : base(6, 15, 4, 4, true) { } // TODO: verify flare falloff

        public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
        {
            switch ((SID)status.ID)
            {
                case SID.BloomingWelt:
                    SpreadTargets.Add(actor);
                    break;
                case SID.FuriousWelt:
                    StackTargets.Add(actor);
                    break;
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.FuriousWelt:
                    NextMechanic = Mechanic.Spreads;
                    StackTargets.Clear();
                    SpreadTargets.Clear();
                    SpreadTargets.AddRange(module.Raid.WithoutSlot());
                    SpreadRadius = 6;
                    break;
                case AID.StingingWelt:
                    NextMechanic = Mechanic.Done;
                    SpreadTargets.Clear();
                    break;
            }
        }
    }

    class Flamerake : Components.GenericAOEs
    {
        private Angle _offset;
        private DateTime _activation;

        private static AOEShapeCross _first = new(20, 6);
        private static AOEShapeRect _rest = new(8, 20);

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            if (_activation == default)
                yield break;

            if (NumCasts == 0)
            {
                yield return new(_first, module.Bounds.Center, _offset, _activation);
            }
            else
            {
                float offset = NumCasts == 1 ? 6 : 14;
                for (int i = 0; i < 4; ++i)
                {
                    var dir = i * 90.Degrees() + _offset;
                    yield return new(_rest, module.Bounds.Center + offset * dir.ToDirection(), dir, _activation);
                }
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.FlamerakeAOE11:
                case AID.FlamerakeAOE12:
                    if (NumCasts == 0)
                    {
                        ++NumCasts;
                        _activation = module.WorldState.CurrentTime.AddSeconds(2.1f);
                    }
                    break;
                case AID.FlamerakeAOE21:
                case AID.FlamerakeAOE22:
                    if (NumCasts == 1)
                    {
                        ++NumCasts;
                        _activation = module.WorldState.CurrentTime.AddSeconds(2.5f);
                    }
                    break;
                case AID.FlamerakeAOE31:
                case AID.FlamerakeAOE32:
                    if (NumCasts == 2)
                    {
                        ++NumCasts;
                        _activation = default;
                    }
                    break;
            }
        }

        public override void OnEventEnvControl(BossModule module, uint directorID, byte index, uint state)
        {
            if (directorID == 0x80034E77 && index == 4)
            {
                switch (state)
                {
                    case 0x00010001:
                        _offset = 45.Degrees();
                        _activation = module.WorldState.CurrentTime.AddSeconds(8.5f);
                        break;
                    case 0x00800080:
                        _offset = 0.Degrees();
                        _activation = module.WorldState.CurrentTime.AddSeconds(8.5f);
                        break;
                    // 00080004 when rotation ends
                }
            }
        }
    }
}
