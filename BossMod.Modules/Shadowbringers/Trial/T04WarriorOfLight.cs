namespace BossMod.Shadowbringers.Trial.T04WarriorOfLight;

public enum OID : uint
{
    Boss = 0x2DDD, // R5.100, Warrior of Light P1
    Helper = 0x233C, // R0.500
    BossP2 = 0x2DDE, // R5.100, Warrior of Light P2
    SpectralNinja = 0x2DE3, // R1.350
    SpectralWhiteMage = 0x2DE4, // R1.350
    SpectralBlackMage = 0x2DE5, // R1.350
    SpectralDarkKnight = 0x2DE6, // R1.350
    SpectralBard = 0x2DE7, // R1.350
    SpectralWarrior = 0x2DE8, // R1.350
    SpectralSummoner = 0x2DE9, // R1.350
    WyrmOfLight = 0x2DEA, // R4.200
    SpectralEgi = 0x2DEB, // R1.800
    SwordMarker = 0x1EA1A1, // R2.000, EventObj
    BrimstoneActor = 0x1EAFFE, // R0.500, EventObj
}

public enum AID : uint
{
    AutoAttack = 872, // Boss/BossP2->player, no cast, single-target
    AbsoluteHoly = 20237, // Helper->players, no cast, range 6 circle
    AbsoluteBlizzardIIIHit = 20238, // Helper->self, no cast, range 70 circle
    AbsoluteFireIIIHit = 20239, // Helper->self, no cast, range 70 circle, Pyretic
    CoruscantSaberIn = 20240, // Boss->self, 7.0s cast, range 10 circle
    CoruscantSaberOut = 20241, // Boss->self, 7.0s cast, range 5-60 donut
    ImbuedAbsoluteFireIII = 20242, // Boss/BossP2->self, 3.0s cast, single-target
    ImbuedAbsoluteBlizzardIII = 20243, // Boss/BossP2->self, 3.0s cast, single-target
    RadiantBraver = 20246, // BossP2->self, no cast, range 60 90-degree cone
    RadiantBraverAOE = 20247, // Helper->self, no cast, range 60 90-degree cone
    RadiantDesperado = 20248, // BossP2->self, no cast, range 60 width 8 rect
    RadiantDesperadoRepeat = 20249, // BossP2->self, no cast, range 60 width 8 rect
    RadiantMeteor = 20250, // BossP2->self, 6.0s cast, single-target
    RadiantMeteorSpread = 20251, // Helper->player, 6.0s cast, range 20 circle
    SuitonSan = 20252, // Helper->self, 6.0s cast, range 60 width 60 rect + knockback
    KatonSan = 20253, // Helper->players, 5.0s cast, range 6 circle stack
    BrimstoneEarth = 20254, // Helper->location, 8.0s cast, range 6 circle
    BrimstoneEarthRepeat = 20255, // Helper->location, no cast, growing circles
    DelugeOfDeath = 20256, // Helper->player, 5.0s cast, range 100 circle (proximity)
    MeteorImpact = 20257, // Helper->self, no cast, range 4 circle
    MeteoricBurst = 20258, // Helper->self, no cast, tower resolution
    PerfectDecimation = 20259, // Helper->self, 6.8s cast, range 60 45-degree cone
    FlareBreath = 20260, // SpectralEgi->self, 4.0s cast, range 60 90-degree cone
    Cauterize = 20261, // WyrmOfLight->self, 4.0s cast, range 40 width 20 rect
    ShiningWave = 20262, // Helper->self, no cast, Sword of Light resolve
    TerrorUnleashed = 20263, // Boss->self, 3.0s cast, range 70 circle
    TheBitterEnd = 20264, // Boss/BossP2->player, 5.0s cast, tankbuster
    ElddragonDive = 20265, // BossP2->self, 5.0s cast, range 70 circle
    SolemnConfiteor = 20266, // Helper->location, 3.0s cast, range 6 circle
    AbsoluteHolyCast = 20267, // BossP2->self, 5.0s cast, single-target
    AbsoluteBlizzardIII = 20269, // Boss/BossP2->self, 5.0s cast, single-target
    AbsoluteFireIII = 20270, // Boss/BossP2->self, 5.0s cast, single-target
    ToTheLimit1 = 20276, // BossP2->self, 3.0s cast, single-target
    ToTheLimit2 = 20277, // BossP2->self, 3.0s cast, single-target
    ToTheLimit3 = 20278, // BossP2->self, 3.0s cast, single-target
    SpecterOfLight = 20279, // BossP2->self, 3.0s cast, single-target
    SuitonSanVisual = 20280, // SpectralNinja->self, 3.0s cast, single-target
    KatonSanVisual = 20281, // SpectralNinja->self, 3.0s cast, single-target
    BrimstoneEarthVisual = 20282, // SpectralDarkKnight->self, 8.0s cast, single-target
    DelugeOfDeathVisual = 20283, // SpectralBard->self, 3.0s cast, single-target
    TwincastWHM = 20284, // SpectralWhiteMage->self, 3.0s cast, single-target
    TwincastBLM = 20285, // SpectralBlackMage->self, 3.0s cast, single-target
    PerfectDecimationVisual = 20286, // SpectralWarrior->self, 5.0s cast, single-target
    Summon = 20287, // SpectralSummoner->self, 3.0s cast, single-target
    SummonWyrm = 20289, // Boss/BossP2->self, 3.0s cast, single-target
    SwordOfLight = 20290, // Boss/BossP2->self, 3.0s cast, single-target
    SolemnConfiteorVisual = 20291, // Boss/BossP2->self, 3.0s cast, single-target
    RadiantDesperadoMarker = 20294, // Helper->player, no cast, single-target
    RadiantSacrament = 20296, // Helper->self, 6.0s cast, range 60 width 40 rect
    ImbuedCoruscanceIn = 20299, // Boss/BossP2->self, 7.0s cast, range 10 circle
    ImbuedCoruscanceOut = 20300, // BossP2->self, 7.0s cast, range 5-60 donut
    RadiantDesperadoCast = 20829, // BossP2->self, 6.0s cast, single-target
    FlareBreathVisual = 21073, // SpectralSummoner->self, 9.0s cast, single-target
    RadiantBraverCast = 21076, // BossP2->self, 6.0s cast, single-target
    TwincastHit = 21278, // SpectralWhiteMage/BlackMage->BossP2, no cast
    Ascendance = 21297, // Boss->self, 6.0s cast, range 60 circle
    AbsoluteTeleport = 21298, // Boss->self, 5.0s cast, single-target
    UltimateCrossover = 21627, // BossP2->self, 7.0s cast, single-target
    UltimateCrossoverAOE = 21628, // Helper->self, 6.0s cast, range 60 circle
}

