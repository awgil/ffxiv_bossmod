namespace BossMod.Endwalker.TreasureHunt.ShiftingGymnasionAgonon.GymnasiouLeon;

public enum OID : uint
{
    Boss = 0x3D27, //R=5.95
    BossAdd = 0x3D28, //R=3.5
    BossHelper = 0x233C,
    BonusAddLyssa = 0x3D4E, //R=3.75
}

public enum AID : uint
{
    AutoAttack = 870, // 3D27/3D4E/3D28->player, no cast, single-target
    InfernoBlast = 32204, // 3D27->self, 3.5s cast, range 46 width 20 rect
    Roar = 32201, // 3D27->self, 3.0s cast, range 12 circle
    Pounce = 32200, // 3D27->player, 5.0s cast, single-target
    MagmaChamber = 32202, // 3D27->self, 3.0s cast, single-target
    MagmaChamber2 = 32203, // 233C->location, 3.0s cast, range 8 circle
    FlareStar = 32815, // 3D27->self, 3.0s cast, single-target
    HeavySmash = 32317, // 3D4E->location, 3.0s cast, range 6 circle
    FlareStar2 = 32816, // 233C->self, 7.0s cast, range 40 circle, AOE with dmg fall off, damage seems to stop falling after about range 10-12
    MarkOfTheBeast = 32205, // 3D28->self, 3.0s cast, range 8 120-degree cone
}

class InfernoBlast(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.InfernoBlast), new AOEShapeRect(46, 20));
class Roar(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Roar), new AOEShapeCircle(12));
class FlareStar(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.FlareStar), new AOEShapeCircle(12));
class MarkOfTheBeast(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.MarkOfTheBeast), new AOEShapeCone(8, 60.Degrees()));
class Pounce(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.Pounce));
class MagmaChamber(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.MagmaChamber2), 8);
class HeavySmash(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.HeavySmash), 6);

class LeonStates : StateMachineBuilder
{
    public LeonStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<InfernoBlast>()
            .ActivateOnEnter<Roar>()
            .ActivateOnEnter<FlareStar>()
            .ActivateOnEnter<MarkOfTheBeast>()
            .ActivateOnEnter<Pounce>()
            .ActivateOnEnter<MagmaChamber>()
            .ActivateOnEnter<HeavySmash>()
            .Raw.Update = () => module.Enemies(OID.Boss).All(e => e.IsDead) && module.Enemies(OID.BossAdd).All(e => e.IsDead) && module.Enemies(OID.BonusAddLyssa).All(e => e.IsDead);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 909, NameID = 11997)]
public class Leon(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(20))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.BossAdd))
            Arena.Actor(s, ArenaColor.Object);
        foreach (var s in Enemies(OID.BonusAddLyssa))
            Arena.Actor(s, ArenaColor.Vulnerable);
    }

    public override void CalculateAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.CalculateAIHints(slot, actor, assignment, hints);
        foreach (var e in hints.PotentialTargets)
        {
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.BonusAddLyssa => 3,
                OID.BossAdd => 2,
                OID.Boss => 1,
                _ => 0
            };
        }
    }
}
