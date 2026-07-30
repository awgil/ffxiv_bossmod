namespace BossMod.Stormblood.Trial.T03Shinryu;

public enum OID : uint
{
    Shinryu = 0x1983, // R22.000, x?
    Platform = 0x1EA1A1, // R2.000, x?, EventObj type : Destructible square tiles. 9 platforms.
    RightWing = 0x1B1A, // R15.000, x?, Part type
    LeftWing = 0x1B19, // R15.000, x?, Part type
    WaterSpout = 0x1E8536, // R2.000, x?, EventObj type : Water Spout
    WaterPuddles = 0x1E950D, // R0.500, x?, EventObj type
    Icicle = 0x1B16, // R2.500, x?
    EyeOfTheStorm = 0x1B17, // R1.000, Hurricane with pulsing knockback
    Cocoon1 = 0x1B13, // R3.000, x?
    MassiveCocoon = 0x1C86, // R6.000, x?
    Ginryu = 0x1B14, // R1.800, x?
    Hakkinryu = 0x1C83, // R3.600, x?
    Fetters = 0x1B15, // R1.000, x? （仮）temporary　鎖 : Fetters, then atb button mash.
    Tail = 0x1B12, // R17.940, x?, Helper type
    Helper = 0x18D6 // R0.500, x?, mixed types : Helper types~
}

public enum AID : uint
{
    AutoAttack = 8105, // RightWing/LeftWing->player, no cast, single-target
    TidalWave = 8075, // Helper->self, 10.0s cast, range 80+R width 60 rect
    TidalWaveCast = 8106, // Shinryu->self, 10.0s cast, single-target
    Levinbolt = 8092, // Helper->player, no cast, range 5 circle
    LevinboltVisual = 8091, // RightWing->self, 6.0s cast, single-target
    AkhMornVisual = 8100, // Shinryu->players, 4.0s cast, ??? : tank stack?
    AkhMorn1 = 8101, // Shinryu->players, no cast, ??? :
    SummonIcicle = 8095, // LeftWing->self, 4.0s cast, single-target
    IcicleImpact = 8096, // Icicle->self, no cast, range 6 circle
    Spikesicle = 8097, // Icicle->self, 2.5s cast, range 62+R width 10 rect
    Hellfire = 8076, // Helper->self, 10.0s cast, range 60 circle
    HellfireVisual = 8107, // Shinryu->self, 10.0s cast, single-target
    _Ability_ = 8074, // Shinryu->self, no cast, single-target
    MeteorImpact = 9291, // Helper->self, 5.0s cast, range 60 circle
    MeteorImpact1 = 8086, // Cocoon1/MassiveCocoon->self, 4.0s cast, range 60 circle
    _Ability_1 = 8514, // Cocoon1/MassiveCocoon->self, no cast, single-target
    Attack1 = 870, // Hakkinryu/Ginryu->player, no cast, single-target
    Collapse = 8728, // Hakkinryu->self, no cast, range 8+R ?-degree cone
    Protostar = 8085, // Shinryu->self, 6.0s cast, range 80 circle
    Protostar1 = 8123, // Helper->self, no cast, range 50 circle
    DarkMatter = 8088, // Shinryu->self, 3.0s cast, range 60 circle
    _Ability_2 = 8488, // Shinryu->self, no cast, single-target
    GyreCharge = 8104, // Shinryu->self, no cast, range 100+R width 60 rect
    GyreChargeVisual = 8180, // Helper->self, 6.3s cast, range 100+R width 60 rect
    _Ability_3 = 8081, // Shinryu->self, no cast, single-target
    TailSlap1 = 8083, // Tail->self, 3.0s cast, range 40 width 20 rect
    TailSlap2 = 9130, // Tail->self, 3.0s cast, range 40 width 20 rect

