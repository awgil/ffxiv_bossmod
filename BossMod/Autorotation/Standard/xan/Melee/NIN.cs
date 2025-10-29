using BossMod.Data;
using BossMod.NIN;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using System.Collections.ObjectModel;
using static BossMod.AIHints;

namespace BossMod.Autorotation.xan;

public sealed class NIN(RotationModuleManager manager, Actor player) : Attackxan<AID, TraitID>(manager, player, PotionType.Dexterity)
{
    public enum Track { Hide = SharedTrack.Count, ForkedRaiju, PhantomCannon }
    public enum HideStrategy { Automatic, Manual }
    public enum RaijuStrategy { Manual, Automatic }

    public static RotationModuleDefinition Definition()
    {
        var def = new RotationModuleDefinition("xan NIN", "Ninja", "Standard rotation (xan)|Melee", "xan", RotationModuleQuality.Basic, BitMask.Build(Class.ROG, Class.NIN), 100);

        def.DefineShared().AddAssociatedActions(AID.Dokumori);

        def.Define(Track.Hide).As<HideStrategy>("Hide")
            .AddOption(HideStrategy.Automatic, "Auto", "Use when out of combat to restore charges")
            .AddOption(HideStrategy.Manual, "Manual", "Do not use automatically");

        def.Define(Track.ForkedRaiju).As<RaijuStrategy>("Forked Raiju")
            .AddOption(RaijuStrategy.Manual, "Manual", "Do not use automatically")
            .AddOption(RaijuStrategy.Automatic, "Auto", "Use when out of melee range");

        def.AbilityTrack(Track.PhantomCannon, "PCAN", "Phantom Cannoneer: use cannons on primary target ASAP").AddAssociatedActions(PhantomID.PhantomFire, PhantomID.HolyCannon, PhantomID.DarkCannon, PhantomID.ShockCannon, PhantomID.SilverCannon);

        return def;
    }

    public int Ninki;
    public int Kazematoi;
    public (float Left, int Param) Mudra;
    public (float Left, int Param) TenChiJin;
    public bool HiddenStatus; // no max, ends when combat starts
    public float ShadowWalker; // max 20
    public float Kassatsu; // max 15
    public float PhantomKamaitachi; // max 45
    public float Meisui; // max 30
    public (float Left, int Stacks) Raiju;
    public float TenriJindo;

    public float TargetTrickLeft; // max 15
    public float TargetMugLeft; // max 20

    public int NumAOETargets;
    public int NumRangedAOETargets;

    // 25y for hellfrog - ninjutsu have a range of 20y
    private Enemy? BestRangedAOETarget;

    // these aren't the same cdgroup :(
    public float AssassinateCD => ReadyIn(Unlocked(AID.DreamWithinADream) ? AID.DreamWithinADream : AID.Assassinate);

    private ReadOnlyCollection<int> Mudras => Array.AsReadOnly([Mudra.Param & 3, (Mudra.Param >> 2) & 3, (Mudra.Param >> 4) & 3]);

    private readonly Dictionary<AID, (int Len, int Last)> Combos = new()
    {
        [AID.FumaShuriken] = (1, 0),
        [AID.Katon] = (2, 1),
        [AID.GokaMekkyaku] = (2, 1),
        [AID.Raiton] = (2, 2),
        [AID.Hyoton] = (2, 3),
        [AID.HyoshoRanryu] = (2, 3),
        [AID.Huton] = (3, 1),
        [AID.Doton] = (3, 2),
        [AID.Suiton] = (3, 3)
    };

    private AID CurrentNinjutsu => Mudras switch
    {
        [1 or 2 or 3, 0, 0] => AID.FumaShuriken,
        [_, 1, 0] => Kassatsu > GCD ? AID.GokaMekkyaku : AID.Katon,
        [_, 2, 0] => AID.Raiton,
        [_, 3, 0] => Kassatsu > GCD ? AID.Hyoton : AID.HyoshoRanryu,
        [_, _, 1] => AID.Huton,
        [_, _, 2] => AID.Doton,
        [_, _, 3] => AID.Suiton,
        _ => AID.Ninjutsu
    };

    private bool Hidden => HiddenStatus || ShadowWalker > AnimLock;

    private bool CanTrickInCombat => Unlocked(AID.Suiton);

    private static readonly uint[] NoUnhideZones = [
        452
    ];

