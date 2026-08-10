namespace BossMod.Dawntrail.Trial.T03QueenEternal;

sealed class ProsecutionOfWar(BossModule module) : Components.SingleTargetCast(module, (uint)AID.ProsecutionOfWar);
sealed class VirtualShiftRoyalDomain(BossModule module) : Components.RaidwideCasts(module, [(uint)AID.VirtualShift1, (uint)AID.VirtualShift2, (uint)AID.VirtualShift3, (uint)AID.RoyalDomain]);
sealed class BrutalCrown(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BrutalCrown, new AOEShapeDonut(5f, 60f));
sealed class DynasticDiadem(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DynasticDiadem, new AOEShapeDonut(6f, 70f));
sealed class RoyalBanishment(BossModule module) : Components.SimpleAOEGroupsByTimewindow(module, [(uint)AID.RoyalBanishment], new AOEShapeCone(100f, 15f.Degrees()));

abstract class RaidwideMulti(BossModule module, uint aid) : Components.RaidwideCast(module, aid, "multiple Raidwides");
sealed class RoyalBanishmentRaidwide(BossModule module) : RaidwideMulti(module, (uint)AID.RoyalBanishmentVisual);
sealed class AbsoluteAuthorityRaidwide(BossModule module) : RaidwideMulti(module, (uint)AID.AbsoluteAuthorityRaidwide1);

sealed class T03QueenEternalStates : StateMachineBuilder
{
    public T03QueenEternalStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChanges>()
            .ActivateOnEnter<Besiegement>()
            .ActivateOnEnter<LegitimateForce>()
            .ActivateOnEnter<Aethertithe>()
            .ActivateOnEnter<WaltzOfTheRegalia>()
            .ActivateOnEnter<WaltzOfTheRegaliaBait>()
            .ActivateOnEnter<RuthlessRegalia>()
            .ActivateOnEnter<ProsecutionOfWar>()
            .ActivateOnEnter<VirtualShiftRoyalDomain>()
            .ActivateOnEnter<AbsoluteAuthorityRaidwide>()
            .ActivateOnEnter<DownburstKB>()
            .ActivateOnEnter<PowerfulGustKB>()
            .ActivateOnEnter<PowerfulGustDownburstRW>()
            .ActivateOnEnter<BrutalCrown>()
            .ActivateOnEnter<AbsoluteAuthorityCircle>()
            .ActivateOnEnter<AuthoritysGaze>()
            .ActivateOnEnter<AuthoritysHold>()
            .ActivateOnEnter<AbsoluteAuthorityDorito>()
            .ActivateOnEnter<AbsoluteAuthorityFlare>()
            .ActivateOnEnter<DynasticDiadem>()
            .ActivateOnEnter<DivideAndConquer>()
            .ActivateOnEnter<RoyalBanishment>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 984, NameID = 13029)]
public sealed class T03QueenEternal(WorldState ws, Actor primary) : BossModule(ws, primary, new(100f, 100f), new ArenaBoundsSquare(20f))
{
    public static readonly ArenaBoundsRect FinalBounds = new(20f, 15f), SplitGravityBounds = new(12f, 8f);

    public static Shape[] GetXArenaRects() => [new Rectangle(new(100f, 82.5f), 12.5f, 2.5f), new Rectangle(new(100f, 102.5f), 12.5f, 2.5f),
    new Cross(new(100f, 92.5f), 15f, 2.5f, 45f.Degrees())];
    public static ArenaBoundsCustom GetXArena() => new(GetXArenaRects());
    public static Rectangle[] GetSplitArenaRects() => [new Rectangle(new(108f, 94f), 4f, 8f), new Rectangle(new(92f, 94f), 4f, 8f)];
    public static ArenaBoundsCustom GetSplitArena() => new(GetSplitArenaRects());
}
