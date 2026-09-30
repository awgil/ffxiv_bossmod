namespace BossMod.Shadowbringers.Trial.T01Titania;

public enum OID : uint
{
    Boss = 0x27ED, // R3.600, Titania
    Helper = 0x233C, // R0.500
    Mustardseed = 0x27EE, // R2.400
    Peaseblossom = 0x27EF, // R2.850
    Puck = 0x27F0, // R2.850
    SpiritOfDew = 0x27F1,
    MustardseedGiant = 0x27F2, // R12.000
    PeaseblossomGiant = 0x27F3, // R12.100
    PuckGiant = 0x27F4, // R12.000
    SpiritOfFlame = 0x2878, // R2.000
    WaterPuddles = 0x1EABE5, // R0.500, EventObj
    WaterPuddlesBurst = 0x1E9E3C, // R0.500, EventObj
    RootGrowth = 0x1EABE4, // R0.500, EventObj
}

public enum AID : uint
{
    AutoAttack = 872, // Boss/Peaseblossom/Puck->player, no cast, single-target
    FrostRune = 15658, // Boss->self, 3.0s cast, single-target
    GrowthRune = 15662, // Boss->self, 3.0s cast, single-target
    MidsummerNightsDream = 15664, // Boss->self, 4.0s cast, single-target
    MidsummerNightsDreamHit = 15665, // Boss->self, no cast
    BeingMortal = 15666, // Boss->self, 4.0s cast, single-target
    Peasebomb = 15668, // Peaseblossom->self, 5.0s cast, single-target
    LeafstormVisual = 15672, // Mustardseed->self, 2.5s cast, single-target
    LoveInIdleness = 15677, // Boss->self, 4.0s cast, single-target
    LeafstormGiant = 15678, // MustardseedGiant->self, 4.5s cast, single-target
    PeasebombGiant = 15679, // PeaseblossomGiant->self, 5.0s cast, single-target
    PucksRebukeVisual = 15682, // PuckGiant->self, no cast, single-target
    MistRune = 15685, // Boss->self, 3.0s cast, single-target
    FlameRune = 15687, // Boss->self, 3.0s cast, single-target
    FrostRuneAOE = 15694, // Helper->self, 6.0s cast, range 10 circle
    WoodsEmbraceAOE = 15696, // Helper->self, no cast, range 4 width 6 cross
    BeingMortalAOE = 15697, // Helper->self, 12.5s cast, range 60 circle
    Pease = 15698, // Helper->player, 5.0s cast, range 6 circle spread
    HardSwipe = 15699, // Peaseblossom->player, 4.0s cast, single-target, tankbuster
    Pummel = 15700, // Puck->player, 4.0s cast, single-target, tankbuster
    Leafstorm = 15701, // Helper->self, 3.0s cast, range 50 20-degree cone
    PucksCaprice = 15702, // PuckGiant->self, 4.0s cast, range 50 circle
    PucksBreath = 15703, // PuckGiant->player, 5.0s cast, range 6 circle stack
    PucksRebukeNear = 15704, // Helper->self, 5.0s cast, range 5 circle
    PucksRebuke = 15705, // Helper->self, 5.0s cast, range 60 circle knockback
    DivinationRune = 15707, // Boss->player, 4.0s cast, range 60 cone, tankbuster
    BrightSabbath = 15708, // Boss->self, 4.0s cast, range 60 circle
    PhantomRuneIn = 15709, // Boss->self, 5.0s cast, range 10 circle
    PhantomRuneOut = 15710, // Boss->self, 5.0s cast, range 5-60 donut
    WarAndPease = 15789, // Helper->player, 5.0s cast, range 10 circle stack
    LeafstormRepeat = 15875, // Helper->self, 5.0s cast, range 50 20-degree cone
    GentleBreeze = 16259, // Puck->self, 2.5s cast, range 60 width 4 rect
    Uplift = 16927, // Helper->player, 5.0s cast, range 6 circle spread
    FlameHammer = 17267, // SpiritOfFlame->self, no cast, range 6 circle
    PeaseblossomSpecial = 18058, // PeaseblossomGiant->self, no cast, single-target
    PuckSpecial = 18059, // PuckGiant->self, no cast
}

public enum IconID : uint
{
    Stack = 62, // player
    Divination = 230, // player
}

