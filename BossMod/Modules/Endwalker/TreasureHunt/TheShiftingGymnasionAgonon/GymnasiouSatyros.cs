namespace BossMod.Endwalker.TreasureHunt.ShiftingGymnasionAgonon.GymnasiouSatyros;

public enum OID : uint
{
    Boss = 0x3D2D, //R=7.5
    BossAdd = 0x3D2E, //R=2.08
    BossHelper = 0x233C,
    StormsGrip = 0x3D2F, //R=1.0
    BonusAddLyssa = 0x3D4E, //R=3.75, bonus loot adds
    BonusAddLampas = 0x3D4D, //R=2.001, bonus loot adds
}

public enum AID : uint
{
    AutoAttack = 872, // Boss/BossAdd->player, no cast, single-target
    AutoAttack2 = 870, // BonusAddLyssa->player, no cast, single-target
    StormWingA = 32220, // Boss->self, 5.0s cast, single-target
    StormWingB = 32219, // Boss->self, 5.0s cast, single-target
    StormWing2 = 32221, // BossHelper->self, 5.0s cast, range 40 90-degree cone
    DreadDive = 32218, // Boss->player, 5.0s cast, single-target
    FlashGale = 32222, // Boss->location, 3.0s cast, range 6 circle
    unknown = 32199, // 3D2F->self, no cast, single-target
    WindCutter = 32227, // 3D2F->self, no cast, range 4 circle
    BigHorn = 32226, // BossAdd->player, no cast, single-target
    Wingblow = 32224, // Boss->self, 4.0s cast, single-target
    Wingblow2 = 32225, // BossHelper->self, 4.0s cast, range 15 circle

    HeavySmash = 32317, // BossAdd->location, 3.0s cast, range 6 circle
    Telega = 9630, // BonusAdds->self, no cast, single-target, bonus add disappear
}

class HeavySmash(BossModule module) : Components.StandardAOEs(module, AID.HeavySmash, 6);
class StormWing(BossModule module) : Components.StandardAOEs(module, AID.StormWing2, new AOEShapeCone(40, 45.Degrees()));
class FlashGale(BossModule module) : Components.StandardAOEs(module, AID.FlashGale, 6);
class WindCutter(BossModule module) : Components.PersistentVoidzone(module, 4, m => m.Enemies(OID.StormsGrip));
class Wingblow(BossModule module) : Components.StandardAOEs(module, AID.Wingblow2, new AOEShapeCircle(15));
class DreadDive(BossModule module) : Components.SingleTargetCast(module, AID.DreadDive);

class SatyrosStates : StateMachineBuilder
{
    public SatyrosStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<StormWing>()
            .ActivateOnEnter<FlashGale>()
            .ActivateOnEnter<WindCutter>()
            .ActivateOnEnter<Wingblow>()
            .ActivateOnEnter<DreadDive>()
            .ActivateOnEnter<HeavySmash>()
            .Raw.Update = () => module.Enemies(OID.Boss).All(e => e.IsDead) && module.Enemies(OID.BossAdd).All(e => e.IsDead) && module.Enemies(OID.BonusAddLyssa).All(e => e.IsDead) && module.Enemies(OID.BonusAddLampas).All(e => e.IsDead);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 909, NameID = 12003)]
public class Satyros(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(20))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.BossAdd))
            Arena.Actor(s, ArenaColor.Object);
        foreach (var s in Enemies(OID.BonusAddLyssa))
            Arena.Actor(s, ArenaColor.Vulnerable);
        foreach (var s in Enemies(OID.BonusAddLampas))
            Arena.Actor(s, ArenaColor.Vulnerable);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var e in hints.PotentialTargets)
        {
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.BonusAddLampas => 4,
                OID.BonusAddLyssa => 3,
                OID.BossAdd => 2,
                OID.Boss => 1,
                _ => 0
            };
        }
    }
}
