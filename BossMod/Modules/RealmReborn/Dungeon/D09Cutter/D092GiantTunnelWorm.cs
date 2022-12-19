using System;
using System.Collections.Generic;

namespace BossMod.RealmReborn.Dungeon.D09Cutter.D092GiantTunnelWorm
{
    public enum OID : uint
    {
        Boss = 0x536, // x1
        BottomlessDesertHelper = 0x64A, // x1
        SandPillarHelper = 0x64B, // x7
    };

    public enum AID : uint
    {
        AutoAttack = 870, // Boss->player, no cast
        Sandstorm = 529, // Boss->self, no cast, range 10.5 90-degree cleave
        SandCyclone = 1111, // Boss->player, no cast, random single-target
        Earthbreak = 531, // Boss->self, no cast, range 14.5 aoe
        BottomlessDesert = 1112, // BottomlessDesertHelper->self, no cast, raidwide drawin
        SandPillar = 1113, // SandPillarHelper->self, no cast, range 4.5 aoe
    };

    class Sandstorm : Components.Cleave
    {
        public Sandstorm() : base(ActionID.MakeSpell(AID.Sandstorm), new AOEShapeCone(10.5f, 45.Degrees())) { }
    }

    // TODO: pillars teleport right before cast, so we don't show them for now...
    class Submerge : Components.GenericAOEs
    {
        private AOEShapeCircle _shape = new(14.5f);

        public Submerge() : base(ActionID.MakeSpell(AID.Earthbreak)) { }

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            // TODO: proper timings...
            if (!module.PrimaryActor.IsTargetable)
                yield return new(_shape, module.PrimaryActor.Position, module.PrimaryActor.Rotation);
        }
    }

    class D092GiantTunnelWormStates : StateMachineBuilder
    {
        public D092GiantTunnelWormStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<Sandstorm>()
                .ActivateOnEnter<Submerge>();
        }
    }

    public class D092GiantTunnelWorm : BossModule
    {
        public D092GiantTunnelWorm(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(-140, 150), 20)) { }
    }
}