    _Ability_4 = 8084, // Tail->self, no cast, single-target
    _Ability_5 = 8142, // Shinryu->self, no cast, single-target
    _Ability_6 = 8082, // Shinryu->self, no cast, single-target
    IceStorm = 8098, // LeftWing->self, 6.0s cast, single-target
    BurningChains = 8144, // Helper->self, no cast : Burning chains tether on two players. Run apart to break chains
    IceStormRaidwide = 8099, // Helper->self, no cast, range 60 circle
    _Ability_7 = 8143, // Shinryu->self, no cast, single-target
    Dragonfist = 9455, // Shinryu->self, no cast, single-target
    DragonfistVisual = 9456, // Helper->self, 4.0s cast, range 16 circle
    DiamondDust = 8078, // Helper->self, 10.0s cast, range 60 circle
    DiamondDust1 = 8109, // Shinryu->self, 10.0s cast, single-target
    Fireball = 8732, // Ginryu->location, 2.5s cast, range 4 circle
    DeathSentence = 8731, // Hakkinryu->player, 4.0s cast, single-target
    SpikedTail = 8729, // Ginryu->player, 1.0s cast, single-target

    JudgmentBolt = 8077, // Helper->self, 10.0s cast, range 60 circle
    JudgmentBolt1 = 8108, // Shinryu->self, 10.0s cast, single-target

    EarthenFury = 8079, // Helper->self, 10.0s cast, range 60 circle
    EarthenFuryCast = 8110, // Shinryu->self, 10.0s cast, single-target
    EarthenFurySmash = 9146, // 18D6->location, Range 20, width 20 rectangle : cracks or destroys one square.
    AkhRhai = 8102, // Helper->location, no cast, range 4 circle
    AkhRhai1 = 8103, // Helper->location, no cast, range 4 circle
    HypernovaCast = 8089, // RightWing->self, 6.0s cast, single-target
    Hypernova = 8090, // Helper->players, no cast, range 8 circle stack

    SuperCycloneKB = 8984, // Helper->self, 0.5s cast, range 90 circle : distance 5 knockback
    AerialBlastKB = 8080, // Helper->self, 10.0s cast, range 60 circle : distance 15 knockback
    AerialBlast1 = 8111, // Shinryu->self, 10.0s cast, single-target
    BlazingTrail = 8730, // Ginryu->self, 3.0s cast, range 15+R width 11 rect

    EarthBreath = 8093, // Shinryu->self, 9.0s cast, range 6+R ?-degree cone
    EarthBreath1 = 8094, // Helper->self, 4.5s cast, range 80+R 60.000-degree cone
}

public enum SID : uint
{
    FireResistanceUp = 520, // none->player, extra=0x0
    LightningResistanceDownII = 1260, // none->player, extra=0x0
    Paralysis = 17, // Helper->player, extra=0x0
    Fetters = 667, // Boss->player, extra=0xEC4
    Affixed = 1267, // Boss->player, extra=0xE/0xC/0x14/0x18/0x12/0xD/0x13/0xF
    BurningChains = 769, // none->player, extra=0x0 : run apart to break chains
    ThinIce = 911 // none->player, extra=0x12C, Diamond Dust gives thin ice. pc slips and slides
}

public enum IconID : uint
{
    LevinMarker = 24, // player : Levin Bolt Spread marker
    BurningChainsIcon = 97, // player :
    HyperNovaStackIcon = 62, // player->self
    BaitAwayIcon = 98, // player->self : green marker.  Might bait a tail slap? Unconfirmed.
    EarthBreathIcon = 23 // player->self
}

public enum TetherID : uint
{
    BurningTether = 9 // player->player
}

/*
 * Tidal wave.  Water spout appears to one side of arena and knocks players back 35
 * away from itself.
 * Leaves puddles scattered around the arena.
 */
sealed class TidalWave(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.TidalWave, 35f, kind: Kind.DirForward)
{
    // Add a safe zone at knockback origin so ai knows where to run.
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count != 0)
        {
            ref readonly var kb = ref Casters.Ref(0);
            var act = kb.Activation;
            if (!IsImmune(slot, act))
            {
                hints.AddForbiddenZone(new SDInvertedCircle(kb.Origin, 12f), act);
            }
        }
    }
}

/*
 * Right wing casts levinbolt.  Spread marker icon.
 */
