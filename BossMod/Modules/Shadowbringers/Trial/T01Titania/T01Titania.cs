namespace BossMod.Shadowbringers.Trial.T01Titania;

public enum OID : uint
{
    Helper = 0x233C, // R0.500, x?, mixed types
    Titania = 0x27ED, // R3.600, x?
    Actor1ea1a1 = 0x1EA1A1, // R2.000, x?, EventObj type
    WaterPuddles = 0x1EABE5, // R0.500, x?, EventObj type puddle counter for summoning spirit of dew
    WaterPuddlesBurst = 0x1E9E3C, // R0.500, x?, EventObj type : if spirit of dew happens these take over as puddles.
    RootGrowth = 0x1EABE4, // R0.500, x?, EventObj type
    Puck = 0x27F0, // R2.850, x?
    PuckGiant = 0x27F4, // R12.000, x?
    Actor1eabe6 = 0x1EABE6, // R0.500, x?, EventObj type
    Exit = 0x1E850B, // R0.500, x?, EventObj type
    SpiritOfFlame = 0x2878, // R2.000, x?
    SpiritOfDew = 0x27F1,
    Peaseblossom = 0x27EF, // R2.850, x?
    Mustardseed = 0x27EE, // R2.400, x?
    PeaseblossomGiant = 0x27F3, // R12.100, x?
    MustardseedGiant = 0x27F2, // R12.000, x?
}

public enum AID : uint
{
    AutoAttack = 872, // Titania/Puck/Peaseblossom->player, no cast, single-target
    BrightSabbath = 15708, // Titania->self, 4.0s cast, range 60 circle
    PhantomRuneDonut = 15710, // Titania->self, 5.0s cast, range 5-60 donut
    PhantomRuneCircle = 15709, // Titania->self, 5.0s cast, range 10 circle
    DivinationRune = 15707, // Titania->self/player, 4.0s cast, range 60 ?-degree cone
    MistRune = 15685, // Titania->self, 3.0s cast, single-target
    FlameRune = 15687, // Titania->self, 3.0s cast, single-target : Flame stack cast marker for Flamehammer probable
    FlameHammer = 17267, // SpiritOfFlame->players, no cast, range 6 circle
    MidsummerNightsDream = 15664, // Titania->self, 4.0s cast, single-target : Arena change? Does size change TODO
    GrowthRune = 15662, // Titania->self, 3.0s cast, single-target : Cast for woodsEmbrace cross
    WoodsEmbrace = 15696, // Helper->self, no cast, range 4 width 6 cross
    FrostRune = 15658, // Titania->self, 3.0s cast, single-target
    FrostRune1 = 15694, // Helper->self, 6.0s cast, range 10 circle
    Uplift = 16927, // Helper->player, 5.0s cast, range 8 circle
    _Weaponskill_ = 15665, // Titania->self, no cast, single-target
    Leafstorm = 15672, // Mustardseed->self, 2.5s cast, single-target
    GentleBreeze = 16259, // Puck->self, 2.5s cast, range 60 width 4 rect
    Leafstorm1 = 15701, // Helper->self, 3.0s cast, range 50 20.000-degree cone
    Pease = 15698, // Helper->player, 5.0s cast, range 6 circle
    Peasebomb = 15668, // Peaseblossom->self, 5.0s cast, single-target : spread markers with pease 6circle?
    Pummel = 15700, // Puck->player, 4.0s cast, single-target
    LoveInIdleness = 15677, // Titania->self, 4.0s cast, single-target : Revive the trees and make them giant.
    _Spell_ = 17888, // Helper->Puck/Peaseblossom/Mustardseed, no cast, single-target
    _AutoAttack_ = 18059, // PuckGiant->player, no cast, single-target
    _AutoAttack_1 = 18058, // PeaseblossomGiant->player, no cast, single-target
    Leafstorm2 = 15678, // MustardseedGiant->self, 4.5s cast, single-target
    Leafstorm3 = 15875, // Helper->self, 5.0s cast, range 50 20.000-degree cone
    WarAndPease = 15789, // Helper->players, 5.0s cast, range 10 circle
    Peasebomb1 = 15679, // PeaseblossomGiant->self, 5.0s cast, single-target
    PucksBreath = 15703, // PuckGiant->players, 5.0s cast, range 6 circle
    PucksRebuke = 15682, // PuckGiant->self, no cast, single-target
    PucksRebuke1 = 15705, // Helper->self, 5.0s cast, range 60 circle
    PucksRebuke2 = 15704, // Helper->self, 5.0s cast, range 5 circle
    PucksCaprice = 15702, // PuckGiant->self, 4.0s cast, range 50 circle
    BeingMortal = 15666, // Titania->self, 4.0s cast, single-target
    BeingMortal1 = 15697, // Helper->self, 12.5s cast, range 60 circle
    HardSwipe = 15699, // Peaseblossom->player, 4.0s cast, single-target
}

