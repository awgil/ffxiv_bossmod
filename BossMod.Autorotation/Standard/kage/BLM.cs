using BossMod.BLM;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using static BossMod.AIHints;

namespace BossMod.Autorotation.kage;

public sealed class KageBLM(RotationModuleManager manager, Actor player) : TypedRotationModule<KageBLM.Strategy>(manager, player)
{
    public struct Strategy
    {
        public Track<Targeting> Targeting;
        public Track<AOEStrategy> AOE;

        [Track("Ley Lines", MinLevel = 52, Action = AID.LeyLines)]
        public Track<LeyLinesStrategy> LeyLines;

        [Track("Manafont", MinLevel = 30, Action = AID.Manafont)]
        public Track<OffensiveStrategy> Manafont;

        [Track("Amplifier", MinLevel = 86, Action = AID.Amplifier)]
        public Track<OffensiveStrategy> Amplifier;

        [Track("Polyglot", MinLevel = 70, Actions = [AID.Xenoglossy, AID.Foul])]
        public Track<PolyglotStrategy> Polyglot;

        [Track("Thunder", MinLevel = 6, Actions = [AID.HighThunder, AID.Thunder3, AID.Thunder1, AID.HighThunder2, AID.Thunder4, AID.Thunder2])]
        public Track<OffensiveStrategy> Thunder;

        [Track("Triplecast / Swiftcast (movement)", MinLevel = 18, Actions = [AID.Triplecast, ClassShared.AID.Swiftcast])]
        public Track<MovementStrategy> Movement;

        [Track("Manaward", MinLevel = 30, Action = AID.Manaward)]
        public Track<ManawardStrategy> Manaward;

        [Track("Potion")]
        public Track<PotionStrategy> Potion;

        [Track("Opener", MinLevel = 2, UiPriority = -10, Context = StrategyContext.Plan)]
        public Track<OpenerStrategy> Opener;
    }

    public enum LeyLinesStrategy
    {
        [Option("Use at 2 charges when movement allows 2.5s of standing still; not on trash about to die")]
        Automatic,
        [Option("Use as soon as possible")]
        ASAP,
        [Option("Do not use")]
        Delay
    }

    public enum PolyglotStrategy
    {
        [Option("Spend in Astral Fire above 1 stack (one kept for movement); always before overcapping, before downtime and on a dying target")]
        Automatic,
        [Option("Spend as soon as available")]
        ASAP,
        [Option("Only to avoid overcapping")]
        Overcap,
        [Option("Do not use")]
        Delay
    }

    public enum MovementStrategy
    {
        [Option("Triplecast, then Swiftcast, while moving with only casts available")]
        Automatic,
        [Option("Do not use for movement")]
        Delay
    }

    public enum ManawardStrategy
    {
        [Option("Before predicted raidwides")]
        Automatic,
        [Option("Do not use")]
        Delay
    }

    public enum PotionStrategy
    {
        [Option("Do not use automatically")]
        Manual,
        [Option("Use in the opener and with Ley Lines")]
        AlignWithBurst,
        [Option("Use as soon as possible")]
        Immediate
    }

    public enum OpenerStrategy
    {
        [Option("Fire III at -4s")]
        Standard,
        [Option("No pre-pull cast")]
        None
    }

    public static RotationModuleDefinition Definition()
    {
        return new RotationModuleDefinition("Kage BLM", "Black Mage", "Standard rotation (Kage)|Caster", "Kagekazu", RotationModuleQuality.WIP, BitMask.Build(Class.BLM, Class.THM), 100).WithStrategies<Strategy>();
    }

    private const uint CircleOfPowerSID = 738;
    private const uint SwiftcastSID = 167;

    private int Astral;
    private int Umbral;
    private int Hearts;
    private int Polyglot;
    private int AstralSoul;
    private bool Paradox;
    private float EnochianLeft;
    private float FirestarterLeft;
    private float ThunderheadLeft;
    private bool Instant;
    private bool InLeyLines;

    private float DowntimeIn;
    private bool AOEMode;
    private int NumAOETargets;
    private bool IsMoving;
    private float AnimLockDelay;
    private Targeting TargetMode;

