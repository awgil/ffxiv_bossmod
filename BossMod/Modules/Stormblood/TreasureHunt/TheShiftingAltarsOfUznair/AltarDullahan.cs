namespace BossMod.Stormblood.TreasureHunt.ShiftingAltarsOfUznair.AltarDullahan;

public enum OID : uint
{
    Boss = 0x2533, //R=3.8
    BossAdd = 0x2563, //R=1.8
    BossHelper = 0x233C,
    AltarQueen = 0x254A, // R0,840, icon 5, needs to be killed in order from 1 to 5 for maximum rewards
    AltarGarlic = 0x2548, // R0,840, icon 3, needs to be killed in order from 1 to 5 for maximum rewards
    AltarTomato = 0x2549, // R0,840, icon 4, needs to be killed in order from 1 to 5 for maximum rewards
    AltarOnion = 0x2546, // R0,840, icon 1, needs to be killed in order from 1 to 5 for maximum rewards
    AltarEgg = 0x2547, // R0,840, icon 2, needs to be killed in order from 1 to 5 for maximum rewards
    BonusAdd_AltarMatanga = 0x2545, // R3.420
};

public enum AID : uint
{
    AutoAttack = 870, // 2533->player, no cast, single-target
    AutoAttack2 = 872, // BonusAdd_AltarMatanga/BonusAdd_Mandragoras->player, no cast, single-target
    AutoAttack3 = 6497, // 2563->player, no cast, single-target
    IronJustice = 13316, // 2533->self, 3,0s cast, range 8+R 120-degree cone
    Cloudcover = 13477, // 2533->location, 3,0s cast, range 6 circle
    TerrorEye = 13644, // 2563->location, 3,5s cast, range 6 circle
    StygianRelease = 13314, // 2533->self, 3,5s cast, range 50+R circle, small raidwide dmg, knockback 20 from source
    VillainousRebuke = 13315, // 2533->players, 4,5s cast, range 6 circle

    PluckAndPrune = 6449, // AltarEgg->self, 3,5s cast, range 6+R circle
    PungentPirouette = 6450, // AltarGarlic->self, 3,5s cast, range 6+R circle
    TearyTwirl = 6448, // AltarOnion->self, 3,5s cast, range 6+R circle
    Pollen = 6452, // AltarQueen->self, 3,5s cast, range 6+R circle
    HeirloomScream = 6451, // AltarTomato->self, 3,5s cast, range 6+R circle
    unknown = 9636, // BonusAdd_AltarMatanga->self, no cast, single-target
    Spin = 8599, // BonusAdd_AltarMatanga->self, no cast, range 6+R 120-degree cone
    RaucousScritch = 8598, // BonusAdd_AltarMatanga->self, 2,5s cast, range 5+R 120-degree cone
    Hurl = 5352, // BonusAdd_AltarMatanga->location, 3,0s cast, range 6 circle

    Telega = 9630, // bonusadds->self, no cast, single-target, bonus add disappear
};

class IronJustice : Components.SelfTargetedAOEs
{
    public IronJustice() : base(ActionID.MakeSpell(AID.IronJustice), new AOEShapeCone(11.8f, 60.Degrees())) { }
}

class Cloudcover : Components.LocationTargetedAOEs
{
    public Cloudcover() : base(ActionID.MakeSpell(AID.Cloudcover), 6) { }
}

class TerrorEye : Components.LocationTargetedAOEs
{
    public TerrorEye() : base(ActionID.MakeSpell(AID.TerrorEye), 6) { }
}

class VillainousRebuke : Components.StackWithCastTargets
{
    public VillainousRebuke() : base(ActionID.MakeSpell(AID.VillainousRebuke), 6) { }
}

class StygianRelease : Components.RaidwideCast
{
    public StygianRelease() : base(ActionID.MakeSpell(AID.StygianRelease)) { }
}

class PluckAndPrune : Components.SelfTargetedAOEs
{
    public PluckAndPrune() : base(ActionID.MakeSpell(AID.PluckAndPrune), new AOEShapeCircle(6.84f)) { }
}

class TearyTwirl : Components.SelfTargetedAOEs
{
    public TearyTwirl() : base(ActionID.MakeSpell(AID.TearyTwirl), new AOEShapeCircle(6.84f)) { }
}

class HeirloomScream : Components.SelfTargetedAOEs
{
    public HeirloomScream() : base(ActionID.MakeSpell(AID.HeirloomScream), new AOEShapeCircle(6.84f)) { }
}

