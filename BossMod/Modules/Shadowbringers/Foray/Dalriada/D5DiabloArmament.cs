namespace BossMod.Shadowbringers.Foray.Dalriada.D5DiabloArmament;

public enum OID : uint
{
    Boss = 0x31B3, // R28.500, x1
    Helper = 0x233C, // R0.500, x33, mixed types
    DiabolicBit = 0x31B4, // R1.200, x0 (spawn during fight)
    Aether = 0x31B5, // R1.500, x0 (spawn during fight)

    GateRed = 0x1EB1D6,
    GateYellow = 0x1EB1D7,
    GateGreen = 0x1EB1D8,
    GateBlue = 0x1EB1D9,
}

public enum AID : uint
{
    BrutalCamisado = 24899, // Boss->player, no cast, single-target
    AethericExplosion = 23751, // Helper->self, 5.0s cast, ???
    AethericExplosion1 = 23750, // Boss->self, 5.0s cast, single-target
    AetherochemicalLaser = 23717, // Boss->self, no cast, range 60 width 60 rect
    AetherochemicalLaser1 = 23716, // Boss->self, no cast, range 60 width 22 rect
    ExplosionShort1 = 24721, // Helper->self, 10.0s cast, range 60 width 22 rect
    ExplosionShort2 = 23718, // Helper->self, 10.0s cast, range 60 width 22 rect
    ExplosionShort3 = 23719, // Helper->self, 10.0s cast, range 60 width 22 rect
    ExplosionMid1 = 23720, // Helper->self, 8.0s cast, range 60 width 22 rect
    ExplosionMid2 = 23721, // Helper->self, 8.0s cast, range 60 width 22 rect
    ExplosionMid3 = 24722, // Helper->self, 8.0s cast, range 60 width 22 rect
    ExplosionLong1 = 23722, // Helper->self, 6.0s cast, range 60 width 22 rect
    ExplosionLong2 = 23723, // Helper->self, 6.0s cast, range 60 width 22 rect
    ExplosionLong3 = 24723, // Helper->self, 6.0s cast, range 60 width 22 rect
    AdvancedDeathRay = 23748, // Boss->self, 5.0s cast, single-target
    AdvancedDeathRay1 = 23749, // Helper->players, no cast, range 70 width 8 rect
    DiabolicGate = 23711, // Boss->self, 4.0s cast, single-target
    DiabolicGate1 = 25028, // Helper->self, 5.0s cast, ???
    DiabolicGateDeathwall = 24994, // Helper->self, no cast, range ?-60 donut
    RuinousPseudomen = 23712, // Boss->self, 15.0s cast, single-target
    RuinousPseudomen1 = 24995, // Helper->self, 1.5s cast, range 80 width 24 rect
    RuinousPseudomen2 = 23713, // Helper->self, 1.0s cast, single-target
    RuinousPseudomen3 = 24908, // Helper->self, 1.5s cast, range 100 width 24 rect
    RuinousPseudomen4 = 23714, // Boss->self, no cast, single-target
    RuinousPseudomen5 = 24911, // Helper->self, 4.5s cast, range 80 width 24 rect
    UltimatePseudoterror = 23715, // Boss->self, 4.0s cast, range 15-70 donut
    MagitekBit = 23724, // Boss->self, 4.0s cast, single-target
    DiabolicBitMove = 23725, // 31B4->location, no cast, single-target
    AssaultCannon = 23726, // 31B4->self, 7.0s cast, range 100 width 6 rect
    AdvancedDeathIV = 23727, // Boss->self, 4.0s cast, single-target
    AdvancedDeathIV1 = 23728, // Helper->location, 7.0s cast, range 1 circle (but actually 10)
    LightPseudopillar = 23729, // Boss->self, 3.0s cast, single-target
    LightPseudopillar1 = 23730, // Helper->location, 4.0s cast, range 10 circle
    AethericBoom = 23732, // Helper->self, 5.0s cast, ???
    AethericBoom1 = 23731, // Boss->self, 5.0s cast, single-target
    Aetheroplasm = 23733, // 31B5->self, no cast, range 6 circle
    DeadlyDealingAOE = 23746, // Boss->location, 7.0s cast, range 6 circle
    DeadlyDealingKB = 23747, // Helper->self, 7.5s cast, ???
    VoidSystemsOverload = 23736, // Helper->self, 5.0s cast, ???
    VoidSystemsOverload1 = 23735, // Boss->self, 5.0s cast, single-target
    VoidSystemsOverload2 = 25364, // Boss->self, 5.0s cast, single-target
    PillarOfShamash1 = 23737, // Helper->self, 8.0s cast, range 70 20-degree cone
    PillarOfShamash2 = 23738, // Helper->self, 9.5s cast, range 70 20-degree cone
    PillarOfShamash3 = 23739, // Helper->self, 11.0s cast, range 70 20-degree cone
    PillarOfShamashTarget = 23741, // Helper->player, no cast, single-target
    PillarOfShamashSpread = 23740, // Helper->player, no cast, range 70 width 4 rect
    PillarOfShamashStack = 23742, // Helper->players, no cast, range 70 width 8 rect
    AdvancedNoxBoss = 23743, // Boss->self, 4.0s cast, single-target
    AdvancedNoxFirst = 23744, // Helper->self, 10.0s cast, range 10 circle
    AdvancedNoxRest = 23745, // Helper->self, no cast, range 10 circle
    FusionBurst = 23734, // Aether->self, no cast, range 100 circle
}

