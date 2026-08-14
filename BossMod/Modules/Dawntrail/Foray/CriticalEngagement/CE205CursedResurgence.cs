namespace BossMod.Dawntrail.Foray.CriticalEngagement.CE205CursedResurgence;

public enum OID : uint
{
    ClaretDragonHelper = 0x233C, // R0.500, x19, Helper type
    AetherialWardDirectionEObj = 0x1EC094, // R0.500, x4, EventObj type, 0x00040020 after ward dead
    Actor1ea1a1 = 0x1EA1A1, // R2.000, x2, EventObj type
    Actor1ec096 = 0x1EC096, // R0.500, x1, EventObj type
    Actor1ec093 = 0x1EC093, // R0.500, x1, EventObj type, 0x00040008 after ward dead
    ClaretDragon = 0x4C46, // R5.000, x1
    ClaretDragon2 = 0x4D25, // R1.000, x1
    Necrohaze = 0x4C47, // R1.500, x0 (spawn during fight)
    AetherialWardEObj = 0x1EC095, // R0.500, x0 (spawn during fight), EventObj type, spawns just before AetherialWard, 0x00010002 .1s before AetherialWard creation, destroyed 0.4s after ward dies
    AetherialWard = 0x4C48, // R7.000, x0 (spawn during fight)
}

public enum AID : uint
{
    UnknownAbility = 48279, // ClaretDragon2->self, no cast, ???
    AutoAttack = 48259, // ClaretDragon->player, no cast, single-target
    HowlingDarkness = 48277, // ClaretDragon->self, 5.0s cast, single-target
    HowlingDarkness1 = 48278, // ClaretDragonHelper->self, no cast, ???
    SnakingNecrobreath = 48260, // ClaretDragon->self, 6.0s cast, range 60 270.000-degree cone
    GraveMoldCast = 48261, // ClaretDragon->self, 5.0s cast, single-target
    GraveMold = 48262, // ClaretDragonHelper->self, 6.0s cast, range 8 circle
    Necrohaze1 = 48263, // Necrohaze->self, no cast, range 5 circle
    Soar = 50488, // ClaretDragon->self, 4.0s cast, single-target
    UnknownAbility2 = 48302, // ClaretDragon->self, no cast, single-target
    CauterizeCast = 48264, // ClaretDragon->self, 6.0s cast, single-target
    Cauterize = 48265, // ClaretDragonHelper->self, 7.0s cast, range 40 width 10 rect
    Catching = 48267, // Necrohaze->self, no cast, range 30 width 10 rect
    UnknownWeaponskill = 48266, // ClaretDragon->self, no cast, single-target
    AetherialWard = 48271, // ClaretDragon->self, 4.0+0.5s cast, single-target
    Necrohaze2 = 50484, // ClaretDragonHelper->self, 4.0s cast, range 5 circle, center puddle while casting AetherialWard
    UnknownAbility3 = 48275, // ClaretDragon->self, no cast, single-target
    Necrohaze3 = 48269, // ClaretDragonHelper->self, no cast, range 5 circle, center puddle during AetherialWard, control visibility with AetherialWard
    Necrohaze4 = 48268, // ClaretDragonHelper->location, no cast, range 5 circle, moving puddles during AetherialWard, casts Necrohaze1 before movement starts
    UnknownAbility4 = 48276, // ClaretDragon->self, no cast, single-target
    BreathInThrees = 48270, // ClaretDragon->self, 5.0s cast, range 60 120.000-degree cone
    BreathInThrees1 = 48248, // ClaretDragon->self, 2.5s cast, range 60 120.000-degree cone
}

public enum SID : uint
{
    GradualZombification = 5059, // Necrohaze/ClaretDragonHelper->player, extra=0x1
    ZombieProof = 5138, // Necrohaze/ClaretDragonHelper->player, extra=0x0
    VulnerabilityUp = 2347, // Necrohaze/ClaretDragonHelper/ClaretDragon->player, extra=0x1/0x2/0x3
    Zombification = 2305, // Necrohaze/ClaretDragonHelper->player, extra=0x0
    UnknownStatus = 2056, // ClaretDragon->ClaretDragon, extra=0x164
    Heavy = 1796, // none->Necrohaze, extra=0x32
    DirectionalInvincibility = 1125, // none->AetherialWard, extra=0x0
}

