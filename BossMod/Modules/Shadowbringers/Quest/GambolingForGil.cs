namespace BossMod.Shadowbringers.Quest.GambolingForGil;

public enum OID : uint
{
    Boss = 0x29D2, // R0.500, x1
    Whirlwind = 0x29D5, // R1.000, x0 (spawn during fight)
}

public enum AID : uint
{
    WarDance = 17197, // Boss->self, 3.0s cast, range 5 circle
    CharmingChasse = 17198, // Boss->self, 3.0s cast, range 40 circle
    HannishFire1 = 17204, // 29D6->location, 3.3s cast, range 6 circle
    Foxshot = 17289, // Boss->player, 6.0s cast, width 4 rect charge
    HannishWaters = 17214, // 2A0B->self, 5.0s cast, range 40+R 30-degree cone
    RanaasFinish = 15646, // Boss->self, 6.0s cast, range 15 circle
}

class Foxshot(BossModule module) : Components.BaitAwayChargeCast(module, AID.Foxshot, 2);
class FoxshotKB(BossModule module) : Components.Knockback(module, stopAtWall: true)
{
    private readonly List<Actor> Casters = [];
    private Whirlwind? ww;

    public override IEnumerable<Source> Sources(int slot, Actor actor) => Casters.Select(c => new Source(c.Position, 25, Module.CastFinishAt(c.CastInfo)));

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        ww ??= Module.FindComponent<Whirlwind>();

        if (Casters.FirstOrDefault() is not Actor source)
            return;

        var sources = ww?.Sources(Module).Select(p => p.Position).ToList() ?? [];
        if (sources.Count == 0)
            return;

        hints.AddForbiddenZone(p =>
        {
            foreach (var s in sources)
                if (Intersect.RayCircle(source.Position, source.DirectionTo(p), s, 6) < 1000)
                    return true;

            return false;
        }, Module.CastFinishAt(source.CastInfo));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Foxshot)
            Casters.Add(caster);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Foxshot)
            Casters.Remove(caster);
    }
}
class Whirlwind(BossModule module) : Components.PersistentVoidzone(module, 6, m => m.Enemies(OID.Whirlwind).Where(x => !x.IsDead));
class WarDance(BossModule module) : Components.StandardAOEs(module, AID.WarDance, new AOEShapeCircle(5));
class CharmingChasse(BossModule module) : Components.CastGaze(module, AID.CharmingChasse);
class HannishFire(BossModule module) : Components.StandardAOEs(module, AID.HannishFire1, 6);
class HannishWaters(BossModule module) : Components.StandardAOEs(module, AID.HannishWaters, new AOEShapeCone(40, 15.Degrees()));
class RanaasFinish(BossModule module) : Components.StandardAOEs(module, AID.RanaasFinish, new AOEShapeCircle(15));

class RanaaMihgoStates : StateMachineBuilder
{
    public RanaaMihgoStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<WarDance>()
            .ActivateOnEnter<CharmingChasse>()
            .ActivateOnEnter<HannishFire>()
            .ActivateOnEnter<HannishWaters>()
            .ActivateOnEnter<RanaasFinish>()
            .ActivateOnEnter<Whirlwind>()
            .ActivateOnEnter<Foxshot>()
            .ActivateOnEnter<FoxshotKB>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 68786, NameID = 8489)]
public class RanaaMihgo(WorldState ws, Actor primary) : BossModule(ws, primary, new(520.47f, 124.99f), WeirdBounds)
{
    public static readonly ArenaBoundsCustom WeirdBounds = new(17.5f, new(CurveApprox.Ellipse(17.5f, 16f, 0.01f)));
}
