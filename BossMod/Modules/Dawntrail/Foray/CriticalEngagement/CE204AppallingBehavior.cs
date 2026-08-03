namespace BossMod.Dawntrail.Foray.CriticalEngagement.CE204AppallingBehavior;

public enum OID : uint
{
    Pallmagia = 0x4D8F,
    Helper = 0x233C,
    Pallmagia1 = 0x4D91, // R1.000, x1
    Pallkeeper = 0x4D90, // R2.300, x4
    PallkeeperVFX = 0x1EC02A, // R0.500, x4, EventObj type - Used to display the cast type hints vfx in-game

    _Gen_Actor1ea1a1 = 0x1EA1A1, // R2.000, x2, EventObj type
    _Gen_Actor1ec029 = 0x1EC029, // R0.500, x1, EventObj type
    _Gen_Actor1ec02c = 0x1EC02C, // R0.500, x0 (spawn during fight), EventObj type
    _Gen_Actor1ec02b = 0x1EC02B, // R0.500, x0 (spawn during fight), EventObj type
}

public enum AID : uint
{
    ArenaMapChange = 49771, // 4D91->self, no cast, range ?-25 donut
    AutoAttack = 50494, // Pallmagia->player, no cast, single-target

    BadBreathBoss = 50490, // Pallmagia->self, 4.3+0.7s cast, single-target
    BadBreath = 50491, // Helper->self, 5.0s cast, range 50 100-degree cone
    PlaincrackerBoss = 50492, // Pallmagia->self, 4.3+0.7s cast, single-target
    Plaincracker = 50493, // Helper->self, 5.0s cast, range 15 circle
    GreatWhirlwindCast = 49798, // Pallmagia->self, 4.3+0.7s cast, single-target
    GreatWhirlwind = 50450, // Helper->self, 5.0s cast, ???
    OccultMissileCast = 49795, // Pallmagia->self, 3.3+0.7s cast, single-target
    OccultMissile = 49797, // Helper->location, 4.0s cast, range 6 circle
    LilliputianLyricCast = 49791, // Pallmagia->self, 4.3+0.7s cast, single-target
    LilliputianLyric = 49792, // Helper->self, 5.0s cast, range 40 180-degree cone
    MagicHammerCast = 49793, // Pallmagia->self, 3.0s cast, single-target
    MagicHammer = 49794, // Helper->location, 5.5s cast, range 8 circle

    Summon = 49772, // Pallmagia->self, 3.0s cast, single-target
    EsotericInstruction = 49773, // Pallmagia->self, 13.0s cast, single-target
    EsotericInstructionSwap = 49774, // Pallmagia->self, 13.0s cast, single-target // TODO check if this one is casted they will swap
    ReversePolarity = 49775, // Pallmagia->self, 5.0s cast, single-target
    BadBreathPallkeeperVisual = 49776, // 4D90->self, no cast, single-target
    BadBreathPallkeeper = 49777, // Helper->self, 3.0s cast, range 50 100-degree cone
    PlaincrackerPallkeeperVisual = 49778, // 4D90->self, no cast, single-target
    PlaincrackerPallkeeper = 49779, // Helper->self, 3.0s cast, range 30 circle
    PallKeeperTeleport = 49786, // 4D90->location, no cast, single-target
    PallKeeperTeleport1 = 49785, // 4D90->location, no cast, single-target
    PallKeeperTeleport2 = 49784, // 4D90->location, no cast, single-target

    _Spell_Roulette = 49787, // Pallmagia->self, 4.0s cast, single-target
    _Spell_Roulette1 = 49788, // Helper->self, no cast, range 5 circle
    _Spell_Roulette2 = 49789, // Helper->self, no cast, range 5-12 donut
    _Spell_Roulette3 = 49790, // Helper->self, no cast, range 12-20 donut

    _Spell_ = 49799, // Helper->self, 5.0s cast, single-target
}

public enum SID : uint
{
    Gen = 2056, // none->Pallmagia/4D90, extra=0x485/0x486/0x490
}

public enum TetherID : uint
{
    IconTether = 14, // 4D90->Pallmagia
    SwapTether = 207, // 4D90->4D90
}

sealed class BadBreath(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BadBreath, new AOEShapeCone(50.0f, 50.0f.Degrees()));
sealed class Plaincracker(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Plaincracker, new AOEShapeCircle(15.0f));
sealed class GreatWhirlwind(BossModule module) : Components.RaidwideCast(module, (uint)AID.GreatWhirlwind);
sealed class LilliputianLyric(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LilliputianLyric, new AOEShapeCone(40.0f, 90.0f.Degrees()));

sealed class OccultMissile : Components.SimpleAOEs
{
    public OccultMissile(BossModule module) : base(module, (uint)AID.OccultMissile, 6.0f, 8)
    {
        MaxDangerColor = 4;
    }
}

sealed class MagicHammer : Components.SimpleAOEs
{
    public MagicHammer(BossModule module) : base(module, (uint)AID.MagicHammer, 8.0f, 8)
    {
        MaxDangerColor = 4;
    }
}

