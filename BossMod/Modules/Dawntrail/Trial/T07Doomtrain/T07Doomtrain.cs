namespace BossMod.Dawntrail.Trial.T07Doomtrain;

public enum OID : uint
{
    _Gen_Aether = 0x233C, // R0.500, x?, Helper type
    _Gen_Doomtrain = 0x4A33, // R1.000, x?, Doomtrain Helper
    _Gen_Actor1ea1a1 = 0x1EA1A1, // R0.500, x?, EventObj type
    Doomtrain = 0x4A30, // R19.040, x? - First train visual probably
    LevinSignal = 0x4A31, // R1.000, x? - The portals from north that shoot rays from north to south on upper and lower levels
    KinematicTurret = 0x4A32, // R1.200, turrets that shoot from sides
    _Gen_Exit = 0x1E850B, // R0.500, x?, EventObj type
    AetherIntermission = 0x4A34, // R1.500, x The aether enemy during intermission
    DoomtrainIntermission = 0x4B7E, // R19.040, x? - Doomtrain during the intermission
    GhostTrain = 0x4B80, // R2.720, x?
    ArcaneRevelation = 0x4A36, // R1.000, x?  - Large red outlined circle
}

public enum AID : uint
{
    _Ability_ = 45662, // 233C->player, no cast, single-target
    // tank buster spreads with red icon
    LightningBurstVisual = 45660, // 4A30->self, 5.0s cast, single-target
    LightningBurst = 45661, // 233C->player, 0.5s cast, range 5 circle
    // Lightning Express is ramming the car and knocking everybody back
    LightningExpress = 45618, // 4A30->self, 6.0s cast, range 70 width 70 rect
    // Beams that shoot out of portal visuals.
    // 'Levin signal readies plasma beam.'/ 'levin signal uses plasma beam'
    PlasmaBeamUpper = 45620, // 4A31->self, 1.0s cast, range 30 width 5 rect :
    PlasmaBeamLower = 45619, // 4A31->self, 1.0s cast, range 30 width 5 rect :
    PlasmaBeamMedium = 45621, // 4A31->self, 1.0s cast, range 20 width 5 rect
    PlasmaBeamShort = 45622, // 4A31->self, 1.0s cast, range 10 width 5 rect
    // Windpipe knockback.  Draw in and rectangle AOE (Blastpipe) at front of car.
    WindpipeVisual = 45625, // 4A30->self, 6.0+1.0s cast, single-target
    WindpipeDrawIn = 45667, // 233C->self, 7.0s cast, range 30 width 20 rect
    BlastpipeVisual = 45626, // 4A30->self, no cast, single-target
    Blastpipe = 45627, // 233C->self, 3.0s cast, range 10 width 20 rect
    UnlimitedExpressVisual = 45623, // 4A30->self, 5.0s cast, single-target : Switch to car 2 on first cast/ switch to car 3 on second cast / switch to car 4 on third cast
    UnlimitedExpress = 45624, // 233C->self, 5.9s cast, range 70 width 70 rect
    _Ability_1 = 45641, // 4A30->location, no cast, single-target
    _Ability_TurretCrossing = 45628, // 4A30->self, 3.0s cast, single-target
    ElectrayLong = 45629, // 4A32->self, 5.0s cast, range 25 width 5 rect : lower deck
    ElectrayShort = 45633, // 4A32->self, 5.0s cast, range 10 width 5 rect : lower deck
    ElectrayMedium = 45632, // 4A32->self, 5.0s cast, range 15 width 5 rect : lower deck
    Electray3 = 45631, // 4A32->self, 5.0s cast, range 20 width 5 rect
    ElectrayUpper = 45630, // 4A32->self, 5.0s cast, range 25 width 5 rect : Upper Deck
    HeadOnEmissionVisual = 45634, // 4A30->self, 6.0+1.0s cast, single-target : Ground floor?
    ThunderousBreathLowerDeck = 45635, // 233C->self, 7.0s cast, range 70 width 70 rect
    HeadOnEmissionVisual1 = 45636, // 4A30->self, 6.0+1.0s cast, single-target : Uppder deck
    HeadlightUpperDeck = 45637, // 4A33->self, 7.0s cast, range 30 width 20 rect
    RunawayTrain = 45638, // 4A30->self, 5.0s cast, single-target : sends to circle arena
    _Ability_Overdraught = 45639, // 4A34->self, no cast, single-target
    AetherSurgeVisual = 45642, // 4A34->self, 6.0s cast, single-target
    AetherSurge = 45643, // 233C->self, 6.0s cast, range 30 45.000-degree cone
    AetherialRay = 45640, // 233C->self, no cast, range 50 ?-degree cone
    RunawayTrainVisual = 45644, // 4B7E->self, no cast, single-target
    RunawayTrainRaidwide = 45645, // 233C->self, no cast, range 20 circle
    ShockwaveVisual = 45646, // 4A30->self, no cast, single-target
    Shockwave = 45647, // 233C->self, no cast, range 50 circle
    ArcaneRevelationVisual = 47527, // 4A30->self, 2.0+1.0s cast, single-target
    _Ability_HailOfThunder = 45656, // 4A30->self, no cast, single-target - indicator moves 2
    _Ability_HailOfThunder2 = 45657, // 4A30->self, no cast, single-target - indicator moves 3
    HailOfThunder = 45659, // 233C->location, 3.2s cast, range 16 circle
    DerailmentSiege = 45648, // 4A30->self, 6.0+1.0s cast, single-target
    DerailmentSiegeStack = 45650, // 233C->self, no cast, range 5 circle
    _Ability_DerailmentSiege2 = 45651, // 233C->self, 0.5s cast, range 5 circle
    DerailmentSiegeCircle = 45649, // 233C->self, 10.0s cast, range 5 circle
    Derail = 45654, // 233C->self, 10.0s cast, range 30 width 20 rect
    DerailVisual = 45653, // 4A30->self, 10.1s cast, single-target
    _Ability_2 = 45655, // 4A30->location, no cast, single-target
    _Ability_BatteringArms = 47529, // 4A30->self, 6.0+1.0s cast, single-target
    _Ability_BatteringArms1 = 47236, // 233C->self, no cast, range 5 circle
    _Ability_BatteringArms2 = 47237, // 233C->self, 0.5s cast, range 5 circle
}

