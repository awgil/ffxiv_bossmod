using FFXIVClientStructs.FFXIV.Client.Game.Gauge;

namespace BossMod.Autorotation.Legacy;

public sealed class LegacyBRD : LegacyModule
{
    public enum Track { AOE, Songs, Potion, DOTs, ApexArrow, BlastArrow, RagingStrikes, Bloodletter, EmpyrealArrow, Barrage, Sidewinder }
    public enum AOEStrategy { SingleTarget, AutoTargetHitPrimary, AutoTargetHitMost, AutoOnPrimary, ForceAOE }
    public enum SongStrategy { Automatic, Extend, Overextend, ForceWM, ForceMB, ForceAP, ForcePP, Delay }
    public enum PotionStrategy { Manual, Burst, Force }
    public enum DotStrategy { Automatic, AutomaticExtendOnly, Forbid, ForceExtend, ExtendIgnoreBuffs, ExtendDelayed }
    public enum ApexArrowStrategy { Automatic, Delay, ForceAnyGauge, ForceHighGauge, ForceCapGauge }
    public enum OffensiveStrategy { Automatic, Delay, Force }
    public enum BloodletterStrategy { Automatic, Delay, Force, KeepOneCharge, KeepTwoCharges }

    public static RotationModuleDefinition Definition()
    {
        // TODO: think about target overrides where they make sense
        var res = new RotationModuleDefinition("Legacy BRD", "Old pre-refactoring module", "veyn", RotationModuleQuality.WIP, BitMask.Build((int)Class.BRD), 100);

        res.Define(Track.AOE).As<AOEStrategy>("AOE", uiPriority: 110)
            .AddOption(AOEStrategy.SingleTarget, "ST", "Use single-target actions")
            .AddOption(AOEStrategy.AutoTargetHitPrimary, "AutoTargetHitPrimary", "Use aoe actions if profitable select best target that ensures primary target is hit")
            .AddOption(AOEStrategy.AutoTargetHitMost, "AutoTargetHitMost", "Use aoe actions if profitable select a target that ensures maximal number of targets are hit")
            .AddOption(AOEStrategy.AutoOnPrimary, "AutoOnPrimary", "Use aoe actions on primary target if profitable")
            .AddOption(AOEStrategy.ForceAOE, "AOE", "Use aoe rotation on primary target even if it's less total damage than single-target")
            .AddAssociatedActions(BRD.AID.QuickNock, BRD.AID.Ladonsbite, BRD.AID.RainOfDeath, BRD.AID.Shadowbite);

        res.Define(Track.Songs).As<SongStrategy>("Songs", uiPriority: 100)
            .AddOption(SongStrategy.Automatic, "Automatic")
            .AddOption(SongStrategy.Extend, "Extend", "Extend until last tick")
            .AddOption(SongStrategy.Overextend, "Overextend", "Extend until last possible moment")
            .AddOption(SongStrategy.ForceWM, "ForceWM", "Force switch to Wanderer's Minuet")
            .AddOption(SongStrategy.ForceMB, "ForceMB", "Force switch to Mage's Ballad")
            .AddOption(SongStrategy.ForceAP, "ForceAP", "Force switch to Army's Paeon")
            .AddOption(SongStrategy.ForcePP, "ForcePP", "Force Pitch Perfect (assuming WM is up)")
            .AddOption(SongStrategy.Delay, "Delay", "Do not use any songs; stay songless if needed")
            .AddAssociatedActions(BRD.AID.WanderersMinuet, BRD.AID.MagesBallad, BRD.AID.ArmysPaeon, BRD.AID.PitchPerfect);

        res.Define(Track.Potion).As<PotionStrategy>("Potion", uiPriority: 90)
            .AddOption(PotionStrategy.Manual, "Manual", "Do not use automatically")
            .AddOption(PotionStrategy.Burst, "Burst", "Use right before burst", 270, 30)
            .AddOption(PotionStrategy.Force, "Force", "Use ASAP", 270, 30)
            .AddAssociatedAction(ActionDefinitions.IDPotionDex);

        // TODO: think about multidotting, probably should be a separate track (default is primary only, think about interactions with ij etc)
        res.Define(Track.DOTs).As<DotStrategy>("DOTs", uiPriority: 80)
            .AddOption(DotStrategy.Automatic, "Automatic", "Apply dots asap, reapply when either dots about to expire or in buff window")
            .AddOption(DotStrategy.AutomaticExtendOnly, "AutomaticExtendOnly", "Do not apply new dots, extend existing normally with IJ")
            .AddOption(DotStrategy.Forbid, "Forbid", "Do not apply new or extend existing dots")
            .AddOption(DotStrategy.ForceExtend, "ForceExtend", "Force extend dots via IJ ASAP")
            .AddOption(DotStrategy.ExtendIgnoreBuffs, "ExtendIgnoreBuffs", "Extend dots via IJ only if they are about to fall off (but don't risk proc overwrites), don't extend early under buffs")
            .AddOption(DotStrategy.ExtendDelayed, "ExtendDelayed", "Extend dots via IJ at last possible moment, even if it might overwrite proc")
            .AddAssociatedActions(BRD.AID.VenomousBite, BRD.AID.Windbite, BRD.AID.IronJaws, BRD.AID.CausticBite, BRD.AID.Stormbite);

        res.Define(Track.ApexArrow).As<ApexArrowStrategy>("Apex", uiPriority: 70)
            .AddOption(ApexArrowStrategy.Automatic, "Automatic", "Use at 80+ if buffs are about to run off, use at 100 asap unless raid buffs are imminent")
            .AddOption(ApexArrowStrategy.Delay, "Delay", "Delay")
            .AddOption(ApexArrowStrategy.ForceAnyGauge, "ForceAnyGauge", "Force at any gauge (even if it means no BA)")
            .AddOption(ApexArrowStrategy.ForceHighGauge, "ForceHighGauge", "Force at 80+ gauge")
            .AddOption(ApexArrowStrategy.ForceCapGauge, "ForceCapGauge", "Force at 100 gauge (don't delay until raidbuffs)")
            .AddAssociatedActions(BRD.AID.ApexArrow);

        res.Define(Track.BlastArrow).As<OffensiveStrategy>("Blast", uiPriority: 60)
            .AddOption(OffensiveStrategy.Automatic, "Automatic", "Use normally")
            .AddOption(OffensiveStrategy.Delay, "Delay", "Delay")
            .AddOption(OffensiveStrategy.Force, "Force", "Force use ASAP")
            .AddAssociatedActions(BRD.AID.BlastArrow);

        res.Define(Track.RagingStrikes).As<OffensiveStrategy>("RS", uiPriority: 50)
            .AddOption(OffensiveStrategy.Automatic, "Automatic", "Use normally")
            .AddOption(OffensiveStrategy.Delay, "Delay", "Delay")
            .AddOption(OffensiveStrategy.Force, "Force", "Force use ASAP")
            .AddAssociatedActions(BRD.AID.RagingStrikes, BRD.AID.BattleVoice, BRD.AID.RadiantFinale);

        res.Define(Track.Bloodletter).As<BloodletterStrategy>("BL", uiPriority: 40)
            .AddOption(BloodletterStrategy.Automatic, "Automatic", "Pool for raid buffs, otherwise use freely")
            .AddOption(BloodletterStrategy.Delay, "Delay", "Do not use, allowing overcap")
            .AddOption(BloodletterStrategy.Force, "Force", "Force use all charges")
            .AddOption(BloodletterStrategy.KeepOneCharge, "KeepOneCharge", "Keep 1 charge, use if 2+ charges available")
            .AddOption(BloodletterStrategy.KeepTwoCharges, "KeepTwoCharges", "Keep 2 charges, use if overcap is imminent")
            .AddAssociatedActions(BRD.AID.Bloodletter, BRD.AID.RainOfDeath);

        res.Define(Track.EmpyrealArrow).As<OffensiveStrategy>("EA", uiPriority: 30)
            .AddOption(OffensiveStrategy.Automatic, "Automatic", "Use normally")
            .AddOption(OffensiveStrategy.Delay, "Delay", "Delay")
            .AddOption(OffensiveStrategy.Force, "Force", "Force use ASAP")
            .AddAssociatedActions(BRD.AID.EmpyrealArrow);

        res.Define(Track.Barrage).As<OffensiveStrategy>("Barrage", uiPriority: 20)
            .AddOption(OffensiveStrategy.Automatic, "Automatic", "Use normally")
            .AddOption(OffensiveStrategy.Delay, "Delay", "Delay")
            .AddOption(OffensiveStrategy.Force, "Force", "Force use ASAP")
            .AddAssociatedActions(BRD.AID.Barrage);

        res.Define(Track.Sidewinder).As<OffensiveStrategy>("SW", uiPriority: 10)
            .AddOption(OffensiveStrategy.Automatic, "Automatic", "Use normally")
            .AddOption(OffensiveStrategy.Delay, "Delay", "Delay")
            .AddOption(OffensiveStrategy.Force, "Force", "Force use ASAP")
            .AddAssociatedActions(BRD.AID.Sidewinder);

        return res;
    }

