namespace BossMod.Endwalker.DeepDungeon.EurekaOrthos.DD80ProtoKaliya;

public enum OID : uint
{
    Boss = 0x3D18, // R5.0
    WeaponsDrone = 0x3D19, // R2.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 31421, // Boss->players, no cast, range 6+R 90-degree cone
    AetheromagnetismKB = 31431, // Helper->player, no cast, single-target, knockback 10, away from source
    AetheromagnetismPull = 31430, // Helper->player, no cast, single-target, pull 10, between centers
    AutoCannons = 31432, // WeaponsDrone->self, 4.0s cast, range 41+R width 5 rect
    Barofield = 31427, // Boss->self, 3.0s cast, single-target

    CentralizedNerveGasVisual = 31423, // Boss->self, 4.5s cast, range 25+R 120-degree cone
    CentralizedNerveGas = 32933, // Helper->self, 5.3s cast, range 25+R 120-degree cone
    LeftwardNerveGasVisual = 31424, // Boss->self, 4.5s cast, range 25+R 180-degree cone
    LeftwardNerveGas = 32934, // Helper->self, 5.3s cast, range 25+R 180-degree cone
    RightwardNerveGasVisual = 31425, // Boss->self, 4.5s cast, range 25+R 180-degree cone
    RightwardNerveGas = 32935, // Helper->self, 5.3s cast, range 25+R 180-degree cone
    NerveGasRingVisual = 31426, // Boss->self, 5.0s cast, range 8-30 donut
    NerveGasRing = 32930, // Helper->self, 7.2s cast, range 8-30 donut
    Resonance = 31422, // Boss->player, 5.0s cast, range 12 90-degree cone, tankbuster

    NanosporeJet = 31429, // Boss->self, 5.0s cast, range 100 circle
}

public enum SID : uint
{
    Barofield = 3420, // none->Boss, extra=0x0, damage taken when near boss
    NegativeChargePlayer = 3419, // none->player, extra=0x0
    PositiveChargePlayer = 3418, // none->player, extra=0x0
    NegativeChargeDrone = 3417, // none->WeaponsDrone, extra=0x0
    PositiveChargeDrone = 3416, // none->WeaponsDrone, extra=0x0
}

public enum TetherID : uint
{
    Magnetism = 38
}

class Magnetism(BossModule module) : Components.Knockback(module, ignoreImmunes: true)
{
    private readonly HashSet<(Actor, uint)> statusOnActor = [];
    public readonly HashSet<(Actor, Source)> sourceByActor = [];

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        if (sourceByActor.Count > 0 && Module.FindComponent<NerveGasRingAndAutoCannons>()!.ActiveAOEs(slot, actor).Any(z => z.Shape == NerveGasRingAndAutoCannons.donut))
        {
            var x = sourceByActor.FirstOrDefault(x => x.Item1 == actor);
            yield return new(x.Item2.Origin, x.Item2.Distance, x.Item2.Activation, Kind: x.Item2.Kind);
        }
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => (Module.FindComponent<NerveGasRingAndAutoCannons>()?.ActiveAOEs(slot, actor).Any(z => z.Shape.Check(pos, z.Origin, z.Rotation)) ?? false) ||
        (Module.FindComponent<Barofield>()?.ActiveAOEs(slot, actor).Any(z => z.Shape.Check(pos, z.Origin, z.Rotation)) ?? false) || !Module.InBounds(pos);

    private bool IsPull(Actor source, Actor target)
    {
        return statusOnActor.Contains((source, (uint)SID.NegativeChargeDrone)) && statusOnActor.Contains((target, (uint)SID.PositiveChargePlayer)) ||
            statusOnActor.Contains((source, (uint)SID.PositiveChargeDrone)) && statusOnActor.Contains((target, (uint)SID.NegativeChargePlayer));
    }

