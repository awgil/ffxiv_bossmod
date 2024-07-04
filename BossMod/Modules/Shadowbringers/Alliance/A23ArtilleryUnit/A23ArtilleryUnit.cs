namespace BossMod.Shadowbringers.Alliance.A23ArtilleryUnit;

class ManeuverVoltArray(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.ManeuverVoltArray));

class LowerLaser1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.LowerLaser1), new AOEShapeCone(30, 30.Degrees()));
class UpperLaser1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.UpperLaser1), new AOEShapeDonutSector(6, 16, 30.Degrees()));
class UpperLaser2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.UpperLaser2), new AOEShapeDonutSector(16, 23, 30.Degrees()));
class UpperLaser3(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.UpperLaser3), new AOEShapeDonutSector(23, 30, 30.Degrees()));

class EnergyBombardment2(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.EnergyBombardment2), 4);
class UnknownWeaponskill(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.UnknownWeaponskill), 8);
class ManeuverRevolvingLaser(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ManeuverRevolvingLaser), new AOEShapeDonut(4, 60));

class R010Laser(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.R010Laser), new AOEShapeRect(60, 6));
class R030Hammer(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.R030Hammer), new AOEShapeCircle(18));

class UpperLaser(BossModule module) : Components.GenericAOEs(module, ActionID.MakeSpell(AID.UpperLaser4))
{
    private readonly List<Actor> _castersUpperLaser1 = [];
    private readonly List<Actor> _castersUpperLaser2 = [];
    private readonly List<Actor> _castersUpperLaser3 = [];

    private static readonly AOEShape _shapeUpperLaser1 = new AOEShapeDonutSector(6, 16, 30.Degrees());
    private static readonly AOEShape _shapeUpperLaser2 = new AOEShapeDonutSector(16, 23, 30.Degrees());
    private static readonly AOEShape _shapeUpperLaser3 = new AOEShapeDonutSector(23, 30, 30.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        return _castersUpperLaser1.Count > 0
            ? _castersUpperLaser1.Select(c => new AOEInstance(_shapeUpperLaser1, c.Position, c.CastInfo!.Rotation, c.CastInfo!.NPCFinishAt))
            : _castersUpperLaser2.Count > 0
            ? _castersUpperLaser2.Select(c => new AOEInstance(_shapeUpperLaser2, c.Position, c.CastInfo!.Rotation, c.CastInfo!.NPCFinishAt))
            : _castersUpperLaser3.Select(c => new AOEInstance(_shapeUpperLaser3, c.Position, c.CastInfo!.Rotation, c.CastInfo!.NPCFinishAt));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        CastersForSpell(spell.Action)?.Add(caster);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        CastersForSpell(spell.Action)?.Remove(caster);
    }

    private List<Actor>? CastersForSpell(ActionID spell) => (AID)spell.ID switch
    {
        AID.UpperLaser1 => _castersUpperLaser1,
        AID.UpperLaser2 => _castersUpperLaser2,
        AID.UpperLaser3 => _castersUpperLaser3,
        _ => null
    };
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 736, NameID = 9650)]
public class A23ArtilleryUnit(WorldState ws, Actor primary) : BossModule(ws, primary, DefaultBounds.Center, DefaultBounds)
{
    private static readonly List<Shape> union = [new Circle(new(200, -100), 29.5f)];
    private static readonly List<Shape> difference = [new Circle(new(200, -100), 6)];

    public static readonly ArenaBoundsComplex DefaultBounds = new(union, difference);
}