    public enum Song { None, MagesBallad, ArmysPaeon, WanderersMinuet }

    // full state needed for determining next action
    public class State(RotationModule module) : CommonState(module)
    {
        public Song ActiveSong;
        public float ActiveSongLeft; // 45 max
        public int Repertoire;
        public int SoulVoice;
        public int NumCoda;
        public float StraightShotLeft;
        public float BlastArrowLeft;
        public float ShadowbiteLeft;
        public float RagingStrikesLeft;
        public float BattleVoiceLeft;
        public float RadiantFinaleLeft;
        public float ArmysMuseLeft;
        public float BarrageLeft;
        public float PelotonLeft; // 30 max
        public float TargetCausticLeft;
        public float TargetStormbiteLeft;
        public Actor? BestLadonsbiteTarget;
        public int NumLadonsbiteTargets; // range 12 90-degree cone
        public Actor? BestRainOfDeathTarget;
        public int NumRainOfDeathTargets; // range 8 circle around target

        // upgrade paths
        public BRD.AID BestBurstShot => Unlocked(BRD.AID.BurstShot) ? BRD.AID.BurstShot : BRD.AID.HeavyShot;
        public BRD.AID BestRefulgentArrow => Unlocked(BRD.AID.RefulgentArrow) ? BRD.AID.RefulgentArrow : BRD.AID.StraightShot;
        public BRD.AID BestCausticBite => Unlocked(BRD.AID.CausticBite) ? BRD.AID.CausticBite : BRD.AID.VenomousBite;
        public BRD.AID BestStormbite => Unlocked(BRD.AID.Stormbite) ? BRD.AID.Stormbite : BRD.AID.Windbite;
        public BRD.AID BestLadonsbite => Unlocked(BRD.AID.Ladonsbite) ? BRD.AID.Ladonsbite : BRD.AID.QuickNock;

        // statuses
        public BRD.SID ExpectedCaustic => Unlocked(BRD.AID.CausticBite) ? BRD.SID.CausticBite : BRD.SID.VenomousBite;
        public BRD.SID ExpectedStormbite => Unlocked(BRD.AID.Stormbite) ? BRD.SID.Stormbite : BRD.SID.Windbite;

        public bool Unlocked(BRD.AID aid) => Module.ActionUnlocked(ActionID.MakeSpell(aid));
        public bool Unlocked(BRD.TraitID tid) => Module.TraitUnlocked((uint)tid);

