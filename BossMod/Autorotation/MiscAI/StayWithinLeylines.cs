using static BossMod.ActionQueue;

namespace BossMod.Autorotation.MiscAI;

public sealed class StayWithinLeylines(RotationModuleManager manager, Actor player) : RotationModule(manager, player)
{
    public enum Tracks
    {
        UseRetrace
    }

    public enum RetraceDefinition
    {
        No = 0,
        Yes = 1,
    }

    public static RotationModuleDefinition Definition()
    {
        RotationModuleDefinition def = new("Misc AI: Stay within leylines when active", "Black Mage utility module.", "Misc", "Taurenkey", RotationModuleQuality.Basic, BitMask.Build(Class.BLM), 1000);

        var config = def.Define(Tracks.UseRetrace).As<RetraceDefinition>("Use Retrace", "Use Retrace");

        config.AddOption(RetraceDefinition.No, "No");
        config.AddOption(RetraceDefinition.Yes, "Yes");

        return def;
    }

    public override void Execute(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        bool InLeyLines = Player.FindStatus(738) != null;

        if (!InLeyLines)
        {
            var zone = World.Actors.FirstOrDefault(x => x.OID == 0x179 && x.OwnerID == Player.InstanceID);
            if (zone != null)
            {
                var retrace = ActionID.MakeSpell(BLM.AID.Retrace);
                var cd = ActionDefinitions.Instance.Spell(BLM.AID.Retrace)?.MainCooldownGroup;
                var strat = strategy.Option(Tracks.UseRetrace).As<RetraceDefinition>();
                //try Retrace First
                if (strat == RetraceDefinition.Yes && ActionUnlocked(retrace) && cd.HasValue && World.Client.Cooldowns[cd.Value].Elapsed <= 2f)
                    Hints.ActionsToExecute.Push(retrace, null, Priority.Low, targetPos: Player.PosRot.XYZ());
                else
                    Hints.GoalZones.Add(Hints.GoalSingleTarget(zone.Position, 1f));
            }
        }

    }
}