public enum IconID : uint
{
    AdvancedDeathRay = 230, // player->self
    AccelerationBomb = 267, // player->self
    PillarOfShamashSpread = 23, // player->self
}

public enum SID : uint
{
    AccelerationBomb = 2657, // none->player, extra=0x0
}

class AdvancedDeathRay(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeRect(70, 4), (uint)IconID.AdvancedDeathRay, AID.AdvancedDeathRay1);
class AethericExplosion(BossModule module) : Components.RaidwideCast(module, AID.AethericExplosion);

class Explosion1 : Components.GroupedAOEs
{
    public Explosion1(BossModule module) : base(module, [AID.ExplosionShort1, AID.ExplosionShort3, AID.ExplosionShort2], new AOEShapeRect(60, 11))
    {
        Color = ArenaColor.Danger;
    }
}
class Explosion2(BossModule module) : Components.GroupedAOEs(module, [AID.ExplosionMid1, AID.ExplosionMid3, AID.ExplosionMid2], new AOEShapeRect(60, 11))
{
    private Explosion3? ex3;
    public override void Update()
    {
        ex3 ??= Module.FindComponent<Explosion3>();
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => base.ActiveAOEs(slot, actor).Select(a => a with { Color = ex3?.ActiveCasters.Count() > 0 ? ArenaColor.Danger : ArenaColor.AOE });
}
class Explosion3(BossModule module) : Components.GroupedAOEs(module, [AID.ExplosionLong1, AID.ExplosionLong2, AID.ExplosionLong3], new AOEShapeRect(60, 11));

class DiabolicGate(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<(WPos Source, Angle Rotation, DateTime Activation)> Charges = [];
    private static readonly float[] Delays = [3.94f, 3.05f, 4.26f];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Charges.Take(2).Select((r, i) => new AOEInstance(new AOEShapeRect(100, 12), r.Source, r.Rotation, r.Activation, Color: i == 0 ? ArenaColor.Danger : ArenaColor.AOE)).Reverse();

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.RuinousPseudomen)
        {
            var gates = WorldState.Actors.Where(a => (OID)a.OID is OID.GateRed or OID.GateYellow or OID.GateGreen or OID.GateBlue);

            Actor? nextGate(Actor caster)
            {
                var across = gates.Exclude(caster).First(g => g.Position.InRect(caster.Position, caster.Rotation.ToDirection() * 1000, 12));
                return gates.Exclude(across).First(g => g.OID == across.OID);
            }

            Charges.Add((caster.Position, caster.Rotation, Module.CastFinishAt(spell, 1.5f)));
            var src = caster;
            for (var i = 0; i < 3; i++)
            {
                var next = nextGate(src);
                if (next == null)
                {
                    ReportError($"Missing gate for {src}");
                    return;
                }
                src = next;
                Charges.Add((next.Position, next.Rotation, Charges[^1].Activation.AddSeconds(Delays[i])));
            }
        }

        // last pseudomen isn't aligned with the gate
        if ((AID)spell.Action.ID == AID.RuinousPseudomen5 && Charges.Count > 0)
        {
            var c = Charges[^1];
            Charges.RemoveAt(Charges.Count - 1);
            c.Source = caster.Position;
            c.Rotation = caster.Rotation;
            Charges.Add(c);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.RuinousPseudomen1 or AID.RuinousPseudomen3 or AID.RuinousPseudomen5)
            Charges.RemoveAt(0);
    }
}

class PseudomenBounds(BossModule module) : Components.GenericAOEs(module)
{
    private const float SmallArenaRadius = 17f;

    private DateTime Activation;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Activation == default ? [] : [new AOEInstance(new AOEShapeDonut(SmallArenaRadius, 60), Arena.Center, default, Activation)];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.DiabolicGate1)
            Activation = Module.CastFinishAt(spell);
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x33)
        {
            if (state == 0x00020001)
            {
                Arena.Bounds = new ArenaBoundsCircle(SmallArenaRadius);
                Activation = default;
            }
            else if (state == 0x00080004)
                Arena.Bounds = new ArenaBoundsCircle(29.5f);
        }
    }
}

