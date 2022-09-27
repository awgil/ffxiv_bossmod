using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.RealmReborn.Dungeon.D10StoneVigil.D103Isgebind
{
    public enum OID : uint
    {
        Boss = 0x5AF, // x1
        Helper = 0x233C, // x8
    };

    public enum AID : uint
    {
        AutoAttack = 870, // Boss->player, no cast
        RimeWreath = 1025, // Boss->self, 3.0s cast, raidwide
        FrostBreath = 1022, // Boss->self, 1.0s cast, range 27 ?-degree cone cleave
        SheetOfIce = 1023, // Boss->location, 2.5s cast, range 5 aoe
        SheetOfIce2 = 1024, // Helper->location, 3.0s cast, range 5 aoe
        Cauterize = 1026, // Boss->self, 4.0s cast, range 48 width 20 rect aoe
        Touchdown = 1027, // Boss->self, no cast, range 5 aoe around center
    };

    class RimeWreath : Components.RaidwideCast
    {
        public RimeWreath() : base(ActionID.MakeSpell(AID.RimeWreath)) { }
    }

    class FrostBreath : Components.Cleave
    {
        public FrostBreath() : base(ActionID.MakeSpell(AID.FrostBreath), new AOEShapeCone(27, 60.Degrees())) { } // TODO: verify angle
    }

    class SheetOfIce : Components.LocationTargetedAOEs
    {
        public SheetOfIce() : base(ActionID.MakeSpell(AID.SheetOfIce), 5) { }
    }

    class SheetOfIce2 : Components.LocationTargetedAOEs
    {
        public SheetOfIce2() : base(ActionID.MakeSpell(AID.SheetOfIce2), 5) { }
    }

    class Cauterize : Components.SelfTargetedLegacyRotationAOEs
    {
        public Cauterize() : base(ActionID.MakeSpell(AID.Cauterize), new AOEShapeRect(48, 10)) { }
    }

    class Touchdown : Components.GenericAOEs
    {
        private AOEShapeCircle _shape = new(5);

        public Touchdown() : base(ActionID.MakeSpell(AID.Touchdown)) { }

        public override IEnumerable<(AOEShape shape, WPos origin, Angle rotation, DateTime time)> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            // TODO: proper timings...
            if (!module.PrimaryActor.IsTargetable && !module.FindComponent<Cauterize>()!.ActiveCasters.Any())
                yield return (_shape, module.Bounds.Center, new(), module.WorldState.CurrentTime);
        }
    }

    class D103IsgebindStates : StateMachineBuilder
    {
        public D103IsgebindStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<RimeWreath>()
                .ActivateOnEnter<FrostBreath>()
                .ActivateOnEnter<SheetOfIce>()
                .ActivateOnEnter<SheetOfIce2>()
                .ActivateOnEnter<Cauterize>()
                .ActivateOnEnter<Touchdown>();
        }
    }

    public class D103Isgebind : BossModule
    {
        public D103Isgebind(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsSquare(new(0, -248), 20)) { }
    }
}
