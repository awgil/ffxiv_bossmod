namespace BossMod.Stormblood.TreasureHunt.ShiftingAltarsOfUznair.AltarSkatene;

public enum OID : uint
{
    Boss = 0x2535, //R=4.48
    BossAdd = 0x255E, //R=0.9
    BossHelper = 0x233C,
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    AutoAttack2 = 6499, // BossAdd->player, no cast, single-target
    RecklessAbandon = 13311, // Boss->player, 3.0s cast, single-target
    Tornado = 13309, // Boss->location, 3.0s cast, range 6 circle
    VoidCall = 13312, // Boss->self, 3.5s cast, single-target
    Chirp = 13310, // Boss->self, 3.5s cast, range 8+R circle
}

class Chirp(BossModule module) : Components.StandardAOEs(module, AID.Chirp, new AOEShapeCircle(12.48f));
class Tornado(BossModule module) : Components.StandardAOEs(module, AID.Tornado, 6);
class VoidCall(BossModule module) : Components.CastHint(module, AID.VoidCall, "Calls adds");
class RecklessAbandon(BossModule module) : Components.SingleTargetDelayableCast(module, AID.RecklessAbandon);

class SkateneStates : StateMachineBuilder
{
    public SkateneStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Chirp>()
            .ActivateOnEnter<Tornado>()
            .ActivateOnEnter<VoidCall>()
            .ActivateOnEnter<RecklessAbandon>()
            .Raw.Update = () => module.Enemies(OID.Boss).All(e => e.IsDead) && module.Enemies(OID.BossAdd).All(e => e.IsDead);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 586, NameID = 7587)]
public class Skatene(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(20))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.BossAdd))
            Arena.Actor(s, ArenaColor.Object);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var e in hints.PotentialTargets)
        {
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.BossAdd => 2,
                OID.Boss => 1,
                _ => 0
            };
        }
    }
}