class UltimatePseudoterror(BossModule module) : Components.StandardAOEs(module, AID.UltimatePseudoterror, new AOEShapeDonut(15, 70));
class AssaultCannon(BossModule module) : Components.StandardAOEs(module, AID.AssaultCannon, new AOEShapeRect(100, 3));
class AdvancedDeath(BossModule module) : Components.StandardAOEs(module, AID.AdvancedDeathIV1, 10);
class LightPseudopillar(BossModule module) : Components.StandardAOEs(module, AID.LightPseudopillar1, 10);
class DeadlyDealing(BossModule module) : Components.StandardAOEs(module, AID.DeadlyDealingAOE, 6);
class DeadlyDealingKB(BossModule module) : Components.KnockbackFromCastTarget(module, AID.DeadlyDealingKB, 30, stopAtWall: true);
class VoidSystemsOverload(BossModule module) : Components.RaidwideCast(module, AID.VoidSystemsOverload);
class PillarOfShamashCone(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<Actor> Short = [];
    private readonly List<Actor> Mid = [];
    private readonly List<Actor> Long = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var s in Short)
            yield return Cone(s, Mid.Count > 0 ? ArenaColor.Danger : ArenaColor.AOE);

        foreach (var m in Mid)
            yield return Cone(m, Short.Count == 0 ? ArenaColor.Danger : ArenaColor.AOE);

        if (Short.Count == 0)
            foreach (var l in Long)
                yield return Cone(l, Mid.Count == 0 ? ArenaColor.Danger : ArenaColor.AOE);
    }

    private AOEInstance Cone(Actor caster, uint color = 0) => new(new AOEShapeCone(70, 10.Degrees()), caster.Position, caster.Rotation, Module.CastFinishAt(caster.CastInfo), color == 0 ? ArenaColor.AOE : color);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.PillarOfShamash1:
                Short.Add(caster);
                break;
            case AID.PillarOfShamash2:
                Mid.Add(caster);
                break;
            case AID.PillarOfShamash3:
                Long.Add(caster);
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.PillarOfShamash1:
                Short.Remove(caster);
                break;
            case AID.PillarOfShamash2:
                Mid.Remove(caster);
                break;
            case AID.PillarOfShamash3:
                Long.Remove(caster);
                break;
        }
    }
}

class AccelerationBomb(BossModule module) : Components.StayMove(module)
{
    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.AccelerationBomb)
            SetState(Raid.FindSlot(actor.InstanceID), new(Requirement.Stay, status.ExpireAt));
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.AccelerationBomb)
            ClearState(Raid.FindSlot(actor.InstanceID));
    }
}

class ShamashSpread(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeRect(70, 2), (uint)IconID.PillarOfShamashSpread, AID.PillarOfShamashSpread);

class ShamashStack(BossModule module) : Components.SimpleLineStack(module, 4, 70, AID.PillarOfShamashTarget, AID.PillarOfShamashStack, 5.1f);

class TheDiabloArmamentStates : StateMachineBuilder
{
    public TheDiabloArmamentStates(BossModule module) : base(module)
    {
        // TODO add nox exaflares
        TrivialPhase()
            .ActivateOnEnter<AdvancedDeathRay>()
            .ActivateOnEnter<AethericExplosion>()
            .ActivateOnEnter<Explosion1>()
            .ActivateOnEnter<Explosion2>()
            .ActivateOnEnter<Explosion3>()
            .ActivateOnEnter<PseudomenBounds>()
            .ActivateOnEnter<DiabolicGate>()
            .ActivateOnEnter<UltimatePseudoterror>()
            .ActivateOnEnter<AssaultCannon>()
            .ActivateOnEnter<AdvancedDeath>()
            .ActivateOnEnter<LightPseudopillar>()
            .ActivateOnEnter<DeadlyDealing>()
            .ActivateOnEnter<DeadlyDealingKB>()
            .ActivateOnEnter<VoidSystemsOverload>()
            .ActivateOnEnter<PillarOfShamashCone>()
            .ActivateOnEnter<AccelerationBomb>()
            .ActivateOnEnter<ShamashSpread>()
            .ActivateOnEnter<ShamashStack>()
            ;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 778, NameID = 10007)]
public class TheDiabloArmament(WorldState ws, Actor primary) : BossModule(ws, primary, new(-720, -760), new ArenaBoundsCircle(29.5f))
{
    public override bool DrawAllPlayers => true;
}