// TODO add spell timers depending on cast version
sealed class EsotericInstruction(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> aoes = [];
    private readonly AOEShapeCone cone = new(50.0f, 50.0f.Degrees());
    private readonly AOEShapeCircle circle = new((30.0f));

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.EsotericInstruction)
        {
            Service.Logger.Info("no swap");
        }

        if (spell.Action.ID == (uint)AID.EsotericInstructionSwap)
        {
            Service.Logger.Info("time to swap");
        }
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (actor.OID == (uint)OID.PallkeeperVFX)
        {
            // the animation comes from a different actor but on the same position
            var pallKeeper = Module.Enemies((uint)OID.Pallkeeper).Closest(actor.Position);
            if (pallKeeper == null)
            {
                return;
            }

            if (state == 65538)
            {
                aoes.Add(new(cone, actor.Position, actor.Rotation, actorID: pallKeeper.InstanceID));
            }

            if (state == 1048608)
            {
                aoes.Add(new(circle, actor.Position, actor.Rotation, actorID: pallKeeper.InstanceID));
            }
        }
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.SwapTether)
        {
            var tetherInfo = tether;
            var pallKeeperSource = aoes.FindIndex(aoe => aoe.ActorID == source.InstanceID);
            var pallKeeperTarget = aoes.FindIndex(aoe => aoe.ActorID == tetherInfo.Target);
            if (pallKeeperSource < 0 || pallKeeperTarget < 0)
            {
                return;
            }

            var aoeInstances = CollectionsMarshal.AsSpan(aoes);
            ref var aoe1 = ref aoeInstances[pallKeeperSource];
            ref var aoe2 = ref aoeInstances[pallKeeperTarget];
            (aoe1.Origin, aoe2.Origin) = (aoe2.Origin, aoe1.Origin);
            (aoe1.Rotation, aoe2.Rotation) = (aoe2.Rotation, aoe1.Rotation);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.BadBreathPallkeeper or (uint)AID.PlaincrackerPallkeeper)
        {
            if (aoes.Count > 0)
            {
                aoes.RemoveAt(0);
            }
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        int shown = 0;
        foreach (ref var aoe in CollectionsMarshal.AsSpan(aoes))
        {
            if (shown >= 2)
            {
                break;
            }

            aoe.Color = shown == 0 ? Colors.Danger : Colors.AOE;
            aoe.Risky = shown == 0;
            shown++;
        }

        return CollectionsMarshal.AsSpan(aoes)[..shown];
    }
}

sealed class Roulette(BossModule module) : BossComponent(module)
{

    /*
     00:09:33.393 | INF | [BossModReborn] actor created 2015276
    00:09:33.393 | INF | [BossModReborn] actor created 2015275
    00:09:37.928 | INF | [BossModReborn] actor 2015276 state 65538
    00:09:37.928 | INF | [BossModReborn] actor 2015275 state 65538
    00:10:14.213 | INF | [BossModReborn] actor 2015276 state 262160
    00:10:14.213 | INF | [BossModReborn] actor 2015275 state 262176
    cardinal are safe

    start was also this:
    2c - 0 rotation - north & south out, east and west inner
    2b - 0 rotation ^

     */
}

[SkipLocalsInit]
sealed class CE204AppallingBehaviorStates : StateMachineBuilder
{
    public CE204AppallingBehaviorStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BadBreath>()
            .ActivateOnEnter<Plaincracker>()
            .ActivateOnEnter<EsotericInstruction>()
            .ActivateOnEnter<GreatWhirlwind>()
            .ActivateOnEnter<OccultMissile>()
            .ActivateOnEnter<LilliputianLyric>()
            .ActivateOnEnter<MagicHammer>()
            .ActivateOnEnter<Roulette>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Dummy,
    StatesType = typeof(CE204AppallingBehaviorStates),
    ConfigType = null, // replace null with typeof(PallmagiaConfig) if applicable
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID), // replace null with typeof(AID) if applicable
    StatusIDType = typeof(SID), // replace null with typeof(SID) if applicable
    TetherIDType = typeof(TetherID), // replace null with typeof(TetherID) if applicable
    IconIDType = null, // replace null with typeof(IconID) if applicable
    PrimaryActorOID = (uint)OID.Pallmagia,
    Contributors = "Equilius",
    Expansion = BossModuleInfo.Expansion.Dawntrail,
    Category = BossModuleInfo.Category.Foray,
    GroupType = BossModuleInfo.GroupType.CFC,
    GroupID = 1093u,
    NameID = 14714u,
    SortOrder = 1,
    PlanLevel = 0)]
[SkipLocalsInit]
public sealed class CE204AppallingBehavior(WorldState ws, Actor primary) : BossModule(ws, primary, new(807.000f, -562.000f), new ArenaBoundsCircle(20f))
{
    protected override bool CheckPull() => base.CheckPull() && Raid.Player()!.Position.InCircle(Arena.Center, 20f);
}
