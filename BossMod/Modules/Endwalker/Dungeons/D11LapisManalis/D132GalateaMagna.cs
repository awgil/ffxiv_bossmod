// CONTRIB: made by malediktus, not checked
namespace BossMod.Endwalker.Dungeon.D13LapisManalis.D132GalateaMagna
{
    public enum OID : uint
    {
        Boss = 0x3971, //R=5.0
        WildBeasts = 0x3D03, //R=0.5
        Helper = 0x233C,
        _Gen_Actor3cff = 0x3CFF, // R1,320
        _Gen_Actor3d00 = 0x3D00, // R1,700
        _Gen_Actor3d02 = 0x3D02, // R4,000
        _Gen_Actor3d01 = 0x3D01, // R2,850
        _Gen_Actor3d04 = 0x3D04, // R2,000
    }

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    _Ability_ = 32812, // Boss->location, no cast, single-target
    CallOfTheMountain = 31356, // Boss->self, 3,0s cast, single-target
    WildlifeCrossing = 31357, // WildBeasts->self, no cast, range 7 width 10 rect
    AlbionsEmbrace = 31365, // Boss->player, 5,0s cast, single-target
    RightSlam = 32813, // Boss->self, 5,0s cast, range 80 width 20 rect
    LeftSlam = 32814, // Boss->self, 5,0s cast, range 80 width 20 rect
    KnockOnIce = 31358, // Boss->self, 4,0s cast, single-target
    KnockOnIce2 = 31359, // Helper->self, 6,0s cast, range 5 circle
    Icebreaker = 31361, // Boss->3D04, 5,0s cast, range 17 circle
    IcyThroes = 31362, // Boss->self, no cast, single-target
    IcyThroes2 = 32783, // Helper->self, 5,0s cast, range 6 circle
    IcyThroes3 = 31363, // Helper->player, 5,0s cast, range 6 circle
    IcyThroes4 = 32697, // Helper->self, 5,0s cast, range 6 circle
    RoarOfAlbion = 31364, // Boss->self, 7,0s cast, range 60 circle
};

public enum IconID : uint
{
    _Gen_Icon_218 = 218, // player
    _Gen_Icon_210 = 210, // _Gen_Actor3d04
    _Gen_Icon_139 = 139, // player
};
public enum TetherID : uint
{
    _Gen_Tether_12 = 12, // Boss->_Gen_Actor3d04
};



    // class WindsOfWinter : Components.RaidwideCast
    // {
    //     public WindsOfWinter() : base(ActionID.MakeSpell(AID.WindsOfWinter), "Stun Albus Griffin, Raidwide") { }
    // }

    // class GoldenTalons : Components.SelfTargetedAOEs
    // {
    //     public GoldenTalons() : base(ActionID.MakeSpell(AID.GoldenTalons), new AOEShapeCone(8, 45.Degrees())) { }
    // }

    // class Freefall : Components.LocationTargetedAOEs
    // {
    //     public Freefall() : base(ActionID.MakeSpell(AID.Freefall), 8) { }
    // }

    class D130AlbusGriffinStates : StateMachineBuilder
    {
        public D130AlbusGriffinStates(BossModule module) : base(module)
        {
            TrivialPhase();
                // .ActivateOnEnter<Freefall>()
                // .ActivateOnEnter<WindsOfWinter>()
                // .ActivateOnEnter<GoldenTalons>()
        }
    }

    [ModuleInfo(CFCID = 896, NameID = 12245)]
    public class D130AlbusGriffin : BossModule
    {
        public D130AlbusGriffin(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(350, -394), 19.5f)) { }
    }
}
