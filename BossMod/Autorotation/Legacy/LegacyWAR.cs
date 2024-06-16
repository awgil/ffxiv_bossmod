using FFXIVClientStructs.FFXIV.Client.Game;

namespace BossMod.Autorotation.Legacy;

public sealed record class LegacyWAR(WorldState World, Actor Player, AIHints Hints) : RotationModule(World, Player, Hints)
{
    public enum Track { AOE, GCD, Infuriate, Potion, InnerRelease, Upheaval, PrimalRend, Onslaught }
    public enum AOEStrategy { SingleTarget, ForceAOE, Auto, AutoFinishCombo }
    public enum GCDStrategy { Automatic, Spend, ConserveIfNoBuffs, Conserve, ForceExtendST, ForceSPCombo, TomahawkIfNotInMelee, ComboFitBeforeDowntime, PenultimateComboThenSpend, ForceSpend }
    public enum InfuriateStrategy { Automatic, Delay, ForceIfNoNC, AutoUnlessIR, ForceIfChargesCapping }
    public enum PotionStrategy { Manual, Immediate, DelayUntilRaidBuffs, Force }
    public enum OffensiveStrategy { Automatic, Delay, Force }
    public enum OnslaughtStrategy { Automatic, Forbid, NoReserve, Force, ForceReserve, ReserveTwo, UseOutsideMelee }