        public override string ToString()
        {
            return $"g={ActiveSong}/{ActiveSongLeft:f3}/{Repertoire}/{SoulVoice}/{NumCoda}, AOE={NumRainOfDeathTargets}/{NumLadonsbiteTargets}, no-dots={ForbidDOTs}, RB={RaidBuffsLeft:f3}, SS={StraightShotLeft:f3}, BA={BlastArrowLeft:f3}, SB={ShadowbiteLeft:f3}, Buffs={RagingStrikesLeft:f3}/{BattleVoiceLeft:f3}/{RadiantFinaleLeft:f3}, Muse={ArmysMuseLeft:f3}, Barr={BarrageLeft:f3}, Dots={TargetStormbiteLeft:f3}/{TargetCausticLeft:f3}, PotCD={PotionCD:f3}, BVCD={CD(BRD.AID.BattleVoice):f3}, BLCD={CD(BRD.AID.Bloodletter):f3}, GCD={GCD:f3}, ALock={AnimationLock:f3}+{AnimationLockDelay:f3}, lvl={Level}";
        }
    }

    private readonly State _state;

    public LegacyBRD(RotationModuleManager manager, Actor player) : base(manager, player)
    {
        _state = new(this);
    }

    public override void Execute(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay, float forceMovementIn, bool isMoving)
    {
        _state.UpdateCommon(primaryTarget, estimatedAnimLockDelay);
        if (_state.AnimationLockDelay < 0.1f)
            _state.AnimationLockDelay = 0.1f; // TODO: reconsider; we generally don't want triple weaves or extra-late proc weaves

        var gauge = World.Client.GetGauge<BardGauge>();
        _state.ActiveSong = (Song)((byte)gauge.SongFlags & 3);
        _state.ActiveSongLeft = gauge.SongTimer * 0.001f;
        _state.Repertoire = gauge.Repertoire;
        _state.SoulVoice = gauge.SoulVoice;
        _state.NumCoda = BitOperations.PopCount((uint)gauge.SongFlags & 0x70);

        _state.StraightShotLeft = _state.StatusDetails(Player, BRD.SID.HawksEye, Player.InstanceID, 30).Left;
        _state.BlastArrowLeft = _state.StatusDetails(Player, BRD.SID.BlastArrowReady, Player.InstanceID, 10).Left;
        _state.ShadowbiteLeft = _state.StatusDetails(Player, BRD.SID.ShadowbiteReady, Player.InstanceID, 30).Left;
        _state.RagingStrikesLeft = _state.StatusDetails(Player, BRD.SID.RagingStrikes, Player.InstanceID, 20).Left;
        _state.BattleVoiceLeft = _state.StatusDetails(Player, BRD.SID.BattleVoice, Player.InstanceID, 15).Left;
        _state.RadiantFinaleLeft = _state.StatusDetails(Player, BRD.SID.RadiantFinale, Player.InstanceID, 15).Left;
        _state.ArmysMuseLeft = _state.StatusDetails(Player, BRD.SID.ArmysMuse, Player.InstanceID, 10).Left;
        _state.BarrageLeft = _state.StatusDetails(Player, BRD.SID.Barrage, Player.InstanceID, 10).Left;
        _state.PelotonLeft = _state.StatusDetails(Player, BRD.SID.Peloton, Player.InstanceID, 30).Left;

        // TODO: multidot support
        _state.TargetCausticLeft = _state.StatusDetails(primaryTarget, _state.ExpectedCaustic, Player.InstanceID, 45).Left;
        _state.TargetStormbiteLeft = _state.StatusDetails(primaryTarget, _state.ExpectedStormbite, Player.InstanceID, 45).Left;

        var aoeStrategy = strategy.Option(Track.AOE).As<AOEStrategy>();
        (_state.BestLadonsbiteTarget, _state.NumLadonsbiteTargets) = _state.Unlocked(BRD.AID.QuickNock) ? CheckAOETargeting(aoeStrategy, primaryTarget, 12, NumTargetsHitByLadonsbite, IsHitByLadonsbite) : (null, 0);
        (_state.BestRainOfDeathTarget, _state.NumRainOfDeathTargets) = _state.Unlocked(BRD.AID.RainOfDeath) ? CheckAOETargeting(aoeStrategy, primaryTarget, 8, NumTargetsHitByRainOfDeath, IsHitByRainOfDeath) : (null, 0);

        // TODO: refactor all that, it's kinda senseless now
        BRD.AID gcd = GetNextBestGCD(strategy);
        PushResult(gcd, gcd is BRD.AID.QuickNock or BRD.AID.Ladonsbite ? _state.BestLadonsbiteTarget : primaryTarget);

        ActionID ogcd = default;
        var deadline = _state.GCD > 0 && gcd != default ? _state.GCD : float.MaxValue;
        if (_state.CanWeave(deadline - _state.OGCDSlotLength)) // first ogcd slot
            ogcd = GetNextBestOGCD(strategy, deadline - _state.OGCDSlotLength);
        if (!ogcd && _state.CanWeave(deadline)) // second/only ogcd slot
            ogcd = GetNextBestOGCD(strategy, deadline);
        PushResult(ogcd, ogcd == ActionID.MakeSpell(BRD.AID.RainOfDeath) ? _state.BestRainOfDeathTarget : primaryTarget);
    }

    //protected override void QueueAIActions()
    //{
    //    if (_state.Unlocked(AID.HeadGraze))
    //    {
    //        var interruptibleEnemy = Autorot.Hints.PotentialTargets.Find(e => e.ShouldBeInterrupted && (e.Actor.CastInfo?.Interruptible ?? false) && e.Actor.Position.InCircle(Player.Position, 25 + e.Actor.HitboxRadius + Player.HitboxRadius));
    //        SimulateManualActionForAI(ActionID.MakeSpell(AID.HeadGraze), interruptibleEnemy?.Actor, interruptibleEnemy != null);
    //    }
    //    if (_state.Unlocked(AID.SecondWind))
    //        SimulateManualActionForAI(ActionID.MakeSpell(AID.SecondWind), Player, Player.InCombat && Player.HPMP.CurHP < Player.HPMP.MaxHP * 0.5f);
    //    if (_state.Unlocked(AID.WardensPaean))
    //    {
    //        var esunableTarget = FindEsunableTarget();
    //        SimulateManualActionForAI(ActionID.MakeSpell(AID.WardensPaean), esunableTarget, esunableTarget != null);
    //    }
    //    if (_state.Unlocked(AID.Peloton))
    //        SimulateManualActionForAI(ActionID.MakeSpell(AID.Peloton), Player, !Player.InCombat && _state.PelotonLeft < 3 && _strategy.ForceMovementIn == 0);
    //}

