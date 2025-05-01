using static BossMod.ActorCastEvent;

namespace BossMod.Autorotation.MiscAI;
public sealed class AvaricePositional(RotationModuleManager manager, Actor player) : RotationModule(manager, player)
{
    public static RotationModuleDefinition Definition()
    {
        RotationModuleDefinition def = new("Misc AI: Follow Avarice positionals", "Module for use with other rotation plugins.", "AI", "erdelf", RotationModuleQuality.Basic, new(~0ul), 1000);
        return def;
    }

    private uint[]? avaricePositionalStatus;

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        if (!Player.InCombat || primaryTarget is not { Omnidirectional: false } || Player.FindStatus(ClassShared.AID.TrueNorth) != null ||
            primaryTarget.TargetID == Player.InstanceID && primaryTarget.CastInfo == null && primaryTarget.NameID != 541)
        {
            return;
        }

        
        if(avaricePositionalStatus == null)
        {
            if (Service.PluginInterface?.TryGetData<uint[]>("Avarice.PositionalStatus", out var ret) ?? false)
            {
                avaricePositionalStatus = ret;
            }
            else
            {
                return;
            }
        }

        var positional = avaricePositionalStatus[1] switch
        {
            2 => Positional.Flank,
            1 => Positional.Rear,
            _ => Positional.Any
        };

        if (positional == Positional.Any)
        {
            return;
        }

        var correct = positional switch
        {
            Positional.Flank => MathF.Abs(primaryTarget.Rotation.ToDirection().Dot((Player.Position - primaryTarget.Position).Normalized())) < 0.7071067f,
            Positional.Rear => primaryTarget.Rotation.ToDirection().Dot((Player.Position - primaryTarget.Position).Normalized())             < -0.7071068f
        };
            
        Hints.RecommendedPositional = (primaryTarget, positional, true, correct);
        Hints.GoalZones.Add(Hints.GoalSingleTarget(primaryTarget, positional));
    }
}