public enum SID : uint
{
    DeadMansOverdraught = 4720, // none->Boss, extra=0x0
    MagicVulnerabilityUp = 2941, // Helper->player, extra=0x0
    Distance = 4541, // none->GhostTrain, extra=0x578 (170 degree rotation)/0x960 (106 degree rotation) Aetherial Ray Ghost train?
    Unk1 = 2056, // none->IntermissionTrain, extra=0x400
    Unk2 = 2552, // none->GhostTrain, extra=0x42B
    Stop = 4176, // none->GhostTrain, extra=0x0
    Invincibility = 1570, // none->player, extra=0x0
    SystemLock = 2578, // none->player, extra=0x0
    UpHigh = 4721, // none->player, extra=0x0
    HeadlightThunderBreath = 3913, // none->Boss, extra=0x3D8 (ground first)/0x3D7 (air first)
    DesignatedConductor = 4719, // none->player, extra=0x0
    PhysicalVulnerabilityUp = 2940, // Helper->player, extra=0x0
}

public enum IconID : uint
{
    LightningBurstIcon = 343, // player->self
    Horn = 642, // IntermissionTrain->self
    AetherialRayIcon = 412, // player->self
    DoubleToot = 637, // IntermissionTrain->self
    TripleToot = 638, // IntermissionTrain->self
    Plummet = 499, // player->self
}

sealed class ElectrayShort(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ElectrayShort, new AOEShapeRect(10f, 2.5f));
sealed class ElectrayMedium(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ElectrayMedium, new AOEShapeRect(15f, 2.5f));
sealed class ElectrayLong(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ElectrayLong, new AOEShapeRect(25f, 2.5f));
sealed class LightningBurstTankBuster(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCircle(5f), (uint)IconID.LightningBurstIcon, (uint)AID.LightningBurst, 5f, centerAtTarget: true, tankbuster: true, damageType: AIHints.PredictedDamageType.Tankbuster);