    public override string DescribeState() => _state.ToString();

    private int NumTargetsHitByLadonsbite(Actor primary) => Hints.NumPriorityTargetsInAOECone(Player.Position, 12, (primary.Position - Player.Position).Normalized(), 45.Degrees());
    private int NumTargetsHitByRainOfDeath(Actor primary) => Hints.NumPriorityTargetsInAOECircle(primary.Position, 8);
    private bool IsHitByLadonsbite(Actor primary, Actor check) => Hints.TargetInAOECone(check, Player.Position, 12, (primary.Position - Player.Position).Normalized(), 45.Degrees());
    private bool IsHitByRainOfDeath(Actor primary, Actor check) => Hints.TargetInAOECircle(check, primary.Position, 8);

    private (Actor?, int) CheckAOETargeting(AOEStrategy strategy, Actor? primaryTarget, float range, Func<Actor, int> numTargets, Func<Actor, Actor, bool> check) => strategy switch
    {
        AOEStrategy.AutoTargetHitPrimary => FindBetterTargetBy(primaryTarget, range, t => primaryTarget == null || check(t, primaryTarget) ? numTargets(t) : 0),
        AOEStrategy.AutoTargetHitMost => FindBetterTargetBy(primaryTarget, range, numTargets),
        AOEStrategy.AutoOnPrimary => (primaryTarget, primaryTarget != null ? numTargets(primaryTarget) : 0),
        AOEStrategy.ForceAOE => (primaryTarget, int.MaxValue),
        _ => (null, 0)
    };

    // old BRDRotation
    private bool CanRefreshDOTsIn(int numGCDs)
    {
        var minLeft = Math.Min(_state.TargetStormbiteLeft, _state.TargetCausticLeft);
        return minLeft > _state.GCD && minLeft <= _state.GCD + 2.5f * numGCDs;
    }

    // heuristic to determine whether currently active dots were applied under raidbuffs (assumes dots are actually active)
    // it's not easy to directly determine active dot potency
    private bool AreActiveDOTsBuffed()
    {
        // dots last for 45s => their time of application Td = t + dotsLeft - 45
        // assuming we're using BV as the main buff, its cd is 120 => it was last used at Ts = t + bvcd - 120, and lasted until Te = Ts + 15
        // so dots are buffed if Ts < Td < Te => t + bvcd - 120 < t + dotsLeft - 45 < t + bvcd - 105 => bvcd - 75 < dotsLeft < bvcd - 60
        // this works when BV is off cd (dotsLeft < -60 is always false)
        // this doesn't really work if dots are not up (can return true if bvcd is between 75 and 60)
        var dotsLeft = Math.Min(_state.TargetStormbiteLeft, _state.TargetCausticLeft);
        var bvCD = _state.CD(BRD.AID.BattleVoice);
        return dotsLeft > bvCD - 75 && dotsLeft < bvCD - 60;
    }

    // IJ generally has to be used at last possible gcd before dots fall off -or- before major buffs fall off (to snapshot buffs to dots), but in some cases we want to use it earlier:
    // - 1 gcd earlier if we don't have RA proc (otherwise we might use filler, it would proc RA, then on next gcd we'll have to use IJ to avoid dropping dots and potentially waste another RA)
    // - 1/2 gcds earlier if we're waiting for more gauge for AA
    private bool ShouldUseIronJawsAutomatic(ApexArrowStrategy aaStrategy, OffensiveStrategy baStrategy)
    {
        var refreshDotsDeadline = Math.Min(_state.TargetStormbiteLeft, _state.TargetCausticLeft);
        if (refreshDotsDeadline <= _state.GCD)
            return false; // don't bother, we won't make it...
        if (refreshDotsDeadline <= _state.GCD + 2.5f)
            return true; // last possible gcd to refresh dots - just use IJ now
        if (AreActiveDOTsBuffed())
            return false; // never extend buffed dots early: we obviously don't want to use multiple IJs in a single buff window, and outside buff window we don't want to overwrite buffed ticks, even if that means risking losing a proc

        // ok, dots aren't falling off imminently, and they are not buffed - see if we want to ij early and overwrite last ticks
        if (_state.StraightShotLeft <= _state.GCD && refreshDotsDeadline <= _state.GCD + 5 && !ShouldUseApexArrow(aaStrategy) && (_state.BlastArrowLeft <= _state.GCD || baStrategy == OffensiveStrategy.Delay))
            return true; // refresh 1 gcd early, if we would be forced to cast BS otherwise - if so, we could proc RA and then overwrite it by IJ on next gcd (TODO: i don't really like these conditions...)
        if (_state.BattleVoiceLeft <= _state.GCD)
            return false; // outside buff window, so no more reasons to extend early

        // under buffs, we might want to do early IJ, so that AA can be slightly delayed, or so that we don't risk proc overwrites
        int maxRemainingGCDs = 1; // by default, refresh on last possible GCD before we either drop dots or drop major buffs
        if (_state.StraightShotLeft <= _state.GCD)
            ++maxRemainingGCDs; // 1 extra gcd if we don't have RA proc (if we don't refresh early, we might use filler, which could give us a proc; then on next gcd we'll be forced to IJ to avoid dropping dots, which might give another proc)
        // if we're almost at the gauge cap, we want to delay AA/BA (but still fit them into buff window), so we want to IJ earlier
        if (_state.SoulVoice is > 50 and < 100) // best we can hope for over 4 gcds is ~25 gauge (4 ticks + EA) - TODO: improve condition
            maxRemainingGCDs += _state.Unlocked(BRD.AID.BlastArrow) ? 2 : 1; // 1/2 gcds for AA/BA; only under buffs - outside buffs it's simpler to delay AA
        return _state.BattleVoiceLeft <= _state.GCD + 2.5f * maxRemainingGCDs;
    }

