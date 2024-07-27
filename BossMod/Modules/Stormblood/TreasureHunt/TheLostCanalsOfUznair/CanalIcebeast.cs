namespace BossMod.Stormblood.TreasureHunt.LostCanalsOfUznair.CanalIcebeast;

public enum OID : uint
{
    Boss = 0x1F15, //R=7.5
    BossHelper = 0x233C,
    BossAdd1 = 0x1F0E, // R1.560
    BossAdd2 = 0x1F0F, // R1.600
    BonusAddAbharamu = 0x1EBF, // R3.420
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    AutoAttack2 = 872, // BonusAddAbharamu/1F0E->player, no cast, single-target
    FrumiousJaws = 9556, // Boss->player, no cast, single-target
    AbsoluteZero = 9558, // Boss->self, 5.0s cast, range 38+R 90-degree cone
    Blizzard = 967, // 1F0F->player, 1.0s cast, single-target
    Eyeshine = 9557, // Boss->self, 4.0s cast, range 38+R circle, gaze
    Freezeover = 4486, // 1F0E->location, 3.0s cast, range 3 circle
    PlainPound = 4487, // 1F0E->self, 3.0s cast, range 3+R circle

    unknown = 9636, // BonusAddAbharamu->self, no cast, single-target
    Spin = 8599, // BonusAddAbharamu->self, no cast, range 6+R 120-degree cone
    RaucousScritch = 8598, // BonusAddAbharamu->self, 2.5s cast, range 5+R 120-degree cone
    Hurl = 5352, // BonusAddAbharamu->location, 3.0s cast, range 6 circle
    Telega = 9630, // BonusAdds->self, no cast, single-target, bonus adds disappear
}

class Eyeshine(BossModule module) : Components.CastGaze(module, ActionID.MakeSpell(AID.Eyeshine));
class AbsoluteZero(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.AbsoluteZero), new AOEShapeCone(45.5f, 45.Degrees()));
class Freezeover(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Freezeover), 6);
class PlainPound(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.PlainPound), new AOEShapeCircle(4.56f));
class RaucousScritch(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.RaucousScritch), new AOEShapeCone(8.42f, 30.Degrees()));
class Hurl(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Hurl), 6);
class Spin(BossModule module) : Components.Cleave(module, ActionID.MakeSpell(AID.Spin), new AOEShapeCone(9.42f, 60.Degrees()), (uint)OID.BonusAddAbharamu);

class IcebeastStates : StateMachineBuilder
{
    public IcebeastStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Eyeshine>()
            .ActivateOnEnter<AbsoluteZero>()
            .ActivateOnEnter<Freezeover>()
            .ActivateOnEnter<PlainPound>()
            .ActivateOnEnter<Hurl>()
            .ActivateOnEnter<RaucousScritch>()
            .ActivateOnEnter<Spin>()
            .Raw.Update = () => module.Enemies(OID.Boss).All(e => e.IsDead) && module.Enemies(OID.BossAdd1).All(e => e.IsDead) && module.Enemies(OID.BossAdd2).All(e => e.IsDead) && module.Enemies(OID.BonusAddAbharamu).All(e => e.IsDead);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 268, NameID = 6650)]
public class Icebeast(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, -420), new ArenaBoundsCircle(20))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.BossAdd1))
            Arena.Actor(s, ArenaColor.Object);
        foreach (var s in Enemies(OID.BossAdd2))
            Arena.Actor(s, ArenaColor.Object);
        foreach (var s in Enemies(OID.BonusAddAbharamu))
            Arena.Actor(s, ArenaColor.Vulnerable);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var e in hints.PotentialTargets)
        {
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.BonusAddAbharamu => 3,
                OID.BossAdd1 or OID.BossAdd2 => 2,
                OID.Boss => 1,
                _ => 0
            };
        }
    }
}
