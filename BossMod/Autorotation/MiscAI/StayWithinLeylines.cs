using BossMod.Autorotation.xan;

namespace BossMod.Autorotation.MiscAI;

public sealed class StayWithinLeylines(RotationModuleManager manager, Actor player) : RotationModule(manager, player)
{
    public enum Tracks
    {
        UseRetrace,
        UseBetweenTheLines,
        Goal
    }

    public enum RetraceDefinition
    {
        No = 0,
        Yes = 1,
    }

    public enum BetweenTheLinesDefinition
    {
        No = 0,
        Yes = 1,
    }

    public static RotationModuleDefinition Definition()
    {
        RotationModuleDefinition def = new("Misc AI: Stay within leylines when active", "Black Mage utility module.", "AI", "Taurenkey", RotationModuleQuality.Basic, BitMask.Build(Class.BLM), 1000);

        var retrace = def.Define(Tracks.UseRetrace).As<RetraceDefinition>("Use Retrace", "Use Retrace", renderer: typeof(DefaultOffRenderer));
        retrace.AddOption(RetraceDefinition.No);
        retrace.AddOption(RetraceDefinition.Yes);

        var btl = def.Define(Tracks.UseBetweenTheLines).As<BetweenTheLinesDefinition>("Use Between The Lines", "Use Between The Lines", renderer: typeof(DefaultOffRenderer));
        btl.AddOption(BetweenTheLinesDefinition.No);
        btl.AddOption(BetweenTheLinesDefinition.Yes);

        def.Define(Tracks.Goal).As<EnabledByDefault>("Goal", "Add goal zone to Leylines (for AI movement)", renderer: typeof(DefaultOnRenderer))
            .AddOption(EnabledByDefault.Enabled)
            .AddOption(EnabledByDefault.Disabled);

        return def;
    }

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        if (!Player.InCombat)
            return;

        if (Player.FindStatus(BLM.SID.CircleOfPower) != null)
            return;

        var zone = World.Actors.FirstOrDefault(x => x.OID == 0x179 && x.OwnerID == Player.InstanceID);
        if (zone == null)
            return;

        var retrace = ActionID.MakeSpell(BLM.AID.Retrace);
        var btl = ActionID.MakeSpell(BLM.AID.BetweenTheLines);
        var retraceCd = ActionDefinitions.Instance.Spell(BLM.AID.Retrace)?.MainCooldownGroup;
        var btlCd = ActionDefinitions.Instance.Spell(BLM.AID.BetweenTheLines)?.MainCooldownGroup;
        var retraceStrat = strategy.Option(Tracks.UseRetrace).As<RetraceDefinition>();
        var btlStrat = strategy.Option(Tracks.UseBetweenTheLines).As<BetweenTheLinesDefinition>();

        //BTL first, followed by retrace, then walk
        if (btlStrat == BetweenTheLinesDefinition.Yes && ActionUnlocked(btl) && btlCd.HasValue && World.Client.Cooldowns[btlCd.Value].Elapsed <= 2f && !isMoving)
            Hints.ActionsToExecute.Push(btl, Player, ActionQueue.Priority.Low, targetPos: zone.PosRot.XYZ());
        else if (retraceStrat == RetraceDefinition.Yes && ActionUnlocked(retrace) && retraceCd.HasValue && World.Client.Cooldowns[retraceCd.Value].Elapsed <= 2f && !isMoving)
            Hints.ActionsToExecute.Push(retrace, null, ActionQueue.Priority.Low, targetPos: Player.PosRot.XYZ());
        else if (strategy.Option(Tracks.Goal).As<EnabledByDefault>().IsEnabled())
            Hints.GoalZones.Add(Hints.GoalSingleTarget(zone.Position, 0.2f));
    }
}