// For first pass the knockback distance is estimated.
sealed class LightningExpress(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.LightningExpress, 16f, kind: Kind.DirForward);

/**
 * Plasma beams have a cast of 0.7 seconds. The Levin signal actors spawn about 7 seconds earlier.
 * We can extrapolate the plasma beam casts based off levin signal positions. Then we create the aoe
 * instances ahead of time so we have time to move out of the way.
 */
class LevinSignal(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<(Actor Caster, bool Ground, DateTime Activation, float MaxLen, float Offset)> _casters = [];

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.LevinSignal)
        {
            // if actor.PosRot.Y then they are at ground level
            var ground = actor.PosRot.Y < 2;

            var car = Module.FindComponent<CarGeometry>();
            // set default value for ground to 30 and upperdeck to 0 cull levin signal that are elevated and not over platforms.
            var maxLen = ground ? 30 : 0;
            var offset = 0;
            var posX = actor.PosRot.X;
            if (car?.Car == 2)
            {
                if (posX is > 95f and < 100)
                {
                    maxLen = 20;
                }
                else if (posX is > 100f and < 105)
                {
                    maxLen = 10;
                }
            }
            // car 3 has two platforms of length 20
            if (car?.Car == 3)
            {
                // left side platforms
                if (posX < 95f)
                {
                    if (ground)
                    {
                        maxLen = 10;
                    }
                    else
                    {
                        maxLen = 20;
                        offset = 10;
                    }
                }
                // right side platforms
                else if (posX > 105f)
                {
                    if (ground)
                    {
                        maxLen = 10;
                    }
                    else
                    {
                        maxLen = 20;
                        offset = 10;
                    }
                }
            }
            if (car?.Car == 5)
            {
                // left side platform, length 10
                if (posX < 95f)
                {
                    if (ground)
                    {
                        maxLen = 20;
                    }
                    else
                    {
                        maxLen = 10;
                        offset = 20;
                    }
                }
                // right side platform
                else if (posX > 105f)
                {
                    if (ground)
                    {
                        maxLen = 10;
                    }
                    else
                    {
                        maxLen = 10;
                        offset = 10;
                    }
                }
            }
            _casters.Add((actor, ground, WorldState.FutureTime(7), maxLen, offset));
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _casters.Count;
        if (count == 0)
            return [];
        var aoes = new AOEInstance[count];
        for (var i = 0; i < count; i++)
        {
            var c = _casters[i];

            aoes[i] = new(new AOEShapeRect(c.MaxLen, 2.5f), c.Caster.Position + new WDir(0, c.Offset), default, c.Activation);
        }
        return aoes;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (((AID)spell.Action.ID is AID.PlasmaBeamLower or AID.PlasmaBeamMedium or AID.PlasmaBeamShort or AID.PlasmaBeamUpper))
        {
            _casters.RemoveAll(c => c.Caster == caster);
            ++NumCasts;
        }
    }
}

// TODO: Show the indicator arrow.
sealed class WindpipeDrawIn(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.WindpipeDrawIn, 20f)
{
    private readonly CarGeometry _car = module.FindComponent<CarGeometry>()!;
    private List<Knockback> _kb = [];

    // need safe walls for car 2 and car 5
    private readonly List<SafeWall> safeWalls2 = [new(new(100.1f, 150.1f), new(104.9f, 150.1f)), new(new(95.1f, 160.1f), new(99.9f, 160.1f))];
    private readonly List<SafeWall> safeWalls5 = [new(new(105.1f, 305.1f), new(109.9f, 305.1f))];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.WindpipeDrawIn)
        {
            switch (_car.Car)
            {
                case 2:
                    _kb = [new(spell.LocXZ, 20f, Module.CastFinishAt(spell), safeWalls: safeWalls2, kind: Kind.DirBackward, ignoreImmunes: true)];
                    break;
                case 5:
                    _kb = [new(spell.LocXZ, 20f, Module.CastFinishAt(spell), safeWalls: safeWalls5, kind: Kind.DirBackward, ignoreImmunes: true)];
                    break;
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.WindpipeDrawIn)
        {
            _kb.Clear();
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_kb.Count != 0)
        {
            var kb = _kb[slot];
            if (!IsImmune(slot, WorldState.CurrentTime))
            {
                // It is a draw in, we use mirrorZ to make safe zones to keep us from moving towards boss.
                var dir = kb.Direction.ToDirection().MirrorZ();
                var _walls = _car.Car == 2 ? safeWalls2 : safeWalls5;
                var len = _walls.Count;
                if (len > 0)
                {
                    var swalls = new SafeWall[len];
                    for (var i = 0; i < _walls.Count; i++)
                    {
                        swalls[i] = _walls[i];
                    }
                    hints.AddForbiddenZone(new SDKnockbackFixedDirectionAgainstSafewalls(dir, swalls, 20f, len), WorldState.CurrentTime);
                }
            }
        }
    }
}

