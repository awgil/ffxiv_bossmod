using BossMod.Data;

namespace BossMod.Autorotation.xan;

public class PhantomAI(RotationModuleManager manager, Actor player) : AIBase<PhantomAI.Strategy>(manager, player)
{
    public struct Strategy
    {
        [Track("Cannoneer: Use cannon actions on best AOE target as soon as they become available", Actions = [PhantomID.DarkCannon, PhantomID.HolyCannon, PhantomID.ShockCannon, PhantomID.SilverCannon, PhantomID.PhantomFire])]
        public Track<EnabledByDefault> Cannoneer;

        [Track("Ranger: Use Phantom Aim on cooldown", Action = PhantomID.PhantomAim)]
        public Track<EnabledByDefault> Ranger;

        [Track("Time Mage: Use Comet ASAP if it will be instant", Actions = [PhantomID.OccultComet, PhantomID.OccultQuick])]
        public Track<EnabledByDefault> TimeMage;

        [Track("Chemist: Raise", Action = PhantomID.Revive)]
        public Track<RaiseStrategy> Chemist;

        [Track("Samurai: Use Iainuki on best AOE target", Action = PhantomID.Iainuki)]
        public Track<EnabledByDefault> Samurai;

        [Track("Bard: Use Aria/Rime in combat", Actions = [PhantomID.OffensiveAria, PhantomID.HerosRime])]
        public Track<EnabledByDefault> Bard;

        [Track("Monk: Use Kick to maintain buff, use Counterstance during downtime", Actions = [PhantomID.PhantomKick, PhantomID.Counterstance, PhantomID.OccultCounter])]
        public Track<EnabledByDefault> Monk;

        [Track("Monk: Use Occult Chakra", Action = PhantomID.OccultChakra)]
        public Track<ChakraStrategy> Chakra;

        [Track("Oracle: Predict", Actions = [PhantomID.Predict, PhantomID.PhantomJudgment, PhantomID.Cleansing, PhantomID.Blessing, PhantomID.Starfall])]
        public Track<PredictStrategy> Predict;

        [Track("Mystic Knight: Use blades/Magic Shell on cooldown", Actions = [PhantomID.SunderingSpellblade, PhantomID.MagicShell, PhantomID.HolySpellblade, PhantomID.BlazingSpellblade])]
        public Track<EnabledByDefault> MysticKnight;

        [Track("Gladiator: Finish", Actions = [PhantomID.Finisher, PhantomID.Defend, PhantomID.LongReach, PhantomID.BladeBlitz])]
        public Track<EnabledByDefault> Gladiator;

        [Track("Dancer: Dance", Actions = [PhantomID.Dance, PhantomID.PhantomSwordDance, PhantomID.TemptingTango, PhantomID.Jitterbug, PhantomID.MysteryWaltz, PhantomID.Quickstep])]
        public Track<EnabledByDefault> Dancer;
    }

    public enum RaiseStrategy
    {
        [Option("Disabled")]
        Never,
        [Option("Out of combat")]
        OutOfCombat,
        [Option("Always")]
        InCombat
    }

    public enum PredictStrategy
    {
        [Option("Use first available damage action that isn't Starfall")]
        AutoConservative,
        [Option("Use first available damage action, including Starfall if HP is high enough")]
        AutoGreedy,
        [Option("Use Starfall")]
        AutoSuperGreedy,
        [Option("Use Blessing (heal)")]
        HealOnly,
        [Option("Don't use")]
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
        [Option("If HP or MP is low")]
        Any,
        [Option("If HP is below 30%")]
        HP,
        [Option("If MP is below 30%")]
        MP,
        [Option("Never")]
        Disabled
    }

