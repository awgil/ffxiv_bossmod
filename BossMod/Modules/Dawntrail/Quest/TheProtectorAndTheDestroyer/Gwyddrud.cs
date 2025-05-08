namespace BossMod.Dawntrail.Quest.TheProtectorAndTheDestroyer;

class RollingThunder(BossModule module) : Components.StandardAOEs(module, AID.RollingThunder, new AOEShapeCone(20, 22.5f.Degrees()))
{
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var i = 0;
        foreach (var aoe in base.ActiveAOEs(slot, actor))
        {
            if (++i > 6)
                break;
            yield return aoe with { Color = i <= 2 ? ArenaColor.Danger : ArenaColor.AOE };
        }
    }
}
class CracklingHowl(BossModule module) : Components.RaidwideCast(module, AID.CracklingHowl);
class VioletVoltage(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> aoes = [];

    private DateTime? Finish;
    private int CastsToRecord;

    private static readonly AOEShape Shape = new AOEShapeCone(20, 90.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        for (var i = 0; i < Math.Min(2, aoes.Count); i++)
            yield return aoes[i] with { Color = i == 0 ? ArenaColor.Danger : ArenaColor.AOE };
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var aoe in ActiveAOEs(slot, actor).Take(1))
            hints.AddForbiddenZone(aoe.Shape, aoe.Origin, aoe.Rotation, aoe.Activation);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.VioletVoltage3X)
        {
            Finish = Module.CastFinishAt(spell);
            CastsToRecord = 3;
        }

        if (spell.Action.ID == (uint)AID.VioletVoltage4X)
        {
            Finish = Module.CastFinishAt(spell);
            CastsToRecord = 4;
        }

        if (spell.Action.ID == (uint)AID.VioletVoltageVisual && Finish != null && CastsToRecord > 0)
        {
            aoes.Add(new AOEInstance(Shape, caster.Position, caster.Rotation, Finish.Value.AddSeconds(aoes.Count * 2)));
            CastsToRecord--;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.VioletVoltageAOE && aoes.Count > 0)
            aoes.RemoveAt(0);
    }
}
class UntamedCurrent(BossModule module) : Components.GroupedAOEs(module, [AID.UntamedCurrent1, AID.UntamedCurrent2, AID.UntamedCurrent4, AID.UntamedCurrent5, AID.UntamedCurrent6, AID.UntamedCurrent7, AID.UnnamedCurrent1, AID.UnnamedCurrent2], new AOEShapeCircle(5));

class UntamedCurrentSpread(BossModule module) : Components.SpreadFromCastTargets(module, AID.UntamedCurrentSpread, 5);

class BallOfLevin(BossModule module) : Components.Adds(module, (uint)OID.BallOfLevin)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var h in hints.PotentialTargets)
            if (h.Actor.OID == (uint)OID.BallOfLevin && h.Actor.Position.InCircle(Arena.Center, 20))
                h.Priority = (int)((20 - h.Actor.DistanceToHitbox(Module.PrimaryActor)) * 100);
    }
}
class SuperchargedLevin(BossModule module) : Components.Adds(module, (uint)OID.SuperchargedLevin);

class GwyddrudStates : StateMachineBuilder
{
    public GwyddrudStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<CracklingHowl>()
            .ActivateOnEnter<VioletVoltage>()
            .ActivateOnEnter<RollingThunder>()
            .ActivateOnEnter<UntamedCurrent>()
            .ActivateOnEnter<UntamedCurrentSpread>()
            .ActivateOnEnter<BallOfLevin>()
            .ActivateOnEnter<SuperchargedLevin>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 70478, NameID = 13170, PrimaryActorOID = (uint)OID.BossP2)]
public class Gwyddrud(WorldState ws, Actor primary) : BossModule(ws, primary, new(349, -14), new ArenaBoundsCircle(19.5f));