sealed class Blastpipe(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Blastpipe, new AOEShapeRect(10f, 10f), riskyWithSecondsLeft: 3);

sealed class UnlimitedExpress(BossModule module) : Components.RaidwideCast(module, (uint)AID.UnlimitedExpress);

/**
 * Show the respective aoe shapes for Thunderous breath and Headlight.
 */
sealed class HeadOnEmission(BossModule module) : Components.GenericAOEs(module)
{
    private readonly CarGeometry _car = module.FindComponent<CarGeometry>()!;
    private readonly List<AOEInstance> _aoes = [];
    //private DateTime _activation;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];

        var aoes = CollectionsMarshal.AsSpan(_aoes);
        var color = Colors.AOE;

        for (var i = 0; i < count; ++i)
        {
            ref var aoe = ref aoes[i];
            if (count > 0)
            {
                aoe.Color = color;
                aoe.Risky = true;
            }
        }
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.ThunderousBreathLowerDeck)
        {
            _aoes.Add(new(_car.GroundShape, Module.Arena.Center));
        }
        else if ((AID)spell.Action.ID is AID.HeadlightUpperDeck)
        {
            _aoes.Add(new(_car.AirShape, Module.Arena.Center));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is (AID.ThunderousBreathLowerDeck or AID.HeadlightUpperDeck))
        {
            _aoes.Clear();
        }
    }
}

// Aether surge : multiple cone aoe from center outwards. 6.0s cast, range 30 45.000-degree cone
sealed class AetherSurge(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AetherSurge, new AOEShapeCone(15f, 22.5f.Degrees()), riskyWithSecondsLeft: 6d);

/**
 * Ghost train targets a party member inside the arena with Aetherial Ray:
 * a range 50 cone from Ghost train to target party member.
 * Ghost train gets an additional status 'Distance' to determine how much they will rotate around the arena between
 * cast start and end.
 * SID.Distance = 4541
 * SID.Distance.Extra 0x578 : 170 degree,
 * SID.Distance.Extra 0x960 (106 degree rotation)
 */
class AetherialRay(BossModule module) : Components.GenericBaitAway(module, (uint)AID.AetherialRay, damageType: AIHints.PredictedDamageType.Tankbuster)
{
    private Angle _nextRotation;
    private Actor? _nextTarget;
    private DateTime _nextActivation;

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if ((SID)status.ID == SID.Distance)
        {
            _nextRotation = status.Extra switch
            {
                0x578 => 170.Degrees(),
                0x960 => 106.Degrees(),
                _ => default
            };
            if (_nextRotation == default)
                ReportError($"Unrecognized status {status.Extra:X} on Ghost Train, don't know where to predict");
        }

        if ((SID)status.ID == SID.Stop)
        {
            foreach (ref var bait in CollectionsMarshal.AsSpan(CurrentBaits))
                bait.Source = actor;
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if ((IconID)iconID == IconID.Horn)
        {
            _nextTarget = WorldState.Actors.Find(targetID);
            _nextActivation = WorldState.FutureTime(9.7f);
        }
        // source is GhostTrain, _nextTarget is the party member that is dragging the cone marker
        if ((IconID)iconID == IconID.Horn && _nextTarget != null)
        {
            CurrentBaits.Add(new(actor, _nextTarget, new AOEShapeCone(50, 17.5f.Degrees()), _nextActivation));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            NumCasts++;
            CurrentBaits.Clear();
        }
    }
}