    public override void Exec(StrategyValues strategy, Enemy? primaryTarget)
    {
        SelectPrimaryTarget(strategy, ref primaryTarget, range: 3);

        var gauge = World.Client.GetGauge<NinjaGauge>();
        Ninki = gauge.Ninki;
        Kazematoi = gauge.Kazematoi;

        Mudra = Status(SID.Mudra);
        ShadowWalker = StatusLeft(SID.ShadowWalker);
        Kassatsu = StatusLeft(SID.Kassatsu);
        PhantomKamaitachi = StatusLeft(SID.PhantomKamaitachiReady);
        HiddenStatus = StatusStacks(SID.Hidden) > 0;
        TargetTrickLeft = MathF.Max(
            StatusDetails(primaryTarget, SID.TrickAttack, Player.InstanceID).Left,
            StatusDetails(primaryTarget, SID.KunaisBane, Player.InstanceID).Left
        );
        TargetMugLeft = MathF.Max(
            StatusDetails(primaryTarget, SID.VulnerabilityUp, Player.InstanceID).Left,
            StatusDetails(primaryTarget, SID.Dokumori, Player.InstanceID).Left
        );
        Raiju = Status(SID.RaijuReady);
        TenChiJin = Status(SID.TenChiJin);
        Meisui = StatusLeft(SID.Meisui);
        TenriJindo = StatusLeft(SID.TenriJindoReady);

        if (HiddenStatus && !NoUnhideZones.Contains(World.CurrentCFCID))
            Hints.StatusesToCancel.Add(((uint)SID.Hidden, Player.InstanceID));

        (BestRangedAOETarget, NumRangedAOETargets) = SelectTarget(strategy, primaryTarget, 20, IsSplashTarget);

        NumAOETargets = NumMeleeAOETargets(strategy);

        var pos = GetNextPositional(primaryTarget?.Actor);
        UpdatePositionals(primaryTarget, ref pos);

        OGCD(strategy, primaryTarget);

        if (CountdownRemaining > 0)
        {
            if (CountdownRemaining < 6)
                UseMudra(AID.Suiton, primaryTarget, endCondition: CountdownRemaining < 1);

            return;
        }

        GoalZoneCombined(strategy, 3, Hints.GoalAOECircle(5), AID.DeathBlossom, minAoe: 3, maximumActionRange: 20);

        if (TenChiJin.Left > GCD)
        {
            if (NumRangedAOETargets > 2)
            {
                PushGCD(TenChiJin.Param switch
                {
                    0 => AID.FumaJin,
                    1 => AID.TCJHyoton,
                    2 or 3 => AID.TCJKaton,
                    _ => AID.TCJDoton
                }, BestRangedAOETarget);
            }
            else
            {
                PushGCD(TenChiJin.Param switch
                {
                    0 => AID.FumaTen,
                    1 or 3 => AID.TCJRaiton,
                    2 => AID.TCJKaton,
                    _ => AID.TCJSuiton
                }, primaryTarget);
            }
            return;
        }

        if (Mudra.Left == 0 && strategy.Enabled(Track.PhantomCannon))
        {
            if (DutyActionReadyIn(PhantomID.SilverCannon) <= GCD)
                PushGCD((AID)PhantomID.SilverCannon, primaryTarget, 10);
            if (DutyActionReadyIn(PhantomID.ShockCannon) <= GCD)
                PushGCD((AID)PhantomID.ShockCannon, primaryTarget, 8);
            if (DutyActionReadyIn(PhantomID.PhantomFire) <= GCD)
                PushGCD((AID)PhantomID.PhantomFire, primaryTarget, 6);
        }

        if (PhantomKamaitachi > GCD && Mudra.Left == 0 && ShouldPK(BestRangedAOETarget))
            PushGCD(AID.PhantomKamaitachi, BestRangedAOETarget);

        if (Raiju.Stacks > 0 && Mudra.Left == 0)
        {
            if (strategy.Option(Track.ForkedRaiju).As<RaijuStrategy>() == RaijuStrategy.Automatic && Player.DistanceToHitbox(primaryTarget) is > 3 and <= 20)
                PushGCD(AID.ForkedRaiju, primaryTarget);

            if (OnCooldown(AID.TenChiJin))
                PushGCD(AID.FleetingRaiju, primaryTarget);
        }

        var useNinjutsuAOE = NumRangedAOETargets > 2;

        // TODO save charges for trick
        if (Unlocked(AID.Raiton))
        {
            if (Unlocked(AID.Huton) && ReadyIn(AID.TrickAttack) < 15 && ShadowWalker == 0)
            {
                if (useNinjutsuAOE)
                    UseMudra(AID.Huton, BestRangedAOETarget);
                else
                    UseMudra(AID.Suiton, primaryTarget);
            }

            if (OnCooldown(AID.Kassatsu) && AssassinateCD > Kassatsu && (ReadyIn(AID.TrickAttack) > Kassatsu || !CanTrickInCombat))
            {
                if (useNinjutsuAOE)
                    // this will get auto transformed to goka mekkyaku
                    UseMudra(AID.Katon, BestRangedAOETarget);
                else
                    UseMudra(Kassatsu > 0 && Unlocked(AID.HyoshoRanryu) ? AID.HyoshoRanryu : AID.Raiton, primaryTarget);
            }

            // some condition changed during cast
            if (Mudra.Left > 0)
            {
                if (useNinjutsuAOE)
                    PushGCD(AID.Katon, BestRangedAOETarget);
                else
                    PushGCD(AID.Raiton, primaryTarget);
            }
        }
        else
            UseMudra(AID.FumaShuriken, primaryTarget);

        var useMeleeAOE = NumAOETargets > 2;

        if (useMeleeAOE && Unlocked(AID.DeathBlossom))
        {
            if (ComboLastMove == AID.DeathBlossom)
                PushGCD(AID.HakkeMujinsatsu, Player);

            PushGCD(AID.DeathBlossom, Player);
        }
        else
        {
            if (ComboLastMove == AID.GustSlash && primaryTarget != null)
                PushGCD(GetComboEnder(primaryTarget.Actor), primaryTarget);

            if (ComboLastMove == AID.SpinningEdge)
                PushGCD(AID.GustSlash, primaryTarget);

            PushGCD(AID.SpinningEdge, primaryTarget);
        }
    }

