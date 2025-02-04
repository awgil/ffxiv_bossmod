namespace BossMod.Autorotation;

public sealed class ClassRPRUtility(RotationModuleManager manager, Actor player) : RoleMeleeUtility(manager, player)
{
    public enum Track { ArcaneCrest = SharedTrack.Count, Ingress, Egress, Regress }
    public enum IngressStrategy { None, CharacterForward, CharacterBackward, CameraForward, CameraBackward }
    public enum EgressStrategy { None, CharacterForward, CharacterBackward, CameraForward, CameraBackward }
    public enum RegressStrategy { None, Use }

    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(RPR.AID.TheEnd);

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: RPR", "Cooldown Planner support for Utility Actions.\nNOTE: This is NOT a rotation preset! All Utility modules are STRICTLY for cooldown-planning usage.", "Utility for planner", "Akechi", RotationModuleQuality.Excellent, BitMask.Build((int)Class.RPR), 100);
        DefineShared(res, IDLimitBreak3);

        DefineSimpleConfig(res, Track.ArcaneCrest, "Crest", "", 600, RPR.AID.ArcaneCrest, 5);

        res.Define(Track.Ingress).As<IngressStrategy>("Hell's Ingress", "Ingress", 30)
            .AddOption(IngressStrategy.None, "None", "No use.", 0, 0, ActionTargets.Self, 20)
            .AddOption(IngressStrategy.CharacterForward, "CharacterForward", "Dashes in the Forward direction relative to the Character", 20, 10, ActionTargets.Self, 20)
            .AddOption(IngressStrategy.CharacterBackward, "CharacterBackward", "Dashes in the Backward direction relative to the Character", 20, 10, ActionTargets.Self, 20)
            .AddOption(IngressStrategy.CameraForward, "CameraForward", "Dashes in the Forward direction relative to the Camera", 20, 10, ActionTargets.Self, 20)
            .AddOption(IngressStrategy.CameraBackward, "CameraBackward", "Dashes in the Backward direction relative to the Camera", 20, 10, ActionTargets.Self, 20)
            .AddAssociatedActions(RPR.AID.HellsIngress);

        res.Define(Track.Egress).As<EgressStrategy>("Hell's Egress", "Egress", 30)
            .AddOption(EgressStrategy.None, "None", "No use.", 0, 0, ActionTargets.Self, 20)
            .AddOption(EgressStrategy.CharacterForward, "CharacterForward", "Dashes in the Forward direction relative to the Character", 20, 10, ActionTargets.Self, 20)
            .AddOption(EgressStrategy.CharacterBackward, "CharacterBackward", "Dashes in the Backward direction relative to the Character", 20, 10, ActionTargets.Self, 20)
            .AddOption(EgressStrategy.CameraForward, "CameraForward", "Dashes in the Forward direction relative to the Camera", 20, 10, ActionTargets.Self, 20)
            .AddOption(EgressStrategy.CameraBackward, "CameraBackward", "Dashes in the Backward direction relative to the Camera", 20, 10, ActionTargets.Self, 20)
            .AddAssociatedActions(RPR.AID.HellsEgress);

        res.Define(Track.Regress).As<RegressStrategy>("Regress", "Regress", 30)
            .AddOption(RegressStrategy.None, "None", "No use.", 0, 0, ActionTargets.Self, 20)
            .AddOption(RegressStrategy.Use, "Use", "Use Regress", 20, 10, ActionTargets.Self, 20)
            .AddAssociatedActions(RPR.AID.Regress);

        return res;
    }

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        ExecuteShared(strategy, IDLimitBreak3, primaryTarget);
        ExecuteSimple(strategy.Option(Track.ArcaneCrest), RPR.AID.ArcaneCrest, Player);

        var reg = strategy.Option(Track.Regress);
        var regStrat = strategy.Option(Track.Regress).As<RegressStrategy>();
        var zone = World.Actors.FirstOrDefault(x => x.OID == 0x4C3 && x.OwnerID == Player.InstanceID);
        if (regStrat != RegressStrategy.None)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(RPR.AID.Regress), Player, reg.Priority(), reg.Value.ExpireIn, targetPos: zone!.PosRot.XYZ());

        var ing = strategy.Option(Track.Ingress);
        if (ing.As<IngressStrategy>() != IngressStrategy.None && Player.FindStatus(RPR.SID.Threshold) == null)
        {
            var angle = ing.As<IngressStrategy>() switch
            {
                IngressStrategy.CharacterBackward => Player.Rotation + 180.Degrees(),
                IngressStrategy.CameraForward => World.Client.CameraAzimuth + 180.Degrees(),
                IngressStrategy.CameraBackward => World.Client.CameraAzimuth,
                _ => Player.Rotation
            };
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(RPR.AID.HellsIngress), Player, ing.Priority(), ing.Value.ExpireIn, facingAngle: angle);
        }

        var egg = strategy.Option(Track.Egress);
        if (egg.As<EgressStrategy>() != EgressStrategy.None && Player.FindStatus(RPR.SID.Threshold) == null)
        {
            var angle = egg.As<EgressStrategy>() switch
            {
                EgressStrategy.CharacterForward => Player.Rotation + 180.Degrees(),
                EgressStrategy.CameraBackward => World.Client.CameraAzimuth + 180.Degrees(),
                EgressStrategy.CameraForward => World.Client.CameraAzimuth,
                _ => Player.Rotation
            };
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(RPR.AID.HellsEgress), Player, egg.Priority(), egg.Value.ExpireIn, facingAngle: angle);
        }
    }
}