//RunawayTrainRaidwide = 45645, // 233C->self, no cast, range 20 circle
sealed class RunawayTrainRaidwide(BossModule module) : Components.RaidwideCast(module, (uint)AID.RunawayTrainRaidwide);

// arcane revelation. This is the spell with the diamond pattern on the floor and the lightning
// traverses n points to cast Hail of thunder.  Use the actor OID.ArcaneRevelation to mark the indicator for hail of thunder.
// The circle is so big that pc gets trapped in a corner pocket on the wrong end
// Would be good to have some logic that can avoid the center radius of aoe and be +/- range of
// middle of car to more easily avoid pathing into crush zone.
// like if bait.center.z is > arena.center.z: path to safezones north, else path safezone south
// if bait.center.x > arena.center.x: path to safezones.west, else path safezone east
sealed class ArcaneRevelation(BossModule module) : Components.GenericBaitAway(module, centerAtTarget: true)
{
    //when arcane revelation mob created, add bait.
    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.ArcaneRevelation)
            CurrentBaits.Add(new(Module.PrimaryActor, actor, new AOEShapeCircle(16f)));
    }
    //when arcane revelation mob is gone, remove it.
    public override void Update()
    {
        if (CurrentBaits.Count > 0 && Module.Enemies((uint)OID.ArcaneRevelation).All(x => x.IsDead)) // if adds die baits get cancelled
            CurrentBaits.Clear();
    }
}

// HailOfThunder, kept this separate because it highlights the difference between the arcane revelation
// bait indicator and the HailOfThunder cast.
sealed class HailOfThunder(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HailOfThunder, new AOEShapeCircle(16f), riskyWithSecondsLeft: 3.2d);

sealed class Electray3(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Electray3, new AOEShapeRect(20f, 2.5f), riskyWithSecondsLeft: 5d);

sealed class ElectrayUpper(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ElectrayUpper)
        {
            // make the rectangle lengthFront shorter because it only needs cover the platform from edge of arena.
            _aoes.Add(new(new AOEShapeRect(10f, 2.5f), caster.CastInfo!.LocXZ, caster.CastInfo!.Rotation, Module.CastFinishAt(caster.CastInfo)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ElectrayUpper)
        {
            _aoes.Clear();
        }
    }
}

sealed class RunawayTrain(BossModule module) : Components.RaidwideCast(module, (uint)AID.RunawayTrain);

sealed class Shockwave(BossModule module) : Components.RaidwideCast(module, (uint)AID.Shockwave);

// Rectangle that covers all of car 4, portal opens in the back to escape to car 5
sealed class Derail(BossModule module) : Components.GenericAOEs(module)
{
    // Cheat by making portal visible as an inverted danger zone at WPos
    private readonly WPos escapePortal = new(100f, 262.5f);
    private readonly List<AOEInstance> _aoes = [];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Derail)
        {
            _aoes.Add(new(new AOEShapeDonut(2, 32), escapePortal, default, Module.CastFinishAt(spell, 0.6d)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Derail)
        {
            _aoes.Clear();
        }
    }
}

// Derailment Siege stack
sealed class DerailmentSiegeCircle(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.DerailmentSiegeCircle, 5, 8);

// Would be cleaner probably to have all arena changes tied to a ENVC state switch.
// TODO should probably move this over to geometry with the rest of the arena changes.
sealed class ArenaChangeCar4(BossModule module) : BossComponent(module)
{
    private readonly CarGeometry _car = module.FindComponent<CarGeometry>()!;

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Derail)
        {
            _car.Car = 5;
        }
    }
}

[SkipLocalsInit]
sealed class DoomtrainStates : StateMachineBuilder
{
    public DoomtrainStates(BossModule module) : base(module)
    {
        SimplePhase(0, P1, "P1")
            .ActivateOnEnter<CarGeometry>()
            .Raw.Update = () => Module.Enemies((uint)OID.AetherIntermission).FirstOrDefault()?.IsDead == true;
        DeathPhase(1, P2);
    }

