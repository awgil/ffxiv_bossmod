using BossMod.AI;
using BossMod.Autorotation;
using BossMod.Autorotation.xan;
using static BossMod.PartyRolesConfig;

namespace BossMod.Endwalker.Ultimate.TOP.AI;
internal class StandardTank(RotationModuleManager manager, Actor player) : AIRotationModule(manager, player)
{
    public static RotationModuleDefinition Definition()
    {
        return new RotationModuleDefinition("TOP Mitty - Tank", "TOP Mitty - Tank", "TOP", "xan", RotationModuleQuality.WIP, BitMask.Build(Class.PLD, Class.WAR, Class.GNB, Class.DRK), 90, 90, typeof(TOP));
    }

    private readonly PartyRolesConfig _config = Service.Config.Get<PartyRolesConfig>();
    private Assignment PlayerAssignment => _config[World.Party.Members[0].ContentId];
    private Actor? Cotank
    {
        get
        {
            switch (PlayerAssignment)
            {
                case Assignment.OT:
                    foreach (var (i, actor) in World.Party.WithSlot())
                        if (_config[World.Party.Members[i].ContentId] == Assignment.MT)
                            return actor;
                    return null;
                case Assignment.MT:
                    foreach (var (i, actor) in World.Party.WithSlot())
                        if (_config[World.Party.Members[i].ContentId] == Assignment.OT)
                            return actor;
                    return null;
                default:
                    return null;
            }
        }
    }

    private TankAI.TankActions TankActions => TankAI.ActionsForJob(Player.Class);

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        if (Bossmods.ActiveModule is not TOP module)
            return;

        MitPanto(primaryTarget, module.StateMachine);
    }

    private void UseBuff(TankAI.Buff buff, Actor? target)
    {
        if (buff.CanUse?.Invoke(this) ?? true)
        {
            Hints.ActionsToExecute.Push(buff.ID, target, ActionQueue.Priority.Medium);
        }
    }

    private void MitPanto(Actor? primaryTarget, StateMachine stateMachine)
    {
        // first stack happens 12.1 seconds after transition, last stack happens 18 seconds later
        // both tanks should use party mit to cover as much as they can
        // war/pld have 30s mit so can use whenever, gnb/drk should ensure last stack is mitted. first stack usually has beefy shields
        var timeSincePantoCast = stateMachine.ActiveState?.ID switch
        {
            0x10001 => stateMachine.TimeSinceTransition,
            0x10010 => stateMachine.TimeSinceTransition + 12.1f,
            _ => -1
        };
        if (timeSincePantoCast < 0)
            return;

        var lastStackIn = 30.1f - timeSincePantoCast;
        var useMitIn = lastStackIn - TankActions.PartyMit.Duration;
        Hints.ActionsToExecute.Push(TankActions.PartyMit.ID, Player, ActionQueue.Priority.Medium, default, useMitIn);
    }
}