    private bool ShouldUseIronJaws(DotStrategy dotStrategy, ApexArrowStrategy aaStrategy, OffensiveStrategy baStrategy) => dotStrategy switch
    {
        DotStrategy.Forbid => false,
        DotStrategy.ForceExtend => true,
        DotStrategy.ExtendIgnoreBuffs => CanRefreshDOTsIn(_state.StraightShotLeft <= _state.GCD ? 2 : 1),
        DotStrategy.ExtendDelayed => CanRefreshDOTsIn(1),
        _ => ShouldUseIronJawsAutomatic(aaStrategy, baStrategy)
    };

    // you get 5 gauge for every repertoire tick, meaning every 15s you get 5 gauge from EA + up to 25 gauge (*80% = 20 average) from songs
    // using AA at 80+ gauge procs BA, meaning AA at <80 gauge is rarely worth it
    private bool ShouldUseApexArrow(ApexArrowStrategy strategy) => strategy switch
    {
        ApexArrowStrategy.Delay => false,
        ApexArrowStrategy.ForceAnyGauge => _state.SoulVoice > 0,
        ApexArrowStrategy.ForceHighGauge => _state.SoulVoice >= 80,
        ApexArrowStrategy.ForceCapGauge => _state.SoulVoice >= 100,
        _ => _state.SoulVoice switch
        {
            >= 100 => _state.CD(BRD.AID.BattleVoice) >= _state.GCD + 45, // use asap, unless we are unlikely to have 80+ gauge by the next buff window (TODO: reconsider time limit)
            >= 80 => _state.BattleVoiceLeft > _state.GCD
                ? _state.BattleVoiceLeft < _state.GCD + 5 // under buffs, don't delay AA if doing that will make BA miss buffs (TODO: also don't delay if it can drift barrage past third gcd...)
                : _state.CD(BRD.AID.BattleVoice) - _state.GCD is >= 45 and < 55, // outside buffs, delay unless we risk entering a window where next buffs are imminent and we can't AA (TODO: reconsider window size)
            _ => false // never use AA at <80 gauge automatically; assume manual planning for things like end-of-fight or downtimes
        }
    };

    private float SwitchAtRemainingSongTimer(SongStrategy strategy) => strategy switch
    {
        SongStrategy.Automatic => _state.ActiveSong switch
        {
            Song.WanderersMinuet => 3, // WM->MB transition when no more repertoire ticks left
            Song.MagesBallad => _state.NumRainOfDeathTargets < 3 ? 12 : 3, // MB->AP transition asap as long as we won't end up songless (active song condition 15 == 45 - (120 - 2*45); get extra MB tick at 12s to avoid being songless for a moment), unless we're doing aoe rotation
            Song.ArmysPaeon => _state.Repertoire == 4 ? 15 : 3, // AP->WM transition asap as long as we'll have MB ready when WM ends, if we either have full repertoire or AP is about to run out anyway
            _ => 3
        },
        SongStrategy.Extend => 3,
        SongStrategy.Overextend => 0, // TODO: think more about it...
        SongStrategy.ForcePP => _state.Repertoire > 0 ? 0 : 3, // if we still have PP charges, don't switch; otherwise switch after last tick (assuming we're under WM)
        _ => 3
    };

    private bool ShouldUsePotion(PotionStrategy strategy) => strategy switch
    {
        PotionStrategy.Manual => false,
        PotionStrategy.Burst => !Player.InCombat ? _state.CountdownRemaining < 2 : _state.TargetingEnemy && _state.CD(BRD.AID.RagingStrikes) < _state.GCD + 3.5f, // pre-pull or RS ready in 2 gcds (assume pot -> late-weaved WM -> RS)
        PotionStrategy.Force => true,
        _ => false
    };

    // by default, we use RS asap as soon as WM is up
    private bool ShouldUseRagingStrikes(OffensiveStrategy strategy) => strategy switch
    {
        OffensiveStrategy.Delay => false,
        OffensiveStrategy.Force => true,
        _ => _state.TargetingEnemy && (_state.ActiveSong == Song.WanderersMinuet || !_state.Unlocked(BRD.AID.WanderersMinuet))
    };

    // by default, we pool bloodletter for burst
    private bool ShouldUseBloodletter(BloodletterStrategy strategy) => strategy switch
    {
        BloodletterStrategy.Delay => false,
        BloodletterStrategy.Force => true,
        BloodletterStrategy.KeepOneCharge => _state.CD(BRD.AID.Bloodletter) <= 15 + _state.AnimationLock,
        BloodletterStrategy.KeepTwoCharges => _state.Unlocked(BRD.TraitID.EnhancedBloodletter) && _state.CD(BRD.AID.Bloodletter) <= _state.AnimationLock,
        _ => !_state.Unlocked(BRD.AID.WanderersMinuet) || // don't try to pool BLs at low level (reconsider)
            _state.ActiveSong == Song.MagesBallad || // don't try to pool BLs during MB, it's risky
            _state.BattleVoiceLeft > _state.AnimationLock || // don't pool BLs during buffs
            _state.CD(BRD.AID.Bloodletter) - (_state.Unlocked(BRD.TraitID.EnhancedBloodletter) ? 0 : 15) <= Math.Min(_state.CD(BRD.AID.RagingStrikes), _state.CD(BRD.AID.BattleVoice)) // don't pool BLs if they will overcap before next buffs
    };

