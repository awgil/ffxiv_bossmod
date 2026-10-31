namespace BossMod.Autorotation.MiscAI;

public sealed class PassiveAggressive(RotationModuleManager manager, Actor player) : RotationModule(manager, player)
{
    public enum Track { Pull, GCD }
    public enum Flag { Enabled, Disabled }

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Passive aggression", "Pull mobs without targeting them", "AI", "xan", RotationModuleQuality.Basic, new(~0ul), 1000, 1);

        res.Define(Track.Pull).As<Flag>("Pull")
            .AddOption(Flag.Enabled, "Pull priority mobs")
            .AddOption(Flag.Disabled, "Do nothing");

        res.Define(Track.GCD).As<Flag>("Pull with GCDs")
            .AddOption(Flag.Enabled, "Allow GCD usage")
            .AddOption(Flag.Disabled, "Use OGCDs only");

        return res;
    }

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        if (strategy.Option(Track.Pull).As<Flag>() != Flag.Enabled)
            return;

        float maxRange;

        if (Player.Role is Role.Healer or Role.Ranged)
            maxRange = 25;
        else if (Player.Class is Class.MNK or Class.PGL) // lmao
            maxRange = 3;
        else
            maxRange = 20;

        if (Hints.PotentialTargets.Where(p => p.Priority >= 0 && !p.Actor.AggroPlayer && !p.Actor.IncomingEffects.Any(e => e.SourceInstanceId == Player.InstanceID)).MinBy(p => (p.Actor.Position - Player.Position).LengthSq()) is not { } target)
            return;

        Hints.GoalZones.Add(AIHints.GoalSingleTarget(target.Actor.Position, target.Actor.HitboxRadius + maxRange + Player.HitboxRadius, 20));

        var actionRequested = false;

        if (Player.Role == Role.Tank)
            actionRequested |= UseOGCD(ClassShared.AID.Provoke, target);

        if (Player.Class.GetClassCategory() == ClassCategory.PhysRanged)
        {
            actionRequested |= UseOGCD(ClassShared.AID.HeadGraze, target);
            actionRequested |= UseOGCD(ClassShared.AID.LegGraze, target);
            actionRequested |= UseOGCD(ClassShared.AID.FootGraze, target);

            if (Player.Class == Class.MCH)
                actionRequested |= UseOGCD(MCH.AID.Dismantle, target);
        }

        if (Player.Class.GetClassCategory() == ClassCategory.Caster)
            actionRequested |= UseOGCD(ClassShared.AID.Addle, target);

        if (!actionRequested && strategy.Option(Track.GCD).As<Flag>() == Flag.Enabled)
            PullWithGCD(target, ref primaryTarget);
    }

    bool UseOGCD<AID>(AID action, AIHints.Enemy actor) where AID : Enum
    {
        if (ActionDefinitions.Instance.Spell(action) is not { } def)
            return false;

        if (def.ReadyIn(World.Client.Cooldowns, World.Client.DutyActions) <= GCD)
        {
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(action), actor.Actor, ActionQueue.Priority.Medium);
            return true;
        }

        return false;
    }

    void UseGCD<AID>(AID action, AIHints.Enemy actor) where AID : Enum
    {
        Hints.ActionsToExecute.Push(ActionID.MakeSpell(action), actor.Actor, ActionQueue.Priority.VeryHigh);
    }

    void PullWithGCD(AIHints.Enemy target, ref Actor? primaryTarget)
    {
        switch (Player.Class)
        {
            case Class.PLD:
            case Class.GLA:
                UseGCD(PLD.AID.ShieldLob, target);
                break;
            case Class.WAR:
            case Class.MRD:
                UseGCD(WAR.AID.Tomahawk, target);
                break;
            case Class.DRK:
                UseGCD(DRK.AID.Unmend, target);
                break;
            case Class.GNB:
                UseGCD(GNB.AID.LightningShot, target);
                break;

            case Class.DRG:
            case Class.LNC:
                UseGCD(DRG.AID.PiercingTalon, target);
                break;
            case Class.NIN:
            case Class.ROG:
                UseGCD(NIN.AID.ThrowingDagger, target);
                break;
            case Class.SAM:
                UseGCD(SAM.AID.Enpi, target);
                break;
            case Class.RPR:
                UseGCD(RPR.AID.Harpe, target);
                break;
            case Class.VPR:
                UseGCD(VPR.AID.WrithingSnap, target);
                break;

            case Class.WHM:
                UseGCD(WHM.AID.Aero, target);
                break;
            case Class.SCH:
                UseGCD(SCH.AID.Bio1, target);
                break;
            case Class.AST:
                UseGCD(AST.AID.Combust, target);
                break;
            case Class.SGE:
                UseGCD(SGE.AID.Dosis, target);
                break;

            case Class.MNK:
            case Class.PGL:
            case Class.BRD:
            case Class.ARC:
            case Class.MCH:
            case Class.DNC:
            case Class.THM:
            case Class.BLM:
            case Class.SMN:
            case Class.ACN:
            case Class.RDM:
            case Class.PCT:
                primaryTarget = target.Actor;
                break;
        }
    }
}
