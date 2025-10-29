using BossMod.Data;

namespace BossMod.Autorotation.xan;

public class PhantomAI(RotationModuleManager manager, Actor player) : AIBase(manager, player)
{
    public enum Track
    {
        Cannoneer,
        Ranger,
        TimeMage,
        Chemist,
        Samurai,
        Bard,
        Monk,
        Predict,
        Chakra
    }

    public enum RaiseStrategy
    {
        Never,
        OutOfCombat,
        InCombat
    }

    public enum PredictStrategy
    {
        AutoConservative,
        AutoGreedy,
        AutoSuperGreedy,
        HealOnly,
        Disabled
    }

    enum Prediction
    {
        None,
        Judgment,
        Cleansing,
        Blessing,
        Starfall
    }

    public enum ChakraStrategy
    {
        Any,
        HP,
        MP,
        Disabled
    }

    public static RotationModuleDefinition Definition()
    {
        var def = new RotationModuleDefinition("Phantom Job AI", "Basic phantom job action automation", "AI (xan)", "xan", RotationModuleQuality.WIP, new(~0ul), MaxLevel: 100);

        def.AbilityTrack(Track.Cannoneer, "Cannoneer", "Cannoneer: Use cannon actions on best AOE target as soon as they become available")
            .AddAssociatedActions(PhantomID.DarkCannon, PhantomID.HolyCannon, PhantomID.ShockCannon, PhantomID.SilverCannon, PhantomID.PhantomFire);

        def.AbilityTrack(Track.Ranger, "Ranger", "Ranger: Use Phantom Aim on cooldown")
            .AddAssociatedActions(PhantomID.PhantomAim);

        def.AbilityTrack(Track.TimeMage, "TimeMage", "Time Mage: Use Comet ASAP if it will be instant")
            .AddAssociatedActions(PhantomID.OccultComet, PhantomID.OccultQuick);

        def.Define(Track.Chemist).As<RaiseStrategy>("Chemist", "Chemist: Raise")
            .AddOption(RaiseStrategy.Never, "Never", "Disabled")
            .AddOption(RaiseStrategy.OutOfCombat, "OutOfCombat", "Out of combat")
            .AddOption(RaiseStrategy.InCombat, "InCombat", "Always")
            .AddAssociatedActions(PhantomID.Revive);

        def.AbilityTrack(Track.Samurai, "Samurai", "Samurai: Use Iainuki on best AOE target")
            .AddAssociatedActions(PhantomID.Iainuki);

        def.AbilityTrack(Track.Bard, "Bard", "Bard: Use Aria/Rime in combat")
            .AddAssociatedActions(PhantomID.OffensiveAria, PhantomID.HerosRime);

        def.AbilityTrack(Track.Monk, "Monk", "Monk: Use Kick to maintain buff, use Counterstance during downtime")
            .AddAssociatedActions(PhantomID.PhantomKick, PhantomID.Counterstance, PhantomID.OccultCounter);

        def.Define(Track.Predict).As<PredictStrategy>("Predict", "Oracle: Predict")
            .AddOption(PredictStrategy.AutoConservative, "Use first available damage action that isn't Starfall")
            .AddOption(PredictStrategy.AutoGreedy, "Use first available damage action; allow Starfall if HP is high enough")
            .AddOption(PredictStrategy.AutoSuperGreedy, "Use Starfall, regardless of HP")
            .AddOption(PredictStrategy.HealOnly, "Use Blessing (heal)")
            .AddOption(PredictStrategy.Disabled, "Don't use")
            .AddAssociatedActions(PhantomID.Predict, PhantomID.PhantomJudgment, PhantomID.Cleansing, PhantomID.Blessing, PhantomID.Starfall);

        def.Define(Track.Chakra).As<ChakraStrategy>("Chakra", "Monk: Use Occult Chakra")
            .AddOption(ChakraStrategy.Any, "Any", "If HP or MP is low")
            .AddOption(ChakraStrategy.HP, "HP", "Only if HP is below 30%")
            .AddOption(ChakraStrategy.MP, "MP", "Only if MP is below 30%")
            .AddOption(ChakraStrategy.Disabled, "Disabled", "Never")
            .AddAssociatedActions(PhantomID.OccultChakra);

        return def;
    }

    public static readonly uint[] UndeadMobs = [
        13921, // caoineag
        13922, // crescent ghost
        13923, // crescent geshunpest
        13924, // crescent armor
        13925, // crescent troubadour
        13926, // crescent gourmand
        13927, // crescent dullahan
    ];

    public static readonly uint[] UndesirableStatus = [
        1706, // Evasion Up on Tower Scarab (hallways)
        2556, // Magic Damage Up on Tower Abyss (bridges)
        4539, // Invincibility on Guardian Knight (lockwards)
        // 4458, // Armed to the Teeth on Guardian Weapon, can only be pilfered, not dispelled
    ];

    public static readonly uint[] SlowableMobs = [
        0x35de,
        0x35df,
        0x35e0,
        0x35e1,
        0x35e2,
        0x35e3,
        0x35e4,
        0x35e5,
        0x35e6,
        0x35e9,
        0x35eb,
        0x35ed,
        0x35ef
    ];

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        if (World.Client.CountdownRemaining > 0)
            return;

