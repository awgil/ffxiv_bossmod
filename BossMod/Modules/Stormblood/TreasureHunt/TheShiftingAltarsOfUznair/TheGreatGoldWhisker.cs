namespace BossMod.Stormblood.TreasureHunt.ShiftingAltarsOfUznair.TheGreatGoldWhisker;

public enum OID : uint
{
    Boss = 0x2541, //R=2.4
    BossHelper = 0x233C,
    BonusAddGoldWhisker = 0x2544, // R0.540
}

public enum AID : uint
{
    AutoAttack = 870, // Boss/BonusAddGoldWhisker->player, no cast, single-target
    TripleTrident = 13364, // Boss->players, 3.0s cast, single-target
    Tingle = 13365, // Boss->self, 4.0s cast, range 10+R circle
    FishOutOfWater = 13366, // Boss->self, 3.0s cast, single-target
    Telega = 9630, // BonusAddGoldWhisker->self, no cast, single-target
}

class TripleTrident(BossModule module) : Components.SingleTargetDelayableCast(module, AID.TripleTrident);
class FishOutOfWater(BossModule module) : Components.CastHint(module, AID.FishOutOfWater, "Spawns adds");
class Tingle(BossModule module) : Components.StandardAOEs(module, AID.Tingle, new AOEShapeCircle(12.4f));

class TheGreatGoldWhiskerStates : StateMachineBuilder
{
    public TheGreatGoldWhiskerStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<TripleTrident>()
            .ActivateOnEnter<FishOutOfWater>()
            .ActivateOnEnter<Tingle>()
            .Raw.Update = () => module.Enemies(OID.Boss).All(e => e.IsDead) && module.Enemies(OID.BonusAddGoldWhisker).All(e => e.IsDead);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 586, NameID = 7599)]
public class TheGreatGoldWhisker(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(20))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.BonusAddGoldWhisker))
            Arena.Actor(s, ArenaColor.Vulnerable);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var e in hints.PotentialTargets)
        {
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.BonusAddGoldWhisker => 2,
                OID.Boss => 1,
                _ => 0
            };
        }
    }
}