    private float GCDLength => ActionSpeed.GCDRounded(World.Client.PlayerStats.SpellSpeed, World.Client.PlayerStats.Haste, Player.Level);
    private int MP => (int)Player.HPMP.CurMP;
    private bool MPFull => Player.HPMP.CurMP >= Player.HPMP.MaxMP;
    private int MaxPolyglot => Player.Level >= 98 ? 3 : Player.Level >= 80 ? 2 : 1;
    private bool InFire => Astral > 0;
    private bool InIce => Umbral > 0;

    public override void Execute(in Strategy strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        AnimLockDelay = estimatedAnimLockDelay;
        IsMoving = isMoving;

        var target = Hints.FindEnemy(primaryTarget);
        if (target?.Priority is Enemy.PriorityInvincible or Enemy.PriorityForbidden || target?.Priority == Enemy.PriorityPointless && Hints.PriorityTargets.Any())
            target = null;

        TargetMode = strategy.Targeting.Value == Targeting.AutoTryPri ? (target != null ? Targeting.AutoPrimary : Targeting.Auto) : strategy.Targeting.Value;
        if (TargetMode == Targeting.Auto && target == null)
        {
            target = Hints.PriorityTargets.Where(e => Player.DistanceToHitbox(e.Actor) <= 25).MinBy(e => Player.DistanceToHitbox(e.Actor)) ?? target;
            primaryTarget = target?.Actor;
        }

        var gauge = World.Client.GetGauge<BlackMageGauge>();
        Astral = gauge.AstralStacks;
        Umbral = gauge.UmbralStacks;
        Hearts = gauge.UmbralHearts;
        Polyglot = gauge.PolyglotStacks;
        AstralSoul = gauge.AstralSoulStacks;
        Paradox = gauge.ParadoxActive;
        EnochianLeft = gauge.EnochianTimer / 1000f;
        FirestarterLeft = SelfStatusLeft(SID.Firestarter);
        ThunderheadLeft = SelfStatusLeft(SID.Thunderhead);
        Instant = SelfStatusLeft(SID.Triplecast) > 0 || SelfStatusLeft(SwiftcastSID) > 0;
        InLeyLines = SelfStatusLeft(CircleOfPowerSID) > 0;

        DowntimeIn = Manager.Planner?.EstimateTimeToNextDowntime() is (var downNow, var stateLeft) ? (downNow ? 0 : stateLeft) : float.MaxValue;
        NumAOETargets = target == null ? 0 : Hints.NumPriorityTargetsInAOECircle(target.Actor.Position, 5);
        AOEMode = Unlocked(AID.Fire2) && strategy.AOE.Value switch
        {
            AOEStrategy.ForceAOE => true,
            AOEStrategy.AOE => NumAOETargets >= 2,
            _ => false
        };

        if (UsePotion(strategy))
            Hints.ActionsToExecute.Push(ActionDefinitions.IDPotionInt, Player, ActionQueue.Priority.Medium);

        if (World.Client.CountdownRemaining is > 0 and var countdown)
        {
            if (target != null && strategy.Opener.Value == OpenerStrategy.Standard && countdown < 4 && Unlocked(AID.Fire3))
                PushGCD(AID.Fire3, target.Actor, 10);
            return;
        }

        if (target == null)
            return;

        Hints.GoalZones.Add(Hints.GoalSingleTarget(target.Actor, Player, World.Actors, 25));

        GCDs(strategy, target);
        if (Player.InCombat)
            OGCDs(strategy, target);
    }

    #region GCD