sealed class Levinbolt(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.LevinMarker, (uint)AID.Levinbolt, 5f, 6f);

/*
 * Puddles that stay on the arena for several minutes. Have different
 * interactions with various elements.
 */
sealed class WaterPuddles(BossModule module) : BossComponent(module)
{
    private bool _levinCasting;
    private bool _fireCasting;
    private DateTime _activation;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);
        if (spell.Action.ID == (uint)AID.LevinboltVisual)
        {
            _levinCasting = true;
            _activation = WorldState.FutureTime(6d);
        }

        else if (spell.Action.ID == (uint)AID.HellfireVisual)
        {
            _fireCasting = true;
            _activation = WorldState.FutureTime(10d);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        base.OnCastFinished(caster, spell);
        if (spell.Action.ID == (uint)AID.LevinboltVisual)
            _levinCasting = false;
        else if (spell.Action.ID == (uint)AID.HellfireVisual)
            _fireCasting = false;
    }

    public static List<Actor> GetPuddles(BossModule module)
    {
        var orbs = module.Enemies((uint)OID.WaterPuddles);
        var count = orbs.Count;
        if (count == 0)
            return [];

        var filteredWater = new List<Actor>(count);
        for (var i = 0; i < count; ++i)
        {
            var z = orbs[i];
            if (!z.IsDead)
                filteredWater.Add(z);
        }
        return filteredWater;
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        var orbs = GetPuddles(Module);

        if (orbs.Count != 0)
        {
            if (_levinCasting)
                hints.Add("Avoid puddles during lightning bolts.");
            else if (_fireCasting)
                hints.Add("Stand in puddles during hellfire.");
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var puddles = GetPuddles(Module);
        var count = puddles.Count;
        // If puddles exist they should be drawn as dangerous during lightning
        // safe during fire.
        if (count != 0 && (_levinCasting || _fireCasting))
        {
            var puddlez = new ShapeDistance[count];
            for (var i = 0; i < count; ++i)
            {
                var p = puddles[i];

                if (_levinCasting)
                    //purposefully slightly larger avoidance zone
                    puddlez[i] = new SDCircle(p.Position, 5f);
                else if (_fireCasting)
                    // purposefully a little smaller than the puddle.
                    puddlez[i] = new SDInvertedCircle(p.Position, 4.5f);
            }
            if (_levinCasting)
                hints.AddForbiddenZone(new SDUnion(puddlez), _activation);
            if (_fireCasting)
                hints.AddForbiddenZone(new SDIntersection(puddlez), _activation);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var orbs = GetPuddles(Module);
        var count = orbs.Count;
        for (var i = 0; i < count; ++i)
        {
            if (_levinCasting)
                Arena.ZoneCircle(orbs[i].Position, 5f, Colors.Danger);
            else if (_fireCasting)
                Arena.ZoneCircle(orbs[i].Position, 5f, Colors.SafeFromAOE);
            else
                Arena.ZoneCircleOutline(orbs[i].Position, 5f, Colors.Object);
        }
    }
}

// Big bait away cones.
sealed class EarthBreathBait(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCone(60f, 30f.Degrees()), (uint)IconID.EarthBreathIcon, (uint)AID.EarthBreath);

sealed class EarthBreathAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.EarthBreath1, new AOEShapeCone(60f, 30f.Degrees()));

sealed class IcicleAdds(BossModule module) : Components.AddsMulti(module, [(uint)OID.Icicle]);

// Icicles land and do circle aoe
sealed class IcicleSummon(BossModule module) : Components.GenericAOEs(module)
{
    private readonly AOEShapeCircle circle = new(6f);
    private readonly List<AOEInstance> _aoes = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    //Icicles impact with an aoe circle of r6
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.IcicleImpact)
        {
            _aoes.Add(new(circle, caster.Position.Quantized(), default, WorldState.FutureTime(0d)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Spikesicle)
        {
            {
                // We clean up the aoe that matches the casting OID position.
                _aoes.RemoveAll(c => c.Origin == caster.Position.Quantized());
                ++NumCasts;
            }
        }
    }
}

