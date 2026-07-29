namespace BossMod.Heavensward.Dungeon.D08FractalContinuum.D082Minotaur;

public enum OID : uint
{
    Boss = 0x1013, // R4.0
    ContinuumConservator = 0x1015, // R1.0
    FlawedShabti = 0x1014, // R1.1
    FlawedNaga = 0x1017, // R1.350, x1
    Urstrix = 0x1016, // R1.700, x1
    BiomassIncubator1 = 0x1E99EA, // R2.0
    BiomassIncubator2 = 0x1E99E8, // R2.0
    BiomassIncubator3 = 0x1E99EB, // R2.0
    BiomassIncubator4 = 0x1E99E9 // R2.0
}

public enum AID : uint
{
    AutoAttack1 = 870, // Boss->player, no cast, single-target
    AutoAttack2 = 871, // Urstrix->player, no cast, single-target
    AutoAttack3 = 872, // FlawedNaga->player, no cast, single-target
    Shot = 2150, // ContinuumConservator->player, no cast, single-target

    Feast = 3976, // Boss->Urstrix/FlawedNaga/ContinuumConservator/FlawedShabti, 10.0s cast, single-target
    OneOneTonzeSwipe = 3969, // Boss->self, 3.0s cast, range 5+R 120-degree cone
    OneOneOneTonzeSwing = 3970, // Boss->self, 4.0s cast, range 8+R circle, knockback 20, away from source
    OneOneOneOneTonzeSwing = 3975, // Boss->self, 11.0s cast, range 20+R circle
    DisorientingGroan = 3972, // Boss->self, 3.0s cast, range 80+R circle, knockback 20, away from source
    ZoomIn = 3974, // Boss->player, no cast, width 8 rect charge, knockback 15, away from source, rushes 3 random (?) players back to back
    TenTonzeSlash = 3971 // Boss->self, 4.0s cast, range 40+R 60-degree cone
}

class OneOneTonzeSwipe(BossModule module) : Components.SimpleAOEs(module, (uint)AID.OneOneTonzeSwipe, new AOEShapeCone(9f, 60f.Degrees()));
class OneOneOneTonzeSwing(BossModule module) : Components.SimpleAOEs(module, (uint)AID.OneOneOneTonzeSwing, 12f);
class TenTonzeSlash(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TenTonzeSlash, new AOEShapeCone(44f, 30f.Degrees()));

class OneOneOneOneTonzeSwing(BossModule module) : BossComponent(module)
{
    private bool casting;
    private readonly List<Actor> _incubators = module.Enemies([(uint)OID.BiomassIncubator1, (uint)OID.BiomassIncubator2, (uint)OID.BiomassIncubator3, (uint)OID.BiomassIncubator4]);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.OneOneOneOneTonzeSwing)
        {
            casting = true;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.OneOneOneOneTonzeSwing)
        {
            casting = false;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!casting)
        {
            return;
        }

        Actor? closest = null;
        var minDistSq = float.MaxValue;

        var count = _incubators.Count;
        for (var i = 0; i < count; ++i)
        {
            var incubator = _incubators[i];
            if (incubator.IsTargetable)
            {
                hints.GoalZones.Add(AIHints.GoalSingleTarget(incubator, 2f, 5f));
                var distSq = (actor.Position - incubator.Position).LengthSq();
                if (distSq < minDistSq)
                {
                    minDistSq = distSq;
                    closest = incubator;
                }
            }
        }
        hints.InteractWithTarget = closest;
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (!casting)
        {
            return;
        }

        var count = _incubators.Count;
        for (var i = 0; i < count; ++i)
        {
            if (_incubators[i].IsTargetable)
            {
                hints.Add("Use an incubator to stun the boss!");
                return;
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (!casting)
        {
            return;
        }

        var count = _incubators.Count;
        for (var i = 0; i < count; ++i)
        {
            var incubator = _incubators[i];
            if (incubator.IsTargetable)
            {
                Arena.ZoneCircleOutline(incubator.Position, 3f, Colors.Safe);
            }
        }
    }
}

class DisorientingGroan(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.DisorientingGroan, 20f, stopAtWall: true);

class D082MinotaurStates : StateMachineBuilder
{
    public D082MinotaurStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<OneOneOneOneTonzeSwing>()
            .ActivateOnEnter<OneOneTonzeSwipe>()
            .ActivateOnEnter<OneOneOneTonzeSwing>()
            .ActivateOnEnter<DisorientingGroan>()
            .ActivateOnEnter<TenTonzeSlash>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 35, NameID = 3429, SortOrder = 5)]
public class D082Minotaur(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsCustom arena = new([new Polygon(new(-160.632f, 91.323f), 19.5f, 24)],
    [new Rectangle(new(-180.138f, 96.181f), 20f, 1.25f, -74.089f.Degrees()), new Rectangle(new(-141.271f, 85.777f), 20f, 1.25f, -74.205f.Degrees())]);

    private static readonly uint[] adds = [(uint)OID.FlawedShabti, (uint)OID.ContinuumConservator, (uint)OID.Urstrix, (uint)OID.FlawedNaga];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(this, adds);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.FlawedShabti or (uint)OID.ContinuumConservator or (uint)OID.Urstrix or (uint)OID.FlawedShabti => 1,
                _ => 0
            };
        }
    }
}