sealed class HowlingDarkness(BossModule module) : Components.RaidwideCast(module, (uint)AID.HowlingDarkness);
sealed class SnakingNecrobreath(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SnakingNecrobreath, new AOEShapeCone(60f, 135f.Degrees()));
sealed class GraveMold(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GraveMold, 8f);
sealed class Cauterize(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Cauterize, new AOEShapeRect(40f, 5f));
sealed class Catching(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        return CollectionsMarshal.AsSpan(_aoes);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Cauterize)
        {
            var actors = WorldState.Actors.ToArray();
            var count = actors.Length;

            for (var i = 0; i < count; i++)
            {
                ref var actor = ref actors[i];
                if (actor.OID == (uint)OID.Necrohaze)
                {
                    // does boss only cast from W/E, or cast it also happen N/S?
                    var offset = (actor.Position - spell.LocXZ).Rotate(spell.Rotation.ToDirection().MirrorX());
                    if (Intersect.CircleAARectEdge(offset, 5f, 5f, 40f))
                    {
                        _aoes.Add(new(new AOEShapeRect(30f, 5f), actor.Position, actor.Rotation, Module.CastFinishAt(spell, 0.6d)));
                    }
                }
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID == (uint)AID.Catching)
        {
            _aoes.RemoveAt(0);
        }
    }
}
// safe to make AI move towards boss?
sealed class BreathInThrees(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.BreathInThrees, (uint)AID.BreathInThrees1], new AOEShapeCone(60f, 60f.Degrees()))
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        var aoes = ActiveAOEs(slot, actor);
        var len = aoes.Length;
        if (len != 0)
        {
            hints.GoalZones.Add(AIHints.GoalDonut(Module.PrimaryActor.Position, 5f, 30f));
        }
    }
}

sealed class Necrohaze(BossModule module) : Components.GenericAOEs(module)
{
    // regular voidzone
    // when AetherialWard is up, starts moving
    // casts change from Necrohaze1 to Necrohaze4 when EObjAnim 0x00010002
    private readonly List<Actor> _hazes = [];
    private bool _isMoving = false;
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_hazes.Count == 0)
        {
            return [];
        }

        List<AOEInstance> aoes = [];
        var hazes = CollectionsMarshal.AsSpan(_hazes);
        var count = hazes.Length;
        for (var i = 0; i < count; i++)
        {
            ref var haze = ref hazes[i];
            if (_isMoving)
            {
                var position = haze.Position;
                var rotation = haze.Rotation;
                var offset = position - Arena.Center;
                var direction = offset.Cross(rotation.ToDirection()) > 0.0f;
                var length = 5.0f / offset.Length();
                var lengthDirection = (direction ? -length : length).Radians();
                aoes.Add(new(new AOEShapeArcCapsule(5.0f, lengthDirection, Arena.Center), position, rotation));
            }
            else
            {
                aoes.Add(new(new AOEShapeCircle(5f), haze.Position));
            }
        }

        return CollectionsMarshal.AsSpan(aoes);
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.Necrohaze)
        {
            _hazes.Add(actor);
        }
    }

    public override void OnActorDeath(Actor actor)
    {
        if (actor.OID == (uint)OID.Necrohaze)
        {
            _hazes.Remove(actor);
        }
    }

    public override void OnActorRenderflagsChange(Actor actor, int renderflags)
    {
        if (actor.OID == (uint)OID.Necrohaze && renderflags == 16384)
        {
            _hazes.Remove(actor);
        }
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (actor.OID == (uint)OID.AetherialWardDirectionEObj && state == 0x00040020u)
        {
            _isMoving = false;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.AetherialWard)
        {
            _isMoving = true;
        }
    }
}

sealed class NecrohazeMiddle(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Necrohaze2, 5f)
{
    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        // remove on ward death so no blip between this and voidzone
    }

    public override void OnActorDeath(Actor actor)
    {
        if (actor.OID == (uint)OID.AetherialWard)
        {
            Casters.Clear();
        }
    }
}

