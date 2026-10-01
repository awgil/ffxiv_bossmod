namespace BossMod.Shadowbringers.Trial.T03Hades;

public enum OID : uint
{
    Boss = 0x2949, // R6.750
    BossP2 = 0x294A, // R20.000, Hades big
    Helper = 0x233C, // R0.500
    ShadowOfTheAncients = 0x27AE, // R1.250
    ShadowOfTheAncientsStack = 0x27B7, // R1.250
    AetherialGaol = 0x294B, // R8.000
    NetherBlastPart1 = 0x29D8, // R1.000
    NetherBlastPart2 = 0x29D9, // R1.000
    NetherBlastPart3 = 0x29DA, // R1.000
    NetherBlastPart4 = 0x29DB, // R1.000
    BrokenFaithObject = 0x1EAD28, // R0.500, EventObj
    DoomTower = 0x1EAD29, // R0.500, EventObj
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    AutoAttackP2 = 16764, // BossP2->player, no cast, single-target
    RavenousAssault = 16728, // Boss->player, 5.0s cast, range 5 circle
    BadFaithVisual = 16713, // Boss->self, 5.0s cast, single-target, visual
    BadFaithVisualAlt = 16714, // Boss->self, 5.0s cast, single-target, visual
    BadFaith = 16715, // Helper->self, 5.0s cast, range 20 width 20 rect
    BadFaithAlt = 16716, // Helper->self, 5.0s cast, range 20 width 20 rect
    BrokenFaith = 16717, // Boss->self, 3.0s cast, single-target
    BrokenFaithAOE = 16718, // Helper->self, no cast, range 10 circle
    Double = 16719, // Boss->self, 3.0s cast, single-target
    DarkEruptionVisual = 16720, // Boss->self, 3.0s cast, single-target, visual
    DarkEruptionAOE = 16722, // Helper->location, 3.0s cast, range 6 circle
    DarkEruption = 16723, // Helper->location, no cast, range 6 circle
    ShadowSpreadVisual = 16724, // Boss->self, 3.0s cast, single-target, visual
    ShadowSpread = 16726, // Helper->self, 3.0s cast, range 40 30-degree cone
    ShadowSpreadAlt = 16727, // Helper->self, 3.0s cast, range 40 30-degree cone
    AncientDarkness = 17811, // ShadowOfTheAncientsStack->player, 5.0s cast, range 5 circle
    AncientWaterIII = 17812, // ShadowOfTheAncientsStack->players, 5.0s cast, range 6 circle
    AncientAero = 17813, // ShadowOfTheAncientsStack->self, 5.0s cast, range 40 width 8 rect
    AncientDarkIV = 17815, // Boss->self, 5.0s cast, range 100 circle
    Titanomachy = 16768, // BossP2->self, 4.0s cast, range 100 circle
    ShadowStream = 16732, // BossP2->self, 5.0s cast, range 100 width 16 rect
    DualStrikeVisual = 16737, // BossP2->self, 5.0s cast, single-target, visual
    DualStrike = 16738, // Helper->player, 5.0s cast, range 5 circle
    EchoOfTheLostL = 16739, // BossP2->self, 7.0s cast, range 100 90-degree cone
    EchoOfTheLostR = 16740, // BossP2->self, 7.0s cast, range 100 90-degree cone
    WailOfTheLost = 16741, // BossP2->self, 5.0s cast, range 40 width 40 rect
    Captivity = 16744, // BossP2->self, 5.0s cast, single-target
    CaptivityCast = 16745, // Helper->player, no cast, range 8 circle
    ChorusOfTheLost = 16748, // BossP2->self, 30.0s cast, range 100 circle
    HellbornYawpVisual = 16750, // BossP2->self, 5.0s cast, single-target, visual
    HellbornYawp = 16751, // Helper->self, 4.0s cast, range 100 60-degree cone
    PolydegmonsPurgationVisual = 16752, // BossP2->self, 5.0s cast, single-target, visual
    PolydegmonsPurgation = 16753, // Helper->self, 5.0s cast, range 100 width 16 rect
    PolydegmonsPurgationAlt = 16754, // Helper->self, 5.0s cast, range 100 width 16 rect
    NetherBlast = 16755, // NetherBlastPart->player, no cast, range 6 circle
    LifeInCaptivity = 16757, // BossP2->self, 4.0s cast, range 100 circle
    BlackCauldronVisual = 16758, // BossP2->self, no cast, single-target, visual
    BlackCauldron = 16730, // Helper->self, no cast, range 100 circle
    TheDarkDevoursVisual = 16759, // BossP2->self, 3.0s cast, single-target, visual
    TheDarkDevours = 16761, // Helper->self, no cast, range 100 circle
}