    private void GCDs(in Strategy strategy, Enemy target)
    {
        var dying = target.Actor.HPRatio < 0.03f && !target.Actor.IsStrikingDummy;
        var polyAction = AOEMode && Unlocked(AID.Foul) || !Unlocked(AID.Xenoglossy) ? AID.Foul : AID.Xenoglossy;

        // Polyglot: never overcap, dump before downtime / on a dying target; otherwise kept for movement
        if (Polyglot > 0 && strategy.Polyglot.Value != PolyglotStrategy.Delay)
        {
            var overcap = Polyglot >= MaxPolyglot && (EnochianLeft <= 5 || Unlocked(AID.Amplifier) && ReadyIn(AID.Amplifier) < 5);
            var spend = strategy.Polyglot.Value switch
            {
                PolyglotStrategy.ASAP => true,
                PolyglotStrategy.Overcap => overcap,
                _ => overcap || dying || DowntimeIn < GCDLength * Polyglot + 1 || InFire && Polyglot > 1
            };
            if (spend)
                PushGCD(polyAction, target.Actor, 70);
            // movement fallback: instant
            PushGCD(polyAction, target.Actor, 5);
        }

        // Thunder from Thunderhead when the DoT is about to fall off
        if (strategy.Thunder.Value != OffensiveStrategy.Delay && ThunderheadLeft > GCD && (strategy.Thunder.Value == OffensiveStrategy.Force || ThunderLeft(target.Actor) < 3 && target.Actor.HPRatio > ThunderHPThreshold(target.Actor)))
            PushGCD(ThunderAction, target.Actor, 65);

        if (AOEMode)
            AoE(target);
        else if (InFire)
            Fire(target);
        else if (InIce)
            Ice(target);
        else
            PushGCD(MP < 7500 && Unlocked(AID.Blizzard3) ? AID.Blizzard3 : Unlocked(AID.Fire3) ? AID.Fire3 : AID.Fire1, target.Actor, 20);
    }

    // Astral Fire: Fire III (Firestarter) to AF3 > Flare Star > Fire IV (keep 800 MP for Despair) > Paradox > Despair
    private void Fire(Enemy target)
    {
        var f4Cost = Hearts > 0 ? 800 : 1600;

        // AF1 after Transpose: Firestarter Fire III, or Paradox to make one
        if (Astral < 3)
        {
            if (FirestarterLeft > GCD)
                PushGCD(AID.Fire3, target.Actor, 50);
            else if (Paradox && Unlocked(AID.Paradox))
                PushGCD(AID.Paradox, target.Actor, 50);
            else if (Unlocked(AID.Fire3))
                PushGCD(AID.Fire3, target.Actor, 49);
        }

        if (Unlocked(AID.FlareStar) && AstralSoul >= 6)
            PushGCD(AID.FlareStar, target.Actor, 45);

        if (Unlocked(AID.Fire4))
        {
            if (MP - f4Cost >= (Unlocked(AID.Despair) ? 800 : 0))
                PushGCD(AID.Fire4, target.Actor, 40);
        }
        else if (MP >= 1600 + (Unlocked(AID.Despair) ? 800 : 0))
        {
            PushGCD(AID.Fire1, target.Actor, 40);
        }

        // Paradox in AF3 (Firestarter is carried to the next Umbral Ice)
        if (Paradox && Unlocked(AID.Paradox) && MP >= 1600)
            PushGCD(AID.Paradox, target.Actor, 38);

        if (Unlocked(AID.Despair) && MP >= 800)
            PushGCD(AID.Despair, target.Actor, 35);

        // out of MP: Manafont is an oGCD; otherwise switch to ice (instant Blizzard III via Transpose + Swiftcast/Triplecast)
        if (MP < 800 && !(Unlocked(AID.Manafont) && ReadyIn(AID.Manafont) <= GCD))
            PushGCD(Unlocked(AID.Blizzard3) ? AID.Blizzard3 : AID.Blizzard1, target.Actor, 20);

        // movement fallbacks
        if (FirestarterLeft > GCD && Astral == 3)
            PushGCD(AID.Fire3, target.Actor, 6);
    }

