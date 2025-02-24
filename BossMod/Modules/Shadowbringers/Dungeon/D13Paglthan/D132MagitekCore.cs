namespace BossMod.Shadowbringers.Dungeon.D13Paglthan.D132MagitekCore;

public enum OID : uint
{
    _Gen_ = 0x2E20, // R4.000, x1
    _Gen_1 = 0x2E1E, // R5.000, x1
    _Gen_2 = 0x3346, // R0.500, x1
    _Gen_3 = 0x3345, // R0.500, x1
    _Gen_4 = 0x3344, // R0.500, x2
    _Gen_5 = 0x3353, // R2.000, x3
    _Gen_6 = 0x3355, // R1.000, x1
    _Gen_7 = 0x3343, // R0.500, x1
    Helper = 0x233C, // R0.500, x6, Helper type
    _Gen_MagitekFortress = 0x32FE, // R1.000, x1
    _Gen_TemperedImperial = 0x31AD, // R0.500, x3 (spawn during fight)
    _Gen_TelotekPredator = 0x31AF, // R2.100, x2 (spawn during fight)
    Boss = 0x31AC, // R2.300, x1
    _Gen_MarkIITelotekColossus = 0x31AE, // R3.000, x0 (spawn during fight)
    _Gen_TelotekSkyArmor = 0x31B0, // R2.000, x0 (spawn during fight)
    _Gen_MagitekMissile = 0x31B2, // R1.000, x0 (spawn during fight)
}

public enum AID : uint
{
    _AutoAttack_Attack = 870, // _Gen_7/_Gen_1/_Gen_/_Gen_4/_Gen_TemperedImperial/_Gen_TelotekPredator/_Gen_TelotekSkyArmor/_Gen_MarkIITelotekColossus->player/3326/31D4, no cast, single-target
    _Spell_Fire = 24827, // _Gen_2->318A, 1.0s cast, single-target
    _AutoAttack_Attack1 = 872, // _Gen_3/_Gen_6->31D4/3326, no cast, single-target
    _Weaponskill_ = 24866, // _Gen_5->3326/31D4, 3.0s cast, single-target
    _Weaponskill_1 = 21174, // _Gen_1->self, no cast, range 20 ?-degree cone
    _Weaponskill_2 = 24864, // _Gen_5->3326/31D4, no cast, single-target
    _Weaponskill_MagitekClaw = 23706, // _Gen_TelotekPredator->player, 4.0s cast, single-target
    _Weaponskill_3 = 21175, // _Gen_->self, no cast, range 9 circle
    _Ability_StableCannon = 23700, // Helper->self, no cast, range 60 width 10 rect
    _Weaponskill_DefensiveReaction = 23710, // Boss->self, 5.0s cast, range 60 circle
    _Ability_Aethershot = 23708, // _Gen_TelotekSkyArmor->location, 4.0s cast, range 6 circle
    __ = 10758, // _Gen_MagitekMissile->self, no cast, single-target
    _Ability_GroundToGroundBallistic = 23703, // Helper->location, 5.0s cast, range 40 circle
    _Weaponskill_Exhaust = 23705, // _Gen_MarkIITelotekColossus->self, 4.0s cast, range 40 width 7 rect
    _Weaponskill_ExplosiveForce = 23704, // _Gen_MagitekMissile->player, no cast, single-target
    _Ability_2TonzeMagitekMissile = 23701, // Helper->location, 5.0s cast, range 12 circle
}

class DefensiveReaction(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID._Weaponskill_DefensiveReaction));
class Aethershot(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID._Ability_Aethershot), 6);
class Exhaust(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_Exhaust), new AOEShapeRect(40, 3.5f));

class TwoTonze(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID._Ability_2TonzeMagitekMissile), 12);

class StableCannon(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly WPos[] Cannons = [new(-185, 28.3f), new(-175, 28.3f), new(-165, 28.3f)];

    private readonly List<AOEInstance> aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => aoes;

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index is >= 8 and <= 10)
        {
            switch (state)
            {
                case 0x00200010:
                    aoes.Add(new AOEInstance(new AOEShapeRect(60, 5), Cannons[index - 8], default, WorldState.FutureTime(12.6f)));
                    break;
                case 0x00040004:
                    aoes.Clear();
                    break;
            }
        }
    }


    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID._Ability_StableCannon)
            aoes.RemoveAt(0);
    }
}