public enum SID : uint
{
    Pyretic = 960, // Helper->player
    ImbuedSaber = 2377, // Boss->Boss
}

public enum IconID : uint
{
    ProximityBait = 87, // player, Deluge of Death
    Stack = 161, // player, Absolute Holy
    RadiantBraver = 234, // player
}

public enum TetherID : uint
{
    FlareBreath = 17, // SpectralEgi->player, flare breath bait
}

class CoruscantSaberIn(BossModule module) : Components.StandardAOEs(module, AID.CoruscantSaberIn, 10);
class CoruscantSaberOut(BossModule module) : Components.StandardAOEs(module, AID.CoruscantSaberOut, new AOEShapeDonut(5, 60));
class ImbuedCoruscanceIn(BossModule module) : Components.StandardAOEs(module, AID.ImbuedCoruscanceIn, 10);
class ImbuedCoruscanceOut(BossModule module) : Components.StandardAOEs(module, AID.ImbuedCoruscanceOut, new AOEShapeDonut(5, 60));
class RadiantMeteorSpread(BossModule module) : Components.SpreadFromCastTargets(module, AID.RadiantMeteorSpread, 20);
class SuitonSan(BossModule module) : Components.KnockbackFromCastTarget(module, AID.SuitonSan, 30, kind: Kind.DirForward);
class KatonSan(BossModule module) : Components.StackWithCastTargets(module, AID.KatonSan, 6, 6);
class BrimstoneEarth(BossModule module) : Components.StandardAOEs(module, AID.BrimstoneEarth, 6);
class PerfectDecimation(BossModule module) : Components.StandardAOEs(module, AID.PerfectDecimation, new AOEShapeCone(60, 22.5f.Degrees()), 4);
class FlareBreathTether(BossModule module) : Components.BaitAwayTethers(module, new AOEShapeCone(60, 45.Degrees()), (uint)TetherID.FlareBreath, AID.FlareBreath);
class FlareBreathAOE(BossModule module) : Components.StandardAOEs(module, AID.FlareBreath, new AOEShapeCone(60, 45.Degrees()));
class Cauterize(BossModule module) : Components.StandardAOEs(module, AID.Cauterize, new AOEShapeRect(40, 10));
class DelugeOfDeath(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCircle(20), (uint)IconID.ProximityBait, AID.DelugeOfDeath, 5.1f, centerAtTarget: true);
class TerrorUnleashed(BossModule module) : Components.RaidwideCast(module, AID.TerrorUnleashed, "Heal to full");
class TheBitterEnd(BossModule module) : Components.BaitAwayCast(module, AID.TheBitterEnd, new AOEShapeCircle(5), true, true);
class ElddragonDive(BossModule module) : Components.RaidwideCast(module, AID.ElddragonDive);
class SolemnConfiteor(BossModule module) : Components.StandardAOEs(module, AID.SolemnConfiteor, 6);
class RadiantSacrament(BossModule module) : Components.StandardAOEs(module, AID.RadiantSacrament, new AOEShapeRect(60, 20));
class Ascendance(BossModule module) : Components.RaidwideCast(module, AID.Ascendance);
class UltimateCrossoverAOE(BossModule module) : Components.RaidwideCast(module, AID.UltimateCrossoverAOE, "Tank LB3 when 'Transcend your limits...'");
class AbsoluteHoly(BossModule module) : Components.StackWithIcon(module, (uint)IconID.Stack, AID.AbsoluteHoly, 6, 5.1f);
class RadiantDesperado(BossModule module) : Components.MultiLineStack(module, 4, 60, AID.RadiantDesperadoMarker, AID.RadiantDesperado, 6);
class TwincastTowers(BossModule module) : Components.GenericTowers(module, AID.MeteoricBurst)
{
    public override void OnMapEffect(byte index, uint state)
    {
        var pos = TowerPos(index);
        if (pos == default)
            return;
        if (state == 0x00020001)
            Towers.Add(new(pos, 4, 2, 5, activation: WorldState.FutureTime(10)));
        else if (state == 0x00080004)
            Towers.RemoveAll(t => t.Position.AlmostEqual(pos, 1));
    }