    public static RotationModuleDefinition Definition()
    {
        // TODO: think about target overrides where they make sense (ST stuff, esp things like onslaught?)
        var res = new RotationModuleDefinition("Legacy WAR", "Old pre-refactoring module", BitMask.Build((int)Class.WAR), 90);

        var aoe = res.AddConfig(Track.AOE, new("AOE", UIPriority: 90));
        aoe.AddOption(AOEStrategy.SingleTarget, new(0x80ffffff, ActionTargets.None, "ST", "Use single-target rotation"));
        aoe.AddOption(AOEStrategy.ForceAOE, new(0x8000ffff, ActionTargets.None, "AOE", "Use aoe rotation"));
        aoe.AddOption(AOEStrategy.Auto, new(0x8000ff00, ActionTargets.None, "Auto", "Use aoe rotation if 3+ targets would be hit, otherwise use single-target rotation; break combo if necessary"));
        aoe.AddOption(AOEStrategy.AutoFinishCombo, new(0x80ff00ff, ActionTargets.None, "AutoFinishCombo", "Use aoe rotation if 3+ targets would be hit, otherwise use single-target rotation; finish combo route before switching"));

        var gcd = res.AddConfig(Track.GCD, new("Gauge", "GCD", UIPriority: 80));
        gcd.AddOption(GCDStrategy.Automatic, new(0x80ffffff, ActionTargets.None, "Automatic")); // spend gauge either under raid buffs or if next downtime is soon (so that next raid buff window won't cover at least 4 GCDs); TODO reconsider...
        gcd.AddOption(GCDStrategy.Spend, new(0x8000ff00, ActionTargets.None, "Spend", "Spend gauge freely, ensure ST is properly maintained"));
        gcd.AddOption(GCDStrategy.ConserveIfNoBuffs, new(0x8000ffff, ActionTargets.None, "ConserveIfNoBuffs", "Conserve unless under raid buffs"));
        gcd.AddOption(GCDStrategy.Conserve, new(0x800000ff, ActionTargets.None, "Conserve", "Conserve as much as possible"));
        gcd.AddOption(GCDStrategy.ForceExtendST, new(0x80ff00ff, ActionTargets.None, "ForceExtendST", "Force extend ST buff, potentially overcapping gauge and/or ST"));
        gcd.AddOption(GCDStrategy.ForceSPCombo, new(0x80ff0080, ActionTargets.None, "ForceSPCombo", "Force SP combo, potentially overcapping gauge"));
        gcd.AddOption(GCDStrategy.TomahawkIfNotInMelee, new(0x80c08000, ActionTargets.None, "TomahawkIfNotInMelee", "Use tomahawk if outside melee"));
        gcd.AddOption(GCDStrategy.ComboFitBeforeDowntime, new(0x80c0c000, ActionTargets.None, "ComboFitBeforeDowntime", "Use combo, unless it can't be finished before downtime and unless gauge and/or ST would overcap"));
        gcd.AddOption(GCDStrategy.PenultimateComboThenSpend, new(0x80400080, ActionTargets.None, "PenultimateComboThenSpend", "Use combo until second-last step, then spend gauge"));
        gcd.AddOption(GCDStrategy.ForceSpend, new(0x8000ffc0, ActionTargets.None, "ForceSpend", "Force gauge spender if possible, even if ST is not up/running out soon"));

        var inf = res.AddConfig(Track.Infuriate, new("Infuriate", UIPriority: 70));
        inf.AddOption(InfuriateStrategy.Automatic, new(0x80ffffff, ActionTargets.None, "Automatic", "Try to delay uses until raidbuffs, avoiding overcap"));
        inf.AddOption(InfuriateStrategy.Delay, new(0x800000ff, ActionTargets.None, "Delay", "Delay, even if risking overcap"));
        inf.AddOption(InfuriateStrategy.ForceIfNoNC, new(0x8000ff00, ActionTargets.None, "ForceIfNoNC", "Force unless NC active"));
        inf.AddOption(InfuriateStrategy.AutoUnlessIR, new(0x8000ffff, ActionTargets.None, "AutoUnlessIR", "Use normally, but not during IR"));
        inf.AddOption(InfuriateStrategy.ForceIfChargesCapping, new(0x8000ff80, ActionTargets.None, "ForceIfChargesCapping", "Force use if charges are about to overcap (unless NC is already active), even if it would overcap gauge"));

        var pot = res.AddConfig(Track.Potion, new("Potion", UIPriority: 60));
        pot.AddOption(PotionStrategy.Manual, new(0x80ffffff, ActionTargets.None, "Manual", "Do not use automatically"));
        pot.AddOption(PotionStrategy.Immediate, new(0x8000ff00, ActionTargets.None, "Immediate", "Use ASAP, but delay slightly during opener", 270, 30));
        pot.AddOption(PotionStrategy.DelayUntilRaidBuffs, new(0x8000ffff, ActionTargets.None, "DelayUntilRaidBuffs", "Delay until raidbuffs", 270, 30));
        pot.AddOption(PotionStrategy.Force, new(0x800000ff, ActionTargets.None, "Force", "Use ASAP, even if without ST", 270, 30));

        var ir = res.AddConfig(Track.InnerRelease, new("IR", UIPriority: 50));
        ir.AddOption(OffensiveStrategy.Automatic, new(0x80ffffff, ActionTargets.None, "Automatic", "Use normally"));
        ir.AddOption(OffensiveStrategy.Delay, new(0x800000ff, ActionTargets.None, "Delay", "Delay"));
        ir.AddOption(OffensiveStrategy.Force, new(0x8000ff00, ActionTargets.None, "Force", "Force use ASAP (even during downtime or without ST)"));

        var uph = res.AddConfig(Track.Upheaval, new("Upheaval", UIPriority: 40));
        uph.AddOption(OffensiveStrategy.Automatic, new(0x80ffffff, ActionTargets.None, "Automatic", "Use normally"));
        uph.AddOption(OffensiveStrategy.Delay, new(0x800000ff, ActionTargets.None, "Delay", "Delay"));
        uph.AddOption(OffensiveStrategy.Force, new(0x8000ff00, ActionTargets.None, "Force", "Force use ASAP (even without ST)"));

        var pr = res.AddConfig(Track.Upheaval, new("PR", UIPriority: 30));
        pr.AddOption(OffensiveStrategy.Automatic, new(0x80ffffff, ActionTargets.None, "Automatic", "Use normally"));
        pr.AddOption(OffensiveStrategy.Delay, new(0x800000ff, ActionTargets.None, "Delay", "Delay"));
        pr.AddOption(OffensiveStrategy.Force, new(0x8000ff00, ActionTargets.None, "Force", "Force use ASAP (do not delay to raidbuffs)"));

        var onsl = res.AddConfig(Track.Onslaught, new("Onslaught", UIPriority: 20));
        onsl.AddOption(OnslaughtStrategy.Automatic, new(0x80ffffff, ActionTargets.None, "Automatic", "Always keep one charge reserved, use other charges under raidbuffs or to prevent overcapping"));
        onsl.AddOption(OnslaughtStrategy.Forbid, new(0x800000ff, ActionTargets.None, "Forbid", "Forbid automatic use"));
        onsl.AddOption(OnslaughtStrategy.NoReserve, new(0x8000ffff, ActionTargets.None, "NoReserve", "Do not reserve charges: use all charges if under raidbuffs, otherwise use as needed to prevent overcapping"));
        onsl.AddOption(OnslaughtStrategy.Force, new(0x8000ff00, ActionTargets.None, "Force", "Use all charges ASAP"));
        onsl.AddOption(OnslaughtStrategy.ForceReserve, new(0x80ff0000, ActionTargets.None, "ForceReserve", "Use all charges except one ASAP"));
        onsl.AddOption(OnslaughtStrategy.ReserveTwo, new(0x80ffff00, ActionTargets.None, "ReserveTwo", "Reserve 2 charges, trying to prevent overcap"));
        onsl.AddOption(OnslaughtStrategy.UseOutsideMelee, new(0x80ff00ff, ActionTargets.None, "UseOutsideMelee", "Use as gapcloser if outside melee range"));

        // TODO: consider these:
        //public bool Aggressive; // if true, we use buffs and stuff at last possible moment; otherwise we make sure to keep at least 1 GCD safety net
        //public bool OnslaughtHeadroom; // if true, consider onslaught to have slightly higher animation lock than in reality, to account for potential small movement animation delay

        return res;
    }

