namespace BossMod.Stormblood.Dungeon.D09DrownedCityOfSkalla.D092TheOldOne;

public enum OID : uint
{
    TheOldOne = 0x1FAC, // R4.600, x?
    Subservient = 0x1FAD, // R1.725, x?
    Helper = 0x18D6
}

public enum AID : uint
{
    AutoAttack = 29791, // TheOldOne->player, no cast, single-target

    MysticLight = 9815, // TheOldOne->self, 4.0s cast, range 40+R 60-degree cone
    MysticFlame = 9816, // TheOldOne->self, 3.0s cast, single-target
    MysticFlame1 = 9817, // Helper->self, 3.5s cast, range 8 circle
    ShiftingLight = 9818, // TheOldOne->self, 3.0s cast, range 20+R circle
    Shatterstone = 9824, // Helper->self, 2.0s cast, range 5 circle : Duty action : This is not the duty action.
    OrderToDetonate = 9819 // TheOldOne->self, 20.0s cast, single-target
}

public enum SID : uint
{
    Invincibility = 325, // none->Helper/TheOldOne/_Gen_, extra=0x0 : Boss goes invincible while subservient are out.
    Transfiguration = 1448 // none->player/3F7F/40C0/3F75, extra=0x4A : players transformed.
}

sealed class MysticLight(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MysticLight, new AOEShapeCone(40f, 30f.Degrees()));

sealed class MysticFlame(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MysticFlame1, 8f);

sealed class SubservientAdds(BossModule module) : Components.Adds(module, (uint)OID.Subservient, priority: 1);

sealed class ShiftingLight(BossModule module) : Components.RaidwideCast(module, (uint)AID.ShiftingLight);

// pc gets turned into a sprite.  When that happens go kill the subservients with shatterstone duty action.
sealed class Shatterstone(BossModule module) : BossComponent(module)
{
    // store whether or not we have the transfiguration status
    private BitMask _transfigurationStatus;

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.Transfiguration && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
        {
            _transfigurationStatus[slot] = true;
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
        {
            if (status.ID == (uint)SID.Transfiguration)
                _transfigurationStatus[slot] = false;
        }
    }

    public IEnumerable<Actor> Subservients => Module.Enemies((uint)OID.Subservient).Where(x => !x.IsDeadOrDestroyed);

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.Actors(Subservients, Colors.Enemy);
    }

    // Move toward the nearest Subservient add. If transfigure then use the dutyaction to  attack.
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        var attackRadius = 4;

        var closestSub = Subservients.MaxBy(x => x.PosRot.Z);
        if (closestSub != null)
        {
            var pos = closestSub.Position;
            WPos optimalAttackPosition = new(pos.X, pos.Z + 1);
            // move towards nearest subservient add.
            hints.GoalZones.Add(AIHints.GoalSingleTarget(optimalAttackPosition, attackRadius - 2, 10));
            // use duty action if you are transformed.
            if (actor.DistanceToHitbox(closestSub) < attackRadius - 1 && _transfigurationStatus[slot])
                hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.Shatterstone), null, ActionQueue.Priority.High, targetPos: closestSub.PosRot.XYZ());
        }
    }
}

sealed class OrderToDetonate(BossModule module) : Components.RaidwideCast(module, (uint)AID.OrderToDetonate);

[SkipLocalsInit]
sealed class D092TheOldOneStates : StateMachineBuilder
{
    public D092TheOldOneStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<MysticLight>()
            .ActivateOnEnter<MysticFlame>()
            .ActivateOnEnter<SubservientAdds>()
            .ActivateOnEnter<ShiftingLight>()
            .ActivateOnEnter<Shatterstone>()
            .ActivateOnEnter<OrderToDetonate>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed,
    StatesType = typeof(D092TheOldOneStates),
    ConfigType = null, // replace null with typeof(TheOldOneConfig) if applicable
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID),
    StatusIDType = typeof(SID),
    TetherIDType = null, // replace null with typeof(TetherID) if applicable
    IconIDType = null, // replace null with typeof(IconID) if applicable
    PrimaryActorOID = (uint)OID.TheOldOne,
    Contributors = "wen",
    Expansion = BossModuleInfo.Expansion.Stormblood,
    Category = BossModuleInfo.Category.Dungeon,
    GroupType = BossModuleInfo.GroupType.CFC,
    GroupID = 279u,
    NameID = 6908u,
    SortOrder = 2,
    PlanLevel = 0)]

[SkipLocalsInit]
public sealed class D092TheOldOne(WorldState ws, Actor primary) : BossModule(ws, primary, new(115f, 4f), new ArenaBoundsCircle(20f));
