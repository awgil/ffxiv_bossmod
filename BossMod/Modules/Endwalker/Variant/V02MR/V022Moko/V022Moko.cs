namespace BossMod.Endwalker.Variant.V02MR.V022Moko;

class AzureAuspice(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.AzureAuspice), new AOEShapeDonut(6, 60));
class BoundlessAzure(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.BoundlessAzureAOE), new AOEShapeRect(60, 5, 60));
class KenkiRelease(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.KenkiRelease));
class IronRain(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.IronRain), 10);

class IaiKasumiGiri1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.IaiKasumiGiri1), new AOEShapeCone(60, 135.Degrees()));
class IaiKasumiGiri2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.IaiKasumiGiri2), new AOEShapeCone(60, 135.Degrees()));
class IaiKasumiGiri3(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.IaiKasumiGiri3), new AOEShapeCone(60, 135.Degrees()));
class IaiKasumiGiri4(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.IaiKasumiGiri4), new AOEShapeCone(60, 135.Degrees()));

class DoubleKasumiGiri(BossModule module) : Components.GenericAOEs(module, ActionID.MakeSpell(AID.KenkiRelease))
{
    private readonly List<Actor> _castersShadowFirst = [];
    private readonly List<Actor> _castersShadowNext = [];

    private static readonly AOEShape _shapeShadowFirst = new AOEShapeCone(60, 135.Degrees());
    private static readonly AOEShape _shapeShadowNext = new AOEShapeCone(60, 135.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        return _castersShadowFirst.Count > 0
            ? _castersShadowFirst.Select(c => new AOEInstance(_shapeShadowFirst, c.Position, c.CastInfo!.Rotation, c.CastInfo!.NPCFinishAt))
            : _castersShadowNext.Select(c => new AOEInstance(_shapeShadowNext, c.Position, c.CastInfo!.Rotation, c.CastInfo!.NPCFinishAt));
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
        AID.DoubleKasumiGiriFirst1 => _castersShadowFirst,
        AID.DoubleKasumiGiriFirst2 => _castersShadowFirst,
        AID.DoubleKasumiGiriFirst3 => _castersShadowFirst,
        AID.DoubleKasumiGiriFirst4 => _castersShadowFirst,
        AID.DoubleKasumiGiriRest1 => _castersShadowNext,
        AID.DoubleKasumiGiriRest2 => _castersShadowNext,
        AID.DoubleKasumiGiriRest3 => _castersShadowNext,
        AID.DoubleKasumiGiriRest4 => _castersShadowNext,
        _ => null
    };
}

//Route 1
class Unsheathing(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Unsheathing), 3);
class VeilSever(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.VeilSever), new AOEShapeRect(40, 2.5f));

//Route 2
class ScarletAuspice(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ScarletAuspice), new AOEShapeCircle(6));
class MoonlessNight(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.MoonlessNight));
class Clearout(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Clearout), new AOEShapeCone(25, 90.Degrees()));
class BoundlessScarlet(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.BoundlessScarlet), new AOEShapeRect(60, 5, 60));
class Explosion(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Explosion), new AOEShapeRect(60, 15, 60));

//Route 3
class GhastlyGrasp(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.GhastlyGrasp), 5);

class YamaKagura(BossModule module) : Components.Knockback(module)
{
    private readonly List<Actor> _casters = [];
    private DateTime _activation;
    private static readonly AOEShapeRect rect = new(40, 5, 40);

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        foreach (var c in _casters)
            yield return new(c.Position, 33, _activation, rect, c.Rotation - 90.Degrees(), Kind.DirLeft);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.YamaKagura)
        {
            _activation = spell.NPCFinishAt;
            _casters.Add(caster);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.YamaKagura)
            _casters.Remove(caster);
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => (Module.FindComponent<GhastlyGrasp>()?.ActiveAOEs(slot, actor).Any(z => z.Shape.Check(pos, z.Origin, z.Rotation)) ?? false) || !Module.InBounds(pos);
}

//Route 4
class Spiritflame(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Spiritflame), 6);
class SpiritMobs(BossModule module) : Components.GenericAOEs(module)
{
    private readonly IReadOnlyList<Actor> _spirits = module.Enemies(OID.Spiritflame);

    private static readonly AOEShapeCircle _shape = new(2);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _spirits.Where(actor => !actor.IsDead).Select(b => new AOEInstance(_shape, b.Position));
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "The Combat Reborn Team", PrimaryActorOID = (uint)OID.Boss, GroupType = BossModuleInfo.GroupType.CFC, Category = BossModuleInfo.Category.Criterion, GroupID = 945, NameID = 12357)]
public class V022Moko(WorldState ws, Actor primary) : BossModule(ws, primary, new(-700, 540), new ArenaBoundsSquare(20))
{
    private Actor? _bossPath2;

    public Actor? Boss() => PrimaryActor;
    public Actor? BossPath2() => _bossPath2;

    protected override void UpdateModule()
    {
        // TODO: this is an ugly hack, think how multi-actor fights can be implemented without it...
        // the problem is that on wipe, any actor can be deleted and recreated in the same frame
        _bossPath2 ??= StateMachine.ActivePhaseIndex == 0 ? Enemies(OID.BossPath2).FirstOrDefault() : null;
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actor(_bossPath2, ArenaColor.Enemy);
    }
}
