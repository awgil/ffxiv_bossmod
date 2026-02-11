using BossMod.Interfaces;
using FFXIVClientStructs.FFXIV.Client.Game.Event;
using FFXIVClientStructs.FFXIV.Client.Game.Fate;

namespace BossMod;

public class HintExecutor(WorldState world, AIHints hints, IAmex amex, IMovementOverride movementOverride) : IHintExecutor
{
    private DateTime _throttleJump;
    private DateTime _throttleInteract;
    private DateTime _throttleFateSync;
    private DateTime _throttleLeaveDuty;

    public unsafe void Execute()
    {
        movementOverride.DesiredDirection = hints.ForcedMovement;
        movementOverride.MisdirectionThreshold = hints.MisdirectionThreshold;
        movementOverride.DesiredSpinDirection = hints.SpinDirection;

        var targetSystem = FFXIVClientStructs.FFXIV.Client.Game.Control.TargetSystem.Instance();
        SetTarget(hints.ForcedTarget, &targetSystem->Target);
        SetTarget(hints.ForcedFocusTarget, &targetSystem->FocusTarget);

        foreach (var s in hints.StatusesToCancel)
        {
            var res = FFXIVClientStructs.FFXIV.Client.Game.StatusManager.ExecuteStatusOff(s.statusId, s.sourceId != 0 ? (uint)s.sourceId : 0xE0000000);
            Service.Log($"[ExecHints] Canceling status {s.statusId} from {s.sourceId:X} -> {res}");
        }
        if (hints.WantJump && world.CurrentTime > _throttleJump)
        {
            //Service.Log($"[ExecHints] Jumping...");
            FFXIVClientStructs.FFXIV.Client.Game.ActionManager.Instance()->UseAction(FFXIVClientStructs.FFXIV.Client.Game.ActionType.GeneralAction, 2);
            _throttleJump = world.FutureTime(0.1f);
        }

        if (hints.ShouldLeaveDuty && world.CurrentTime >= _throttleLeaveDuty)
        {
            EventFramework.LeaveCurrentContent(false);
            _throttleLeaveDuty = world.FutureTime(1.0f);
        }

        if (CheckInteractRange(world.Party.Player(), hints.InteractWithTarget))
        {
            // many eventobj interactions "immediately" start some cast animation (delayed by server roundtrip), and if we keep trying to move toward the target after sending the interact request, it will be canceled and force us to start over
            movementOverride.DesiredDirection = default;

            if (amex.EffectiveAnimationLock == 0 && world.CurrentTime >= _throttleInteract)
            {
                FFXIVClientStructs.FFXIV.Client.Game.Control.TargetSystem.Instance()->InteractWithObject(GetActorObject(hints.InteractWithTarget), false);
                _throttleInteract = world.FutureTime(0.1f);
            }
        }

        HandleFateSync();
    }

    private unsafe void SetTarget(Actor? target, FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject** targetPtr)
    {
        if (target == null || !target.IsTargetable)
            return;

        var obj = GetActorObject(target);

        // 50 in-game units is the maximum distance before nameplates stop rendering (making the mob effectively untargetable)
        // targeting a mob that isn't visible is bad UX
        if (world.Party.Player() is { } player)
        {
            var distSq = (player.PosRot.XYZ() - target.PosRot.XYZ()).LengthSquared();
            if (distSq < 2500)
                *targetPtr = obj;
        }
    }

    private unsafe bool CheckInteractRange(Actor? player, Actor? target)
    {
        var playerObj = GetActorObject(player);
        var targetObj = GetActorObject(target);
        if (playerObj == null || targetObj == null)
            return false;

        // treasure chests have no client-side interact range check at all; just assume they use the standard "small" range, seems to be accurate from testing
        if (targetObj->ObjectKind is FFXIVClientStructs.FFXIV.Client.Game.Object.ObjectKind.Treasure)
            return player?.DistanceToHitbox(target) <= 2.09f;

        return EventFramework.Instance()->CheckInteractRange(playerObj, targetObj, 1, false);
    }

    private unsafe void HandleFateSync()
    {
        var fm = FateManager.Instance();
        var fate = fm->CurrentFate;
        if (fate == null)
            return;

        var shouldDoSomething = hints.WantFateSync switch
        {
            AIHints.FateSync.Enable => !fm->IsSyncedToFate(fate),
            AIHints.FateSync.Disable => fm->IsSyncedToFate(fate),
            _ => false
        };

        if (shouldDoSomething && world.CurrentTime >= _throttleFateSync)
        {
            fm->LevelSync();
            _throttleFateSync = world.FutureTime(0.5f);
        }
    }

    private unsafe FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject* GetActorObject(Actor? actor)
    {
        if (actor == null)
            return null;

        var obj = FFXIVClientStructs.FFXIV.Client.Game.Object.GameObjectManager.Instance()->Objects.IndexSorted[actor.SpawnIndex].Value;
        if (obj == null)
            return null;

        if (obj->EntityId != actor.InstanceID)
            Service.Log($"[ExecHints] Unexpected actor: expected {actor.InstanceID:X} at #{actor.SpawnIndex}, but found {obj->EntityId:X}");

        return obj;
    }
}