class GroundToGroundBallistic(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID._Ability_GroundToGroundBallistic), 10, stopAtWall: true)
{
    private StableCannon? cannons;

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        cannons ??= Module.FindComponent<StableCannon>();

        if (cannons == null)
            return false;

        return cannons.ActiveAOEs(slot, actor).Any(e => e.Check(pos));
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        if (Casters.Count == 0)
            return;

        var aoes = cannons?.ActiveAOEs(slot, actor).ToList();
        if (aoes == null)
            return;

        var source = Casters[0].CastInfo!.LocXZ;
        hints.AddForbiddenZone(p =>
        {
            var dist = (p - source).Normalized();
            var proj = Arena.ClampToBounds(p + dist * 10);
            return aoes.Any(e => e.Check(proj)) ? -1 : 0;
        }, Module.CastFinishAt(Casters[0].CastInfo));
    }
}

class Launchpad(BossModule module) : BossComponent(module)
{
    private bool active;

    private static readonly WPos Position = new(-175, 30);

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index == 0x0D)
        {
            switch (state)
            {
                case 0x00020001:
                    active = true;
                    Arena.Center = MagitekCore.CombinedCenter;
                    Arena.Bounds = MagitekCore.CombinedBounds;
                    break;
                case 0x00080004:
                    active = false;
                    Arena.Center = MagitekCore.DefaultCenter;
                    Arena.Bounds = MagitekCore.DefaultBounds;
                    break;
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (active && actor.PosRot.Y < -18)
            hints.GoalZones.Add(p => 15 - (p - Position).Length());
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (active && actor.PosRot.Y < -18)
            hints.Add("Go to launchpad!", false);
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (active && pc.PosRot.Y < -18)
            Arena.ZoneCircle(Position, 2, ArenaColor.SafeFromAOE);
    }
}

class MagitekMissile(BossModule module) : BossComponent(module)
{
    private const float Radius = 1.5f;
    private readonly List<Actor> Missiles = [];

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID._Gen_MagitekMissile)
            Missiles.Add(actor);
    }

    public override void OnActorDestroyed(Actor actor)
    {
        Missiles.Remove(actor);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID._Weaponskill_ExplosiveForce)
            Missiles.Remove(caster);
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach (var m in Missiles)
            Arena.ZoneCircle(m.Position, Radius, ArenaColor.AOE);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var m in Missiles)
            hints.AddForbiddenZone(ShapeDistance.Capsule(m.Position, m.Rotation, 7, Radius), WorldState.FutureTime(1.5f));
    }
}

class MagitekCoreStates : StateMachineBuilder
{
    public MagitekCoreStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<StableCannon>()
            .ActivateOnEnter<GroundToGroundBallistic>()
            .ActivateOnEnter<Launchpad>()
            .ActivateOnEnter<Aethershot>()
            .ActivateOnEnter<Exhaust>()
            .ActivateOnEnter<MagitekMissile>()
            .ActivateOnEnter<TwoTonze>()
            .ActivateOnEnter<DefensiveReaction>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 777, NameID = 10076)]
public class MagitekCore(WorldState ws, Actor primary) : BossModule(ws, primary, DefaultCenter, DefaultBounds)
{
    public static readonly WPos DefaultCenter = new(-175, 43);
    public static readonly ArenaBounds DefaultBounds = new ArenaBoundsSquare(14.6f);

    private static readonly (WPos, ArenaBoundsCustom) DoubleBounds = MakeCombined();
    public static readonly WPos CombinedCenter = DoubleBounds.Item1;
    public static readonly ArenaBounds CombinedBounds = DoubleBounds.Item2;

    public static (WPos, ArenaBoundsCustom) MakeCombined()
    {
        var ground = CurveApprox.Rect(new WDir(0, 13.75f), new WDir(14.6f, 0), new WDir(0, 14.6f));
        var platform = CurveApprox.Rect(new WDir(0, -20.75f), new WDir(7.5f, 0), new WDir(0, 7.5f));
        var clipper = new PolygonClipper();
        return (new(-175, 29.25f), new(29, clipper.Union(new(ground), new(platform))));
    }

    protected override bool CheckPull() => PrimaryActor.InCombat;

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(WorldState.Actors.Where(x => !x.IsAlly), ArenaColor.Enemy);
    }
}

