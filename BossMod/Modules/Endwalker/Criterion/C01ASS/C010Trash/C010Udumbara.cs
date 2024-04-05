namespace BossMod.Endwalker.Criterion.C01ASS.C010Udumbara;

public enum OID : uint
{
    NBoss = 0x3AD3, // R4.000, x1
    NSapria = 0x3AD4, // R1.440, x2
    SBoss = 0x3ADC, // R4.000, x1
    SSapria = 0x3ADD, // R1.440, x2
};

public enum AID : uint
{
    AutoAttack = 31320, // NBoss/NSapria/SBoss/SSapria->player, no cast, single-target
    NHoneyedLeft = 31067, // NBoss->self, 4.0s cast, range 30 180-degree cone
    NHoneyedRight = 31068, // NBoss->self, 4.0s cast, range 30 180-degree cone
    NHoneyedFront = 31069, // NBoss->self, 4.0s cast, range 30 120-degree cone
    NBloodyCaress = 31071, // NSapria->self, 3.0s cast, range 12 120-degree cone
    SHoneyedLeft = 31091, // SBoss->self, 4.0s cast, range 30 180-degree cone
    SHoneyedRight = 31092, // SBoss->self, 4.0s cast, range 30 180-degree cone
    SHoneyedFront = 31093, // SBoss->self, 4.0s cast, range 30 120-degree cone
    SBloodyCaress = 31095, // SSapria->self, 3.0s cast, range 12 120-degree cone
};

class HoneyedLeft : Components.SelfTargetedAOEs
{
    public HoneyedLeft(AID aid) : base(ActionID.MakeSpell(aid), new AOEShapeCone(30, 90.Degrees())) { }
}
class NHoneyedLeft : HoneyedLeft { public NHoneyedLeft() : base(AID.NHoneyedLeft) { } }
class SHoneyedLeft : HoneyedLeft { public SHoneyedLeft() : base(AID.SHoneyedLeft) { } }

class HoneyedRight : Components.SelfTargetedAOEs
{
    public HoneyedRight(AID aid) : base(ActionID.MakeSpell(aid), new AOEShapeCone(30, 90.Degrees())) { }
}
class NHoneyedRight : HoneyedRight { public NHoneyedRight() : base(AID.NHoneyedRight) { } }
class SHoneyedRight : HoneyedRight { public SHoneyedRight() : base(AID.SHoneyedRight) { } }

class HoneyedFront : Components.SelfTargetedAOEs
{
    public HoneyedFront(AID aid) : base(ActionID.MakeSpell(aid), new AOEShapeCone(30, 60.Degrees())) { }
}
class NHoneyedFront : HoneyedFront { public NHoneyedFront() : base(AID.NHoneyedFront) { } }
class SHoneyedFront : HoneyedFront { public SHoneyedFront() : base(AID.SHoneyedFront) { } }

class BloodyCaress : Components.SelfTargetedAOEs
{
    public BloodyCaress(AID aid) : base(ActionID.MakeSpell(aid), new AOEShapeCone(12, 60.Degrees())) { }
}
class NBloodyCaress : BloodyCaress { public NBloodyCaress() : base(AID.NBloodyCaress) { } }
class SBloodyCaress : BloodyCaress { public SBloodyCaress() : base(AID.SBloodyCaress) { } }

class C010UdumbaraStates : StateMachineBuilder
{
    public C010UdumbaraStates(BossModule module, bool savage) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<NHoneyedLeft>(!savage)
            .ActivateOnEnter<NHoneyedRight>(!savage)
            .ActivateOnEnter<NHoneyedFront>(!savage)
            .ActivateOnEnter<NBloodyCaress>(!savage)
            .ActivateOnEnter<SHoneyedLeft>(savage)
            .ActivateOnEnter<SHoneyedRight>(savage)
            .ActivateOnEnter<SHoneyedFront>(savage)
            .ActivateOnEnter<SBloodyCaress>(savage)
            .Raw.Update = () => (module.PrimaryActor.IsDestroyed || module.PrimaryActor.IsDead) && !module.Enemies(savage ? OID.SSapria : OID.NSapria).Any(a => !a.IsDead);
    }
}
class C010NUdumbaraStates : C010UdumbaraStates { public C010NUdumbaraStates(BossModule module) : base(module, false) { } }
class C010SUdumbaraStates : C010UdumbaraStates { public C010SUdumbaraStates(BossModule module) : base(module, true) { } }

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.NBoss, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 878, NameID = 11511, SortOrder = 3)]
public class C010NUdumbara : SimpleBossModule
{
    public C010NUdumbara(WorldState ws, Actor primary) : base(ws, primary) { }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.NSapria), ArenaColor.Enemy);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.SBoss, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 879, NameID = 11511, SortOrder = 3)]
public class C010SUdumbara : SimpleBossModule
{
    public C010SUdumbara(WorldState ws, Actor primary) : base(ws, primary) { }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.SSapria), ArenaColor.Enemy);
    }
}
