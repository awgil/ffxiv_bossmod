using BossMod.BLM;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using static BossMod.AIHints;

namespace BossMod.Autorotation.kage;

public sealed class KageBLM(RotationModuleManager manager, Actor player) : KageRotation<KageBLM.Strategy>(manager, player)
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

        [Track("Opener", MinLevel = 2, UiPriority = -10)]
        public Track<OpenerStrategy> Opener;
    }

    public enum LeyLinesStrategy
    {
        [Option("Use with raid buffs while standing still; saved for the boss")]
        Automatic,
        [Option("Use as soon as possible")]
        ASAP,
        [Option("Do not use")]
        Delay
    }

    public enum PolyglotStrategy
    {
        [Option("Spend in Astral Fire with raid buffs, keep one for movement; never overcap")]
        Automatic,
        [Option("Use as soon as possible")]
        ASAP,
        [Option("Only to avoid overcapping")]
        Overcap,
        [Option("Do not use")]
        Delay
    }

    public enum MovementStrategy
    {
        [Option("Use Triplecast, then Swiftcast, for movement")]
        Automatic,
        [Option("Do not use")]
        Delay
    }

    public enum ManawardStrategy
    {
        [Option("Use before raidwides")]
        Automatic,
        [Option("Do not use")]
        Delay
    }

    public enum PotionStrategy
    {
        [Option("Do not use automatically")]
        Manual,
        [Option("Use before the pull and with Ley Lines")]
        AlignWithBurst,
        [Option("Use before the pull and with raid buffs")]
        AlignWithRaidBuffs,
        [Option("Use as soon as possible")]
        Immediate
    }

    public enum OpenerStrategy
    {
        [Option("Standard opener (Fire III at -4s)")]
        Standard,
        [Option("Flare opener (Despair before Manafont, Flare for a second Flare Star)", MinLevel = 100)]
        Flare,
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

    private bool AOEMode;
    private int NumAOETargets;
    private bool AoEExitIce;
    private OpenerStrategy OpenerMode;

    protected override bool UsesSpellSpeed => true;
    private bool ManafontReady => Unlocked(AID.Manafont) && ReadyIn(AID.Manafont) <= GCD;
    private int FireCost => Hearts > 0 ? 800 : 1600;
    private bool FlareFinisher => Unlocked(AID.Flare) && !Unlocked(AID.Fire4);
    private bool FireMPOut => MP < (Unlocked(AID.Despair) || FlareFinisher ? 800 : FireCost);
    private bool AoEFireMPOut => Unlocked(AID.Flare) ? MP < (Hearts > 0 ? 800 : 1000) : MP < 3000;
    private bool OpenerManafont => OpenerMode == OpenerStrategy.Standard && CombatTime < 30 && ManafontReady && InFire && AstralSoul < 6 && MP < (Hearts > 0 ? 800 : 1600) + 800;
    private bool FlareOpenerFinish => OpenerMode == OpenerStrategy.Flare && CombatTime < 60 && Unlocked(AID.FlareStar) && Unlocked(AID.Manafont) && ReadyIn(AID.Manafont) > 30 && AstralSoul < 6;
    private bool AoECastSwap => Unlocked(AID.Fire3) && !Unlocked(AID.Freeze);
    private bool InOpener => OpenerMode is OpenerStrategy.Standard or OpenerStrategy.Flare && CombatTime < 35;
    private int MP => (int)Player.HPMP.CurMP;
    private bool MPFull => Player.HPMP.CurMP >= Player.HPMP.MaxMP;
    private int MaxPolyglot => Player.Level >= 98 ? 3 : Player.Level >= 80 ? 2 : 1;
    private bool InFire => Astral > 0;
    private bool InIce => Umbral > 0;
    private bool CantCast => Hints.MaxCastTime < SlideCastTime(InIce ? (Unlocked(AID.Blizzard4) ? AID.Blizzard4 : AID.Blizzard1) : (Unlocked(AID.Fire4) ? AID.Fire4 : AID.Fire1));

    public override void Execute(in Strategy strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        var target = SelectTarget(strategy.Targeting.Value, ref primaryTarget, estimatedAnimLockDelay, 25);
        OpenerMode = strategy.Opener.Value == OpenerStrategy.Flare && !Unlocked(AID.FlareStar) ? OpenerStrategy.Standard : strategy.Opener.Value;

        var gauge = World.Client.GetGauge<BlackMageGauge>();
        Astral = gauge.AstralStacks;
        Umbral = gauge.UmbralStacks;
        Hearts = gauge.UmbralHearts;
        Polyglot = gauge.PolyglotStacks;
        AstralSoul = gauge.AstralSoulStacks;
        Paradox = gauge.ParadoxActive;
        EnochianLeft = gauge.EnochianTimer / 1000f;
        FirestarterLeft = ProcLeft(SID.Firestarter);
        ThunderheadLeft = ProcLeft(SID.Thunderhead);
        Instant = SelfStatusLeft(SID.Triplecast) > 0 || SelfStatusLeft(SwiftcastSID) > 0;
        InLeyLines = SelfStatusLeft(CircleOfPowerSID) > 0;

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
            if (target != null && OpenerMode != OpenerStrategy.None && countdown < 4 && Astral == 0 && Unlocked(AID.Fire3))
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
        var dying = !TimeToKill.WillLive(target.Actor, 5);
        var polyAction = AOEMode && Unlocked(AID.Foul) || !Unlocked(AID.Xenoglossy) ? AID.Foul : AID.Xenoglossy;

        if (Polyglot > 0 && strategy.Polyglot.Value != PolyglotStrategy.Delay)
        {
            var overcap = Polyglot >= MaxPolyglot && (EnochianLeft <= 5 || Unlocked(AID.Amplifier) && ReadyIn(AID.Amplifier) < 5);
            var spend = strategy.Polyglot.Value switch
            {
                PolyglotStrategy.ASAP => true,
                PolyglotStrategy.Overcap => overcap,
                _ => overcap || dying || DowntimeIn < GCDLength * Polyglot + 1 || InFire && !HoldPolyglotForBuffs && (Polyglot > 1 || BuffsActive || Player.Level < 80)
            };
            if (spend)
                PushGCD(polyAction, target.Actor, 70);
            if (CantCast)
                PushGCD(polyAction, target.Actor, 5);
        }

        if (strategy.Thunder.Value != OffensiveStrategy.Delay && ThunderheadLeft > GCD && (strategy.Thunder.Value == OffensiveStrategy.Force || ThunderLeft(target.Actor) < (InOpener ? (InLeyLines ? 7.5f : 5) : 3) && !target.ForbidDOTs && TimeToKill.WillLive(target.Actor, 12)))
            PushGCD(ThunderAction, target.Actor, 65);

        if (CantCast && ThunderheadLeft > GCD && !target.ForbidDOTs)
            PushGCD(ThunderAction, target.Actor, 4);

        if (CantCast && !Instant && ReadyIn(ClassShared.AID.Swiftcast) > GCD && (!Unlocked(AID.Triplecast) || TriplecastCharges == 0))
            PushGCD(AID.Scathe, target.Actor, 1);

        AoEExitIce = false;
        if (AOEMode)
            AoE(strategy, target);
        else if (InFire)
            Fire(target);
        else if (InIce)
            Ice(target);
        else
            PushGCD(MP < 7500 && Unlocked(AID.Blizzard3) ? AID.Blizzard3 : Unlocked(AID.Fire3) ? AID.Fire3 : Unlocked(AID.Fire1) && MP >= 1600 ? AID.Fire1 : AID.Blizzard1, target.Actor, 20);
    }

    private void Fire(Enemy target)
    {
        var f4Cost = Hearts > 0 ? 800 : 1600;

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
            var paradoxReserve = Paradox && Unlocked(AID.Paradox) && Astral == 3 && !FlareOpenerFinish && !(OpenerMode == OpenerStrategy.Standard && CombatTime < 60) ? 1600 : 0;
            if (MP - f4Cost >= (FlareOpenerFinish ? 2400 : Unlocked(AID.Despair) && !OpenerManafont ? 800 : 0) + paradoxReserve)
                PushGCD(AID.Fire4, target.Actor, 40);
        }
        else if (MP >= FireCost + (FlareFinisher ? 800 : 0))
        {
            PushGCD(AID.Fire1, target.Actor, 40);
        }
        else if (FlareFinisher && MP >= 800)
        {
            PushGCD(AID.Flare, target.Actor, 40);
        }

        if (!Unlocked(AID.Fire4) && FirestarterLeft > GCD)
            PushGCD(AID.Fire3, target.Actor, 45);

        if (Paradox && Unlocked(AID.Paradox) && MP >= 1600)
            PushGCD(AID.Paradox, target.Actor, 38);

        if (FlareOpenerFinish && AstralSoul >= 3 && MP >= 800)
            PushGCD(AID.Flare, target.Actor, 36);

        if (OpenerManafont && Polyglot > 0)
            PushGCD(Unlocked(AID.Xenoglossy) ? AID.Xenoglossy : AID.Foul, target.Actor, 36);
        else if (Unlocked(AID.Despair) && MP >= 800)
            PushGCD(AID.Despair, target.Actor, 35);

        if (FireMPOut && !ManafontReady && (Unlocked(AID.Blizzard3) || !CanUseTranspose))
            PushGCD(Unlocked(AID.Blizzard3) ? AID.Blizzard3 : AID.Blizzard1, target.Actor, 20);

        if (CantCast && FirestarterLeft > GCD && Astral == 3)
            PushGCD(AID.Fire3, target.Actor, 6);
    }

    private void Ice(Enemy target)
    {
        if (Umbral < 3 && Unlocked(AID.Blizzard3))
            PushGCD(AID.Blizzard3, target.Actor, 50);

        if (Unlocked(AID.Blizzard4) && Hearts < 3)
            PushGCD(AID.Blizzard4, target.Actor, 45);
        else if (!Unlocked(AID.Blizzard4) && (!MPFull || !Unlocked(AID.Fire1)))
            PushGCD(AID.Blizzard1, target.Actor, 45);

        if (Paradox && Unlocked(AID.Paradox) && (FirestarterLeft > 0 || !Unlocked(AID.Transpose) || Umbral == 3 && Hearts >= 3))
            PushGCD(AID.Paradox, target.Actor, 42);

        if (MPFull && (Hearts >= 3 || !Unlocked(AID.Blizzard4)))
        {
            if (!CanUseTranspose || !Unlocked(AID.Paradox) && FirestarterLeft == 0 && Unlocked(AID.Fire3))
                PushGCD(Unlocked(AID.Fire3) ? AID.Fire3 : AID.Fire1, target.Actor, 40);
        }
        else if (!MPFull)
        {
            PushGCD(AID.Blizzard1, target.Actor, 1);
        }

        if (CantCast && Paradox && Unlocked(AID.Paradox))
            PushGCD(AID.Paradox, target.Actor, 6);
    }

    private void AoE(in Strategy strategy, Enemy target)
    {
        var fireAoE = Unlocked(AID.HighFire2) ? AID.HighFire2 : AID.Fire2;
        var iceAoE = Unlocked(AID.HighBlizzard2) ? AID.HighBlizzard2 : AID.Blizzard2;
        if (InFire)
        {
            if (Unlocked(AID.FlareStar) && AstralSoul >= 6)
                PushGCD(AID.FlareStar, target.Actor, 50);
            if (Unlocked(AID.Flare))
            {
                if (!Unlocked(AID.Blizzard4) && Astral < 3 && MP >= 3000)
                    PushGCD(fireAoE, target.Actor, 46);
                if (MP >= (Hearts > 0 ? 800 : 1000))
                    PushGCD(AID.Flare, target.Actor, 45);
            }
            else if (MP >= 3000)
            {
                PushGCD(fireAoE, target.Actor, 45);
            }
            if (AoEFireMPOut && (!CanUseTranspose || AoECastSwap) && !ManafontReady)
                PushGCD(iceAoE, target.Actor, 20);
            if (AoEFireMPOut && AstralSoul < 6 && CanUseTranspose && !AoECastSwap && !ManafontReady && Polyglot > 0 && Unlocked(AID.Foul))
                PushGCD(AID.Foul, target.Actor, 30);
        }
        else if (InIce)
        {
            AoEExitIce = Unlocked(AID.Blizzard4) ? Hearts >= 3 || MP >= 5000 && Unlocked(AID.Flare)
                : Unlocked(AID.Flare) || !Unlocked(AID.Freeze) ? MPFull
                : MPFull && NeedThunderhead(strategy, target);
            var iceSpell = NumAOETargets == 2 && Unlocked(AID.Blizzard4) ? AID.Blizzard4 : Unlocked(AID.Freeze) ? AID.Freeze : iceAoE;
            if (Unlocked(AID.Blizzard4))
            {
                if ((Hearts < 3 || !Unlocked(AID.Freeze)) && !(MP >= 5000 && Unlocked(AID.Flare) && Hearts >= 3))
                    PushGCD(iceSpell, target.Actor, 45);
            }
            else if (!AoEExitIce || Unlocked(AID.Freeze) && !CanUseTranspose)
            {
                PushGCD(iceSpell, target.Actor, 45);
            }
            if (Paradox && Unlocked(AID.Paradox))
                PushGCD(AID.Paradox, target.Actor, 30);
            if (Unlocked(AID.Blizzard4) ? !CanUseTranspose && (MPFull || Hearts >= 3) : AoEExitIce && (!CanUseTranspose || AoECastSwap))
                PushGCD(fireAoE, target.Actor, 25);
            if (CanUseTranspose && AoEExitIce && !AoECastSwap && Polyglot > 0 && Unlocked(AID.Foul))
                PushGCD(AID.Foul, target.Actor, 30);
        }
        else
        {
            PushGCD(iceAoE, target.Actor, 20);
        }
    }

    private bool NeedThunderhead(in Strategy strategy, Enemy target)
        => strategy.Thunder.Value != OffensiveStrategy.Delay && ThunderheadLeft == 0 && !target.ForbidDOTs && ThunderLeft(target.Actor) < 10 && TimeToKill.WillLive(target.Actor, 15);

    #endregion

    #region oGCD

    private void OGCDs(in Strategy strategy, Enemy target)
    {
        if (strategy.Manafont.Value != OffensiveStrategy.Delay && InFire && (CanWeave(AID.Manafont) || GCD == 0 && ReadyIn(AID.Manafont) <= 0) && (strategy.Manafont.Value == OffensiveStrategy.Force || (AOEMode ? AoEFireMPOut : FireMPOut) && AstralSoul < 6 || OpenerManafont))
            PushOGCD(AID.Manafont, Player, 70);

        if (CanUseTranspose)
        {
            var endOfFire = InFire && FireMPOut && !(Unlocked(AID.Manafont) && ReadyIn(AID.Manafont) < 5) && AstralSoul < 6;
            if (endOfFire && (Instant || !Unlocked(AID.Blizzard3)))
                PushOGCD(AID.Transpose, Player, 65);
            var endOfIce = InIce && MPFull && (Hearts >= 3 || !Unlocked(AID.Blizzard4)) && (!Paradox || FirestarterLeft == 0);
            if (endOfIce && !AOEMode && (FirestarterLeft > GCD || Unlocked(AID.Paradox) || !Unlocked(AID.Fire3)))
                PushOGCD(AID.Transpose, Player, 65);
            if (AOEMode && !AoECastSwap && (InFire && AoEFireMPOut && AstralSoul < 6 && !(Unlocked(AID.Manafont) && ReadyIn(AID.Manafont) <= GCD) || InIce && AoEExitIce))
                PushOGCD(AID.Transpose, Player, 65);
        }

        if (!AOEMode && InFire && FireMPOut && Astral == 3 && !Instant && !(Unlocked(AID.Manafont) && ReadyIn(AID.Manafont) < 5) && AstralSoul < 6)
        {
            if (CanWeave(ClassShared.AID.Swiftcast))
                PushOGCD(ClassShared.AID.Swiftcast, Player, 66);
            else if (CanWeave(AID.Triplecast) && SelfStatusLeft(SID.Triplecast) == 0 && !InLeyLines && TriplecastCharges >= 2)
                PushOGCD(AID.Triplecast, Player, 66);
        }

        if (!AOEMode && InOpener && CombatTime < 10 && InFire && !Instant && ThunderLeft(target.Actor) > 20 && Unlocked(AID.LeyLines) && ReadyIn(AID.LeyLines) < 5 && CanWeave(ClassShared.AID.Swiftcast))
            PushOGCD(ClassShared.AID.Swiftcast, Player, 61);

        if (!AOEMode && FlareOpenerFinish && InFire && AstralSoul >= 3 && !Instant && MP - (Hearts > 0 ? 800 : 1600) < 2400 && CanWeave(AID.Triplecast))
            PushOGCD(AID.Triplecast, Player, 67);

        if (strategy.Amplifier.Value != OffensiveStrategy.Delay && CanWeave(AID.Amplifier) && (strategy.Amplifier.Value == OffensiveStrategy.Force || Polyglot < MaxPolyglot))
            PushOGCD(AID.Amplifier, Player, 60);

        if (strategy.LeyLines.Value != LeyLinesStrategy.Delay && CanWeave(AID.LeyLines) && !InLeyLines && SelfStatusLeft(SID.LeyLines) == 0
            && (strategy.LeyLines.Value == LeyLinesStrategy.ASAP || ShouldLeyLines(target.Actor)))
            PushOGCD(AID.LeyLines, Player, 55);

        if (strategy.Movement.Value == MovementStrategy.Automatic && CantCast && !Instant && Polyglot == 0 && ThunderheadLeft == 0 && !(InFire && FirestarterLeft > 0))
        {
            if (!InLeyLines && CanWeave(AID.Triplecast))
                PushOGCD(AID.Triplecast, Player, 50);
            else if (CanWeave(ClassShared.AID.Swiftcast))
                PushOGCD(ClassShared.AID.Swiftcast, Player, 49);
        }

        if (strategy.Manaward.Value == ManawardStrategy.Automatic && CanWeave(AID.Manaward) && RaidwideWithin(4))
            PushOGCD(AID.Manaward, Player, 45);
    }

    private bool BuffsActive => HasRaidBuffJobs && RaidBuffsLeft > GCD;
    private bool HoldPolyglotForBuffs => HasRaidBuffJobs && RaidBuffsLeft == 0 && RaidBuffsIn <= 15;

    private bool ShouldLeyLines(Actor target)
    {
        if (Hints.MaxCastTime < 2.5f || DowntimeIn <= 15 || !TimeToKill.BurstWorthIt(Bossmods.ActiveModule, Hints, target, 15))
            return false;
        if (InOpener && CombatTime < 10 && InFire)
            return true;
        var full = LeyLinesCharges >= MaxCharges(AID.LeyLines);
        if (!InBossFight || NoRaidBuffs)
            return full;
        if (RaidBuffsLeft > 10 || RaidBuffsIn <= 3)
            return true;
        return full && RaidBuffsIn > 40;
    }

    // queued rather than weave-checked: a weave check fails exactly when the GCD comes up
    private bool CanUseTranspose => Unlocked(AID.Transpose) && ReadyIn(AID.Transpose) <= GCD;

    #endregion

    #region Helpers

    private AID ThunderAction => AOEMode
        ? (Unlocked(AID.HighThunder2) ? AID.HighThunder2 : Unlocked(AID.Thunder4) ? AID.Thunder4 : AID.Thunder2)
        : (Unlocked(AID.HighThunder) ? AID.HighThunder : Unlocked(AID.Thunder3) ? AID.Thunder3 : AID.Thunder1);

    // Firestarter and Thunderhead can have no timer, which would otherwise read as 0s left
    private float ProcLeft(SID sid) => Player.FindStatus(sid) is { } st ? MathF.Max(StatusDuration(st.ExpireAt), st.ExpireAt <= World.CurrentTime ? float.MaxValue : 0) : 0;

    private float ThunderLeft(Actor target)
    {
        float Left(SID sid) => StatusDetails(target, sid, Player.InstanceID, 30).Left;
        return MathF.Max(MathF.Max(Left(SID.HighThunder), Left(SID.HighThunderII)), MathF.Max(MathF.Max(Left(SID.Thunder), Left(SID.ThunderII)), MathF.Max(Left(SID.ThunderIII), Left(SID.ThunderIV))));
    }

    private float SlideCastTime(AID aid) => MathF.Max(0, CastTime(aid) - 0.5f);

    private float CastTime(AID aid)
    {
        if (Instant)
            return 0;
        var def = ActionDefinitions.Instance.Spell(aid);
        if (def == null || def.CastTime == 0)
            return 0;
        if (aid == AID.Foul && Player.Level >= 80)
            return 0;
        var t = def.CastTime * GCDLength / 2.5f;
        var isFire = aid is AID.Fire1 or AID.Fire3 or AID.Fire4 or AID.Fire2 or AID.HighFire2 or AID.Flare or AID.Despair or AID.FlareStar;
        var isIce = aid is AID.Blizzard1 or AID.Blizzard3 or AID.Blizzard4 or AID.Blizzard2 or AID.HighBlizzard2 or AID.Freeze;
        if (isFire && Umbral == 3 || isIce && Astral == 3)
            t *= 0.5f;
        if (aid == AID.Fire3 && FirestarterLeft > GCD || aid == AID.Despair && Player.Level >= 100)
            t = 0;
        return t;
    }

    private bool UsePotion(in Strategy strategy) => strategy.Potion.Value switch
    {
        PotionStrategy.AlignWithBurst => PotionPrepull || Player.InCombat && Unlocked(AID.LeyLines) && ReadyIn(AID.LeyLines) < 3 && InFire,
        PotionStrategy.AlignWithRaidBuffs => PotionWithRaidBuffs,
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

    private int MaxCharges(AID aid) => Unlocked(aid) ? ActionDefinitions.Instance.Spell(aid)!.MaxChargesAtLevel(Player.Level) : 0;
    private void PushGCD(AID aid, Actor? target, int priority)
        => PushAction(ActionID.MakeSpell(aid), target, ActionQueue.Priority.High + priority, castTime: SlideCastTime(aid));

    #endregion
}