    // Umbral Ice: Blizzard III to UI3 > Blizzard IV (hearts) > Paradox (if holding Firestarter) > Transpose + Fire III
    private void Ice(Enemy target)
    {
        if (Umbral < 3 && Unlocked(AID.Blizzard3))
            PushGCD(AID.Blizzard3, target.Actor, 50);

        if (Unlocked(AID.Blizzard4) && Hearts < 3)
            PushGCD(AID.Blizzard4, target.Actor, 45);
        else if (!Unlocked(AID.Blizzard4) && !MPFull)
            PushGCD(AID.Blizzard1, target.Actor, 45);

        // Paradox here only when a Firestarter is held; otherwise it moves to AF1 to make one
        if (Paradox && Unlocked(AID.Paradox) && (FirestarterLeft > 0 || !Unlocked(AID.Transpose)))
            PushGCD(AID.Paradox, target.Actor, 42);

        if (MPFull && (Hearts >= 3 || !Unlocked(AID.Blizzard4)))
        {
            // without Transpose ready (or a Firestarter to spend), hard-cast Fire III at half cast time
            if (!CanUseTranspose || FirestarterLeft == 0 && !Paradox)
                PushGCD(Unlocked(AID.Fire3) ? AID.Fire3 : AID.Fire1, target.Actor, 40);
        }

        if (Paradox && Unlocked(AID.Paradox))
            PushGCD(AID.Paradox, target.Actor, 6);
    }

    // AoE: Freeze (Blizzard IV at 2 targets) > Transpose > Flare x2 > Flare Star > Transpose; Foul / Thunder II as fillers
    private void AoE(Enemy target)
    {
        var fireAoE = Unlocked(AID.HighFire2) ? AID.HighFire2 : AID.Fire2;
        var iceAoE = Unlocked(AID.HighBlizzard2) ? AID.HighBlizzard2 : AID.Blizzard2;
        if (InFire)
        {
            if (Unlocked(AID.FlareStar) && AstralSoul >= 6)
                PushGCD(AID.FlareStar, target.Actor, 50);
            if (Unlocked(AID.Flare) && MP >= (Hearts > 0 ? 800 : 1000))
                PushGCD(AID.Flare, target.Actor, 45);
            else if (!Unlocked(AID.Flare) && MP >= 3000)
                PushGCD(fireAoE, target.Actor, 45);
            if (MP < 800 && !CanUseTranspose && !(Unlocked(AID.Manafont) && ReadyIn(AID.Manafont) <= GCD))
                PushGCD(iceAoE, target.Actor, 20);
        }
        else if (InIce)
        {
            var hearts = NumAOETargets == 2 && Unlocked(AID.Blizzard4) ? AID.Blizzard4 : Unlocked(AID.Freeze) ? AID.Freeze : iceAoE;
            if ((Hearts < 3 || !Unlocked(AID.Freeze)) && !(MP >= 5000 && Unlocked(AID.Flare) && Hearts >= 3))
                PushGCD(hearts, target.Actor, 45);
            if (Paradox && Unlocked(AID.Paradox))
                PushGCD(AID.Paradox, target.Actor, 30);
            if (!CanUseTranspose && (MPFull || Hearts >= 3))
                PushGCD(fireAoE, target.Actor, 25);
        }
        else
        {
            PushGCD(iceAoE, target.Actor, 20);
        }
    }

    #endregion

    #region oGCD

