namespace BossMod.Endwalker.Quest.LifeEphemeralPathEternal;

class AetherstreamTether(BossModule module) : Components.BaitAwayTethers(module, new AOEShapeRect(50, 2), (uint)TetherID.Noulith)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.AetherstreamPlayer or AID.AetherstreamTank)
            CurrentBaits.RemoveAll(x => x.Target.InstanceID == spell.MainTargetID);
    }
}

class Tracheostomy : Components.StandardAOEs
{
    public Tracheostomy(BossModule module) : base(module, AID.Tracheostomy, new AOEShapeDonut(10, 20))
    {
        WorldState.Actors.EventStateChanged.Subscribe((act) =>
        {
            if (act.OID == 0x1EA1A1 && act.EventState == 7)
                Arena.Bounds = new ArenaBoundsCircle(20);
        });
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (spell.Action == WatchedAction)
            Arena.Bounds = new ArenaBoundsCircle(10);
    }
}

class RightScalpel(BossModule module) : Components.StandardAOEs(module, AID.RightScalpel, new AOEShapeCone(15, 105.Degrees()));
class LeftScalpel(BossModule module) : Components.StandardAOEs(module, AID.LeftScalpel, new AOEShapeCone(15, 105.Degrees()));
class Laparotomy(BossModule module) : Components.StandardAOEs(module, AID.Laparotomy, new AOEShapeCone(15, 60.Degrees()));
class Amputation(BossModule module) : Components.StandardAOEs(module, AID.Amputation, new AOEShapeCone(20, 60.Degrees()));

class Hypothermia(BossModule module) : Components.RaidwideCast(module, AID.Hypothermia);
class Cryonics(BossModule module) : Components.StackWithCastTargets(module, AID.Cryonics, 6);
class Craniotomy(BossModule module) : Components.RaidwideCast(module, AID.Craniotomy);
class RightLeftScalpel1(BossModule module) : Components.StandardAOEs(module, AID.RightLeftScalpel, new AOEShapeCone(15, 105.Degrees()));
class RightLeftScalpel2(BossModule module) : Components.StandardAOEs(module, AID.RightLeftScalpel1, new AOEShapeCone(15, 105.Degrees()));
class LeftRightScalpel1(BossModule module) : Components.StandardAOEs(module, AID.LeftRightScalpel, new AOEShapeCone(15, 105.Degrees()));
class LeftRightScalpel2(BossModule module) : Components.StandardAOEs(module, AID.LeftRightScalpel1, new AOEShapeCone(15, 105.Degrees()));

class EnhancedNoulith(BossModule module) : Components.Adds(module, (uint)OID.EnhancedNoulith)
{
    private readonly List<(Actor, Actor)> Tethers = [];
    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.Craniotomy && WorldState.Actors.Find(tether.Target) is Actor target)
            Tethers.Add((source, target));
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.Craniotomy)
            Tethers.RemoveAll(t => t.Item2 == actor);
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach (var t in Tethers)
            Arena.AddLine(t.Item1.Position, t.Item2.Position, ArenaColor.Danger);
    }
}
class Frigotherapy(BossModule module) : Components.SpreadFromCastTargets(module, AID.Frigotherapy1, 5);

class GuildivainOfTheTaintedEdgeStates : StateMachineBuilder
{
    public GuildivainOfTheTaintedEdgeStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AetherstreamTether>()
            .ActivateOnEnter<Tracheostomy>()
            .ActivateOnEnter<RightScalpel>()
            .ActivateOnEnter<LeftScalpel>()
            .ActivateOnEnter<Laparotomy>()
            .ActivateOnEnter<Amputation>()
            .ActivateOnEnter<Hypothermia>()
            .ActivateOnEnter<Cryonics>()
            .ActivateOnEnter<EnhancedNoulith>()
            .ActivateOnEnter<Craniotomy>()
            .ActivateOnEnter<RightLeftScalpel1>()
            .ActivateOnEnter<RightLeftScalpel2>()
            .ActivateOnEnter<LeftRightScalpel1>()
            .ActivateOnEnter<LeftRightScalpel2>()
            .ActivateOnEnter<Frigotherapy>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 69608, NameID = 10733, PrimaryActorOID = (uint)OID.BossP2)]
public class GuildivainOfTheTaintedEdge(WorldState ws, Actor primary) : BossModule(ws, primary, new(224.8f, -855.8f), new ArenaBoundsCircle(20));
