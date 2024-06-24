namespace BossMod.Endwalker.DeepDungeon.EurekaOrthos.DD90Administrator;

public enum OID : uint
{
    Boss = 0x3D23, // R5.950
    EggInterceptor = 0x3D24, // R2.3
    SquareInterceptor = 0x3D25, // R2.3
    OrbInterceptor = 0x3D26, // R2.3
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 31457, // Boss->player, no cast, single-target

    AetherochemicalLaserCone = 31451, // EggInterceptor->self, 6.0s cast, range 50 120-degree cone
    AetherochemicalLaserDonut = 31453, // OrbInterceptor->self, 6.0s cast, range 8-40 donut
    AetherochemicalLaserLine = 31452, // SquareInterceptor->self, 6.0s cast, range 40 width 5 rect

    AetherochemicalLaserCone2 = 32832, // EggInterceptor->self, 8.0s cast, range 50 120-degree cone
    AetherochemicalLaserLine2 = 32833, // SquareInterceptor->self, 8.0s cast, range 40 width 5 rect

    CrossLaser = 31448, // Boss->self, 6.0s cast, range 60 width 10 cross

    PeripheralLasersModelChange1 = 31458, // Boss->self, no cast, single-target
    PeripheralLasersModelChange2 = 31459, // Boss->self, no cast, single-target
    PeripheralLasers = 31447, // Boss->self, 6.0s cast, range 5-60 donut

    HomingLaserVisual = 31460, // Boss->self, no cast, single-target
    HomingLaser = 31461, // Helper->location, 3.0s cast, range 6 circle

    Laserstream = 31456, // Boss->self, 4.0s cast, range 60 circle, raidwide
    ParallelExecution = 31454, // Boss->self, 3.0s cast, range 60 circle, starts action combo

    SalvoScript = 31455, // Boss->self, 3.0s cast, range 60 circle
    SupportSystems = 31449, // Boss->self, 3.0s cast, single-target

    InterceptionSequence = 31450 // Boss->self, 3.0s cast, range 60 circle, starts action combo
}

public enum IconID : uint
{
    Icon1 = 390,
    Icon2 = 391,
    Icon3 = 392
}

class AetheroChemicalLaserCombo(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShape[] _shapes = [new AOEShapeCone(50, 60.Degrees()), new AOEShapeDonut(8, 60), new AOEShapeRect(40, 2.5f),
    new AOEShapeCross(60, 5), new AOEShapeDonut(5, 60)];

    private readonly Dictionary<uint, List<AOEInstance>> _icons = new() {
        { (uint)IconID.Icon1, new List<AOEInstance>() },
        { (uint)IconID.Icon2, new List<AOEInstance>() },
        { (uint)IconID.Icon3, new List<AOEInstance>() }
    };
    private AOEInstance _boss = default;

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
                {
                    foreach (var c in nextIcon)
                        yield return new(c.Shape, c.Origin, c.Rotation, c.Activation, ArenaColor.AOE, false);
                }
                if (_boss != default)
                    yield return new(_boss.Shape, _boss.Origin, _boss.Rotation, _boss.Activation, ArenaColor.AOE, false);
                yield break;
            }
        }
        if (_boss != default)
            yield return new(_boss.Shape, _boss.Origin, _boss.Rotation, _boss.Activation, ArenaColor.Danger);
    }

    public override void OnEventIcon(Actor actor, uint iconID)
    {
        var shapeIndex = (OID)actor.OID switch
        {
            OID.SquareInterceptor => 2,
            OID.OrbInterceptor => 1,
            OID.EggInterceptor => 0,
            _ => default
        };

        var activation = iconID switch
        {
            (uint)IconID.Icon1 => Module.WorldState.FutureTime(7),
            (uint)IconID.Icon2 => Module.WorldState.FutureTime(10.5f),
            (uint)IconID.Icon3 => Module.WorldState.FutureTime(14),
            _ => default
        };

        _icons[iconID].Add(new AOEInstance(_shapes[shapeIndex], actor.Position, (OID)actor.OID == OID.OrbInterceptor ? default : actor.Rotation, activation));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        _boss = (AID)spell.Action.ID switch
        {
            AID.PeripheralLasers => new AOEInstance(_shapes[4], caster.Position, default, spell.NPCFinishAt),
            AID.CrossLaser => new AOEInstance(_shapes[3], caster.Position, spell.Rotation, spell.NPCFinishAt),
            _ => _boss
        };
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.AetherochemicalLaserCone or AID.AetherochemicalLaserLine or AID.AetherochemicalLaserDonut)
        {
            foreach (var icon in _icons)
                if (icon.Value.Count > 0)
                {
                    icon.Value.RemoveAt(0);
                    break;
                }
        }
        if ((AID)spell.Action.ID is AID.PeripheralLasers or AID.CrossLaser)
            _boss = default;
    }
}

class AetherLaserLine(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.AetherochemicalLaserLine), new AOEShapeRect(40, 2.5f), 4)
{
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        return !Module.FindComponent<AetheroChemicalLaserCombo>()!.ActiveAOEs(slot, actor).Any()
            ? ActiveCasters.Select(c => new AOEInstance(Shape, c.Position, c.CastInfo!.Rotation, c.CastInfo.NPCFinishAt, ArenaColor.Danger, Risky)).Take(2)
            .Concat(ActiveCasters.Select(c => new AOEInstance(Shape, c.Position, c.CastInfo!.Rotation, c.CastInfo.NPCFinishAt, ArenaColor.AOE, Risky)).Take(4).Skip(2))
            : ([]);
    }
}

class AetherLaserLine2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.AetherochemicalLaserLine2), new AOEShapeRect(40, 2.5f));
class AetherLaserCone(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.AetherochemicalLaserCone2), new AOEShapeCone(50, 60.Degrees()));
class HomingLasers(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.HomingLaser), 6);
class Laserstream(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Laserstream));

class DD90AdministratorStates : StateMachineBuilder
{
    public DD90AdministratorStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AetheroChemicalLaserCombo>()
            .ActivateOnEnter<AetherLaserLine>()
            .ActivateOnEnter<AetherLaserLine2>()
            .ActivateOnEnter<AetherLaserCone>()
            .ActivateOnEnter<Laserstream>()
            .ActivateOnEnter<HomingLasers>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "legendoficeman, Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 905, NameID = 12102)]
public class DD90Administrator(WorldState ws, Actor primary) : BossModule(ws, primary, new(-300, -300), new ArenaBoundsSquare(20));