/*
 * Icicles shoot across the arena.
 */
sealed class IcicleThrustAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Spikesicle, new AOEShapeRect(64.5f, 5f), maxCasts: 3);

sealed class IcicleThrustKnockback(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.Spikesicle, 10f, shape: new AOEShapeRect(64.5f, 5f), kind: Kind.DirForward, maxCasts: 3);

/*
 * Initial akh morn stack to share damage
 */
sealed class AkhMornStack(BossModule module) : Components.CastSharedTankbuster(module, (uint)AID.AkhMornVisual, 5f);

sealed class BurningChains(BossModule module) : Components.StretchTetherSingle(module, (uint)TetherID.BurningTether, 30f, null, (uint)AID.BurningChains);

// Phase 2 with cocoons and smaller adds.
// Purposefully make these smaller because they are proximity and will otherwise cover whole arena.
sealed class MeteorImpact(BossModule module) : Components.ProximityAOEs(module, (uint)AID.MeteorImpact, 14f);

sealed class Cocoons(BossModule module) : Components.AddsMulti(module, [(uint)OID.Cocoon1, (uint)OID.MassiveCocoon]);

/*
 * Summons all the little dragons
 */
sealed class DragonAdds(BossModule module) : Components.AddsMulti(module, [(uint)OID.Ginryu, (uint)OID.Hakkinryu], priority: 1);
sealed class Fireball(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Fireball, 4f);
sealed class BlazingTrail(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BlazingTrail, new AOEShapeRect(15f, 5.5f));

// Use fetters as the time to update the arena.
sealed class Fetters(BossModule module) : BossComponent(module)
{
    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.Fetters && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
        {
            Arena.Bounds = new ArenaBoundsSquare(30f);
        }
    }
}

/*
 * Fetters happens and then leads in to arena change.  Arena is larger and sections can be destroyed.
 * Initial charge has a knockback component that is probably what actually kills.
 */
// Phase 3
sealed class GyreCharge(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GyreChargeVisual, new AOEShapeRect(100f, 30f));

/*
 * Tail slap hits the square tiles. First hit cracks, and second hit breaks.
 * This class has the tracking for broken tiles and changes arena bounds as needed.
 * Also accounts for Earthen Fury tile breaks.
 */
sealed class TailSlap(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.TailSlap1, (uint)AID.TailSlap2], new AOEShapeRect(40f, 10f))
{
    private readonly List<Square> _active = []; // The main large square arena shape
    private readonly List<Square> _inactive = []; // As a tile breaks it is added to _inactive.

    /*
     * on EAnimState to change appearance as tailslap hits.
     */
    public override void OnActorEAnim(Actor actor, uint state)
    {
        switch (state)
        {
            // 0x00100020 - gets cracked. Will break on next hit.
            case 0x00040008u: // Initial state
                if (_active.Count < 1)
                    // Add the main outline. We do not need more than shape.
                    _active.Add(new Square(Arena.Center, 30f));
                break;
            case 0x00400080u: // Broken platform state.
                // never remove center square
                if (actor.OID == (uint)OID.Platform && !actor.Position.AlmostEqual(default, 1f))
                {
                    _inactive.Add(new Square(actor.Position.Rounded(), 10f));
                    // New arena bounds are the main arena outline minus (difference) the broken tiles.
                    ArenaBoundsCustom tailSlapArena = new([.. _active], [.. _inactive]);
                    Arena.Bounds = tailSlapArena;
                    Arena.Center = tailSlapArena.Center;
                }
                break;
        }
    }
}

sealed class Raidwides(BossModule module) : Components.RaidwideCasts(module, [(uint)AID.IceStormRaidwide, (uint)AID.Protostar, (uint)AID.DarkMatter, (uint)AID.JudgmentBolt, (uint)AID.EarthenFury]);

// Shinryu slaps fist down in center tile of arena.
sealed class Dragonfist(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DragonfistVisual, 16f);

/*
 * Diamond dust gives players thin ice for some seconds.  They slide around while under status.
 */