    private WPos TowerPos(byte index)
    {
        if (index <= 7)
        {
            var start = new WPos(100, 89.5f);
            return Arena.Center + (start - Arena.Center).Rotate((45f * index).Degrees());
        }
        return index switch
        {
            8 => new(104, 100),
            9 => new(96, 100),
            _ => default
        };
    }
}

class SwordOfLight(BossModule module) : Components.GenericAOEs(module, AID.ShiningWave)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeCone _shape = new(50, 26.5f.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnMapEffect(byte index, uint state)
    {
        if (state != 0x00020001)
            return;
        var (pos, rot) = index switch
        {
            0x14 => (new WPos(100, 80), default),
            0x15 => (new WPos(120, 100), 270.Degrees()),
            0x16 => (new WPos(100, 120), 180.Degrees()),
            0x17 => (new WPos(80, 100), 90.Degrees()),
            _ => default
        };
        if (pos != default)
            _aoes.Add(new(_shape, pos, rot, WorldState.FutureTime(2.7f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.ShiningWave)
        {
            ++NumCasts;
            _aoes.RemoveAll(a => a.Activation <= WorldState.CurrentTime);
        }
    }
}

class RadiantBraver(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCone(60, 45.Degrees()), (uint)IconID.RadiantBraver, AID.RadiantBraver, 6)
{
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == IID)
        {
            var target = WorldState.Actors.Find(targetID) ?? actor;
            var source = Module.Enemies(OID.Helper).FirstOrDefault(h => h.Position.AlmostEqual(Arena.Center, 1))
                ?? Module.PrimaryActor;
            CurrentBaits.Add(new(source, target, Shape, WorldState.FutureTime(ActivationDelay)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.RadiantBraver or AID.RadiantBraverAOE)
            CurrentBaits.Clear();
        else
            base.OnEventCast(caster, spell);
    }
}

class AbsoluteFireIce(BossModule module) : Components.StayMove(module)
{
    private Requirement _imbued;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.AbsoluteFireIII:
                Apply(Requirement.Stay, Module.CastFinishAt(spell));
                break;
            case AID.AbsoluteBlizzardIII:
                Apply(Requirement.NoMove, Module.CastFinishAt(spell));
                break;
            case AID.ImbuedAbsoluteFireIII:
                _imbued = Requirement.Stay;
                break;
            case AID.ImbuedAbsoluteBlizzardIII:
                _imbued = Requirement.NoMove;
                break;
            case AID.ImbuedCoruscanceIn:
            case AID.ImbuedCoruscanceOut:
                if (_imbued != Requirement.None)
                    Apply(_imbued, Module.CastFinishAt(spell));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.AbsoluteFireIIIHit or AID.AbsoluteBlizzardIIIHit)
        {
            Array.Fill(PlayerStates, default);
            _imbued = Requirement.None;
        }
    }

    public override void OnStatusGain(Actor actor, in ActorStatus status)
    {
        if ((SID)status.ID == SID.Pyretic)
            SetState(Raid.FindSlot(actor.InstanceID), new(Requirement.Stay, status.ExpireAt, 1));
    }

    public override void OnStatusLose(Actor actor, in ActorStatus status)
    {
        if ((SID)status.ID == SID.Pyretic)
            ClearState(Raid.FindSlot(actor.InstanceID), 1);
    }

    private void Apply(Requirement req, DateTime act)
    {
        foreach (var (i, _) in Raid.WithSlot(true))
            SetState(i, new(req, act));
    }
}