public enum SID : uint
{
    FireResistanceUp = 520, // none->player, extra=0x0
    VulnerabilityUp = 1789, // SpiritOfFlame/Helper->player, extra=0x1/0x2
    Bind = 280, // Helper->player, extra=0x0
    Bleeding = 320, // Helper->player, extra=0x0
    _Gen_ = 2056, // Titania->Titania, extra=0x80/0x7F
}

public enum IconID : uint
{
    DivinationIcon = 230, // player->self
    FireStackIcon = 62, // player->self : fire stack?
    UpliftSpreadIcon = 139, // player->self : spread? Uplift spread?
    MistSpread = 189, // player->self : mist spread maybe?
    ShareStackIcon = 161, // player->self : targeted a dps some sort of stack
}

sealed class BrightSabbath(BossModule module) : Components.RaidwideCast(module, (uint)AID.BrightSabbath);

sealed class PhantomRune(BossModule module)
    : Components.SimpleAOEs(module, (uint)AID.PhantomRuneDonut, new AOEShapeDonut(5f, 60f));

sealed class PhantomRune1(BossModule module)
    : Components.SimpleAOEs(module, (uint)AID.PhantomRuneCircle, new AOEShapeCircle(10f));
//angle is an estimate : tank cleave
sealed class DivinationRune(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCone(60f, 37.5f.Degrees()), (uint)IconID.DivinationIcon, (uint)AID.DivinationRune, tankbuster: true);

sealed class FrostRune(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FrostRune1, new AOEShapeCircle(10f));

sealed class Uplift(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.Uplift, 8f);

sealed class GentleBreeze(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GentleBreeze, new AOEShapeRect(60f, 4f));

sealed class LeafStorm(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.Leafstorm1, (uint)AID.Leafstorm3], new AOEShapeCone(50f, 10f.Degrees()));

sealed class WarAndPease(BossModule module) : Components.BaitAwayCast(module, (uint)AID.WarAndPease, new AOEShapeCircle(10f), centerAtTarget: true);

sealed class PucksBreath(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.PucksBreath, 6f);

sealed class PucksCaprice(BossModule module) : Components.RaidwideCast(module, (uint)AID.PucksCaprice);

sealed class BeingMortal1(BossModule module) : Components.RaidwideCast(module, (uint)AID.BeingMortal1);

// Knockback distance 10 : away from origin
sealed class PucksRebuke1(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.PucksRebuke1, 10f)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count != 0)
        {
            ref readonly var kb = ref Casters.Ref(0);
            var act = kb.Activation;
            if (!IsImmune(slot, act))
            {
                // Make a little donut of safety
                hints.AddForbiddenZone(new SDInvertedDonut(kb.Origin, 5.1f, 9.8f));
            }
        }
        base.AddAIHints(slot, actor, assignment, hints);
    }
}

sealed class PucksRebuke2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PucksRebuke2, new AOEShapeCircle(5f));

// The roots that grow across the arena in a star pattern
// If somebody has a solution that shows the aoe growing along with the animation that would be cool.
sealed class WoodsEmbrace(BossModule module) : Components.GenericBaitAway(module, centerAtTarget: true)
{
    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (actor.OID == (uint)OID.RootGrowth)
        {
            if (state == 0x00010002) // marker is placed
            {
                CurrentBaits.Add(new(actor, actor, new AOEShapeCross(10, 3)));
                CurrentBaits.Add(new(actor, actor, new AOEShapeCross(10, 3), customRotation: 45f.Degrees()));
            }
            if (state == 0x00100020) // first growth : we just place it as long as end state for simplicity
            {
                CurrentBaits.Add(new(actor, actor, new AOEShapeCross(40, 3)));
                CurrentBaits.Add(new(actor, actor, new AOEShapeCross(40, 3), customRotation: 45f.Degrees()));
            }
        }
    }

    public override void Update()
    {
        if (CurrentBaits.Count > 0 && Module.Enemies((uint)OID.RootGrowth).All(x => x.IsDead))
        {
            CurrentBaits.Clear(); // if adds die baits get cancelled
        }
    }
}

sealed class TitaniaAdds(BossModule module) : Components.AddsMulti(module,
[
    (uint)OID.Puck, (uint)OID.PuckGiant, (uint)OID.Mustardseed,
    (uint)OID.MustardseedGiant, (uint)OID.Peaseblossom,
    (uint)OID.PeaseblossomGiant, (uint)OID.SpiritOfFlame,
    (uint)OID.SpiritOfDew
], priority: 1);

sealed class Pease(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Pease, new AOEShapeCircle(6f));

sealed class PeasebombSpread(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.Peasebomb, 6f);

/*
 * Puddles that stay on the arena for several minutes. This class
 * lets us show them if we want like when FireRune is cast.
 */