    void P1(uint id)
    {
        Car1(id, 8.2f);
        Car2(id + 0x10000, 40000);
        Car3(id + 0x20000, 50000f);
        Car4_1(id + 0x30000, 60000f);
        Intermission(id + 0x40000, 70000);
    }

    void P2(uint id)
    {
        ComponentCondition<RunawayTrain>(id, 4.3f, r => r.WatchedAction != default)
            .ActivateOnEnter<RunawayTrain>()
            .SetHint(StateMachine.StateHint.DowntimeStart);

        ComponentCondition<RunawayTrain>(id + 1, 15.2f, r => r.NumCasts > 0, "Raidwide")
            .DeactivateOnExit<RunawayTrain>()
            .SetHint(StateMachine.StateHint.Raidwide);

        Car4_2(id + 0x50000, 80000f);
        Car5(id + 0x60000, 90000f);
    }

    void Car1(uint id, float delay)
    {
        CastStart(id + 0x100, (uint)AID.UnlimitedExpressVisual, 1.9f)
            .ActivateOnEnter<LightningBurstTankBuster>()
            .ActivateOnEnter<LevinSignal>()
            .ActivateOnEnter<LightningExpress>()
            .ActivateOnEnter<WindpipeDrawIn>()
            .ActivateOnEnter<Blastpipe>()
            .ActivateOnEnter<UnlimitedExpress>();
        ComponentCondition<UnlimitedExpress>(id + 0x101, 5.9f, x => x.NumCasts > 0)
            .DeactivateOnExit<UnlimitedExpress>();
        Targetable(id + 0x110, false, 0.2f, "Car 2 start");

    }

    void Car2(uint id, float delay)
    {
        Timeout(id, 0).ExecOnEnter<CarGeometry>(c => c.Car++)
            .ActivateOnEnter<ElectrayShort>()
            .ActivateOnEnter<ElectrayMedium>()
            .ActivateOnEnter<ElectrayLong>()
            .ActivateOnEnter<LevinSignal>()
            .ActivateOnEnter<WindpipeDrawIn>();
        CastStart(id + 0x100, (uint)AID.UnlimitedExpressVisual, 1.9f)
            .ActivateOnEnter<UnlimitedExpress>();
        ComponentCondition<UnlimitedExpress>(id + 0x101, 5.9f, x => x.NumCasts > 0)
            .DeactivateOnExit<UnlimitedExpress>();
        Targetable(id + 0x110, false, 0.2f, "Car 3 start");
    }

    void Car3(uint id, float delay)
    {
        Timeout(id, 0).ExecOnEnter<CarGeometry>(c => c.Car++)
            .ActivateOnEnter<ElectrayShort>()
            .ActivateOnEnter<ElectrayMedium>()
            .ActivateOnEnter<ElectrayLong>()
            .ActivateOnEnter<LevinSignal>()
            .ActivateOnEnter<HeadOnEmission>();

        CastStart(id + 0x100, (uint)AID.UnlimitedExpressVisual, 1.9f)
            .ActivateOnEnter<UnlimitedExpress>();
        ComponentCondition<UnlimitedExpress>(id + 0x101, 5.9f, x => x.NumCasts > 0)
            .DeactivateOnExit<UnlimitedExpress>();
        Targetable(id + 0x110, false, 0.2f, "Car 3 start");
    }

    void Car4_1(uint id, float delay)
    {
        Timeout(id, 0).ExecOnEnter<CarGeometry>(c => c.Car++)
            .ActivateOnEnter<HeadOnEmission>();
        CastStart(id + 0x100, (uint)AID.RunawayTrain, 1.9f)
            .ActivateOnEnter<RunawayTrain>();
        ComponentCondition<RunawayTrain>(id + 0x101, 5.9f, x => x.NumCasts > 0)
            .DeactivateOnExit<RunawayTrain>();
        Targetable(id + 0x110, false, 0.2f, "Intermission start");
    }