        var isMidCombo = CheckMidCombo();

        if (strategy.Enabled(Track.Cannoneer) && !isMidCombo)
        {
            var prio = strategy.Option(Track.Cannoneer).Priority(ActionQueue.Priority.High + 500);

            var bestTarget = primaryTarget?.IsAlly == false ? primaryTarget : null;
            var bestCount = bestTarget == null ? 0 : Hints.NumPriorityTargetsInAOECircle(bestTarget.Position, 5);
            foreach (var tar in Hints.PriorityTargets.Where(x => Player.DistanceToHitbox(x.Actor) <= 30))
            {
                if (tar.Actor == bestTarget)
                    continue;

                var cnt = Hints.NumPriorityTargetsInAOECircle(tar.Actor.Position, 5);
                if (cnt > bestCount)
                {
                    bestTarget = tar.Actor;
                    bestCount = cnt;
                }
            }

            if (bestTarget != null && bestCount > 0)
            {
                var isUndead = UndeadMobs.Contains(bestTarget.NameID);

                UseAction(PhantomID.SilverCannon, bestTarget, prio); // shares CD with holy
                UseAction(PhantomID.ShockCannon, bestTarget, prio); // shares CD with dark

                // use after silver for the extra damage from silver debuff
                UseAction(PhantomID.PhantomFire, bestTarget, prio);

                UseAction(PhantomID.HolyCannon, bestTarget, isUndead ? prio + 1 : prio);
                UseAction(PhantomID.DarkCannon, bestTarget, prio);
            }
        }

        if (strategy.Enabled(Track.Ranger) && primaryTarget?.IsAlly == false && Player.InCombat)
        {
            var prio = strategy.Option(Track.Ranger).Priority(ActionQueue.Priority.Low);

            UseAction(PhantomID.PhantomAim, Player, prio);
        }

        if (strategy.Enabled(Track.TimeMage) && primaryTarget?.IsAlly == false)
        {
            var prio = strategy.Option(Track.TimeMage).Priority(ActionQueue.Priority.High + 500);

            var nextGCD = World.FutureTime(GCD);
            var haveSwift = Player.Statuses.Any(s => InstantCastStatus.Contains(s.ID) && s.ExpireAt > nextGCD);
            if (haveSwift)
                UseAction(PhantomID.OccultComet, primaryTarget, prio);

            UseAction(PhantomID.OccultQuick, Player, prio);
        }

        var option = strategy.Option(Track.Chemist);
        var canRaise = option.As<RaiseStrategy>() switch
        {
            RaiseStrategy.InCombat => true,
            RaiseStrategy.OutOfCombat => !Player.InCombat,
            _ => false
        };

        // check that we have Revive to avoid pointlessly scanning the object table again
        if (canRaise && !isMidCombo && World.Client.DutyActions.Any(d => d.Action.ID == (uint)PhantomID.Revive))
        {
            var prio = option.Priority(ActionQueue.Priority.High + 500);
            if (RaiseUtil.FindRaiseTargets(World, RaiseUtil.Targets.Everyone).FirstOrDefault() is { } tar)
                UseAction(PhantomID.Revive, tar, prio);
        }

        if (strategy.Enabled(Track.Samurai) && primaryTarget?.IsAlly == false && !isMidCombo)
        {
            var prio = strategy.Option(Track.Samurai).Priority(ActionQueue.Priority.High + 500);
            if (UseAction(PhantomID.Iainuki, primaryTarget, prio, 0.8f))
                Hints.GoalZones.Add(Hints.GoalAOECone(primaryTarget, 8, 60.Degrees()));
        }

        if (strategy.Enabled(Track.Bard) && primaryTarget?.IsAlly == false && Player.InCombat)
        {
            var ariaLeft = SelfStatusDetails(PhantomSID.OffensiveAria, 70).Left;
            var rimeLeft = SelfStatusDetails(PhantomSID.HerosRime, 20).Left;
            var prio = strategy.Option(Track.Bard).Priority(ActionQueue.Priority.Low);

            UseAction(PhantomID.HerosRime, Player, prio);

            if (ariaLeft < 10 && rimeLeft < World.Client.AnimationLock)
                UseAction(PhantomID.OffensiveAria, Player, prio);
        }

        if (strategy.Enabled(Track.Monk) && Player.InCombat)
        {
            var prio = strategy.Option(Track.Monk).Priority(ActionQueue.Priority.Low);

            var counterLeft = SelfStatusDetails(PhantomSID.Counterstance, 60).Left;
            if (counterLeft <= 30 && !Hints.PriorityTargets.Any())
                UseAction(PhantomID.Counterstance, Player, prio);

            if (primaryTarget?.IsAlly == false)
            {
                if (UseAction(PhantomID.PhantomKick, primaryTarget, prio))
                    Hints.GoalZones.Add(Hints.GoalAOERect(primaryTarget, 15, 3));

                if (World.Client.ProcTimers[2] > World.Client.AnimationLock && UseAction(PhantomID.OccultCounter, primaryTarget, prio))
                    Hints.GoalZones.Add(Hints.GoalAOECone(primaryTarget, 6, 60.Degrees()));
            }
        }