sealed class WaterPuddles(BossModule module) : BossComponent(module)
{
    private bool _fireCasting;
    private DateTime _activation;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);
        if (spell.Action.ID == (uint)AID.FlameRune)
        {
            _fireCasting = true;
            _activation = WorldState.FutureTime(2.7f);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        base.OnCastFinished(caster, spell);
        if (spell.Action.ID == (uint)AID.FlameRune)
            _fireCasting = false;
    }

    public static List<Actor> GetPuddles(BossModule module)
    {
        /*
         * There are two objects for puddles.  WaterPuddles is 4 puddles regardless.
         * if nobody stacks in a puddle and it summons a spirit of dew then the WaterPuddlesBurst
         * object takes over for the visual representation.
         */
        var orbs = module.Enemies((uint)OID.WaterPuddles);
        var burstOrbs = module.Enemies((uint)OID.WaterPuddlesBurst);
        // Check if there are WaterPuddlesBurst, if not check if there are WaterPuddles, else count = 0
        var count = burstOrbs.Count > 0 ? burstOrbs.Count : (orbs.Count > 0 ? orbs.Count : 0);
        if (count == 0)
            return [];

        var filteredWater = new List<Actor>(count);
        for (var i = 0; i < count; ++i)
        {
            var z = burstOrbs.Count > 0 ? burstOrbs[i] : orbs[i];
            if (!z.IsDead)
                filteredWater.Add(z);
        }
        return filteredWater;
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        var orbs = GetPuddles(Module);
        var count = orbs.Count;

        if (count != 0 && _fireCasting)
            hints.Add("Stand in puddles to get fire resist.");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var puddles = GetPuddles(Module);
        var count = puddles.Count;

        if (count != 0)
        {
            var puddlez = new ShapeDistance[count];
            for (var i = 0; i < count; ++i)
            {
                var p = puddles[i];
                puddlez[i] = new SDInvertedCircle(p.Position, 4.5f);
            }
            if (_fireCasting)
                hints.AddForbiddenZone(new SDIntersection(puddlez), _activation);
        }
        base.AddAIHints(slot, actor, assignment, hints);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var orbs = GetPuddles(Module);
        var count = orbs.Count;
        for (var i = 0; i < count; ++i)
            if (_fireCasting)
                Arena.ZoneCircle(orbs[i].Position, 5f, Colors.SafeFromAOE);
    }
}

// create towers for the water puddles min 1 max 2 for four towers
sealed class WaterTowers(BossModule module) : Components.GenericTowers(module)
{
    public override void OnActorCreated(Actor actor)
    {
        // initial puddles towers.
        if (actor.OID == (uint)OID.WaterPuddles)
        {
            Towers.Add(new(actor.Position, 5f, 1, 2));
        }
        // if nobody soaks a tower they disappear and WaterPuddlesBurst take their place.
        else if (actor.OID == (uint)OID.WaterPuddlesBurst)
        {
            Towers.Clear();
        }
    }

    // A fallback tower removal in case all the towers were soaked and no WaterPuddlesBurst happened.
    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        base.OnCastFinished(caster, spell);
        if (spell.Action.ID == (uint)AID.MistRune)
        {
            Towers.Clear();
        }
    }
}

sealed class FlameRune(BossModule module) : Components.StackWithIcon(module, (uint)IconID.FireStackIcon, (uint)AID.FlameHammer, 5f, 3d, 8, 8);


[SkipLocalsInit]
sealed class TitaniaStates : StateMachineBuilder
{
    public TitaniaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BrightSabbath>()
            .ActivateOnEnter<WaterPuddles>()
            .ActivateOnEnter<WaterTowers>()
            .ActivateOnEnter<FlameRune>()
            .ActivateOnEnter<PhantomRune>()
            .ActivateOnEnter<PhantomRune1>()
            .ActivateOnEnter<DivinationRune>()
            .ActivateOnEnter<FrostRune>()
            .ActivateOnEnter<WoodsEmbrace>()
            .ActivateOnEnter<Uplift>()
            .ActivateOnEnter<TitaniaAdds>()
            .ActivateOnEnter<GentleBreeze>()
            .ActivateOnEnter<LeafStorm>()
            .ActivateOnEnter<WarAndPease>()
            .ActivateOnEnter<PucksBreath>()
            .ActivateOnEnter<PucksRebuke2>()
            .ActivateOnEnter<PucksCaprice>()
            .ActivateOnEnter<Pease>()
            .ActivateOnEnter<PeasebombSpread>()
            .ActivateOnEnter<BeingMortal1>()
            .ActivateOnEnter<PucksRebuke1>()
            ;
    }
}


[ModuleInfo(BossModuleInfo.Maturity.Contributed,
    StatesType = typeof(TitaniaStates),
    ConfigType = null, // replace null with typeof(TitaniaConfig) if applicable
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID),
    StatusIDType = null, // replace null with typeof(SID) if applicable
    TetherIDType = null, // replace null with typeof(TetherID) if applicable
    IconIDType = typeof(IconID),
    PrimaryActorOID = (uint)OID.Titania,
    Contributors = "wen",
    Expansion = BossModuleInfo.Expansion.Shadowbringers,
    Category = BossModuleInfo.Category.Trial,
    GroupType = BossModuleInfo.GroupType.CFC,
    GroupID = 657u,
    NameID = 8361u,
    SortOrder = 1,
    PlanLevel = 0)]

[SkipLocalsInit]
public sealed class Titania(WorldState ws, Actor primary) : BossModule(ws, primary, new(100f, 100f), new ArenaBoundsSquare(20f));
