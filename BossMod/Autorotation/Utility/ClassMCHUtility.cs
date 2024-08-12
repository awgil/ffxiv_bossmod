namespace BossMod.Autorotation;

public sealed class ClassMCHUtility(RotationModuleManager manager, Actor player) : RoleRangedUtility(manager, player)
{
    // Add all MCH tracks to end of list, starting with Tactician
    // SharedTrack.Count here is the "end" of the track list, so we set the first track we want as the "end"
    public enum Track { Tactician = SharedTrack.Count, Dismantle }

    // Add Machinist LB3
    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(MCH.AID.SatelliteBeam);

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: MCH", "Planner support for utility actions", "Aimsucks", RotationModuleQuality.WIP, BitMask.Build((int)Class.MCH), 100);
        DefineShared(res, IDLimitBreak3);

        // Add track for Tactician: 15s long, 15% player damage received reduction
        DefineSimpleConfig(res, Track.Tactician, "Tactician", "Tactician", 400, MCH.AID.Tactician);

        // Add track for Dismantle: 10s long, 10% target damage reduction
        DefineSimpleConfig(res, Track.Dismantle, "Dismantle", "Dismantle", 500, MCH.AID.Dismantle);

        return res;
    }

    public override void Execute(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay, float forceMovementIn, bool isMoving)
    {
        ExecuteShared(strategy, IDLimitBreak3);
        ExecuteSimple(strategy.Option(Track.Tactician), MCH.AID.Tactician, Player);
        ExecuteSimple(strategy.Option(Track.Dismantle), MCH.AID.Dismantle, Player);
    }
}