    // by default, we use EA asap if in combat
    private bool ShouldUseEmpyrealArrow(OffensiveStrategy strategy) => strategy switch
    {
        OffensiveStrategy.Delay => false,
        OffensiveStrategy.Force => true,
        _ => Player.InCombat
    };

    // by default, we use barrage under raid buffs, being careful not to overwrite RA proc
    // TODO: reconsider barrage usage during aoe
    private bool ShouldUseBarrage(OffensiveStrategy strategy) => strategy switch
    {
        OffensiveStrategy.Delay => false,
        OffensiveStrategy.Force => true,
        _ => Player.InCombat // in combat
            && (_state.Unlocked(BRD.AID.BattleVoice) ? _state.BattleVoiceLeft : _state.RagingStrikesLeft) > 0 // and under raid buffs
            && (_state.NumLadonsbiteTargets < 2
                ? _state.StraightShotLeft <= _state.GCD // in non-aoe situation - if there is no RA proc already
                : _state.NumLadonsbiteTargets >= 4 && _state.ShadowbiteLeft > _state.GCD) // in aoe situations - use on shadowbite on 4+ targets (TODO: verify!!!)
    };

    // by default, we use sidewinder asap, unless raid buffs are imminent
    private bool ShouldUseSidewinder(OffensiveStrategy strategy) => strategy switch
    {
        OffensiveStrategy.Delay => false,
        OffensiveStrategy.Force => true,
        _ => Player.InCombat && _state.CD(BRD.AID.BattleVoice) > 45 // TODO: consider exact delay condition
    };

    private BRD.AID GetNextBestGCD(StrategyValues strategy)
    {
        // prepull or no target
        if (!_state.TargetingEnemy || _state.CountdownRemaining > 0.7f)
            return BRD.AID.None;

        if (_state.NumLadonsbiteTargets >= 2)
        {
            // TODO: AA/BA targeting/condition (it might hit fewer targets)
            if (_state.BlastArrowLeft > _state.GCD && strategy.Option(Track.BlastArrow).As<OffensiveStrategy>() != OffensiveStrategy.Delay)
                return BRD.AID.BlastArrow;
            if (ShouldUseApexArrow(strategy.Option(Track.ApexArrow).As<ApexArrowStrategy>()))
                return BRD.AID.ApexArrow;

            // TODO: barraged RA on 3 targets?..
            // TODO: better shadowbite targeting (it might hit fewer targets)
            return _state.ShadowbiteLeft > _state.GCD ? BRD.AID.Shadowbite : _state.BestLadonsbite;
        }
        else
        {
            var strategyDOTs = strategy.Option(Track.DOTs).As<DotStrategy>();
            var forbidApplyDOTs = _state.ForbidDOTs || strategyDOTs is DotStrategy.AutomaticExtendOnly or DotStrategy.Forbid;
            if (_state.Unlocked(BRD.AID.IronJaws))
            {
                // apply dots if not up and allowed by strategy
                if (!forbidApplyDOTs && _state.TargetStormbiteLeft <= _state.GCD)
                    return _state.BestStormbite;
                if (!forbidApplyDOTs && _state.TargetCausticLeft <= _state.GCD)
                    return _state.BestCausticBite;

                // at this point, we have to prioritize IJ, AA/BA and RA procs
                var strategyAA = strategy.Option(Track.ApexArrow).As<ApexArrowStrategy>();
                var strategyBA = strategy.Option(Track.BlastArrow).As<OffensiveStrategy>();
                if (!_state.ForbidDOTs && ShouldUseIronJaws(strategyDOTs, strategyAA, strategyBA))
                    return BRD.AID.IronJaws;

                // there are cases where we want to prioritize RA over AA/BA:
                // - if barrage is about to come off CD, we don't want to delay it needlessly
                // - if delaying RA would force us to IJ on next gcd (potentially overwriting proc)
                // we only do that if there are no explicit AA/BA force strategies (in that case we assume just doing AA/BA is more important than wasting a proc)
                bool highPriorityRA = _state.StraightShotLeft > _state.GCD // RA ready
                    && strategyAA is ApexArrowStrategy.Automatic or ApexArrowStrategy.Delay // no forced AA
                    && strategyBA != OffensiveStrategy.Force // no forced BA
                    && (_state.CD(BRD.AID.Barrage) < _state.GCD + 2.5f || CanRefreshDOTsIn(2)); // either barrage coming off cd or dots falling off imminent
                if (highPriorityRA)
                    return _state.BestRefulgentArrow;

                // BA if possible and not forbidden
                if (_state.BlastArrowLeft > _state.GCD && strategyBA != OffensiveStrategy.Delay)
                    return BRD.AID.BlastArrow;

                // AA depending on conditions
                if (ShouldUseApexArrow(strategyAA))
                    return BRD.AID.ApexArrow;

                // RA/BS
                return _state.StraightShotLeft > _state.GCD ? _state.BestRefulgentArrow : _state.BestBurstShot;
            }
            else
            {
                // pre IJ our gcds are extremely boring: keep dots up and use up straight shot procs asap
                // only HS can proc straight shot, so we're not wasting potential procs here
                // TODO: tweak threshold so that we don't overwrite or miss ticks...
                // TODO: do we care about reapplying dots early under raidbuffs?..
                if (!forbidApplyDOTs && _state.Unlocked(BRD.AID.Windbite) && _state.TargetStormbiteLeft < _state.GCD + 3)
                    return BRD.AID.Windbite;
                if (!forbidApplyDOTs && _state.Unlocked(BRD.AID.VenomousBite) && _state.TargetCausticLeft < _state.GCD + 3)
                    return BRD.AID.VenomousBite;
                return _state.StraightShotLeft > _state.GCD ? BRD.AID.StraightShot : BRD.AID.HeavyShot;
            }
        }
    }