sealed class AetherialWard(BossModule module) : BossComponent(module)
{
    // directions by AetherialWardDirectionObj, 1 for each direction
    // 0x00010002 = invincible cone
    // 0x00040008 = unset invincible
    // 0x00040020 = ward dead, remove invincible
    private readonly List<Angle> _angles = [];

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (actor.OID == (uint)OID.AetherialWardDirectionEObj)
        {
            switch (state)
            {
                case 0x00010002u:
                    _angles.Add(actor.Rotation);
                    break;
                case 0x00040008u:
                case 0x00040020u:
                    _angles.Clear();
                    break;
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_angles.Count != 0)
        {
            var angles = CollectionsMarshal.AsSpan(_angles);
            var count = angles.Length;
            for (var i = 0; i < count; i++)
            {
                ref var angle = ref angles[i];
                // round to exact degrees, small visual gap in minimap
                var adjustedAngle = ((float)Math.Round(angle.Deg)).Degrees();
                Arena.ZoneCone(Arena.Center, 4f, 5f, adjustedAngle, 45f.Degrees(), Colors.Danger);
            }
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_angles.Count != 0)
        {
            var angles = CollectionsMarshal.AsSpan(_angles);
            var count = angles.Length;
            for (var i = 0; i < count; i++)
            {
                ref var angle = ref angles[i];
                if (actor.Position.InCone(Arena.Center, angle, 45f.Degrees()))
                {
                    hints.Add("Attack from unshielded side!");
                    return;
                }
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_angles.Count != 0)
        {
            var angles = CollectionsMarshal.AsSpan(_angles);
            var count = angles.Length;
            for (var i = 0; i < 4; i++)
            {
                var cardinal = Angle.AnglesCardinals[i];
                var found = false;
                for (var j = 0; j < count; j++)
                {
                    ref var angle = ref angles[j];
                    found = found || angle.AlmostEqual(cardinal, 0.1f);
                }

                if (!found)
                {
                    hints.GoalZones.Add(GoalCone(Arena.Center, 5f, 15f, cardinal, 45f.Degrees(), 5f));
                    break;
                }
            }
        }

        Func<WPos, float> GoalCone(WPos center, float innerRadius, float outerRadius, Angle centerDirection, Angle halfAngle, float weight = 1f)
        {
            var innerR = Math.Max(0f, innerRadius);
            var outerR = Math.Max(innerR + 1f, outerRadius);
            var innerSQ = innerR * innerR;
            var outerSQ = outerR * outerR;
            return p =>
            {
                var distSq = (p - center).LengthSq();
                if (distSq <= innerSQ || distSq >= outerSQ)
                {
                    return default;
                }

                return p.InDonutCone(Arena.Center, innerR, outerR, centerDirection, halfAngle) ? weight : default;
            };
        }
    }
}

[SkipLocalsInit]
sealed class CE205CursedResurgenceStates : StateMachineBuilder
{
    public CE205CursedResurgenceStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<HowlingDarkness>()
            .ActivateOnEnter<SnakingNecrobreath>()
            .ActivateOnEnter<GraveMold>()
            .ActivateOnEnter<Cauterize>()
            .ActivateOnEnter<Catching>()
            .ActivateOnEnter<Necrohaze>()
            .ActivateOnEnter<AetherialWard>()
            .ActivateOnEnter<NecrohazeMiddle>()
            .ActivateOnEnter<BreathInThrees>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP,
    StatesType = typeof(CE205CursedResurgenceStates),
    ConfigType = null, // replace null with typeof(CursedResurgenceConfig) if applicable
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID), // replace null with typeof(AID) if applicable
    StatusIDType = typeof(SID), // replace null with typeof(SID) if applicable
    TetherIDType = null, // replace null with typeof(TetherID) if applicable
    IconIDType = null, // replace null with typeof(IconID) if applicable
    PrimaryActorOID = (uint)OID.ClaretDragon,
    Contributors = "gynorhino",
    Expansion = BossModuleInfo.Expansion.Dawntrail,
    Category = BossModuleInfo.Category.Foray,
    GroupType = BossModuleInfo.GroupType.CriticalEngagement,
    GroupID = 1093u,
    NameID = 53u,
    SortOrder = 5,
    PlanLevel = 0)]
[SkipLocalsInit]
public sealed class CE205CursedResurgence(WorldState ws, Actor primary) : BossModule(ws, primary, new(-688f, 150f), new ArenaBoundsSquare(20f))
{
    private Actor? _aetherialWard;
    protected override void UpdateModule()
    {
        _aetherialWard = GetActor((uint)OID.AetherialWard);
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(_aetherialWard);
        Arena.Actor(PrimaryActor);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            if (e.Actor.OID == (uint)OID.ClaretDragon)
            {
                if (!_aetherialWard?.IsDead ?? false)
                {
                    e.Priority = AIHints.Enemy.PriorityInvincible;
                }
                break;
            }
        }
    }

    protected override bool CheckPull() => base.CheckPull() && Raid.Player()!.Position.InSquare(Arena.Center, 20f);
}
