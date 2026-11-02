using FFXIVClientStructs.FFXIV.Client.Game.Gauge;

namespace BossMod.Autorotation.Utility;

public sealed class ClassWHMUtilityEx(RotationModuleManager manager, Actor player) : TypedRotationModule<ClassWHMUtilityEx.Strategy>(manager, player)
{
    public struct Strategy
    {
        [Track(Actions = [WHM.AID.CureII, WHM.AID.CureIII, WHM.AID.Benediction, WHM.AID.MedicaII, WHM.AID.Asylum, WHM.AID.AfflatusSolace, WHM.AID.AfflatusRapture])]
        public Track<HealStrategy> Heal;
    }

    public enum HealStrategy
    {
        [Option("Do nothing")]
        None,
        [Option("Ensure specified targets are full HP by plan entry end", Targets = ActionTargets.Self | ActionTargets.Party)]
        FullHeal
    }

    public static RotationModuleDefinition Definition()
    {
        return new RotationModuleDefinition("Utility: WHM (extra)", "Extra stuff for WHM", "Utility for planner", "xan", RotationModuleQuality.Ok, BitMask.Build(Class.WHM, Class.CNJ), 100).WithStrategies<Strategy>();
    }

    public override void Execute(in Strategy strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        Heal(strategy);
    }

    // TODO: would be nice to be able to estimate how much healing output we have
    void Heal(in Strategy strategy)
    {
        if (strategy.Heal.Value != HealStrategy.FullHeal)
            return;

        var medica = BestActionUnlocked(WHM.AID.MedicaIII, WHM.AID.MedicaII);
        var medicaStatus = medica == WHM.AID.MedicaIII ? WHM.SID.MedicaIII : WHM.SID.MedicaII;

        var gauge = World.Client.GetGauge<WhiteMageGauge>();

        var swift = Player.FindStatus(ClassShared.SID.Swiftcast, World.FutureTime(30))?.ExpireAt > World.FutureTime(GCD);

        var deadline = strategy.Heal.TrackRaw.ExpireIn;

        var targets = strategy.Heal.TrackRaw.Target == StrategyTarget.Automatic
            // tanks are constantly taking autos
            ? World.Party.WithoutSlot().Where(a => a.Class.GetRole() != Role.Tank)
            : Manager.ResolvePartyMembers(strategy.Heal.TrackRaw.Target, strategy.Heal.TrackRaw.TargetParam);

        var healTargets = targets.Where(t =>
        {
            if (!t.IsTargetable)
                return false;

            var predictedPlusRegen = (float)t.PendingHPClamped;
            if (t.FindStatus(medicaStatus, World.FutureTime(15)) is { } medica)
            {
                // pretend medica will last until deadline, since we can just recast it if we end up having a ton of time
                var remainingRegenTicks = MathF.Floor(deadline / 3);

                predictedPlusRegen += t.HPMP.MaxHP * 0.12f * remainingRegenTicks;

                //Service.Log($"{t.Name} {t.PendingHPClamped} will be at {predictedPlusRegen} ({predictedPlusRegen / t.HPMP.MaxHP * 100}%) HP in {deadline} seconds after {remainingRegenTicks} ticks");
            }

            return predictedPlusRegen < t.HPMP.MaxHP;
        });

        // C3 is a gain on 2
        var (c3target, numtargets) = healTargets.Select(t => (t, healTargets.InRadius(t.Position, 10).Count())).MaxBy(t => t.Item2);

        if (deadline >= 12 && numtargets > 0)
        {
            var numMedica = healTargets.Count(t => t.Position.InCircle(Player.Position, 20) && t.FindStatus(medicaStatus, World.FutureTime(15)) == null);
            if (numMedica >= numtargets)
                Hints.ActionsToExecute.Push(ActionID.MakeSpell(medica), Player, ActionQueue.Priority.VeryHigh, castTime: swift ? 0 : 2);
        }

        if (numtargets > 1)
        {
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(WHM.AID.ThinAir), Player, ActionQueue.Priority.Medium);
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(WHM.AID.CureIII), c3target, ActionQueue.Priority.VeryHigh, castTime: swift ? 0 : 2);
        }

        if (healTargets.MinBy(t => t.PendingHPClamped) is { } lowest)
        {
            if (gauge.Lily > 0)
                Hints.ActionsToExecute.Push(ActionID.MakeSpell(WHM.AID.AfflatusSolace), lowest, ActionQueue.Priority.VeryHigh);

            Hints.ActionsToExecute.Push(ActionID.MakeSpell(WHM.AID.Tetragrammaton), lowest, ActionQueue.Priority.Medium);

            Hints.ActionsToExecute.Push(ActionID.MakeSpell(WHM.AID.CureII), lowest, ActionQueue.Priority.VeryHigh, castTime: swift ? 0 : 2);

            if (deadline > World.Client.AnimationLock && deadline <= GCD)
                Hints.ActionsToExecute.Push(ActionID.MakeSpell(WHM.AID.Benediction), lowest, ActionQueue.Priority.Medium);
        }
    }
}