class SpectralAdds(BossModule module) : Components.AddsMulti(module, [(uint)OID.SpectralNinja, (uint)OID.SpectralWhiteMage, (uint)OID.SpectralBlackMage, (uint)OID.SpectralDarkKnight, (uint)OID.SpectralBard, (uint)OID.SpectralWarrior, (uint)OID.SpectralSummoner, (uint)OID.SpectralEgi, (uint)OID.WyrmOfLight], 1);

static class T04WarriorOfLightComponents
{
    public static StateMachineBuilder.Phase ActivateShared(StateMachineBuilder.Phase p) => p
        .ActivateOnEnter<CoruscantSaberIn>()
        .ActivateOnEnter<CoruscantSaberOut>()
        .ActivateOnEnter<ImbuedCoruscanceIn>()
        .ActivateOnEnter<ImbuedCoruscanceOut>()
        .ActivateOnEnter<SuitonSan>()
        .ActivateOnEnter<KatonSan>()
        .ActivateOnEnter<BrimstoneEarth>()
        .ActivateOnEnter<PerfectDecimation>()
        .ActivateOnEnter<FlareBreathTether>()
        .ActivateOnEnter<FlareBreathAOE>()
        .ActivateOnEnter<Cauterize>()
        .ActivateOnEnter<DelugeOfDeath>()
        .ActivateOnEnter<TwincastTowers>()
        .ActivateOnEnter<SwordOfLight>()
        .ActivateOnEnter<TerrorUnleashed>()
        .ActivateOnEnter<SolemnConfiteor>()
        .ActivateOnEnter<RadiantSacrament>()
        .ActivateOnEnter<AbsoluteHoly>()
        .ActivateOnEnter<AbsoluteFireIce>()
        .ActivateOnEnter<SpectralAdds>()
        .ActivateOnEnter<Ascendance>()
        .ActivateOnEnter<TheBitterEnd>();

    public static StateMachineBuilder.Phase ActivateP2(StateMachineBuilder.Phase p) => ActivateShared(p)
        .ActivateOnEnter<RadiantMeteorSpread>()
        .ActivateOnEnter<RadiantBraver>()
        .ActivateOnEnter<RadiantDesperado>()
        .ActivateOnEnter<ElddragonDive>()
        .ActivateOnEnter<UltimateCrossoverAOE>();
}

class T04WarriorOfLightStates : StateMachineBuilder
{
    public T04WarriorOfLightStates(T04WarriorOfLight module) : base(module)
    {
        T04WarriorOfLightComponents.ActivateShared(TrivialPhase());
    }
}

class T04WarriorOfLightP2States : StateMachineBuilder
{
    public T04WarriorOfLightP2States(T04WarriorOfLightP2 module) : base(module)
    {
        T04WarriorOfLightComponents.ActivateP2(TrivialPhase());
    }
}

[ModuleInfo(Contributors = "Kagekazu", Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 738, NameID = 9462)] // TODO: clear Incomplete after imbued saber order + spectral adds pass
public class T04WarriorOfLight(ModuleInit init) : BossModule(init, new(100, 100), new ArenaBoundsSquare(20));

[ModuleInfo(Contributors = "Kagekazu", Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 738, NameID = 9462, PrimaryActorOID = (uint)OID.BossP2, SortOrder = 2)] // TODO: P2 Absolute Fire/Ice + Ult Crossover timing
public class T04WarriorOfLightP2(ModuleInit init) : BossModule(init, new(100, 100), new ArenaBoundsSquare(20));
