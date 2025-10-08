namespace BossMod.Endwalker.DeepDungeon.EurekaOrthos.D80ProtoKaliya;

public enum OID : uint
{
    Boss = 0x3D18,
    Helper = 0x233C,
    WeaponsDrone = 0x3D19, // R2.000, x0 (spawn during fight)
}

public enum AID : uint
{
    AetheromagnetismPull = 31430, // Helper->player, no cast, single-target
    AetheromagnetismPush = 31431, // Helper->player, no cast, single-target
    AutoCannons = 31432, // 3D19->self, 4.0s cast, range 41+R width 5 rect
    Resonance = 31422, // Boss->player, 5.0s cast, range 12 ?-degree cone
    Barofield = 31427, // Boss->self, 3.0s cast, single-target
    NerveGasRing = 32930, // Helper->self, 7.2s cast, range ?-30 donut
    CentralizedNerveGas = 32933, // Helper->self, 5.3s cast, range 25+R 120-degree cone
    LeftwardNerveGas = 32934, // Helper->self, 5.3s cast, range 25+R 180-degree cone
    RightwardNerveGas = 32935, // Helper->self, 5.3s cast, range 25+R 180-degree cone
    NanosporeJet = 31429, // Boss->self, 5.0s cast, range 100 circle
}

public enum TetherID : uint
{
    Magnet = 38, // _Gen_WeaponsDrone->player
}

public enum SID : uint
{
    Barofield = 3420, // none->Boss, extra=0x0
    PositiveChargeDrone = 3416, // none->_Gen_WeaponsDrone, extra=0x0
    NegativeChargeDrone = 3417, // none->_Gen_WeaponsDrone, extra=0x0
    PositiveChargePlayer = 3418, // none->player, extra=0x0
    NegativeChargePlayer = 3419, // none->player, extra=0x0
}

class Aetheromagnetism(BossModule module) : Components.Knockback(module, ignoreImmunes: true)
{
    private enum Charge
    {
        None,
        Neg,
        Pos
    }

    private readonly Source?[] _sources = new Source?[4];

    public override IEnumerable<Source> Sources(int slot, Actor actor) => Utils.ZeroOrOne(_sources[slot]);

    private static (Charge Charge, DateTime Expire) GetCharge(Actor actor)
    {
        foreach (var st in actor.Statuses)
            switch ((SID)st.ID)
            {
                case SID.NegativeChargeDrone:
                case SID.NegativeChargePlayer:
                    return (Charge.Neg, st.ExpireAt + TimeSpan.FromSeconds(1.1));
                case SID.PositiveChargePlayer:
                case SID.PositiveChargeDrone:
                    return (Charge.Pos, st.ExpireAt + TimeSpan.FromSeconds(1.1));
            }

        return (Charge.None, default);
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.Magnet)
        {
            if (!Raid.TryFindSlot(tether.Target, out var slot))
                return;

            var target = Raid[slot]!;

            var (from, to) = (GetCharge(source), GetCharge(target));
            if (from.Charge == Charge.None || to.Charge == Charge.None)
                return;

            _sources[slot] = new(source.Position, 10, to.Expire, Kind: from.Charge == to.Charge ? Kind.AwayFromOrigin : Kind.TowardsOrigin);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.AetheromagnetismPull or AID.AetheromagnetismPush)
        {
            if (Raid.TryFindSlot(spell.MainTargetID, out var slot))
                _sources[slot] = null;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_sources[slot] is not { } source)
            return;

        var attract = source.Kind == Kind.TowardsOrigin;

        var barofield = ShapeContains.Circle(Module.PrimaryActor.Position, 5);
        var arena = ShapeContains.InvertedCircle(Module.PrimaryActor.Position, 8);
        var cannons = Module.Enemies(OID.WeaponsDrone).Select(d => ShapeContains.Rect(d.Position, d.Rotation, 50, 0, 2.5f));
        var all = ShapeContains.Union([barofield, arena, .. cannons]);

        var bossPos = Module.PrimaryActor.Position;
        hints.AddForbiddenZone(p =>
        {
            var dir = (p - source.Origin).Normalized();
            var kb = attract ? -dir : dir;

            // prevent KB through death zone in center
            if (Intersect.RayCircle(p, kb, bossPos, 5) < 1000)
                return true;

            return all(p + kb * 10);
        }, source.Activation);
    }
}

class Barofield(BossModule module) : Components.GenericAOEs(module, AID.Barofield)
{
    private DateTime activation = DateTime.MinValue;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (activation > DateTime.MinValue)
            yield return new AOEInstance(new AOEShapeCircle(5), Module.PrimaryActor.Position, default, activation);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.Barofield or AID.NanosporeJet)
            activation = Module.CastFinishAt(spell);
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.Barofield)
            activation = WorldState.CurrentTime;
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.Barofield)
            activation = DateTime.MinValue;
    }
}
class Resonance(BossModule module) : Components.BaitAwayCast(module, AID.Resonance, new AOEShapeCone(12, 45.Degrees()));
class RightwardNerveGas(BossModule module) : Components.StandardAOEs(module, AID.RightwardNerveGas, new AOEShapeCone(25, 90.Degrees()));
class LeftwardNerveGas(BossModule module) : Components.StandardAOEs(module, AID.LeftwardNerveGas, new AOEShapeCone(25, 90.Degrees()));
class CentralizedNerveGas(BossModule module) : Components.StandardAOEs(module, AID.CentralizedNerveGas, new AOEShapeCone(25, 60.Degrees()));
class AutoCannons(BossModule module) : Components.StandardAOEs(module, AID.AutoCannons, new AOEShapeRect(45, 2.5f))
{
    private Aetheromagnetism? _knockback;
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        _knockback ??= Module.FindComponent<Aetheromagnetism>();

        foreach (var kb in _knockback?.Sources(slot, actor) ?? [])
            if ((kb.Activation - WorldState.CurrentTime).TotalSeconds < 5)
                return;

        base.AddAIHints(slot, actor, assignment, hints);
    }
}
class NerveGasRing(BossModule module) : Components.StandardAOEs(module, AID.NerveGasRing, new AOEShapeDonut(8, 100))
{
    private Aetheromagnetism? _knockback;
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        _knockback ??= Module.FindComponent<Aetheromagnetism>();

        foreach (var _ in _knockback?.Sources(slot, actor) ?? [])
            return;

        base.AddAIHints(slot, actor, assignment, hints);
    }
}

class D80ProtoKaliyaStates : StateMachineBuilder
{
    public D80ProtoKaliyaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AutoCannons>()
            .ActivateOnEnter<CentralizedNerveGas>()
            .ActivateOnEnter<RightwardNerveGas>()
            .ActivateOnEnter<LeftwardNerveGas>()
            .ActivateOnEnter<Resonance>()
            .ActivateOnEnter<Barofield>()
            .ActivateOnEnter<Aetheromagnetism>()
            .ActivateOnEnter<NerveGasRing>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 904, NameID = 12247)]
public class D80ProtoKaliya(WorldState ws, Actor primary) : BossModule(ws, primary, new(-600, -300), new ArenaBoundsCircle(20));