    // full state needed for determining next action
    public class State(WorldState ws, Actor player) : CommonState(ws, player)
    {
        public int Gauge; // 0 to 100
        public float SurgingTempestLeft; // 0 if buff not up, max 60
        public float NascentChaosLeft; // 0 if buff not up, max 30
        public float PrimalRendLeft; // 0 if buff not up, max 30
        public float InnerReleaseLeft; // 0 if buff not up, max 15
        public int InnerReleaseStacks; // 0 if buff not up, max 3

        // upgrade paths
        public AID BestFellCleave => NascentChaosLeft > GCD && Unlocked(AID.InnerChaos) ? AID.InnerChaos : Unlocked(AID.FellCleave) ? AID.FellCleave : AID.InnerBeast;
        public AID BestDecimate => NascentChaosLeft > GCD ? AID.ChaoticCyclone : Unlocked(AID.Decimate) ? AID.Decimate : AID.SteelCyclone;
        public AID BestInnerRelease => Unlocked(AID.InnerRelease) ? AID.InnerRelease : AID.Berserk;
        public AID BestBloodwhetting => Unlocked(AID.Bloodwhetting) ? AID.Bloodwhetting : AID.RawIntuition;

        public AID ComboLastMove => (AID)ComboLastAction;
        //public float InnerReleaseCD => CD(UnlockedInnerRelease ? AID.InnerRelease : AID.Berserk); // note: technically berserk and IR don't share CD, and with level sync you can have both...

        public bool Unlocked(AID aid) => Definitions.Unlocked(aid, Level, UnlockProgress);
        public bool Unlocked(TraitID tid) => Definitions.Unlocked(tid, Level, UnlockProgress);