    // move to circle arena with ghost train
    void Intermission(uint id, float delay)
    {
        Timeout(id, delay, "Boss reappears")
            .SetHint(StateMachine.StateHint.DowntimeEnd)
            .OnEnter(() =>
            {
                // Added the outer donut for railroad tracks and to improve how
                // aetherial ray cone aoe presents on the radar.
                Module.Arena.Center = new(-400, -400);
                Shape[] intermissionCircles = [new Circle(Module.Arena.Center, 14.5f), new Donut(Module.Arena.Center, 22.4f, 27.5f)];
                ArenaBoundsCustom intermissionArena = new(intermissionCircles, AdjustForHitboxInwards: true);
                Module.Arena.Bounds = intermissionArena;
            })
            .ActivateOnEnter<AetherSurge>()
            .ActivateOnEnter<AetherialRay>()
            .ActivateOnEnter<Shockwave>()
            .ActivateOnEnter<RunawayTrainRaidwide>()
            .ActivateOnEnter<RunawayTrain>();

        CastStart(id + 0x100, (uint)AID.RunawayTrain, 1.9f)
            .ActivateOnEnter<RunawayTrain>();
        ComponentCondition<RunawayTrain>(id + 0x101, 5.9f, x => x.NumCasts > 0)
            .DeactivateOnExit<RunawayTrain>();
        Targetable(id + 0x110, false, 0.2f, "Return to car 4");
    }

    // Return to car 4 after intermission with ghost train
    void Car4_2(uint id, float delay)
    {
        //Reset the arena back to car 4
        Timeout(id, 0).ExecOnEnter<CarGeometry>(c => c.Car = 4)
            .ActivateOnEnter<ArenaChangeCar4>()
            .ActivateOnEnter<HailOfThunder>()
            .ActivateOnEnter<ArcaneRevelation>()
            .ActivateOnEnter<DerailmentSiegeCircle>()
            .ActivateOnEnter<Derail>();

        Targetable(id + 0x260, false, 0.2f, "Moves to car 5");
    }

    void Car5(uint id, float delay)
    {
        Targetable(id + 0x110, true, 0.2f, "On car 5")
            .ActivateOnEnter<HeadOnEmission>()
            .ActivateOnEnter<ElectrayLong>()
            .ActivateOnEnter<ElectrayMedium>()
            .ActivateOnEnter<ElectrayShort>()
            .ActivateOnEnter<ElectrayUpper>()
            .ActivateOnEnter<LevinSignal>()
            .ActivateOnEnter<LightningExpress>()
            .ActivateOnEnter<Electray3>()
            .ActivateOnEnter<WindpipeDrawIn>()
            .ActivateOnEnter<Blastpipe>()
            .ActivateOnEnter<LightningBurstTankBuster>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed,
    StatesType = typeof(DoomtrainStates),
    ConfigType = null, // replace null with typeof(DoomtrainConfig) if applicable
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID),
    StatusIDType = typeof(SID),
    TetherIDType = null, // replace null with typeof(TetherID) if applicable
    IconIDType = typeof(IconID),
    PrimaryActorOID = (uint)OID.Doomtrain,
    Contributors = "wen, Xan",
    Expansion = BossModuleInfo.Expansion.Dawntrail,
    Category = BossModuleInfo.Category.Trial,
    GroupType = BossModuleInfo.GroupType.CFC,
    GroupID = 1076u,
    NameID = 14284u,
    SortOrder = 1,
    PlanLevel = 0)]
public class Doomtrain(WorldState ws, Actor primary)
    : BossModule(ws, primary, new(100, 100), new ArenaBoundsRect(10f, 15f))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        if (PrimaryActor.IsTargetable)
        {
            var height = Arena.Bounds.Radius;
            if (PrimaryActor.Position.InRect(Arena.Center, default(Angle), height + 12, height + 12, 10))
                Arena.ActorInsideBounds(Arena.Center - new WDir(0, height), PrimaryActor.Rotation, Colors.Enemy);
            else
                Arena.ActorOutsideBounds(Arena.Center - new WDir(0, height), PrimaryActor.Rotation, Colors.Enemy);
        }

        Arena.Actors(Enemies((uint)OID.AetherIntermission), Colors.Enemy);
        Arena.Actors(Enemies((uint)OID.GhostTrain), Colors.Object, true);
        // use this to see where the arcane revelation aoe center is floating.
        Arena.Actors(Enemies((uint)OID.ArcaneRevelation), Colors.Object, true);
    }
}