    private bool ShouldPK(Enemy? primaryTarget)
    {
        if (RaidBuffsLeft > GCD || TargetTrickLeft > GCD || TargetMugLeft > GCD)
            return true;

        if (!CanFitGCD(PhantomKamaitachi, 1))
            return true;

        return Player.DistanceToHitbox(primaryTarget) is > 3 and <= 20;
    }

    private AID GetComboEnder(Actor primaryTarget)
    {
        if (!Unlocked(AID.ArmorCrush))
            return AID.AeolianEdge;

        if (Kazematoi == 0)
            return AID.ArmorCrush;

        if (Kazematoi >= 4)
            return AID.AeolianEdge;

        return primaryTarget.Omnidirectional || GetCurrentPositional(primaryTarget) == Positional.Rear ? AID.AeolianEdge : AID.ArmorCrush;
    }

    private void UseMudra(AID mudra, Enemy? target, bool startCondition = true, bool endCondition = true)
    {
        (var aid, var tar) = PickMudra(mudra, target, startCondition, endCondition);
        if (aid != AID.None)
            PushGCD(aid == AID.Ninjutsu ? CurrentNinjutsu : aid, tar);
    }

    private (AID action, Enemy? target) PickMudra(AID mudra, Enemy? target, bool startCondition, bool endCondition)
    {
        if (!Unlocked(mudra) || target == null)
            return (AID.None, null);

        // no charges remaining and no kassatsu = we can't use it
        if (Mudra.Param == 0 && ReadyIn(AID.Ten1) > GCD && Kassatsu == 0)
            return (AID.None, null);

        // do nothing if start condition failed - since this could be something like checking ninjutsu CD, we skip it otherwise
        // (since ninjutsu goes on CD as soon as you press the first one)
        if (Mudra.Param == 0 && !startCondition)
            return (AID.None, null);

        // unrecognized action - this really shouldn't happen
        if (!Combos.TryGetValue(mudra, out var combo))
            return (AID.None, null);

        var (len, last) = combo;

        var ten1 = Kassatsu > 0 ? AID.Ten2 : AID.Ten1;
        var jin1 = Kassatsu > 0 ? AID.Jin2 : AID.Jin1;
        var chi1 = Kassatsu > 0 ? AID.Chi2 : AID.Chi1;

        if (len == 1)
        {
            if (Mudras[0] == 0)
                return (ten1, null);
            else if (endCondition)
                return (AID.Ninjutsu, target);
        }

        if (len == 2)
        {
            // early exit
            if (Mudras[0] == last)
                return (AID.Ninjutsu, target);

            if (Mudras[0] == 0)
                return (last == 1 ? (Unlocked(jin1) ? jin1 : chi1) : ten1, null);

            if (Mudras[1] == 0)
                return (last == 1 ? AID.Ten2 : last == 2 ? AID.Chi2 : AID.Jin2, null);
            else if (endCondition)
                return (AID.Ninjutsu, target);
        }

        if (len == 3)
        {
            // early exit
            if (Mudras[0] == last || Mudras[1] == last)
                return (AID.Ninjutsu, target);

            if (Mudras[0] == 0)
                return (last == 1 ? jin1 : ten1, null);

            if (Mudras[1] == 0)
                return (Mudras[0] switch
                {
                    1 => last == 3 ? AID.Chi2 : AID.Jin2,
                    2 => last == 3 ? AID.Ten2 : AID.Jin2,
                    3 => last == 1 ? AID.Chi2 : AID.Ten2,
                    _ => AID.None
                }, null);

            if (Mudras[2] == 0)
                return (last == 1 ? AID.Ten2 : last == 2 ? AID.Chi2 : AID.Jin2, null);
            else if (endCondition)
                return (AID.Ninjutsu, target);
        }

        return (AID.None, null);
    }

