namespace BossMod.Shadowbringers.Dungeon.D04MalikahsWell.D041GreaterArmadillo;

public enum OID : uint
{
    Boss = 0x2679, // R=4.0
    MorningStar = 0x267E, // R=1.68
    PackArmadillo = 0x2683, // R=2.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    AutoAttack2 = 870, // PackArmadillo->player, no cast, single-target
    StoneFlail = 15589, // Boss->player, 4.5s cast, single-target
    FallingRock = 15594, // Helper->location, 3.0s cast, range 4 circle
    HeadToss = 15590, // Boss->player, 5.0s cast, range 6 circle
    RightRoundVisual = 15591, // Boss->MorningStar, 2.5s cast, single-target
    RightRound = 15592, // Helper->self, no cast, range 9 circle, knockback 20, away from source
    FlailSmash = 15593, // Boss->location, 3.0s cast, range 40 circle, distance based
    Earthshake = 15929, // Helper->self, 3.5s cast, range 10-20 donut
    Rehydration = 16776 // PackArmadillo->self, 5.0s cast, single-target
}

class StoneFlail(BossModule module) : Components.SingleTargetCast(module, AID.StoneFlail);
class FallingRock(BossModule module) : Components.StandardAOEs(module, AID.FallingRock, 4);
class FlailSmash(BossModule module) : Components.StandardAOEs(module, AID.FlailSmash, 10);
class HeadToss(BossModule module) : Components.StackWithCastTargets(module, AID.HeadToss, 6, 4, 4);
class Earthshake(BossModule module) : Components.StandardAOEs(module, AID.Earthshake, new AOEShapeDonut(10, 20));
class Rehydration(BossModule module) : Components.CastInterruptHint(module, AID.Rehydration, showNameInHint: true);

class RightRound(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(10);
    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        // the origin of the rightround cast event seems to be weird, using the primaryactor position is not pixel perfect, seen variances of almost 1y, so i increased the circle radius from 9 to 10
        if ((AID)spell.Action.ID == AID.RightRoundVisual)
            _aoe = new(circle, Module.PrimaryActor.Position, default, Module.CastFinishAt(spell, 0.9f));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.RightRound)
            _aoe = null;
    }
}

class D041GreaterArmadilloStates : StateMachineBuilder
{
    public D041GreaterArmadilloStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<RightRound>()
            .ActivateOnEnter<StoneFlail>()
            .ActivateOnEnter<FallingRock>()
            .ActivateOnEnter<FlailSmash>()
            .ActivateOnEnter<HeadToss>()
            .ActivateOnEnter<Earthshake>()
            .ActivateOnEnter<Rehydration>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "The Combat Reborn Team (Malediktus), Ported by Herculezz", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 656, NameID = 8252)]
public class D041GreaterArmadillo(WorldState ws, Actor primary) : BossModule(ws, primary, new(278, 204), new ArenaBoundsCircle(19.5f))
{
    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var e in hints.PotentialTargets)
        {
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.PackArmadillo => 2,
                OID.Boss => 1,
                _ => 0
            };
        }
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.PackArmadillo), ArenaColor.Enemy);
    }
}
