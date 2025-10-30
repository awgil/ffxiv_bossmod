namespace BossMod.Heavensward.DeepDungeon.PalaceOfTheDead.D60TheBlackRider;

public enum OID : uint
{
    Boss = 0x1814, // R3.920, x1
    Voidzone = 0x1E858E, // R0.500, EventObj type, spawn during fight
    VoidsentDiscarnate = 0x18E6, // R1.000, spawn during fight
    Helper = 0x233C, // R0.500, x12, 523 type
}

public enum AID : uint
{
    AutoAttack = 7179, // Boss->player, no cast, range 8+R 90-degree cone
    Geirrothr = 7087, // Boss->self, no cast, range 6+R 90-degree cone, 5.1s after pull, 7.1s after Valfodr + 8.1s after every 2nd HallofSorrow
    HallOfSorrow = 7088, // Boss->location, no cast, range 9 circle
    Infatuation = 7157, // VoidsentDiscarnate->self, 6.5s cast, range 6+R circle
    Valfodr = 7089, // Boss->player, 4.0s cast, width 6 rect charge, knockback 25, dir forward
}

class CleaveAuto(BossModule module) : Components.Cleave(module, default, new AOEShapeCone(11.92f, 45.Degrees()), activeWhileCasting: false);
class Infatuation(BossModule module) : Components.StandardAOEs(module, AID.Infatuation, new AOEShapeCircle(7));
class HallOfSorrow(BossModule module) : Components.PersistentVoidzoneAtCastTarget(module, 9, AID.HallOfSorrow, m => m.Enemies(OID.Voidzone).Where(z => z.EventState != 7), 1.3f);
class Valfodr(BossModule module) : Components.BaitAwayChargeCast(module, AID.Valfodr, 3);
class ValfodrKB(BossModule module) : Components.Knockback(module, AID.Valfodr, stopAtWall: true) // note actual knockback is delayed by upto 1.2s in replay
{
    private int _target;
    private Source? _source;
    private Infatuation? _infatuation;

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        if (_target == slot && _source != null)
            yield return _source.Value;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            _source = new(caster.Position, 25, Module.CastFinishAt(spell));
            _target = Raid.FindSlot(spell.TargetID);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            _target = -1;
            _source = null;
        }
    }

    private Func<WPos, bool>? GetFireballZone()
    {
        _infatuation ??= Module.FindComponent<Infatuation>();
        if (_infatuation == null || _infatuation.Casters.Count == 0)
            return null;

        return ShapeContains.Union([.. _infatuation.Casters.Select(c => ShapeContains.Circle(c.Position, 7))]);
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => GetFireballZone() is var z && z != null && z(pos);

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_target != slot || _source == null)
            return;

        var dangerZone = GetFireballZone();
        if (dangerZone == null)
            return;

        var kbSource = _source.Value.Origin;
        var clamp = Arena.ClampToBounds;

        hints.AddForbiddenZone(p =>
        {
            var dir = (p - kbSource).Normalized();
            var proj = clamp(p + dir * 25);
            return dangerZone(proj);
        }, _source.Value.Activation);
    }
}

class D60TheBlackRiderStates : StateMachineBuilder
{
    public D60TheBlackRiderStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Infatuation>()
            .ActivateOnEnter<HallOfSorrow>()
            .ActivateOnEnter<Valfodr>()
            .ActivateOnEnter<ValfodrKB>()
            .ActivateOnEnter<CleaveAuto>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "legendoficeman, Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 204, NameID = 5309)]
public class D60TheBlackRider(WorldState ws, Actor primary) : BossModule(ws, primary, new(-300, -220), new ArenaBoundsCircle(25));