    private void OGCDs(in Strategy strategy, Enemy target)
    {
        // Manafont at the end of Astral Fire for a second fire phase
        if (strategy.Manafont.Value != OffensiveStrategy.Delay && InFire && CanWeave(AID.Manafont) && (strategy.Manafont.Value == OffensiveStrategy.Force || MP < 800 && AstralSoul < 6))
            PushOGCD(AID.Manafont, Player, 70);

        // phase swaps
        if (CanUseTranspose)
        {
            var endOfFire = InFire && MP < 800 && !(Unlocked(AID.Manafont) && ReadyIn(AID.Manafont) < 5) && AstralSoul < 6;
            if (endOfFire && (Instant || !Unlocked(AID.Blizzard3)))
                PushOGCD(AID.Transpose, Player, 65);
            var endOfIce = InIce && MPFull && (Hearts >= 3 || !Unlocked(AID.Blizzard4)) && (!Paradox || FirestarterLeft == 0);
            if (endOfIce && !AOEMode && (FirestarterLeft > GCD || Paradox))
                PushOGCD(AID.Transpose, Player, 65);
            if (AOEMode && (InFire && MP < 800 && AstralSoul < 6 && !(Unlocked(AID.Manafont) && ReadyIn(AID.Manafont) <= GCD) || InIce && (Hearts >= 3 || MP >= 5000 && Unlocked(AID.Flare))))
                PushOGCD(AID.Transpose, Player, 65);
        }

        // instant Blizzard III after Despair: Swiftcast (or Triplecast) then Transpose
        if (!AOEMode && InFire && MP < 800 && Astral == 3 && !Instant && !(Unlocked(AID.Manafont) && ReadyIn(AID.Manafont) < 5) && AstralSoul < 6)
        {
            if (CanWeave(ClassShared.AID.Swiftcast))
                PushOGCD(ClassShared.AID.Swiftcast, Player, 66);
            else if (CanWeave(AID.Triplecast) && SelfStatusLeft(SID.Triplecast) == 0 && !InLeyLines && TriplecastCharges >= 2)
                PushOGCD(AID.Triplecast, Player, 66);
        }

        if (strategy.Amplifier.Value != OffensiveStrategy.Delay && CanWeave(AID.Amplifier) && (strategy.Amplifier.Value == OffensiveStrategy.Force || Polyglot < MaxPolyglot))
            PushOGCD(AID.Amplifier, Player, 60);

        // keep one charge spare (press when both are up); not on trash about to die
        if (strategy.LeyLines.Value != LeyLinesStrategy.Delay && CanWeave(AID.LeyLines) && !InLeyLines && SelfStatusLeft(SID.LeyLines) == 0
            && (strategy.LeyLines.Value == LeyLinesStrategy.ASAP || !IsMoving && Hints.MaxCastTime >= 2.5f && LeyLinesCharges >= 2 && target.Actor.HPRatio > BossHPThreshold(target.Actor, 0, 0.25f) && DowntimeIn > 15))
            PushOGCD(AID.LeyLines, Player, 55);

        // movement: Triplecast (not in Ley Lines), then Swiftcast, when only casts are available
        if (strategy.Movement.Value == MovementStrategy.Automatic && IsMoving && !Instant && Hints.MaxCastTime < 1.5f && Polyglot == 0 && ThunderheadLeft == 0 && !(InFire && FirestarterLeft > 0))
        {
            if (!InLeyLines && CanWeave(AID.Triplecast))
                PushOGCD(AID.Triplecast, Player, 50);
            else if (CanWeave(ClassShared.AID.Swiftcast))
                PushOGCD(ClassShared.AID.Swiftcast, Player, 49);
        }

        if (strategy.Manaward.Value == ManawardStrategy.Automatic && CanWeave(AID.Manaward) && Hints.PredictedDamage.Any(d => d.Type is PredictedDamageType.Raidwide or PredictedDamageType.Shared && d.Activation > World.CurrentTime && d.Activation <= World.FutureTime(4)))
            PushOGCD(AID.Manaward, Player, 45);
    }

    private bool CanUseTranspose => CanWeave(AID.Transpose);

    // HP floor: bosses 0%, adds in boss fights 10%, trash 25%
    private float BossHPThreshold(Actor target, float adds, float trash)
        => Bossmods.ActiveModule is { } m ? (target == m.PrimaryActor || target.IsStrikingDummy ? 0 : adds) : target.IsStrikingDummy ? 0 : trash;
    private float ThunderHPThreshold(Actor target) => BossHPThreshold(target, 0.1f, 0.25f);

    #endregion

    #region Helpers

    private AID ThunderAction => AOEMode
        ? (Unlocked(AID.HighThunder2) ? AID.HighThunder2 : Unlocked(AID.Thunder4) ? AID.Thunder4 : AID.Thunder2)
        : (Unlocked(AID.HighThunder) ? AID.HighThunder : Unlocked(AID.Thunder3) ? AID.Thunder3 : AID.Thunder1);

