using System;

namespace BossMod.Shadowbringers.Ultimate.TEA
{
    class P3WormholeLimitCut : LimitCut
    {
        public P3WormholeLimitCut() : base(2.7f) { }
    }

    class P3WormholeSacrament : Components.SelfTargetedAOEs
    {
        public P3WormholeSacrament() : base(ActionID.MakeSpell(AID.SacramentWormhole), new AOEShapeCross(100, 8)) { }
    }

    // TODO: movement hints, draw wormholes - need to know radius!
    class P3WormholeRepentance : BossComponent
    {
        public int NumSoaks { get; private set; }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.Repentance1:
                    NumSoaks = 1;
                    break;
                case AID.Repentance2:
                    NumSoaks = 2;
                    break;
                case AID.Repentance3:
                    NumSoaks = 3;
                    break;
            }
        }
    }

    class P3WormholeIncineratingHeat : Components.StackWithCastTargets
    {
        public P3WormholeIncineratingHeat() : base(ActionID.MakeSpell(AID.IncineratingHeat), 5, 8) { }
    }

    class P3WormholeEnumeration : Components.UniformStackSpread
    {
        private BitMask _targets; // we start showing stacks only after incinerating heat is resolved
        private DateTime _activation;

        public P3WormholeEnumeration() : base(5, 0, 3, 3, raidwideOnResolve: false) { } // TODO: verify enumeration radius

        public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
        {
            if (iconID == (uint)IconID.Enumeration)
            {
                _targets.Set(module.Raid.FindSlot(actor.InstanceID));
                _activation = module.WorldState.CurrentTime.AddSeconds(5.1f);
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.Enumeration:
                    Stacks.Clear();
                    break;
                case AID.IncineratingHeat:
                    AddStacks(module.Raid.WithSlot(true).IncludedInMask(_targets).Actors(), _activation);
                    break;
            }
        }
    }

}