class FrostRuneAOE(BossModule module) : Components.StandardAOEs(module, AID.FrostRuneAOE, 10);
class BeingMortalAOE(BossModule module) : Components.RaidwideCast(module, AID.BeingMortalAOE);
class Pease(BossModule module) : Components.SpreadFromCastTargets(module, AID.Pease, 6);
class HardSwipe(BossModule module) : Components.SingleTargetCast(module, AID.HardSwipe);
class Pummel(BossModule module) : Components.SingleTargetCast(module, AID.Pummel);
class Leafstorm(BossModule module) : Components.GroupedAOEs(module, [AID.Leafstorm, AID.LeafstormRepeat], new AOEShapeCone(50, 10.Degrees()));
class PucksCaprice(BossModule module) : Components.RaidwideCast(module, AID.PucksCaprice);
class PucksBreath(BossModule module) : Components.StackWithCastTargets(module, AID.PucksBreath, 6);
class PucksRebukeNear(BossModule module) : Components.StandardAOEs(module, AID.PucksRebukeNear, 5);
class PucksRebuke(BossModule module) : Components.KnockbackFromCastTarget(module, AID.PucksRebuke, 10);
class DivinationRune(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCone(60, 37.5f.Degrees()), (uint)IconID.Divination, AID.DivinationRune, 4f, damageType: AIHints.PredictedDamageType.Tankbuster); // TODO: verify cone half-angle
class BrightSabbath(BossModule module) : Components.RaidwideCast(module, AID.BrightSabbath);
class PhantomRuneIn(BossModule module) : Components.StandardAOEs(module, AID.PhantomRuneIn, 10);
class PhantomRuneOut(BossModule module) : Components.StandardAOEs(module, AID.PhantomRuneOut, new AOEShapeDonut(5, 60));
class WarAndPease(BossModule module) : Components.StackWithCastTargets(module, AID.WarAndPease, 10, 8, 8);
class GentleBreeze(BossModule module) : Components.StandardAOEs(module, AID.GentleBreeze, new AOEShapeRect(60, 2));
class Uplift(BossModule module) : Components.SpreadFromCastTargets(module, AID.Uplift, 6);
class FlameRune(BossModule module) : Components.StackWithIcon(module, (uint)IconID.Stack, AID.FlameHammer, 6, 5.1f, 8, 8);
class TitaniaAdds(BossModule module) : Components.AddsMulti(module, [(uint)OID.Mustardseed, (uint)OID.Peaseblossom, (uint)OID.Puck, (uint)OID.MustardseedGiant, (uint)OID.PeaseblossomGiant, (uint)OID.PuckGiant, (uint)OID.SpiritOfFlame, (uint)OID.SpiritOfDew], 1);

class WoodsEmbrace(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (actor.OID != (uint)OID.RootGrowth)
            return;
        // TODO: confirm EAnim state → cross size mapping and activation delays from a clean replay
        if (state == 0x00010002)
        {
            _aoes.Add(new(new AOEShapeCross(10, 3), actor.Position));
            _aoes.Add(new(new AOEShapeCross(10, 3), actor.Position, 45f.Degrees()));
        }
        if (state == 0x00100020)
        {
            _aoes.Add(new(new AOEShapeCross(40, 3), actor.Position));
            _aoes.Add(new(new AOEShapeCross(40, 3), actor.Position, 45f.Degrees()));
        }
    }

    public override void Update()
    {
        if (_aoes.Count > 0 && Module.Enemies((uint)OID.RootGrowth).All(x => x.IsDead))
            _aoes.Clear();
    }
}

class WaterPuddles(BossModule module) : BossComponent(module)
{
    private bool _fireCasting;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.FlameRune)
            _fireCasting = true;
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.FlameRune)
            _fireCasting = false;
    }

    public static List<Actor> GetPuddles(BossModule module)
    {
        var orbs = module.Enemies((uint)OID.WaterPuddles);
        var burstOrbs = module.Enemies((uint)OID.WaterPuddlesBurst);
        var count = burstOrbs.Count > 0 ? burstOrbs.Count : orbs.Count;
        if (count == 0)
            return [];

        var filtered = new List<Actor>(count);
        for (var i = 0; i < count; ++i)
        {
            var z = burstOrbs.Count > 0 ? burstOrbs[i] : orbs[i];
            if (!z.IsDead)
                filtered.Add(z);
        }
        return filtered;
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (GetPuddles(Module).Count != 0 && _fireCasting)
            hints.Add("Stand in puddles to get fire resist.");
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (!_fireCasting)
            return;
        foreach (var orb in GetPuddles(Module))
            Arena.ZoneCircle(orb.Position, 5, ArenaColor.SafeFromAOE);
    }
}

class WaterTowers(BossModule module) : Components.GenericTowers(module)
{
    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.WaterPuddles)
            Towers.Add(new(actor.Position, 5, 1, 2));
        else if (actor.OID == (uint)OID.WaterPuddlesBurst)
            Towers.Clear();
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        base.OnCastFinished(caster, spell);
        if (spell.Action.ID == (uint)AID.MistRune)
            Towers.Clear();
    }
}

class T01TitaniaStates : StateMachineBuilder
{
    public T01TitaniaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<FrostRuneAOE>()
            .ActivateOnEnter<BeingMortalAOE>()
            .ActivateOnEnter<Pease>()
            .ActivateOnEnter<HardSwipe>()
            .ActivateOnEnter<Pummel>()
            .ActivateOnEnter<Leafstorm>()
            .ActivateOnEnter<PucksCaprice>()
            .ActivateOnEnter<PucksBreath>()
            .ActivateOnEnter<PucksRebukeNear>()
            .ActivateOnEnter<PucksRebuke>()
            .ActivateOnEnter<DivinationRune>()
            .ActivateOnEnter<BrightSabbath>()
            .ActivateOnEnter<PhantomRuneIn>()
            .ActivateOnEnter<PhantomRuneOut>()
            .ActivateOnEnter<WarAndPease>()
            .ActivateOnEnter<GentleBreeze>()
            .ActivateOnEnter<Uplift>()
            .ActivateOnEnter<FlameRune>()
            .ActivateOnEnter<WoodsEmbrace>()
            .ActivateOnEnter<WaterPuddles>()
            .ActivateOnEnter<WaterTowers>()
            .ActivateOnEnter<TitaniaAdds>();
    }
}

[ModuleInfo(Contributors = "Kagekazu", Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 657, NameID = 8361)] // TODO: clear Incomplete after WoodsEmbrace EAnim + water tower pass
public class T01Titania(ModuleInit init) : BossModule(init, new(100, 100), new ArenaBoundsSquare(20));