    private bool IsKnockback(Actor source, Actor target)
    {
        return statusOnActor.Contains((source, (uint)SID.NegativeChargeDrone)) && statusOnActor.Contains((target, (uint)SID.NegativeChargePlayer)) ||
            statusOnActor.Contains((source, (uint)SID.PositiveChargeDrone)) && statusOnActor.Contains((target, (uint)SID.PositiveChargePlayer));
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.Magnetism)
        {
            var target = WorldState.Actors.Find(tether.Target)!;
            var activation = Module.WorldState.FutureTime(10);
            if (IsPull(source, target))
                sourceByActor.Add((target, new(source.Position, 10, activation, Kind: Kind.TowardsOrigin)));
            else if (IsKnockback(source, target))
                sourceByActor.Add((target, new(source.Position, 10, activation)));
        }
    }

    public override void OnUntethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.Magnetism)
        {
            var target = WorldState.Actors.Find(tether.Target)!;
            sourceByActor.RemoveWhere(x => x.Item1 == target);
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID is SID.NegativeChargeDrone or SID.PositiveChargeDrone or SID.PositiveChargePlayer or SID.NegativeChargePlayer)
        {
            statusOnActor.RemoveWhere(x => x.Item1 == actor);
            statusOnActor.Add((actor, status.ID));
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (sourceByActor.Count > 0)
        {
            var target = sourceByActor.FirstOrDefault(x => x.Item1 == actor);
            var offset = target.Item2.Kind == Kind.TowardsOrigin ? 180.Degrees() : default;
            hints.AddForbiddenZone(ShapeDistance.InvertedDonutSector(Module.Center, 15, 18, Angle.FromDirection(target.Item2.Origin - Module.Center) + offset, 35.Degrees()));
        }
    }
}

class Barofield(BossModule module) : Components.GenericAOEs(module)
{
    private DateTime activation;
    private static readonly AOEShapeCircle circle = new(5);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (activation != default)
            yield return new(circle, Module.PrimaryActor.Position, default, activation);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (activation == default && (AID)spell.Action.ID is AID.Barofield or AID.NanosporeJet)
            activation = spell.NPCFinishAt.AddSeconds(0.7f);
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Barofield)
            activation = default;
    }
}

class NerveGasRingAndAutoCannons(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeCross cross = new(43, 2.5f);
    public static readonly AOEShapeDonut donut = new(8, 30);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.NerveGasRing)
        {
            _aoes.Add(new(donut, Module.Center, default, spell.NPCFinishAt, Risky: false));
            _aoes.Add(new(cross, Module.Center, default, spell.NPCFinishAt));
        }
        if (_aoes.Count == 0 && (AID)spell.Action.ID == AID.AutoCannons)
            _aoes.Add(new(cross, Module.Center, default, spell.NPCFinishAt));
        else if (_aoes.Count == 2 && (AID)spell.Action.ID == AID.AutoCannons)
        {
            _aoes.RemoveAt(1);
            _aoes.Add(new(cross, Module.Center, default, spell.NPCFinishAt));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count > 0 && (AID)spell.Action.ID is AID.NerveGasRing or AID.AutoCannons)
            _aoes.RemoveAt(0);
    }
}

class LeftNerveGas(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.LeftwardNerveGas), new AOEShapeCone(30, 90.Degrees()));
class RightNerveGas(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.RightwardNerveGas), new AOEShapeCone(30, 90.Degrees()));
class CentralizedNerveGas(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.CentralizedNerveGas), new AOEShapeCone(30, 60.Degrees()));
class AutoAttack(BossModule module) : Components.Cleave(module, ActionID.MakeSpell(AID.AutoAttack), new AOEShapeCone(11, 45.Degrees()))
{
    private bool Inactive(int slot, Actor actor) => !Module.FindComponent<Barofield>()!.ActiveAOEs(slot, actor).Any();
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Inactive(slot, actor))
            base.AddAIHints(slot, actor, assignment, hints);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (Inactive(pcSlot, pc))
            base.DrawArenaForeground(pcSlot, pc);
    }
}

class Resonance(BossModule module) : Components.BaitAwayCast(module, ActionID.MakeSpell(AID.Resonance), new AOEShapeCone(12, 45.Degrees()), endsOnCastEvent: true)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        if (CurrentBaits.Count > 0)
            hints.Add("Tankbuster cleave");
    }
}

class DD80ProtoKaliyaStates : StateMachineBuilder
{
    public DD80ProtoKaliyaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AutoAttack>()
            .ActivateOnEnter<Barofield>()
            .ActivateOnEnter<Resonance>()
            .ActivateOnEnter<NerveGasRingAndAutoCannons>()
            .ActivateOnEnter<LeftNerveGas>()
            .ActivateOnEnter<RightNerveGas>()
            .ActivateOnEnter<CentralizedNerveGas>()
            .ActivateOnEnter<Magnetism>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus, legendoficeman", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 904, NameID = 12247)]
public class DD80ProtoKaliya(WorldState ws, Actor primary) : BossModule(ws, primary, new(-600, -300), new ArenaBoundsCircle(20));