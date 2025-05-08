namespace BossMod.Heavensward.DeepDungeon.PalaceOfTheDead.D160Todesritter;

public enum OID : uint
{
    Boss = 0x181D, // R3.920, x1
    VoidsentDiscarnate = 0x18EF, // R1.000, x0 (spawn during fight)
    Actor1e86e0 = 0x1E86E0, // R2.000, x1, EventObj type
    Voidzone = 0x1E858E, // R0.500, x0 (spawn during fight), EventObj type
}

public enum AID : uint
{
    AutoAttack = 7179, // Boss->players, no cast, range 8+R(11.92) 90?-degree cone
    Geirrothr = 7154, // Boss->self, no cast, range 6+R(9.92) 90?-degree cone
    HallOfSorrow = 7155, // Boss->location, no cast, range 9 circle
    Infatuation = 7090, // VoidsentDiscarnate->self, 6.5s cast, range 6+R(7) circle
    Valfodr = 7156, // Boss->player, 4.0s cast, width 6 rect charge + kb
}

class CleaveAuto(BossModule module) : Components.Cleave(module, AID.AutoAttack, new AOEShapeCone(11.92f, 45.Degrees()), activeWhileCasting: false);
class HallOfSorrow(BossModule module) : Components.PersistentVoidzoneAtCastTarget(module, 9, AID.HallOfSorrow, m => m.Enemies(OID.Voidzone).Where(z => z.EventState != 7), 1.3f);
class Infatuation(BossModule module) : Components.StandardAOEs(module, AID.Infatuation, new AOEShapeCircle(7));
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

        hints.AddForbiddenZone(p =>
        {
            var dir = (p - kbSource).Normalized();
            var proj = Arena.ClampToBounds(p + dir * 25);
            return dangerZone(proj);
        }, _source.Value.Activation);
    }
}

class D160TodesritterStates : StateMachineBuilder
{
    public D160TodesritterStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<CleaveAuto>()
            .ActivateOnEnter<HallOfSorrow>()
            .ActivateOnEnter<Infatuation>()
            .ActivateOnEnter<Valfodr>()
            .ActivateOnEnter<ValfodrKB>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "LegendofIceman", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 214, NameID = 5438)]
public class D160Todesritter(WorldState ws, Actor primary) : BossModule(ws, primary, new(-300, -300), new ArenaBoundsCircle(25));