public enum IconID : uint
{
    Spread = 139, // player
    CaptivityBait = 120, // player
}

public enum TetherID : uint
{
    NetherBlast = 17,
}

class RavenousAssault(BossModule module) : Components.BaitAwayCast(module, AID.RavenousAssault, new AOEShapeCircle(5), centerAtTarget: true, endsOnCastEvent: true);
class BadFaith(BossModule module) : Components.GroupedAOEs(module, [AID.BadFaith, AID.BadFaithAlt], new AOEShapeRect(20, 10));
class ShadowSpread(BossModule module) : Components.GroupedAOEs(module, [AID.ShadowSpread, AID.ShadowSpreadAlt], new AOEShapeCone(40, 15.Degrees()));
class DarkEruptionAOE(BossModule module) : Components.StandardAOEs(module, AID.DarkEruptionAOE, 6);
class DarkEruptionSpread(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCircle(6), (uint)IconID.Spread, AID.DarkEruption, 3, centerAtTarget: true)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (CurrentBaits.Count != 0 && spell.Action.ID is (uint)AID.DarkEruption or (uint)AID.NetherBlast)
            CurrentBaits.RemoveAt(0);
    }
}

class BrokenFaith(BossModule module) : Components.GenericAOEs(module, AID.BrokenFaithAOE)
{
    private static readonly AOEShapeCircle _shape = new(10);
    private readonly List<AOEInstance> _aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (actor.OID == (uint)OID.BrokenFaithObject && state == 0x00010002u)
            _aoes.Add(new(_shape, actor.Position, default, WorldState.FutureTime(8.5f)));
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if (actor.OID == (uint)OID.BrokenFaithObject)
            _aoes.RemoveAll(i => i.Origin == actor.Position);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            ++NumCasts;
            _aoes.RemoveAll(i => i.Activation <= WorldState.CurrentTime);
        }
    }
}

class AncientWaterIII(BossModule module) : Components.StackWithCastTargets(module, AID.AncientWaterIII, 6, 2, 8);
class AncientDarkness(BossModule module) : Components.BaitAwayCast(module, AID.AncientDarkness, new AOEShapeCircle(5), centerAtTarget: true);
class AncientAero(BossModule module) : Components.StandardAOEs(module, AID.AncientAero, new AOEShapeRect(40, 4));
class AncientDarkIV(BossModule module) : Components.RaidwideCast(module, AID.AncientDarkIV);
class Titanomachy(BossModule module) : Components.RaidwideCast(module, AID.Titanomachy);
class ShadowStreamPurgation(BossModule module) : Components.GroupedAOEs(module, [AID.ShadowStream, AID.PolydegmonsPurgation, AID.PolydegmonsPurgationAlt], new AOEShapeRect(100, 8));
class DualStrike(BossModule module) : Components.BaitAwayCast(module, AID.DualStrike, new AOEShapeCircle(5), centerAtTarget: true, endsOnCastEvent: true);
class WailOfTheLost(BossModule module) : Components.StandardAOEs(module, AID.WailOfTheLost, new AOEShapeRect(40, 20));
class EchoOfTheLost(BossModule module) : Components.GroupedAOEs(module, [AID.EchoOfTheLostL, AID.EchoOfTheLostR], new AOEShapeCone(100, 45.Degrees()));
class HellbornYawp(BossModule module) : Components.BaitAwayCast(module, AID.HellbornYawp, new AOEShapeCone(100, 30.Degrees()));
class CaptivityBait(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCircle(8), (uint)IconID.CaptivityBait, AID.CaptivityCast, 4.7f, centerAtTarget: true);
class LifeInCaptivity(BossModule module) : Components.RaidwideCast(module, AID.LifeInCaptivity);
class TheDarkDevours(BossModule module) : Components.RaidwideCastDelay(module, AID.TheDarkDevoursVisual, AID.TheDarkDevours, 0.5f);
class BlackCauldron(BossModule module) : Components.RaidwideInstant(module, AID.BlackCauldron, 0.5f)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.BlackCauldronVisual)
            Activation = WorldState.FutureTime(Delay);
        base.OnEventCast(caster, spell);
    }
}
class ChorusOfTheLost(BossModule module) : Components.RaidwideCast(module, AID.ChorusOfTheLost);
class ShadowAdds(BossModule module) : Components.AddsMulti(module, [(uint)OID.ShadowOfTheAncients, (uint)OID.ShadowOfTheAncientsStack, (uint)OID.AetherialGaol], 1);

