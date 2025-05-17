namespace BossMod.Endwalker.DeepDungeon.EurekaOrthos.D90Administrator;

public enum OID : uint
{
    Boss = 0x3D23, // R8.000, x1
    Helper = 0x233C, // R0.500, x16, Helper type
    OnionKnightDemiclone = 0x3E1C, // R0.500, x1
    InterceptorCube = 0x3D25, // R2.300, x0 (spawn during fight)
    InterceptorEgg = 0x3D24, // R2.300, x0 (spawn during fight)
    InterceptorOrb = 0x3D26, // R2.300, x0 (spawn during fight)
}

public enum AID : uint
{
    AetherochemicalLaserCube1 = 31452, // InterceptorCube->self, 6.0s cast, range 40 width 5 rect
    AetherochemicalLaserEgg1 = 31451, // InterceptorEgg->self, 6.0s cast, range 50 120-degree cone
    AetherochemicalLaserOrb = 31453, // InterceptorOrb->self, 6.0s cast, range 8-40 donut
    CrossLasers = 31448, // Boss->self, 6.0s cast, range 60 width 10 cross
    PeripheralLasers = 31447, // Boss->self, 6.0s cast, range 5-60 donut

    AetherochemicalLaserCube2 = 32833, // InterceptorCube->self, 8.0s cast, range 40 width 5 rect
    AetherochemicalLaserEgg2 = 32832, // InterceptorEgg->self, 8.0s cast, range 50 120-degree cone
    AutoAttack = 31457, // Boss->player, no cast, single-target
    HomingLaserBoss = 31460, // Boss->self, no cast, single-target
    HomingLaserHelper = 31461, // Helper->location, 3.0s cast, range 6 circle
    InterceptionSequence = 31450, // Boss->self, 3.0s cast, range 60 circle
    Laserstream = 31456, // Boss->self, 4.0s cast, range 60 circle
    ParallelExecution = 31454, // Boss->self, 3.0s cast, range 60 circle
    SalvoScript = 31455, // Boss->self, 3.0s cast, range 60 circle
    SupportSystems = 31449, // Boss->self, 3.0s cast, single-target

    PeripheralLasersModelChange1 = 31458, // Boss->self, no cast, single-target
    PeripheralLasersModelChange2 = 31459, // Boss->self, no cast, single-target
}

public enum IconID : uint
{
    Icon1 = 390,
    Icon2 = 391,
    Icon3 = 392,
}

class AetheroChemicalLaserCombo(BossModule module) : Components.GenericAOEs(module) // Pulled from the fork team, Currently a visual clutter but for now, functions... *-decently-*
{
    private static readonly AOEShape[] _shapes = [new AOEShapeCone(50, 60.Degrees()), new AOEShapeDonut(8, 60), new AOEShapeRect(40, 2.5f),
    new AOEShapeCross(60, 5), new AOEShapeDonut(5, 60)];
    private readonly Dictionary<uint, List<AOEInstance>> _icons = new() {
        { (uint)IconID.Icon1, [] },
        { (uint)IconID.Icon2, [] },
        { (uint)IconID.Icon3, [] }
    };
    private AOEInstance _boss;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var icon in _icons)
        {
            if (icon.Value != null && icon.Value.Count > 0)
            {
                foreach (var c in icon.Value)
                    yield return new(c.Shape, c.Origin, c.Rotation, c.Activation, ArenaColor.Danger);
                var nextIcon = _icons.FirstOrDefault(x => x.Key == icon.Key + 1).Value;
                if (nextIcon != null)
                    foreach (var c in nextIcon)
                        yield return new(c.Shape, c.Origin, c.Rotation, c.Activation, ArenaColor.AOE, false);
                if (_boss != default)
                    yield return new(_boss.Shape, _boss.Origin, _boss.Rotation, _boss.Activation, ArenaColor.AOE, false);
                yield break;
            }
        }
        if (_boss != default)
            yield return new(_boss.Shape, _boss.Origin, _boss.Rotation, _boss.Activation, ArenaColor.Danger);
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        var shapeIndex = (OID)actor.OID switch
        {
            OID.InterceptorCube => 2,
            OID.InterceptorOrb => 1,
            OID.InterceptorEgg => 0,
            _ => default
        };

        var activation = iconID switch
        {
            (uint)IconID.Icon1 => WorldState.FutureTime(7),
            (uint)IconID.Icon2 => WorldState.FutureTime(10.5f),
            (uint)IconID.Icon3 => WorldState.FutureTime(14),
            _ => default
        };

        _icons[iconID].Add(new(_shapes[shapeIndex], actor.Position, (OID)actor.OID == OID.InterceptorOrb ? default : actor.Rotation, activation));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        _boss = (AID)spell.Action.ID switch
        {
            AID.PeripheralLasers => new(_shapes[4], caster.Position, default, Module.CastFinishAt(spell)),
            AID.CrossLasers => new(_shapes[3], caster.Position, spell.Rotation, Module.CastFinishAt(spell)),
            _ => _boss
        };
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.AetherochemicalLaserEgg1 or AID.AetherochemicalLaserCube1 or AID.AetherochemicalLaserOrb)
        {
            foreach (var icon in _icons)
                if (icon.Value.Count > 0)
                {
                    icon.Value.RemoveAt(0);
                    break;
                }
        }
        if ((AID)spell.Action.ID is AID.PeripheralLasers or AID.CrossLasers)
            _boss = default;
    }
}

