using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.RealmReborn.Dungeon.D09Cutter.D093Chimera
{
    public enum OID : uint
    {
        Boss = 0x64C, // x1
        Cacophony = 0x64D, // spawn during fight
        RamsKeeper = 0x1E8713, // EventObj type, spawn during fight
    };

    public enum AID : uint
    {
        AutoAttack = 870, // Boss->player, no cast
        LionsBreath = 1101, // Boss->self, no cast, range 9.7 ?-degree cleave
        RamsBreath = 1102, // Boss->self, 2.0s cast, range 9.7 120-degree cone, -45 degree offset
        DragonsBreath = 1103, // Boss->self, 2.0s cast, range 9.7 120-degree cone, +45 degree offset
        RamsVoice = 1104, // Boss->self, 3.0s cast, range 9.7 aoe
        DragonsVoice = 1442, // Boss->self, 4.5s cast, range 7-30 donut aoe
        RamsKeeper = 1106, // Boss->location, 3.0s cast, range 6 voidzone
        Cacophony = 1107, // Boss->self, no cast, visual, summons orb
        ChaoticChorus = 1108, // Cacophony->self, no cast, range 6 aoe
    };

    class LionsBreath : Components.Cleave
    {
        public LionsBreath() : base(ActionID.MakeSpell(AID.LionsBreath), new AOEShapeCone(9.7f, 60.Degrees())) { } // TODO: verify angle
    }

    class RamsBreath : Components.SelfTargetedAOEs
    {
        public RamsBreath() : base(ActionID.MakeSpell(AID.RamsBreath), new AOEShapeCone(9.7f, 60.Degrees(), -45.Degrees()), true) { }
    }

    class DragonsBreath : Components.SelfTargetedAOEs
    {
        public DragonsBreath() : base(ActionID.MakeSpell(AID.DragonsBreath), new AOEShapeCone(9.7f, 60.Degrees(), 45.Degrees()), true) { }
    }

    class RamsVoice : Components.SelfTargetedAOEs
    {
        public RamsVoice() : base(ActionID.MakeSpell(AID.RamsVoice), new AOEShapeCircle(9.7f), true) { }
    }

    class DragonsVoice : Components.SelfTargetedAOEs
    {
        public DragonsVoice() : base(ActionID.MakeSpell(AID.DragonsVoice), new AOEShapeDonut(7, 30), true) { }
    }

    class RamsKeeper : Components.LocationTargetedAOEs
    {
        public RamsKeeper() : base(ActionID.MakeSpell(AID.RamsKeeper), 6) { }
    }

    class RamsKeeperVoidzone : Components.PersistentVoidzone
    {
        public RamsKeeperVoidzone() : base((uint)OID.RamsKeeper, new AOEShapeCircle(6)) { }
    }

    class ChaoticChorus : Components.GenericSelfTargetedAOEs
    {
        public ChaoticChorus() : base(ActionID.MakeSpell(AID.ChaoticChorus), new AOEShapeCircle(6)) { }

        public override IEnumerable<(WPos, Angle, DateTime)> ImminentCasts(BossModule module)
        {
            // TODO: timings
            return module.Enemies(OID.Cacophony).Where(c => !c.IsDead).Select(c => (c.Position, c.Rotation, module.WorldState.CurrentTime));
        }
    }

    class D093ChimeraStates : StateMachineBuilder
    {
        public D093ChimeraStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<LionsBreath>()
                .ActivateOnEnter<RamsBreath>()
                .ActivateOnEnter<DragonsBreath>()
                .ActivateOnEnter<RamsVoice>()
                .ActivateOnEnter<DragonsVoice>()
                .ActivateOnEnter<RamsKeeper>()
                .ActivateOnEnter<RamsKeeperVoidzone>()
                .ActivateOnEnter<ChaoticChorus>();
        }
    }

    public class D093Chimera : BossModule
    {
        public D093Chimera(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(-170, -200), 30)) { }
    }
}