sealed class ThinIce(BossModule module) : Components.ThinIce(module, 30f, stopAtWall: false);

sealed class EarthenFurySmash(BossModule module) : Components.SimpleAOEs(module, (uint)AID.EarthenFurySmash, new AOEShapeRect(20f, 10f));

sealed class SuperCyclone(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.SuperCycloneKB, 5f)
{
    // Add a safe zone at knockback origin so ai knows where to run.
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count != 0)
        {
            ref readonly var kb = ref Casters.Ref(0);
            var act = kb.Activation;
            if (!IsImmune(slot, act))
            {
                hints.AddForbiddenZone(new SDInvertedCircle(kb.Origin, 7f), act);
            }
        }
    }
}

sealed class AerialBlast(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.AerialBlastKB, 15f)
{
    // Add a safe zone at knockback origin so ai knows where to run.
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count != 0)
        {
            ref readonly var kb = ref Casters.Ref(0);
            var act = kb.Activation;
            if (!IsImmune(slot, act))
            {
                hints.AddForbiddenZone(new SDInvertedCircle(kb.Origin, 7f), act);
            }
        }
    }
}

sealed class Hypernova(BossModule module) : Components.StackWithIcon(module, (uint)IconID.HyperNovaStackIcon, (uint)AID.Hypernova, 7f, 6d);

[SkipLocalsInit]
sealed class ShinryuStates : StateMachineBuilder
{
    public ShinryuStates(BossModule module) : base(module)
    {
        DeathPhase(default, SinglePhase);
    }

    private void SinglePhase(uint id)
    {
        SimpleState(id + 0xFF0000u, 10000f, "???")
            .ActivateOnEnter<TidalWave>()
            .ActivateOnEnter<WaterPuddles>()
            .ActivateOnEnter<Levinbolt>()
            .ActivateOnEnter<EarthBreathBait>()
            .ActivateOnEnter<EarthBreathAOE>()
            .ActivateOnEnter<AkhMornStack>()
            .ActivateOnEnter<IcicleAdds>()
            .ActivateOnEnter<IcicleSummon>()
            .ActivateOnEnter<IcicleThrustAOE>()
            .ActivateOnEnter<IcicleThrustKnockback>()
            .ActivateOnEnter<BurningChains>()

            //Phase 2
            .ActivateOnEnter<Cocoons>()
            .ActivateOnEnter<DragonAdds>()
            .ActivateOnEnter<MeteorImpact>()
            .ActivateOnEnter<Fireball>()
            .ActivateOnEnter<BlazingTrail>()
            .ActivateOnEnter<Fetters>()
            .ActivateOnEnter<Raidwides>()

            //Phase 3 : new arena with sections that can be destroyed.
            .ActivateOnEnter<GyreCharge>()
            .ActivateOnEnter<TailSlap>()
            .ActivateOnEnter<Dragonfist>()
            .ActivateOnEnter<ThinIce>()
            .ActivateOnEnter<EarthenFurySmash>()
            .ActivateOnEnter<SuperCyclone>()
            .ActivateOnEnter<AerialBlast>()
            .ActivateOnEnter<Hypernova>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed,
    StatesType = typeof(ShinryuStates),
    ConfigType = null, // replace null with typeof(ShinryuConfig) if applicable
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID),
    StatusIDType = typeof(SID),
    TetherIDType = typeof(TetherID),
    IconIDType = typeof(IconID),
    PrimaryActorOID = (uint)OID.Shinryu,
    Contributors = "wen",
    Expansion = BossModuleInfo.Expansion.Stormblood,
    Category = BossModuleInfo.Category.Trial,
    GroupType = BossModuleInfo.GroupType.CFC,
    GroupID = 239u,
    NameID = 5640u,
    SortOrder = 1,
    PlanLevel = 0)]

[SkipLocalsInit]
public sealed class T03Shinryu(WorldState ws, Actor primary) : BossModule(ws, primary, default, new ArenaBoundsSquare(20f, mapResolution: 0.3f));
