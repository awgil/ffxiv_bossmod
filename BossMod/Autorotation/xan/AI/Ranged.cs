namespace BossMod.Autorotation.xan;

public class RangedAI(RotationModuleManager manager, Actor player) : AIBase(manager, player)
{
    private DateTime _pelotonLockout = DateTime.MinValue;
    private bool _hadPeloton;

    public enum Track { Peloton, Interrupt, SecondWind, LimitBreak }
    public static RotationModuleDefinition Definition()
    {
        var def = new RotationModuleDefinition("Phys Ranged AI", "Utilities for physical ranged dps - peloton, interrupt, defensive abilities", "xan", RotationModuleQuality.Basic, BitMask.Build(Class.ARC, Class.BRD, Class.MCH, Class.DNC), 100);

        def.AbilityTrack(Track.Peloton, "Peloton").AddAssociatedActions(ClassShared.AID.Peloton);
        def.AbilityTrack(Track.Interrupt, "Head Graze").AddAssociatedActions(ClassShared.AID.HeadGraze);
        def.AbilityTrack(Track.SecondWind, "Second Wind").AddAssociatedActions(ClassShared.AID.SecondWind);
        def.AbilityTrack(Track.LimitBreak, "Limit Break").AddAssociatedActions(ClassShared.AID.Desperado, ClassShared.AID.BigShot);

        return def;
    }

    public override void Execute(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay, float forceMovementIn, bool isMoving)
    {
        if (Player.InCombat || !isMoving)
            _pelotonLockout = World.CurrentTime.AddSeconds(1.5f);

        var peloton = PelotonDuration(Player);
        if (peloton == 0 && _hadPeloton)
        {
            // the peloton status disappeared, which means we entered combat - there doesn't seem to be any server communication that indicates that peloton disappeared *because* of combat
            _pelotonLockout = World.CurrentTime.AddSeconds(1.5f);
        }

        _hadPeloton = peloton > 1;

        // interrupt
        if (strategy.Enabled(Track.Interrupt) && NextChargeIn(ClassShared.AID.HeadGraze) == 0)
        {
            var interruptibleEnemy = Hints.PotentialTargets.FirstOrDefault(e => ShouldInterrupt(e.Actor) && Player.DistanceToHitbox(e.Actor) <= 25);
            if (interruptibleEnemy != null)
                Hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.HeadGraze), interruptibleEnemy.Actor, ActionQueue.Priority.Minimal);
        }

        // peloton
        if (strategy.Enabled(Track.Peloton)
            && World.CurrentTime > _pelotonLockout
            // if player is targeting npc (fate npc, vendor, etc) we assume they want to interact with target;
            // peloton animationlock will be annoying and unhelpful here
            // we use TargetManager because most friendly NPCs aren't Actors (or something)
            && Service.TargetManager.Target == null
            && peloton < 5)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.Peloton), Player, ActionQueue.Priority.Minimal);

        // second wind
        if (strategy.Enabled(Track.SecondWind) && Player.InCombat && HPRatio() <= 0.5)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.SecondWind), Player, ActionQueue.Priority.Medium);

        ExecLB(strategy, primaryTarget);

        if (ActionUnlocked(ActionID.MakeSpell(BossMod.BRD.AID.WardensPaean)) && NextChargeIn(BossMod.BRD.AID.WardensPaean) == 0 && ActionDefinitions.FindEsunaTarget(World) is Actor tar)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(BossMod.BRD.AID.WardensPaean), tar, ActionQueue.Priority.Low);
    }

    private unsafe void ExecLB(StrategyValues strategy, Actor? primaryTarget)
    {
        Actor? lbTarget(float halfWidth) => FindBetterTargetBy(primaryTarget, 30, actor => Hints.NumPriorityTargetsInAOERect(Player.Position, Player.DirectionTo(actor), 30, halfWidth)).Target;

        if (!strategy.Enabled(Track.LimitBreak) || World.Party.WithoutSlot(includeDead: true).Count(x => x.Type == ActorType.Player) > 1)
            return;

        var bars = World.Party.LimitBreakLevel;
        switch (bars)
        {
            case 1:
                if (lbTarget(2) is Actor a)
                    Hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.BigShot), a, ActionQueue.Priority.VeryHigh);
                break;
            case 2:
                if (lbTarget(2.5f) is Actor b)
                    Hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.Desperado), b, ActionQueue.Priority.VeryHigh);
                break;
            case 3:
                var lb3 = Player.Class switch
                {
                    Class.ARC or Class.BRD => ClassBRDUtility.IDLimitBreak3,
                    Class.MCH => ClassMCHUtility.IDLimitBreak3,
                    Class.DNC => ClassDNCUtility.IDLimitBreak3,
                    _ => default
                };
                if (lbTarget(4) is Actor c && lb3 != default)
                    Hints.ActionsToExecute.Push(lb3, c, ActionQueue.Priority.VeryHigh);
                break;
        }
    }

    //private bool PelotonWillExpire(Actor actor) => PelotonDuration(actor) < 5;

    private float PelotonDuration(Actor actor)
    {
        if (World.PendingEffects.PendingStatus(actor.InstanceID, (uint)BossMod.BRD.SID.Peloton) != null)
            return 30;

        if (actor.FindStatus((uint)BossMod.BRD.SID.Peloton) is not ActorStatus st)
            return 0;

        return Math.Max((float)(st.ExpireAt - World.CurrentTime).TotalSeconds, 0);
    }
}