class DoomTowers(BossModule module) : Components.GenericTowers(module)
{
    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.DoomTower)
            Towers.Add(new(actor.Position, 5, 1, 2));
    }

    public override void OnActorDestroyed(Actor actor)
    {
        base.OnActorDestroyed(actor);
        if (actor.OID == (uint)OID.DoomTower)
            Towers.Clear();
    }
}

class NetherBlast(BossModule module) : Components.BaitAwayTethers(module, new AOEShapeCircle(6), (uint)TetherID.NetherBlast, AID.NetherBlast, centerAtTarget: true)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (spell.Action == WatchedAction)
            CurrentBaits.RemoveAll(b => b.Target.IsDead);
    }
}

class T03HadesStates : StateMachineBuilder
{
    private readonly T03Hades _module;

    public T03HadesStates(T03Hades module) : base(module)
    {
        _module = module;
        SimplePhase(0, Phase1, "P1")
            .Raw.Update = () => _module.BossP2() != null || Module.PrimaryActor.IsDeadOrDestroyed;
        SimplePhase(1, Phase2, "P2")
            .Raw.Update = () => Module.PrimaryActor.IsDeadOrDestroyed && (_module.BossP2()?.IsDead ?? false);
    }

    private void Phase1(uint id)
    {
        SimpleState(id, 10000, "P2")
            .ActivateOnEnter<RavenousAssault>()
            .ActivateOnEnter<BadFaith>()
            .ActivateOnEnter<ShadowSpread>()
            .ActivateOnEnter<DarkEruptionSpread>()
            .ActivateOnEnter<DarkEruptionAOE>()
            .ActivateOnEnter<BrokenFaith>()
            .ActivateOnEnter<AncientWaterIII>()
            .ActivateOnEnter<AncientDarkness>()
            .ActivateOnEnter<AncientAero>()
            .ActivateOnEnter<AncientDarkIV>()
            .ActivateOnEnter<ShadowAdds>();
    }

    private void Phase2(uint id)
    {
        SimpleState(id, 10000, "Enrage")
            .ActivateOnEnter<Titanomachy>()
            .ActivateOnEnter<ShadowStreamPurgation>()
            .ActivateOnEnter<DualStrike>()
            .ActivateOnEnter<EchoOfTheLost>()
            .ActivateOnEnter<WailOfTheLost>()
            .ActivateOnEnter<HellbornYawp>()
            .ActivateOnEnter<TheDarkDevours>()
            .ActivateOnEnter<BlackCauldron>()
            .ActivateOnEnter<LifeInCaptivity>()
            .ActivateOnEnter<ChorusOfTheLost>()
            .ActivateOnEnter<CaptivityBait>()
            .ActivateOnEnter<DoomTowers>()
            .ActivateOnEnter<NetherBlast>()
            .ActivateOnEnter<ShadowAdds>();
    }
}

[ModuleInfo(Contributors = "Kagekazu", Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 687, NameID = 8352)] // TODO: clear Incomplete after P1/P2 + Doom towers verify
public class T03Hades(ModuleInit init) : BossModule(init, new(100, 100), new ArenaBoundsCircle(20))
{
    public Actor? BossP2() => Enemies(OID.BossP2).FirstOrDefault(a => !a.IsDestroyed);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actor(BossP2(), ArenaColor.Enemy);
    }
}