    public static RotationModuleDefinition Definition()
    {
        return new RotationModuleDefinition("Phantom Job AI", "Basic phantom job action automation", "AI (xan)", "xan", RotationModuleQuality.WIP, new(~0ul), MaxLevel: 100).WithStrategies<Strategy>();
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

    float DesiredRange = float.MaxValue;

    public override void Execute(in Strategy strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        if (World.Client.CountdownRemaining > 0)
            return;

        DesiredRange = float.MaxValue;

        var isMidCombo = CheckMidCombo();

        if (strategy.Cannoneer.IsEnabled() && !isMidCombo)
        {
            var prio = strategy.Cannoneer.Priority(ActionQueue.Priority.High + 500);

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

        if (strategy.Ranger.IsEnabled() && primaryTarget?.IsAlly == false && Player.InCombat)
        {
            var prio = strategy.Ranger.Priority(ActionQueue.Priority.Low);

            UseAction(PhantomID.PhantomAim, Player, prio);
        }

        if (strategy.TimeMage.IsEnabled() && primaryTarget?.IsAlly == false)
        {
            var prio = strategy.TimeMage.Priority(ActionQueue.Priority.High + 500);

            var nextGCD = World.FutureTime(GCD);
            var haveSwift = Player.Statuses.Any(s => InstantCastStatus.Contains(s.ID) && s.ExpireAt > nextGCD);
            if (haveSwift)
                UseAction(PhantomID.OccultComet, primaryTarget, prio);

            UseAction(PhantomID.OccultQuick, Player, prio);
        }

        var option = strategy.Chemist;
        var canRaise = option.Value switch
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

        if (strategy.Samurai.IsEnabled() && primaryTarget?.IsAlly == false && !isMidCombo)
        {
            var prio = strategy.Samurai.Priority(ActionQueue.Priority.High + 500);
            if (UseAction(PhantomID.Iainuki, primaryTarget, prio, 0.8f))
                Hints.GoalZones.Add(Hints.GoalAOECone(primaryTarget, 8, 60.Degrees()));
        }

        if (strategy.Bard.IsEnabled() && primaryTarget?.IsAlly == false && Player.InCombat)
        {
            var ariaLeft = SelfStatusDetails(PhantomSID.OffensiveAria, 70).Left;
            var rimeLeft = SelfStatusDetails(PhantomSID.HerosRime, 20).Left;
            var prio = strategy.Bard.Priority(ActionQueue.Priority.Low);

            UseAction(PhantomID.HerosRime, Player, prio);

            if (ariaLeft < 10 && rimeLeft < World.Client.AnimationLock)
                UseAction(PhantomID.OffensiveAria, Player, prio);
        }

        if (strategy.Monk.IsEnabled() && Player.InCombat)
        {
            var prio = strategy.Monk.Priority(ActionQueue.Priority.Low);

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

        var chakraOpt = strategy.Chakra;
        if (chakraOpt.Value != ChakraStrategy.Disabled && Player.InCombat)
        {
            var lowHP = Player.HPMP.CurHP < Player.HPMP.MaxHP * 0.3f;
            var lowMP = Player.HPMP.CurMP < Player.HPMP.MaxMP * 0.3f;

            var useOk = chakraOpt.Value switch
            {
                ChakraStrategy.Any => lowHP || lowMP,
                ChakraStrategy.HP => lowHP,
                ChakraStrategy.MP => lowMP,
                _ => false
            };

            if (useOk)
                UseAction(PhantomID.OccultChakra, Player, chakraOpt.Priority(ActionQueue.Priority.Low));
        }

        var predictOpt = strategy.Predict;
        if (predictOpt != PredictStrategy.Disabled)
        {
            var (pred, flags) = GetPrediction();

            var haveTarget = Player.InCombat && predictOpt.Value switch
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

            var canUse = predictOpt.Value switch
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

        if (strategy.MysticKnight.IsEnabled())
        {
            if (primaryTarget?.IsAlly == false)
            {
                if (primaryTarget.FindStatus(PhantomSID.BlazingBane, DateTime.MaxValue) == null)
                    UseAction(PhantomID.BlazingSpellblade, primaryTarget, ActionQueue.Priority.VeryHigh);

                UseAction(PhantomID.HolySpellblade, primaryTarget, ActionQueue.Priority.VeryHigh);
                UseAction(PhantomID.SunderingSpellblade, primaryTarget, ActionQueue.Priority.VeryHigh);
            }

            // shell has permanent uptime (60s duration, 60s cooldown)
            UseAction(PhantomID.MagicShell, Player, ActionQueue.Priority.High);
        }

        if (strategy.Gladiator.IsEnabled())
        {
            if (Hints.PredictedDamage.Any(p => p.Players[0] && p.Activation < World.FutureTime(4)) || EnemiesAutoingMe.Any())
                UseAction(PhantomID.Defend, Player, ActionQueue.Priority.High);

            if (primaryTarget?.IsAlly == false)
            {
                UseAction(PhantomID.Finisher, primaryTarget, ActionQueue.Priority.VeryHigh);
                UseAction(PhantomID.LongReach, primaryTarget, ActionQueue.Priority.VeryHigh);
                if (primaryTarget.Position.InCircle(Player.Position, Player.HitboxRadius + primaryTarget.HitboxRadius + 8))
                    UseAction(PhantomID.BladeBlitz, Player, ActionQueue.Priority.VeryHigh);
            }
        }

        if (strategy.Dancer.IsEnabled())
        {
            if (SelfStatusLeft(PhantomSID.Quickstep) < 5)
                UseAction(PhantomID.Quickstep, Player, ActionQueue.Priority.High);

            if (primaryTarget?.IsAlly == false)
            {
                UseAction(PhantomID.Dance, Player, ActionQueue.Priority.High);

                if (Player.FindStatus(PhantomSID.PoisedToSwordDance) != null)
                    UseAction(PhantomID.PhantomSwordDance, primaryTarget, ActionQueue.Priority.VeryHigh);
                if (Player.FindStatus(PhantomSID.TemptedToTango) != null)
                    UseAction(PhantomID.TemptingTango, primaryTarget, ActionQueue.Priority.VeryHigh);
                if (Player.FindStatus(PhantomSID.Jitterbugged) != null)
                    UseAction(PhantomID.Jitterbug, primaryTarget, ActionQueue.Priority.VeryHigh);
                if (Player.FindStatus(PhantomSID.WillingToWaltz) != null)
                    UseAction(PhantomID.MysteryWaltz, primaryTarget, ActionQueue.Priority.VeryHigh);
            }
        }

        if (DesiredRange < float.MaxValue && primaryTarget != null)
            Hints.GoalZones.Add(Hints.GoalSingleTarget(primaryTarget, DesiredRange, 1));
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

    bool IsTransformedAction(PhantomID p) => p is PhantomID.PhantomJudgment or PhantomID.Cleansing or PhantomID.Blessing or PhantomID.Starfall or PhantomID.PhantomSwordDance or PhantomID.TemptingTango or PhantomID.Jitterbug or PhantomID.MysteryWaltz;

    // returns true if the action is ready to be used, so we can add movement hints for e.g. maximizing aoe targets
    private bool UseAction(PhantomID pid, Actor target, float prio, float castTime = 0)
    {
        var action = ActionID.MakeSpell(pid);
        var cd = IsTransformedAction(pid) ? 0 : DutyActionCD(action);

        if (cd <= GCD)
        {
            DesiredRange = MathF.Min(DesiredRange, ActionDefinitions.Instance[action]?.Range ?? float.MaxValue);
            Hints.ActionsToExecute.Push(action, target, prio, castTime: castTime);
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