        public override string ToString()
        {
            return $"g={Gauge}, RB={RaidBuffsLeft:f1}, ST={SurgingTempestLeft:f1}, NC={NascentChaosLeft:f1}, PR={PrimalRendLeft:f1}, IR={InnerReleaseStacks}/{InnerReleaseLeft:f1}, IRCD={CD(AID.Berserk):f1}/{CD(AID.InnerRelease):f1}, InfCD={CD(AID.Infuriate):f1}, UphCD={CD(AID.Upheaval):f1}, OnsCD={CD(AID.Onslaught):f1}, PotCD={PotionCD:f1}, GCD={GCD:f3}, ALock={AnimationLock:f3}+{AnimationLockDelay:f3}, lvl={Level}/{UnlockProgress}";
        }
    }

    private readonly State _state = new(World, Player);

    public override void Execute(ReadOnlySpan<StrategyValue> strategy, Actor? primaryTarget, ActionQueue actions)
    {
        var questProgress = _questLock.Progress();
        var combo = ComboLastMove;

        var aoe = (AOEStrategy)strategy[(int)Track.AOE].Option switch
        {
            AOEStrategy.ForceAOE => true,
            AOEStrategy.Auto => PreferAOE(),
            AOEStrategy.AutoFinishCombo => combo switch
            {
                WAR.AID.HeavySwing => !WAR.Definitions.Unlocked(WAR.AID.Maim, Player.Level, questProgress) && PreferAOE(),
                WAR.AID.Maim => !WAR.Definitions.Unlocked(WAR.AID.StormPath, Player.Level, questProgress) && PreferAOE(),
                WAR.AID.Overpower => WAR.Definitions.Unlocked(WAR.AID.MythrilTempest, Player.Level, questProgress) || PreferAOE(),
                _ => PreferAOE()
            },
            _ => false,
        };

        // UpdatePlayerState()
        FillCommonPlayerState(_state);
        _state.HaveTankStance = Player.FindStatus(SID.Defiance) != null;

        _state.Gauge = Service.JobGauges.Get<WARGauge>().BeastGauge;

        _state.SurgingTempestLeft = _state.NascentChaosLeft = _state.PrimalRendLeft = _state.InnerReleaseLeft = 0;
        _state.InnerReleaseStacks = 0;
        foreach (var status in Player.Statuses)
        {
            switch ((SID)status.ID)
            {
                case SID.SurgingTempest:
                    _state.SurgingTempestLeft = StatusDuration(status.ExpireAt);
                    break;
                case SID.NascentChaos:
                    _state.NascentChaosLeft = StatusDuration(status.ExpireAt);
                    break;
                case SID.Berserk:
                case SID.InnerRelease:
                    _state.InnerReleaseLeft = StatusDuration(status.ExpireAt);
                    _state.InnerReleaseStacks = status.Extra & 0xFF;
                    break;
                case SID.PrimalRend:
                    _state.PrimalRendLeft = StatusDuration(status.ExpireAt);
                    break;
            }
        }

        FillCommonStrategy(_strategy, ActionDefinitions.IDPotionStr);
        _strategy.ApplyStrategyOverrides(Autorot.Bossmods.ActiveModule?.PlanExecution?.ActiveStrategyOverrides(Autorot.Bossmods.ActiveModule.StateMachine) ?? []);
        _strategy.OnslaughtHeadroom = _config.OnslaughtHeadroom;

        actions.Push(ActionID.MakeSpell(WAR.AID.HeavySwing), primaryTarget, ActionQueue.Priority.High + 500);
    }

    private int NumTargetsHitByAOE() => Hints.NumPriorityTargetsInAOECircle(Player.Position, 5);
    private bool PreferAOE() => NumTargetsHitByAOE() >= 3;

    // TODO: rethink...
    private readonly QuestLockCheck _questLock = new(WAR.Definitions.UnlockQuests);

    // TODO: move into clientstate
    private unsafe WAR.AID ComboLastMove => (WAR.AID)(ActionManager.Instance()->Combo.Timer > 0 ? ActionManager.Instance()->Combo.Action : 0);
}
