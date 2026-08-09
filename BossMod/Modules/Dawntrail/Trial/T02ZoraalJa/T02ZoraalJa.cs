namespace BossMod.Dawntrail.Trial.T02ZoraalJa;

sealed class SoulOverflowCalamitysEdge(BossModule module) : Components.RaidwideCasts(module, [(uint)AID.SoulOverflow1, (uint)AID.SoulOverflow1, (uint)AID.CalamitysEdge]);
sealed class PatricidalPique(BossModule module) : Components.SingleTargetCast(module, (uint)AID.PatricidalPique);
sealed class Burst(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Burst, 8f);

sealed class VorpalTrail(BossModule module) : Components.SimpleChargeAOEGroups(module, [(uint)AID.VorpalTrail1, (uint)AID.VorpalTrail2], 2f);

sealed class T02ZoraalJaStates : StateMachineBuilder
{
    public T02ZoraalJaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SoulOverflowCalamitysEdge>()
            .ActivateOnEnter<DoubleEdgedSwords>()
            .ActivateOnEnter<PatricidalPique>()
            .ActivateOnEnter<Burst>()
            .ActivateOnEnter<VorpalTrail>()
            .Raw.Update = () => module.PrimaryActor.IsDeadOrDestroyed || !module.PrimaryActor.IsTargetable;
    }
}

public abstract class ZoraalJa(WorldState ws, Actor primary) : BossModule(ws, primary, new(100f, 100f), GetDefaultBounds())
{
    public static ArenaBoundsSquare GetDefaultBounds() => new(20f, 45f.Degrees());
}

[ModuleInfo(BossModuleInfo.Maturity.AISupport, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 995u, NameID = 12881u, SortOrder = 1)]
public sealed class T02ZoraalJa(WorldState ws, Actor primary) : ZoraalJa(ws, primary);
