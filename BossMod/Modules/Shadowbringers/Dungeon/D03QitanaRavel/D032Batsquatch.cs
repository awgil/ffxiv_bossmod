namespace BossMod.Shadowbringers.Dungeon.D03QitanaRavel.D032Batsquatch;

public enum OID : uint
{
    Boss = 0x27B0, //R=3.2
    Helper = 0x233C, //R=0.5
}

public enum AID : uint
{
    AutoAttack = 870, // 27B0->player, no cast, single-target
    RipperFang = 15505, // 27B0->player, 4,0s cast, single-target
    Soundwave = 15506, // 27B0->self, 3,0s cast, range 40 circle
    Subsonics = 15507, // 27B0->self, 6,0s cast, single-target
    Subsonics2 = 15508, // 233C->self, no cast, range 40 circle
    FallingRock = 15510, // 233C->self, 2,0s cast, range 3 circle
    FallingRock2 = 15509, // 233C->self, 2,0s cast, range 2 circle
    FallingBoulder = 15511, // 233C->self, 2,0s cast, range 4 circle
    Towerfall = 15512, // 233C->self, 3,0s cast, range 15 30-degree cone
};

class Towerfall : Components.SelfTargetedAOEs
{
    public Towerfall() : base(ActionID.MakeSpell(AID.Towerfall), new AOEShapeCone(15, 15.Degrees())) { }
}

class Soundwave : Components.RaidwideCast
{
    public Soundwave() : base(ActionID.MakeSpell(AID.Soundwave), "Raidwide + towers fall") { }
}

class Subsonics : Components.RaidwideCast
{
    public Subsonics() : base(ActionID.MakeSpell(AID.Subsonics), "Raidwide x11") { }
}

class RipperFang : Components.SingleTargetDelayableCast
{
    public RipperFang() : base(ActionID.MakeSpell(AID.RipperFang)) { }
}

class FallingBoulder : Components.SelfTargetedAOEs
{
    public FallingBoulder() : base(ActionID.MakeSpell(AID.FallingBoulder), new AOEShapeCircle(4)) { }
}

class FallingRock : Components.SelfTargetedAOEs
{
    public FallingRock() : base(ActionID.MakeSpell(AID.FallingRock), new AOEShapeCircle(3)) { }
}

class FallingRock2 : Components.SelfTargetedAOEs
{
    public FallingRock2() : base(ActionID.MakeSpell(AID.FallingRock2), new AOEShapeCircle(2)) { }
}

class D032BatsquatchStates : StateMachineBuilder
{
    public D032BatsquatchStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Towerfall>()
            .ActivateOnEnter<Soundwave>()
            .ActivateOnEnter<Subsonics>()
            .ActivateOnEnter<RipperFang>()
            .ActivateOnEnter<FallingBoulder>()
            .ActivateOnEnter<FallingRock>()
            .ActivateOnEnter<FallingRock2>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 651, NameID = 8232)]
public class D032Batsquatch : BossModule
{
    public D032Batsquatch(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(62, -35), 15)) { }
}