    private float ThunderLeft(Actor target)
    {
        float Left(SID sid) => target.FindStatus((uint)sid, Player.InstanceID) is { } st ? StatusDuration(st.ExpireAt) : 0;
        return MathF.Max(MathF.Max(Left(SID.HighThunder), Left(SID.HighThunderII)), MathF.Max(MathF.Max(Left(SID.Thunder), Left(SID.ThunderII)), MathF.Max(Left(SID.ThunderIII), Left(SID.ThunderIV))));
    }

    private float CastTime(AID aid)
    {
        if (Instant)
            return 0;
        var def = ActionDefinitions.Instance.Spell(aid);
        if (def == null || def.CastTime == 0)
            return 0;
        var t = def.CastTime * GCDLength / 2.5f;
        var isFire = aid is AID.Fire1 or AID.Fire3 or AID.Fire4 or AID.Fire2 or AID.HighFire2 or AID.Flare or AID.Despair or AID.FlareStar;
        var isIce = aid is AID.Blizzard1 or AID.Blizzard3 or AID.Blizzard4 or AID.Blizzard2 or AID.HighBlizzard2 or AID.Freeze;
        if (isFire && Umbral == 3 || isIce && Astral == 3)
            t *= 0.5f;
        if (aid == AID.Fire3 && FirestarterLeft > GCD)
            t = 0;
        return t;
    }

    private bool UsePotion(in Strategy strategy) => strategy.Potion.Value switch
    {
        PotionStrategy.AlignWithBurst => World.Client.CountdownRemaining is > 0 and < 2 || Player.InCombat && Unlocked(AID.LeyLines) && ReadyIn(AID.LeyLines) < 3 && InFire,
        PotionStrategy.Immediate => true,
        _ => false
    };

    private int LeyLinesCharges => Charges(AID.LeyLines);
    private int TriplecastCharges => Charges(AID.Triplecast);
    private int Charges(AID aid)
    {
        if (!Unlocked(aid))
            return 0;
        var def = ActionDefinitions.Instance.Spell(aid)!;
        var max = def.MaxChargesAtLevel(Player.Level);
        var capIn = def.ChargeCapIn(World.Client.Cooldowns, World.Client.DutyActions, Player.Level);
        return max - (int)MathF.Ceiling(capIn / def.Cooldown);
    }

    private bool Unlocked(AID aid) => ActionUnlocked(ActionID.MakeSpell(aid));
    private float ReadyIn(AID aid) => Unlocked(aid) ? ActionDefinitions.Instance.Spell(aid)!.ReadyIn(World.Client.Cooldowns, World.Client.DutyActions) : float.MaxValue;

    private bool CanWeave(AID aid) => CanWeave(ActionID.MakeSpell(aid));
    private bool CanWeave(ClassShared.AID aid) => CanWeave(ActionID.MakeSpell(aid));
    private bool CanWeave(ActionID action)
    {
        if (!ActionUnlocked(action) || ActionDefinitions.Instance[action] is not { } def)
            return false;
        return MathF.Max(def.ReadyIn(World.Client.Cooldowns, World.Client.DutyActions), World.Client.AnimationLock) + def.TotalDuration + AnimLockDelay <= GCD;
    }

    private void PushGCD(AID aid, Actor? target, int priority)
    {
        if (!Unlocked(aid))
            return;
        var action = ActionID.MakeSpell(aid);
        var def = ActionDefinitions.Instance[action];
        if (def == null || def.Range != 0 && target == null)
            return;
        Hints.ActionsToExecute.Push(action, def.Range == 0 ? Player : target, ActionQueue.Priority.High + priority, castTime: CastTime(aid));
    }

    private void PushOGCD(AID aid, Actor? target, int priority) => PushOGCD(ActionID.MakeSpell(aid), target, priority);
    private void PushOGCD(ClassShared.AID aid, Actor? target, int priority) => PushOGCD(ActionID.MakeSpell(aid), target, priority);
    private void PushOGCD(ActionID action, Actor? target, int priority)
    {
        if (!ActionUnlocked(action))
            return;
        Hints.ActionsToExecute.Push(action, target, ActionQueue.Priority.Low + priority);
    }

    #endregion
}