        var chakraOpt = strategy.Option(Track.Chakra);
        var chakraStrategy = chakraOpt.As<ChakraStrategy>();
        if (chakraStrategy != ChakraStrategy.Disabled && Player.InCombat)
        {
            var lowHP = Player.HPMP.CurHP < Player.HPMP.MaxHP * 0.3f;
            var lowMP = Player.HPMP.CurMP < Player.HPMP.MaxMP * 0.3f;

            var useOk = chakraStrategy switch
            {
                ChakraStrategy.Any => lowHP || lowMP,
                ChakraStrategy.HP => lowHP,
                ChakraStrategy.MP => lowMP,
                _ => false
            };

            if (useOk)
                UseAction(PhantomID.OccultChakra, Player, chakraOpt.Priority(ActionQueue.Priority.Low));
        }

        var predictOpt = strategy.Option(Track.Predict);
        var predictStrategy = predictOpt.As<PredictStrategy>();
        if (predictStrategy != PredictStrategy.Disabled)
        {
            var (pred, flags) = GetPrediction();

            var haveTarget = Player.InCombat && predictStrategy switch
            {
                PredictStrategy.AutoConservative or PredictStrategy.AutoGreedy or PredictStrategy.AutoSuperGreedy => primaryTarget?.IsAlly == false,
                PredictStrategy.HealOnly => true,
                _ => false
            };

            if (pred == Prediction.None && haveTarget)
                UseAction(PhantomID.Predict, Player, ActionQueue.Priority.VeryHigh - 10);

            var isHeal = pred == Prediction.Blessing;
            var isDmg = pred is Prediction.Cleansing or Prediction.Judgment or Prediction.Starfall;
            var isSafe = pred != Prediction.Starfall;
            var isLastPrediction = flags == 0xF;

            var canUse = predictStrategy switch
            {
                PredictStrategy.AutoConservative => isDmg && isSafe,
                PredictStrategy.AutoGreedy => isDmg && (isSafe || EnoughHP),
                PredictStrategy.AutoSuperGreedy => !isSafe,
                PredictStrategy.HealOnly => isHeal,
                _ => false
            };

            if (canUse && haveTarget || isLastPrediction)
                UseAction(GetID(pred), Player, predictOpt.Priority(ActionQueue.Priority.High));
        }
    }

    private bool EnoughHP => Player.HPMP.MaxHP * 0.9f < Player.HPMP.CurHP + Player.HPMP.Shield;

    private (Prediction pred, int flags) GetPrediction()
    {
        var deadline = World.Client.AnimationLock;

        foreach (var sid in Player.Statuses)
        {
            if (sid.ExpireAt < World.FutureTime(deadline))
                continue;

            switch ((PhantomSID)sid.ID)
            {
                case PhantomSID.PredictionOfJudgment:
                    return (Prediction.Judgment, sid.Extra);
                case PhantomSID.PredictionOfCleansing:
                    return (Prediction.Cleansing, sid.Extra);
                case PhantomSID.PredictionOfBlessing:
                    return (Prediction.Blessing, sid.Extra);
                case PhantomSID.PredictionOfStarfall:
                    return (Prediction.Starfall, sid.Extra);
            }
        }

        return default;
    }

    private PhantomID GetID(Prediction p) => p switch
    {
        Prediction.Judgment => PhantomID.PhantomJudgment,
        Prediction.Cleansing => PhantomID.Cleansing,
        Prediction.Blessing => PhantomID.Blessing,
        Prediction.Starfall => PhantomID.Starfall,
        _ => PhantomID.None
    };

    public static readonly uint[] InstantCastStatus = [
        (uint)ClassShared.SID.Swiftcast,
        (uint)BossMod.RDM.SID.Dualcast,
        (uint)BossMod.BLM.SID.Triplecast,
        (uint)BossMod.PLD.SID.Requiescat,
        (uint)PhantomSID.OccultQuick
    ];

    // returns true if the action is ready to be used, so we can add movement hints for e.g. maximizing aoe targets
    private bool UseAction(PhantomID pid, Actor target, float prio, float castTime = 0)
    {
        var cd = pid is PhantomID.PhantomJudgment or PhantomID.Cleansing or PhantomID.Blessing or PhantomID.Starfall ? 0 : DutyActionCD(ActionID.MakeSpell(pid));

        if (cd <= GCD)
        {
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(pid), target, prio, castTime: castTime);
            return true;
        }
        return false;
    }

    public static readonly uint[] BreakableComboStatus = [
        (uint)BossMod.NIN.SID.Mudra,
        (uint)BossMod.NIN.SID.TenChiJin,
        (uint)BossMod.RDM.SID.Dualcast,
        (uint)BossMod.DRG.SID.DraconianFire,
        (uint)BossMod.RPR.SID.SoulReaver
    ];

    private bool CheckMidCombo()
    {
        return Player.Statuses.Any(s => BreakableComboStatus.Contains(s.ID));
    }
}
