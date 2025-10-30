using BossMod.AI;
using BossMod.Autorotation;
using BossMod.Pathfinding;

namespace BossMod.RealmReborn.Extreme.Ex3Titan;

sealed class Ex3TitanAIRotation(RotationModuleManager manager, Actor player) : AIRotationModule(manager, player)
{
    public enum Track { Movement }
    public enum MovementStrategy { None, Pathfind, Explicit }

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("AI Experiment", "Experimental encounter-specific rotation", "Encounter AI", "veyn", RotationModuleQuality.WIP, new(~1ul), 1000, 1, RotationModuleOrder.Movement, typeof(Ex3Titan));
        res.Define(Track.Movement).As<MovementStrategy>("Movement", "Movement")
            .AddOption(MovementStrategy.None, "No automatic movement")
            .AddOption(MovementStrategy.Pathfind, "Use standard pathfinding to move")
            .AddOption(MovementStrategy.Explicit, "Move to specific point", supportedTargets: ActionTargets.Area);
        return res;
    }

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        SetForcedMovement(CalculateDestination(strategy.Option(Track.Movement)));
    }

    private WPos CalculateDestination(StrategyValues.OptionRef strategy) => strategy.As<MovementStrategy>() switch
    {
        MovementStrategy.Pathfind => NavigationDecision.Build(NavigationContext, World.CurrentTime, Hints, Player.Position, Speed()).Destination ?? Player.Position,
        MovementStrategy.Explicit => ResolveTargetLocation(strategy.Value),
        _ => Player.Position
    };
}

class Ex3TitanAI(BossModule module) : BossComponent(module)
{
    public bool KillNextBomb;
    private readonly GraniteGaol? _rockThrow = module.FindComponent<GraniteGaol>();

    public override void Update()
    {
        if (KillNextBomb && !Module.Enemies(OID.BombBoulder).Any())
            KillNextBomb = false;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        bool haveGaolers = Module.Enemies(OID.GraniteGaoler).Any(a => a.IsTargetable && !a.IsDead);
        foreach (var e in hints.PotentialTargets)
        {
            e.StayAtLongRange = true;
            switch ((OID)e.Actor.OID)
            {
                case OID.Boss:
                case OID.TitansHeart:
                    e.Priority = 1;
                    e.AttackStrength = 0.25f;
                    e.DesiredPosition = Module.Center - new WDir(0, Arena.Bounds.Radius - 6);
                    e.DesiredRotation = 180.Degrees();
                    e.TankDistance = 0;
                    if (actor.Role == Role.Tank)
                    {
                        // note on tank swaps
                        // during phase 1, each 'repeat' lasts for 52 seconds and has 3 busters
                        // theoretically we can swap to OT right after 1st buster, then MT's vuln will expire right after 3rd buster and he can taunt back
                        // OT's vuln will expire right before 5th buster, so MT will eat 1/4/7/... and OT will eat 2+3/5+6/...
                        // however, in reality phase is going to be extremely short - 1 or 2 tb's?..
                        bool isCurrentTank = actor.InstanceID == Module.PrimaryActor.TargetID;
                        bool needTankSwap = !haveGaolers && Module.FindComponent<MountainBuster>() == null && TankVulnStacks() >= 2;
                        e.PreferProvoking = e.ShouldBeTanked = isCurrentTank != needTankSwap;
                    }
                    break;
                case OID.GraniteGaoler:
                    e.Priority = 2;
                    e.DesiredPosition = Module.Center + (Module.Bounds.Radius - 4) * 30.Degrees().ToDirection(); // move them away from boss, healer gaol spots and upheaval knockback spots
                    e.ShouldBeTanked = Module.PrimaryActor.TargetID != actor.InstanceID && actor.Role == Role.Tank;
                    break;
                case OID.BombBoulder:
                    e.Priority = KillNextBomb && e.Actor.Position.AlmostEqual(Module.Center, 1) ? 3 : 0; // kill center bomb when needed
                    e.ShouldBeTanked = false;
                    break;
                case OID.GraniteGaol:
                    e.Priority = e.Actor.Position.InCircle(Module.PrimaryActor.Position, 5) ? 5 : 4; // prefer killing gaol under boss first
                    e.AttackStrength = 0;
                    e.ShouldBeTanked = false;
                    break;
            }
        }

        // if there are no active mechanics, all except current tank prefer stacking on max melee behind boss, at an angle that allows all positionals
        if (!haveGaolers && !KillNextBomb && actor.InstanceID != Module.PrimaryActor.TargetID && hints.ForbiddenZones.Count == 0)
        {
            if (_rockThrow != null && _rockThrow.PendingFetters[slot])
            {
                var pos = actor.Role == Role.Healer
                    ? Module.Center + Module.Bounds.Radius * (-30).Degrees().ToDirection() // healers should go to the back; 30 degrees will be safe if landslide is baited straight to south (which it should, since it will follow upheaval)
                    : Module.PrimaryActor.Position + new WDir(0, 1);
                hints.AddForbiddenZone(ShapeContains.InvertedCircle(pos, 2), _rockThrow.ResolveAt);
            }
            else
            {
                var pos = StackPosition();
                if (pos != null)
                    hints.AddForbiddenZone(ShapeContains.InvertedCircle(pos.Value, 2), /*module.StateMachine.NextTransitionWithFlag(StateMachine.StateHint.BossCastStart)*/DateTime.MaxValue);
            }
        }
    }

    private WPos? StackPosition()
    {
        var boss = Module.PrimaryActor;
        var res = boss.Position + 3 * (boss.Rotation + 135.Degrees()).ToDirection();
        if (Arena.InBounds(res))
            return res;
        res = boss.Position + 3 * (boss.Rotation - 135.Degrees()).ToDirection();
        if (Arena.InBounds(res))
            return res;
        return null;
    }

    private int TankVulnStacks() => WorldState.Actors.Find(Module.PrimaryActor.TargetID)?.FindStatus(SID.PhysicalVulnerabilityUp)?.Extra ?? 0;
}
