namespace BossMod.RealmReborn.Dungeon.D03Copperbell.D033Gyges
{
    public enum OID : uint
    {
        Boss = 0x38C9,
        Helper = 0x233C, // x5
    }

    public enum AID : uint
    {
        AutoAttack = 6499, // Boss->player, no cast
        GiganticSwing = 28762, // Boss->self, 6.0s cast, range 4-40 donut aoe
        GiganticSmash = 28760, // Boss->location, 6.0s cast, range 10 aoe
        GiganticBlast = 28761, // Helper->self, 6.0s cast, range 8 aoe
        GrandSlam = 28764, // Boss->player, 5.0s cast, tankbuster
        ColossalSlam = 28763, // Boss->self, 4.0s cast, range 40 60-degree cone aoe
    };

    class GiganticSwing : Components.SelfTargetedAOEs
    {
        public GiganticSwing() : base(ActionID.MakeSpell(AID.GiganticSwing), new AOEShapeDonut(4, 40)) { }
    }

    class GiganticSmash : Components.LocationTargetedAOEs
    {
        public GiganticSmash() : base(ActionID.MakeSpell(AID.GiganticSmash), 10) { }
    }

    class GiganticBlast : Components.SelfTargetedAOEs
    {
        public GiganticBlast() : base(ActionID.MakeSpell(AID.GiganticBlast), new AOEShapeCircle(8)) { }
    }

    class GrandSlam : Components.SingleTargetCast
    {
        public GrandSlam() : base(ActionID.MakeSpell(AID.GrandSlam)) { }
    }

    class ColossalSlam : Components.SelfTargetedLegacyRotationAOEs
    {
        public ColossalSlam() : base(ActionID.MakeSpell(AID.ColossalSlam), new AOEShapeCone(40, 30.Degrees())) { }
    }

    class D033GygesStates : StateMachineBuilder
    {
        public D033GygesStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<GiganticSwing>()
                .ActivateOnEnter<GiganticSmash>()
                .ActivateOnEnter<GiganticBlast>()
                .ActivateOnEnter<GrandSlam>()
                .ActivateOnEnter<ColossalSlam>();
        }
    }

    [ModuleInfo(CFCID = 3, NameID = 101)]
    public class D033Gyges : BossModule
    {
        public D033Gyges(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(-100.42f, 6.67f), 20)) { }
    }
}