    private void OGCD(StrategyValues strategy, Enemy? primaryTarget)
    {
        if (!Player.InCombat)
        {
            if (strategy.Option(Track.Hide).As<HideStrategy>() == HideStrategy.Automatic
                && Mudra.Left == 0
                && GCD == 0
                && Unlocked(AID.Ten1)
                && OnCooldown(AID.Ten1))
                PushOGCD(AID.Hide, Player);

            return;
        }

        if (primaryTarget == null)
            return;

        if (ShadowWalker > 0 && ReadyIn(AID.TrickAttack) > ShadowWalker)
            PushOGCD(AID.Meisui, Player);

        if (TenriJindo > 0 && TenChiJin.Left == 0)
            PushOGCD(AID.TenriJindo, BestRangedAOETarget);

        if (ReadyIn(AID.TrickAttack) < 5)
            PushOGCD(AID.Kassatsu, Player);

        var buffsOk = strategy.BuffsOk();

        if (buffsOk)
        {
            if ((!Unlocked(TraitID.Shukiho) || Ninki >= 10) && TargetMugLeft == 0)
                PushOGCD(AID.Mug, primaryTarget);

            if (ReadyIn(AID.Ten1) > GCD && Mudra.Left == 0 && Kassatsu == 0 && ShadowWalker == 0)
                PushOGCD(AID.TenChiJin, Player);

            if (Ninki >= 50)
                PushOGCD(AID.Bunshin, Player);
        }

        if (Hidden && (MugOnCD || !buffsOk))
            // late weave trick during 2min windows with mug/dokumori active; otherwise use on cooldown
            PushOGCD(AID.TrickAttack, primaryTarget, delay: TargetMugLeft == 0 ? 0 : GCD - 0.8f);

        if (ReadyIn(AID.TrickAttack) > 10 || !CanTrickInCombat)
            PushOGCD(Unlocked(AID.DreamWithinADream) ? AID.DreamWithinADream : AID.Assassinate, primaryTarget);

        if (ShouldBhava(strategy))
        {
            if (NumRangedAOETargets > 2 || !Unlocked(AID.Bhavacakra))
                PushOGCD(AID.HellfrogMedium, BestRangedAOETarget);

            PushOGCD(AID.Bhavacakra, primaryTarget, priority: Meisui > 0 ? 50 : 1);
        }
    }

    private bool MugOnCD => TargetMugLeft > 0 || OnCooldown(AID.Mug);

    private bool ShouldBhava(StrategyValues strategy)
        => Ninki >= 50 && (Meisui > 0 || TargetTrickLeft > AnimLock || Ninki > 85);

    private (Positional, bool) GetNextPositional(Actor? primaryTarget)
    {
        if (!Unlocked(AID.AeolianEdge) || primaryTarget == null)
            return (Positional.Any, false);

        return (GetComboEnder(primaryTarget) == AID.AeolianEdge ? Positional.Rear : Positional.Flank, ComboLastMove == AID.GustSlash);
    }
}