class AetherochemicalLaserLine(BossModule module) : Components.StandardAOEs(module, AID.AetherochemicalLaserCube1, new AOEShapeRect(40, 2.5f), 4)
{
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        return !Module.FindComponent<AetheroChemicalLaserCombo>()!.ActiveAOEs(slot, actor).Any()
            ? ActiveCasters.Select(c => new AOEInstance(Shape, c.Position, c.CastInfo!.Rotation, Module.CastFinishAt(c.CastInfo), ArenaColor.Danger, Risky)).Take(2)
                .Concat(ActiveCasters.Select(c => new AOEInstance(Shape, c.Position, c.CastInfo!.Rotation, Module.CastFinishAt(c.CastInfo), ArenaColor.AOE, Risky)).Take(4).Skip(2))
            : ([]);
    }
}

class HomingLasers(BossModule module) : Components.StandardAOEs(module, AID.HomingLaserHelper, 6);
class Laserstream(BossModule module) : Components.RaidwideCast(module, AID.Laserstream);

class SalvoScript(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoe = [];
    private new int NumCasts { get; set; }

    private static readonly AOEShapeRect _rect = new(40, 2.5f);
    private static readonly AOEShapeCone _cone = new(50, 60.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe.Take(14);

    public override void OnActorCreated(Actor actor)
    {
        if (NumCasts == 3)
        {
            AOEShape? shape = (OID)actor.OID switch
            {
                OID.InterceptorCube => _rect,
                OID.InterceptorEgg => _cone,
                _ => null
            };
            if (shape != null)
            {
                _aoe.Add(new(shape, actor.Position, actor.Rotation, WorldState.FutureTime(14f)));
            }
        }
    }
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.SupportSystems)
            ++NumCasts;
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.AetherochemicalLaserEgg2 or AID.AetherochemicalLaserCube2)
        {
            _aoe.Clear();
            NumCasts = 0;
        }
    }
}

class D90AdministratorStates : StateMachineBuilder
{
    public D90AdministratorStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AetheroChemicalLaserCombo>()
            .ActivateOnEnter<AetherochemicalLaserLine>()
            .ActivateOnEnter<HomingLasers>()
            .ActivateOnEnter<SalvoScript>()
            .ActivateOnEnter<Laserstream>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "LegendofIceman, Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 905, NameID = 12102)]
public class D90Administrator(WorldState ws, Actor primary) : BossModule(ws, primary, new(-300, -300), new ArenaBoundsSquare(20));
