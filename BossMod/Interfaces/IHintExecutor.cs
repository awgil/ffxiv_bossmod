using FFXIVClientStructs.FFXIV.Client.Game.Event;
using FFXIVClientStructs.FFXIV.Client.Game.Fate;
using FFXIVClientStructs.FFXIV.Client.UI;

namespace BossMod.Interfaces;

public interface IHintExecutor
{
    void Execute();
}

internal sealed class MockHintExecutor : IHintExecutor
{
    public void Execute() { }
}

internal sealed class HintExecutor(WorldState ws, MovementOverride movementOverride, ActionManagerEx amex, AIHints hints) : IHintExecutor
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
        if (hints.WantJump && ws.CurrentTime > _throttleJump)
        {
            //Service.Log($"[ExecHints] Jumping...");
            FFXIVClientStructs.FFXIV.Client.Game.ActionManager.Instance()->UseAction(FFXIVClientStructs.FFXIV.Client.Game.ActionType.GeneralAction, 2);
            _throttleJump = ws.FutureTime(0.1f);
        }

        if (hints.ShouldLeaveDuty && ws.CurrentTime >= _throttleLeaveDuty)
        {
            EventFramework.LeaveCurrentContent(false);
            _throttleLeaveDuty = ws.FutureTime(1.0f);
        }

        if (CheckInteractRange(ws.Party.Player(), hints.InteractWithTarget))
        {
            // many eventobj interactions "immediately" start some cast animation (delayed by server roundtrip), and if we keep trying to move toward the target after sending the interact request, it will be canceled and force us to start over
            movementOverride.DesiredDirection = default;

            if (amex.EffectiveAnimationLock == 0 && ws.CurrentTime >= _throttleInteract)
            {
                FFXIVClientStructs.FFXIV.Client.Game.Control.TargetSystem.Instance()->InteractWithObject(GetActorObject(hints.InteractWithTarget), false);
                _throttleInteract = ws.FutureTime(0.1f);
            }
        }

        HandleFateSync();
        HandleQTE();
    }

    private unsafe void SetTarget(Actor? target, FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject** targetPtr)
    {
        if (target == null || !target.IsTargetable)
            return;

        var obj = GetActorObject(target);

        // 50 in-game units is the maximum distance before nameplates stop rendering
        // if you target something and then move further away, the client automatically untargets at 54.5 units
        // targeting a mob that isn't visible is bad UX
        if (ws.Party.Player() is { } player)
        {
            var distSq = (player.Position - target.Position).LengthSq();
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

        if (shouldDoSomething && ws.CurrentTime >= _throttleFateSync)
        {
            fm->LevelSync();
            _throttleFateSync = ws.FutureTime(0.5f);
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

    // "QTE" is a UI-less addon that handles communicating with the content director; it spawns one of a few different sub-addons based on what kind of gimmick it is
    // we could simply trigger the director event immediately (or send the "QTE solved" event immediately) when the mechanic starts, but in these cases the UI would continue ticking down until time expires and you "fail", which would be confusing
    // additionally, QTEKeep is meant to stay active for a fixed amount of time, so resolving it immediately (e.g. seat of sacrifice) would be obviously suspicious behavior
    // so instead we'll do some ui tricks that result in the same outcome as a successful mash
    // TODO: if we can find something to hook we can avoid iterating the entire unit list every frame, i feel like that's not a great idea (but GetAddonByName is already a linear search so it's not that much worse)
    private unsafe void HandleQTE()
    {
        var units = RaptureAtkUnitManager.Instance()->AllLoadedUnitsList;
        for (var i = 0; i < units.Count; i++)
        {
            var entry = units.Entries[i].Value;
            if (entry == null || !entry->IsReady || !entry->IsVisible)
                continue;

            if (MemoryExtensions.StartsWith(entry->Name, "_QTEMash"u8))
            {
                var mash = (AddonQTEMash*)entry;
                if (mash->State != 2)
                {
                    // just for visuals :3
                    mash->ProgressBar->SetGaugeValue(mash->ProgressBar->MaxValue, 0, true);
                    // sets internal state variable to 2 (= success), which triggers QTE (parent addon) success callback on next draw; also does ui teardown
                    mash->Complete();
                }
            }
            // _QTEKeep, _QTEKeepTime
            else if (MemoryExtensions.StartsWith(entry->Name, "_QTEKeep"u8))
            {
                ((AddonQTEKeep*)entry)->Progress = 100;
            }

            // TODO: other variants?
        }
    }
}
