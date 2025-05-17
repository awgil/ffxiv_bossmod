using BossMod.Modules.Stormblood.Foray;

namespace BossMod.Stormblood.Foray.BaldesionArsenal.Art;

public enum OID : uint
{
    Boss = 0x265A, // R2.700, x1
    Helper = 0x265C, // R0.500, x20, mixed types
    Orlasrach = 0x265B, // R2.700, x4
}

public enum AID : uint
{
    AutoAttack = 14678, // Boss->player, no cast, single-target
    Thricecull = 14644, // Boss->player, 4.0s cast, single-target
    Legendcarver = 14632, // Boss->self, 4.5s cast, range 15 circle
    Legendspinner = 14633, // Boss->self, 4.5s cast, range ?-22 donut
    AcallamNaSenorach = 14645, // Boss->self, 4.0s cast, range 60 circle
    Mythcall = 14631, // Boss->self, 2.0s cast, single-target
    Mythspinner = 14635, // Orlasrach->self, no cast, range ?-22 donut
    Mythcarver = 14634, // Orlasrach->self, no cast, range 15 circle
    LegendaryGeas = 14642, // Boss->location, 4.0s cast, range 8 circle
    DefilersDeserts = 14643, // Helper->self, 3.5s cast, range 35+R width 8 rect
}

class DefilersDeserts(BossModule module) : Components.StandardAOEs(module, AID.DefilersDeserts, new AOEShapeRect(36, 4));
class AcallamNaSenorach(BossModule module) : Components.RaidwideCast(module, AID.AcallamNaSenorach);
class Thricecull(BossModule module) : Components.SingleTargetCast(module, AID.Thricecull);
class Legendcarver(BossModule module) : Components.StandardAOEs(module, AID.Legendcarver, new AOEShapeCircle(15))
{
    private Mythcarver? mc;

    public override void Update()
    {
        mc ??= Module.FindComponent<Mythcarver>();
        Color = mc?.Casters.Count > 0 ? ArenaColor.Danger : ArenaColor.AOE;
    }
}
class Legendspinner(BossModule module) : Components.StandardAOEs(module, AID.Legendspinner, new AOEShapeDonut(7, 22))
{
    private Mythspinner? ms;

    public override void Update()
    {
        ms ??= Module.FindComponent<Mythspinner>();
        Color = ms?.Casters.Count > 0 ? ArenaColor.Danger : ArenaColor.AOE;
    }
}

abstract class SpearAOEs(BossModule module, Enum bossCast, AOEShape shape) : Components.GenericAOEs(module)
{
    public readonly List<Actor> Casters = [];
    private DateTime Activation;
    public readonly ActionID BossCast = ActionID.MakeSpell(bossCast);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Activation == default ? [] : Casters.Select(c => new AOEInstance(shape, c.Position, Activation: Activation));

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == 1)
            Casters.Add(source);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == BossCast && Casters.Count > 0)
            Activation = WorldState.FutureTime(7.5f);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (Casters.Remove(caster))
            Activation = default;
    }
}

class Mythcarver(BossModule module) : SpearAOEs(module, AID.Legendcarver, new AOEShapeCircle(15));
class Mythspinner(BossModule module) : SpearAOEs(module, AID.Legendspinner, new AOEShapeDonut(7, 22));
class LegendaryGeas(BossModule module) : Components.StandardAOEs(module, AID.LegendaryGeas, 8);

class ArtStates : StateMachineBuilder
{
    public ArtStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AcallamNaSenorach>()
            .ActivateOnEnter<Thricecull>()
            .ActivateOnEnter<Legendcarver>()
            .ActivateOnEnter<Legendspinner>()
            .ActivateOnEnter<Mythcarver>()
            .ActivateOnEnter<Mythspinner>()
            .ActivateOnEnter<LegendaryGeas>()
            .ActivateOnEnter<DefilersDeserts>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 639, NameID = 7968)]
public class Art(WorldState ws, Actor primary) : BAModule(ws, primary, new(-129, 748), new ArenaBoundsCircle(30));