    private ActionID GetNextBestOGCD(StrategyValues strategy, float deadline)
    {
        // potion
        var strategyPotion = strategy.Option(Track.Potion).As<PotionStrategy>();
        if (ShouldUsePotion(strategyPotion) && _state.CanWeave(_state.PotionCD, 1.1f, deadline))
            return ActionDefinitions.IDPotionDex;

        // maintain songs
        var strategySongs = strategy.Option(Track.Songs).As<SongStrategy>();
        if (_state.TargetingEnemy && Player.InCombat && strategySongs != SongStrategy.Delay)
        {
            if (strategySongs == SongStrategy.ForceWM && _state.Unlocked(BRD.AID.WanderersMinuet) && _state.CanWeave(BRD.AID.WanderersMinuet, 0.6f, deadline))
                return ActionID.MakeSpell(BRD.AID.WanderersMinuet);
            if (strategySongs == SongStrategy.ForceMB && _state.Unlocked(BRD.AID.MagesBallad) && _state.CanWeave(BRD.AID.MagesBallad, 0.6f, deadline))
                return ActionID.MakeSpell(BRD.AID.MagesBallad);
            if (strategySongs == SongStrategy.ForceAP && _state.Unlocked(BRD.AID.ArmysPaeon) && _state.CanWeave(BRD.AID.ArmysPaeon, 0.6f, deadline))
                return ActionID.MakeSpell(BRD.AID.ArmysPaeon);

            if (_state.ActiveSong == Song.None)
            {
                // if no song is up, use best available one
                if (_state.Unlocked(BRD.AID.WanderersMinuet) && _state.CanWeave(BRD.AID.WanderersMinuet, 0.6f, deadline))
                    return ActionID.MakeSpell(BRD.AID.WanderersMinuet);
                if (_state.Unlocked(BRD.AID.MagesBallad) && _state.CanWeave(BRD.AID.MagesBallad, 0.6f, deadline))
                    return ActionID.MakeSpell(BRD.AID.MagesBallad);
                if (_state.Unlocked(BRD.AID.ArmysPaeon) && _state.CanWeave(BRD.AID.ArmysPaeon, 0.6f, deadline))
                    return ActionID.MakeSpell(BRD.AID.ArmysPaeon);
            }
            else if (_state.Unlocked(BRD.AID.WanderersMinuet) && _state.ActiveSongLeft < SwitchAtRemainingSongTimer(strategySongs) - 0.1f) // TODO: rethink this extra leeway, we want to make sure we use up last tick's repertoire e.g. in WM
            {
                // once we have WM, we can have a proper song cycle
                if (_state.ActiveSong == Song.WanderersMinuet)
                {
                    if (_state.Repertoire > 0 && _state.CanWeave(BRD.AID.PitchPerfect, 0.6f, deadline))
                        return ActionID.MakeSpell(BRD.AID.PitchPerfect); // spend remaining repertoire before leaving WM
                    if (_state.CanWeave(BRD.AID.MagesBallad, 0.6f, deadline))
                        return ActionID.MakeSpell(BRD.AID.MagesBallad);
                }
                if (_state.ActiveSong == Song.MagesBallad && _state.CD(BRD.AID.WanderersMinuet) < 45 && _state.CanWeave(BRD.AID.ArmysPaeon, 0.6f, deadline))
                    return ActionID.MakeSpell(BRD.AID.ArmysPaeon);
                if (_state.ActiveSong == Song.ArmysPaeon && _state.CD(BRD.AID.MagesBallad) < 45 && _state.CanWeave(BRD.AID.WanderersMinuet, 0.6f, deadline) && _state.GCD < 0.9f)
                    return ActionID.MakeSpell(BRD.AID.WanderersMinuet); // late-weave
            }
        }

        // apply major buffs
        // RS as soon as we enter WM (or just on CD, if we don't have it yet)
        // in opener, it end up being late-weaved after WM (TODO: can we weave it extra-late to ensure 9th gcd is buffed?)
        // in 2-minute bursts, it ends up being early-weaved after first WM gcd (TODO: can we weave it later to ensure 10th gcd is buffed?)
        var strategyRS = strategy.Option(Track.RagingStrikes).As<OffensiveStrategy>();
        if (_state.Unlocked(BRD.AID.RagingStrikes) && ShouldUseRagingStrikes(strategyRS) && _state.CanWeave(BRD.AID.RagingStrikes, 0.6f, deadline))
            return ActionID.MakeSpell(BRD.AID.RagingStrikes);

        // BV+RF 2 gcds after RS (RF first with 1 coda, ? with 2 coda, BV first with 3 coda)
        // visualization:
        // -GCD               0               GCD
        //   * -gcd---------- * -gcd---------- * -gcd---------- * -gcd----------
        //   * ---- RS ------ * -------------- * ---- BV - RF - * --------------
        //           ^^^^----------------^^^----------^^^
        //         20s  20s              max          min
        // GCD is slightly smaller than 2.5 during opener, and slightly smaller than 2.1 during reopener (assuming 4-stack AP)
        // RS should happen in second ogcd slot during opener (t in [-gcd + 1.2, -0.6] == [-1.3 + sksDelta, -0.6], or anywhere during burst (t in [-gcd + 0.6, -0.6] == [-1.5 + sksDelta, -0.6])
        // RS buff starts ticking down from 20 at t+erDelay, so we can imagine that RS has effective time-left == 20+erDelay when applied
        // we want to enable BV/RF between [gcd-0.6, gcd+0.6]
        // at T=gcd RS buff will have remaining 20+erDelay-(T-t) == [opener] 20+erDelay-(2.5-sksDelta)+[-1.3+sksDelta, -0.6] == 17.5+erDelay+sksDelta+[-1.3+sksDelta, -0.6] == [16.2+sksDelta, 16.9]+erDelay+sksDelta
        //                                                       == [burst]  20+erDelay-(2.1-sksDelta)+[-1.5+sksDelta, -0.6] == 17.9+erDelay+sksDelta+[-1.5+sksDelta, -0.6] == [16.4+sksDelta, 17.3]+erDelay+sksDelta
        // so condition is [opener] RSLeft <= 16.8 + [sksDelta, 0.7]+erDelay+sksDelta
        //                 [burst]  RSLeft <= 17.0 + [sksDelta, 0.9]+erDelay+sksDelta
        // but note that if we select too small limit (e.g. 16.8), we might run into a problem: if 0.7+erDelay+sksDelta > 1.2, then we might not allow using buff at min, use something else instead, and push second buff to next GCD
        if (_state.TargetingEnemy && _state.RagingStrikesLeft > _state.AnimationLock && _state.RagingStrikesLeft < 17)
        {
            if (_state.NumCoda == 1 && _state.CanWeave(BRD.AID.RadiantFinale, 0.6f, deadline))
                return ActionID.MakeSpell(BRD.AID.RadiantFinale);
            if (_state.Unlocked(BRD.AID.BattleVoice) && _state.CanWeave(BRD.AID.BattleVoice, 0.6f, deadline))
                return ActionID.MakeSpell(BRD.AID.BattleVoice);
            if (_state.NumCoda > 1 && _state.CanWeave(BRD.AID.RadiantFinale, 0.6f, deadline))
                return ActionID.MakeSpell(BRD.AID.RadiantFinale);
        }

        // TODO: consider moving PP3 check here and delay EA if we actually fuck up

        // EA - important not to drift (TODO: is it actually better to delay it if we're capped on PP/BL?)
        // we should not be at risk of capping BL (since we spend charges asap in WM/MB anyway)
        // we might risk capping PP, but we should've dealt with that on previous slots by using PP2
        // TODO: consider clipping gcd to avoid ea drift...
        var strategyEA = strategy.Option(Track.EmpyrealArrow).As<OffensiveStrategy>();
        if (_state.TargetingEnemy && ShouldUseEmpyrealArrow(strategyEA) && _state.Unlocked(BRD.AID.EmpyrealArrow) && _state.CanWeave(BRD.AID.EmpyrealArrow, 0.6f, deadline))
            return ActionID.MakeSpell(BRD.AID.EmpyrealArrow);

        // PP here should not conflict with anything priority-wise
        // note that we already handle PPx after last repertoire tick before switching to WM (see song cycle code above)
        if (_state.TargetingEnemy && _state.ActiveSong == Song.WanderersMinuet && _state.Repertoire > 0 && _state.CanWeave(BRD.AID.PitchPerfect, 0.6f, deadline))
        {
            if (_state.Repertoire == 3)
                return ActionID.MakeSpell(BRD.AID.PitchPerfect); // PP3 is a no-brainer

            if (strategySongs == SongStrategy.ForcePP)
                return ActionID.MakeSpell(BRD.AID.PitchPerfect); // PPx if strategy says so

            var nextProcIn = _state.ActiveSongLeft % 3.0f;
            if (_state.BattleVoiceLeft > _state.AnimationLock && _state.BattleVoiceLeft <= nextProcIn + 1)
                return ActionID.MakeSpell(BRD.AID.PitchPerfect); // PPx if we won't get any more stacks under buffs (TODO: better leeway)

            // if we're at PP2 and EA is about to come off cd, we might be in a situation where waiting for PP3 would have us choose between delaying EA or wasting its guaranteed proc; in such case we want to PP2 early
            if (_state.Repertoire == 2)
            {
                bool usePP2 = false;
                if (_state.CanWeave(BRD.AID.EmpyrealArrow, 0.6f, _state.GCD))
                {
                    // we're going to use EA in next ogcd slot before GCD => use PP2 if we won't be able to wait until tick and weave before EA
                    usePP2 = !_state.CanWeave(nextProcIn, 0.6f, _state.CD(BRD.AID.EmpyrealArrow));
                }
                else if (_state.CD(BRD.AID.EmpyrealArrow) < _state.GCD + 1.2f)
                {
                    // we're going to use EA just after next GCD => use PP2 if we won't be able to wait until tick and late-weave before next GCD
                    usePP2 = !_state.CanWeave(nextProcIn, 0.6f, _state.GCD);
                }

                if (usePP2)
                    return ActionID.MakeSpell(BRD.AID.PitchPerfect); // PP2 if we might get conflict with EA
            }
        }

        // barrage, under buffs and if there is no proc already
        // TODO: consider moving up to avoid drifting? seems risky...
        var strategyBarrage = strategy.Option(Track.Barrage).As<OffensiveStrategy>();
        if (_state.TargetingEnemy && ShouldUseBarrage(strategyBarrage) && _state.Unlocked(BRD.AID.Barrage) && _state.CanWeave(BRD.AID.Barrage, 0.6f, deadline))
            return ActionID.MakeSpell(BRD.AID.Barrage);

        // sidewinder, unless we're delaying it until buffs
        var strategySW = strategy.Option(Track.Sidewinder).As<OffensiveStrategy>();
        if (_state.TargetingEnemy && ShouldUseSidewinder(strategySW) && _state.Unlocked(BRD.AID.Sidewinder) && _state.CanWeave(BRD.AID.Sidewinder, 0.6f, deadline))
            return ActionID.MakeSpell(BRD.AID.Sidewinder);

        // bloodletter, unless we're pooling them for burst
        var strategyBL = strategy.Option(Track.Bloodletter).As<BloodletterStrategy>();
        if (_state.TargetingEnemy && Player.InCombat && _state.Unlocked(BRD.AID.Bloodletter) && ShouldUseBloodletter(strategyBL) && _state.CanWeave(_state.CD(BRD.AID.Bloodletter) - 30, 0.6f, deadline))
            return ActionID.MakeSpell(_state.NumRainOfDeathTargets >= 2 ? BRD.AID.RainOfDeath : BRD.AID.Bloodletter);

        // no suitable oGCDs...
        return new();
    }
}
