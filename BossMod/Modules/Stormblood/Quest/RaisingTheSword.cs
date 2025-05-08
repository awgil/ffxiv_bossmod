namespace BossMod.Stormblood.Quest.RaisingTheSword;

public enum OID : uint
{
    Boss = 0x1B51,
    Helper = 0x233C,
    AldisSwordOfNald = 0x18D6, // R0.500, x10
    TaintedWindSprite = 0x1B52, // R1.000, x0 (spawn during fight)
}

public enum AID : uint
{
    ShudderingSwipeCast = 8136, // Boss->player, 3.0s cast, single-target
    ShudderingSwipeAOE = 8137, // 18D6->self, 3.0s cast, range 60+R 30-degree cone
    NaldsWhisper = 8141, // 18D6->self, 9.0s cast, range 4 circle
    VictorySlash = 8134, // Boss->self, 3.0s cast, range 6+R 120-degree cone
}

class VictorySlash(BossModule module) : Components.StandardAOEs(module, AID.VictorySlash, new AOEShapeCone(6.5f, 60.Degrees()));
class ShudderingSwipeCone(BossModule module) : Components.StandardAOEs(module, AID.ShudderingSwipeAOE, new AOEShapeCone(60, 15.Degrees()));
class ShudderingSwipeKB(BossModule module) : Components.Knockback(module, AID.ShudderingSwipeCast, stopAtWall: true)
{
    private TheFourWinds? winds;
    private readonly List<Actor> Casters = [];

    public override IEnumerable<Source> Sources(int slot, Actor actor) => Casters.Select(c => new Source(c.Position, 10, Module.CastFinishAt(c.CastInfo), null, c.AngleTo(actor), Kind.DirForward));

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ShudderingSwipeCast)
            Casters.Add(caster);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ShudderingSwipeCast)
            Casters.Remove(caster);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        winds ??= Module.FindComponent<TheFourWinds>();

        var aoes = (winds?.Sources(Module) ?? []).Select(a => ShapeContains.Circle(a.Position, 6)).ToList();
        if (aoes.Count == 0)
            return;

        var windzone = ShapeContains.Union(aoes);
        if (Casters.FirstOrDefault() is Actor c)
            hints.AddForbiddenZone(p =>
            {
                var dir = c.DirectionTo(p);
                var projected = p + dir * 10;
                return windzone(projected);
            }, Module.CastFinishAt(c.CastInfo));
    }
}
class NaldsWhisper(BossModule module) : Components.StandardAOEs(module, AID.NaldsWhisper, new AOEShapeCircle(20));
class TheFourWinds(BossModule module) : Components.PersistentVoidzone(module, 6, m => m.Enemies(OID.TaintedWindSprite).Where(x => x.EventState != 7));

class AldisSwordOfNaldStates : StateMachineBuilder
{
    public AldisSwordOfNaldStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<TheFourWinds>()
            .ActivateOnEnter<NaldsWhisper>()
            .ActivateOnEnter<VictorySlash>()
            .ActivateOnEnter<ShudderingSwipeKB>()
            .ActivateOnEnter<ShudderingSwipeCone>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 270, NameID = 6311)]
public class AldisSwordOfNald(WorldState ws, Actor primary) : BossModule(ws, primary, new(-89.3f, 0), new ArenaBoundsCircle(20.5f));