class PungentPirouette : Components.SelfTargetedAOEs
{
    public PungentPirouette() : base(ActionID.MakeSpell(AID.PungentPirouette), new AOEShapeCircle(6.84f)) { }
}

class Pollen : Components.SelfTargetedAOEs
{
    public Pollen() : base(ActionID.MakeSpell(AID.Pollen), new AOEShapeCircle(6.84f)) { }
}

class StygianReleaseKB : Components.KnockbackFromCastTarget
{
    public StygianReleaseKB() : base(ActionID.MakeSpell(AID.StygianRelease), 20)
    {
        StopAtWall = true;
    }

    public override bool DestinationUnsafe(BossModule module, int slot, Actor actor, WPos pos) => module.FindComponent<TerrorEye>()?.ActiveAOEs(module, slot, actor).Any(z => z.Shape.Check(pos, z.Origin, z.Rotation)) ?? false;
}

class RaucousScritch : Components.SelfTargetedAOEs
{
    public RaucousScritch() : base(ActionID.MakeSpell(AID.RaucousScritch), new AOEShapeCone(8.42f, 30.Degrees())) { }
}

class Hurl : Components.LocationTargetedAOEs
{
    public Hurl() : base(ActionID.MakeSpell(AID.Hurl), 6) { }
}

class Spin : Components.Cleave
{
    public Spin() : base(ActionID.MakeSpell(AID.Spin), new AOEShapeCone(9.42f, 60.Degrees()), (uint)OID.BonusAdd_AltarMatanga) { }
}

class DullahanStates : StateMachineBuilder
{
    public DullahanStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<IronJustice>()
            .ActivateOnEnter<Cloudcover>()
            .ActivateOnEnter<TerrorEye>()
            .ActivateOnEnter<VillainousRebuke>()
            .ActivateOnEnter<StygianRelease>()
            .ActivateOnEnter<StygianReleaseKB>()
            .ActivateOnEnter<PluckAndPrune>()
            .ActivateOnEnter<TearyTwirl>()
            .ActivateOnEnter<HeirloomScream>()
            .ActivateOnEnter<PungentPirouette>()
            .ActivateOnEnter<Pollen>()
            .ActivateOnEnter<Hurl>()
            .ActivateOnEnter<RaucousScritch>()
            .ActivateOnEnter<Spin>()
            .Raw.Update = () => module.Enemies(OID.Boss).All(e => e.IsDead) && module.Enemies(OID.BossAdd).All(e => e.IsDead) && module.Enemies(OID.BonusAdd_AltarMatanga).All(e => e.IsDead) && module.Enemies(OID.AltarEgg).All(e => e.IsDead) && module.Enemies(OID.AltarQueen).All(e => e.IsDead) && module.Enemies(OID.AltarOnion).All(e => e.IsDead) && module.Enemies(OID.AltarGarlic).All(e => e.IsDead) && module.Enemies(OID.AltarTomato).All(e => e.IsDead);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 586, NameID = 7585)]
public class Dullahan : BossModule
{
    public Dullahan(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 20)) { }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.BossAdd))
            Arena.Actor(s, ArenaColor.Object);
        foreach (var s in Enemies(OID.AltarEgg))
            Arena.Actor(s, ArenaColor.Vulnerable);
        foreach (var s in Enemies(OID.AltarTomato))
            Arena.Actor(s, ArenaColor.Vulnerable);
        foreach (var s in Enemies(OID.AltarQueen))
            Arena.Actor(s, ArenaColor.Vulnerable);
        foreach (var s in Enemies(OID.AltarGarlic))
            Arena.Actor(s, ArenaColor.Vulnerable);
        foreach (var s in Enemies(OID.AltarOnion))
            Arena.Actor(s, ArenaColor.Vulnerable);
        foreach (var s in Enemies(OID.BonusAdd_AltarMatanga))
            Arena.Actor(s, ArenaColor.Vulnerable);
    }

    public override void CalculateAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.CalculateAIHints(slot, actor, assignment, hints);
        foreach (var e in hints.PotentialTargets)
        {
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.AltarOnion => 7,
                OID.AltarEgg => 6,
                OID.AltarGarlic => 5,
                OID.AltarTomato => 4,
                OID.AltarQueen or OID.BonusAdd_AltarMatanga => 3,
                OID.BossAdd => 2,
                OID.Boss => 1,
                _ => 0
            };
        }
    }
}
