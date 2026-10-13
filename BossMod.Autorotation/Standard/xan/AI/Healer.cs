using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using static BossMod.Autorotation.TrackPartyHealth;

namespace BossMod.Autorotation.xan;

public class HealerAI(RotationModuleManager manager, Actor player) : AIBase<HealerAI.Strategy>(manager, player)
{
    public enum HealMode
    {
        [Option("Heal everyone")]
        Enabled,
        [Option("Babysit specific target (default: current main tank)", Targets = ActionTargets.Self | ActionTargets.Party)]
        Babysit,
        [Option("Don't heal")]
        Disabled
    }

    public struct Strategy
    {
        public Track<RaiseStrategy> Raise;

        [Track("Raise targets")]
        public Track<RaiseUtil.Targets> RaiseTargets;

        [Track(Actions = [
            BossMod.WHM.AID.CureII, BossMod.WHM.AID.DivineBenison, BossMod.WHM.AID.Tetragrammaton, BossMod.WHM.AID.Benediction, BossMod.WHM.AID.AfflatusSolace, BossMod.WHM.AID.Regen,

            BossMod.SCH.AID.Adloquium, BossMod.SCH.AID.Excogitation, BossMod.SCH.AID.Aetherpact, BossMod.SCH.AID.Lustrate, BossMod.SCH.AID.Protraction,

            BossMod.AST.AID.EssentialDignity, BossMod.AST.AID.CelestialIntersection, BossMod.AST.AID.Exaltation, BossMod.AST.AID.Synastry, BossMod.AST.AID.Benefic, BossMod.AST.AID.BeneficII, BossMod.AST.AID.AspectedBenefic, BossMod.AST.AID.TheArrow, BossMod.AST.AID.TheSpire, BossMod.AST.AID.TheBole, BossMod.AST.AID.TheEwer,

            BossMod.SGE.AID.Soteria, BossMod.SGE.AID.Taurochole, BossMod.SGE.AID.Haima, BossMod.SGE.AID.Krasis, BossMod.SGE.AID.Diagnosis, BossMod.SGE.AID.EukrasianDiagnosis, BossMod.SGE.AID.Druochole
        ])]
        public Track<HealMode> Heal;

        [Track(InternalName = "Esuna2", Action = ClassShared.AID.Esuna)]
        public Track<HintedStrategy> Esuna;

        [Track("Stay near party", InternalName = "Stay near party")]
        public Track<EnabledByDefault> StayNearParty;
        [Track("Allow generic out-of-combat predictive heals on tank (Excogitation, Divine Benison, etc)")]
        public Track<EnabledByDefault> OutOfCombat;
        [Track("Allow automatic use of mitigation and big healing cooldowns (Temperance, Kerachole, Collective Unconscious, etc)", InternalName = "Mitigation", Actions = [
            BossMod.WHM.AID.Temperance, BossMod.WHM.AID.PlenaryIndulgence, BossMod.WHM.AID.DivineCaress, BossMod.WHM.AID.Asylum, BossMod.WHM.AID.LiturgyOfTheBell, BossMod.WHM.AID.Aquaveil,

            BossMod.AST.AID.CollectiveUnconscious, BossMod.AST.AID.Macrocosmos, BossMod.AST.AID.EarthlyStar, BossMod.AST.AID.CelestialOpposition,

            BossMod.SCH.AID.SacredSoil, BossMod.SCH.AID.FeyIllumination, BossMod.SCH.AID.Expedient, BossMod.SCH.AID.SummonSeraph, BossMod.SCH.AID.FeyBlessing, BossMod.SCH.AID.Seraphism,

            BossMod.SGE.AID.Kerachole, BossMod.SGE.AID.Holos, BossMod.SGE.AID.Physis, BossMod.SGE.AID.PhysisII, BossMod.SGE.AID.Philosophia, BossMod.SGE.AID.Panhaima, BossMod.SGE.AID.Zoe, BossMod.SGE.AID.Pneuma
        ])]
        public Track<MitigationMode> Mitigation;
    }

    public enum MitigationMode
    {
        [Option("Use automatically")]
        Automatic,
        [Option("Don't use automatically (leave to plan)")]
        LeaveToPlan
    }

    private readonly TrackPartyHealth Health = new(manager.WorldState);

    // per-frame state shared by all healer jobs, set at the start of Execute
    private bool AutoMit; // Mitigation track allows automatic use
    private float RaidwideIn; // seconds until the next raidwide or shared hit
    private float FullRaidwideIn; // same, but only hits on everyone (GCD shields are wasted on stacks)
    private float MultiHitIn; // seconds until the next hit of a multi-hit raidwide or stack
    private bool MultiHitLong; // a predicted sequence of 8+ hits or 10+ seconds

    public enum RaiseStrategy
    {
        [Option("Don't automatically raise")]
        None,
        [Option("Raise using Swiftcast only")]
        Swiftcast,
        [Option("Raise without requiring Swiftcast")]
        Slowcast,
        [Option("Raise without using Swiftcast")]
        Hardcast,
    }

    public ActionID RaiseAction => Player.Class switch
    {
        Class.CNJ or Class.WHM => ActionID.MakeSpell(BossMod.WHM.AID.Raise),
        Class.ACN or Class.SCH => ActionID.MakeSpell(BossMod.SCH.AID.Resurrection),
        Class.AST => ActionID.MakeSpell(BossMod.AST.AID.Ascend),
        Class.SGE => ActionID.MakeSpell(BossMod.SGE.AID.Egeiro),
        _ => default
    };

    public static RotationModuleDefinition Definition()
    {
        return new RotationModuleDefinition("Healer AI", "Auto-healer", "AI (xan)", "xan", RotationModuleQuality.WIP, BitMask.Build(Class.CNJ, Class.WHM, Class.SCH, Class.SGE, Class.AST), 100).WithStrategies<Strategy>();
    }

    private int ResolveHealTarget(in Strategy strategy) => strategy.Heal.TrackRaw.Target switch
    {
        StrategyTarget.Automatic => World.Actors.Find(Player.TargetID) is { } t ? World.Party.FindSlot(t.TargetID) : -1,
        _ => World.Party.FindSlot(Manager.ResolveTargetOverride(strategy.Heal.TrackRaw.Target, strategy.Heal.TrackRaw.TargetParam)?.InstanceID ?? 0)
    };

    private void HealBabysitTarget(in Strategy strategy, bool predicted, Action<Actor, float> healFun)
    {
        var targetSlot = ResolveHealTarget(strategy);
        if (targetSlot < 0 || World.Party[targetSlot] is not { IsDead: false } target)
            return;
        var st = Health.PartyMemberStates[targetSlot];
        if (st.NoHealStatusRemaining < 1.5f)
            healFun(target, predicted ? st.PredictedHPRatio : st.CurrentHPRatio);
    }

    private void HealLowest(in Strategy strategy, bool predicted, Action<Actor, float> healFun)
    {
        if (strategy.Heal.Value == HealMode.Babysit)
            HealBabysitTarget(strategy, predicted, healFun);
        if (strategy.Heal.Value != HealMode.Enabled)
            return;

        // the first action after a raise goes off on the frame we revive, before we see our own hp
        if (Player.IsDead && Player.FindStatus(ClassShared.SID.Raise) != null)
        {
            healFun(Player, 0.2f);
            return;
        }

        var best = -1;
        var bestRatio = float.MaxValue;
        foreach (var (slot, _) in Health.TrackedMembers)
        {
            var st = Health.PartyMemberStates[slot];
            if (st.NoHealStatusRemaining > 1.5f && st.DoomRemaining == 0)
                continue;
            var ratio = st.DoomRemaining > 0 ? 0.01f : HealRatio(slot);
            if (ratio < bestRatio)
            {
                bestRatio = ratio;
                best = slot;
            }
        }
        if (best >= 0 && World.Party[best] is { } target)
            healFun(target, bestRatio - ThresholdShift(target, best));
    }

    private static readonly uint[] HealOverTimeStatuses = [
        (uint)BossMod.WHM.SID.Regen, (uint)BossMod.WHM.SID.MedicaII, (uint)BossMod.WHM.SID.MedicaIII, (uint)BossMod.WHM.SID.Asylum,
        (uint)BossMod.SGE.SID.Kerakeia, (uint)BossMod.SGE.SID.PhysisII, (uint)BossMod.SGE.SID.Kardion,
        (uint)BossMod.AST.SID.AspectedBenefic, (uint)BossMod.AST.SID.AspectedHelios, (uint)BossMod.AST.SID.HeliosConjunction,
        (uint)BossMod.SCH.SID.FeyUnion,
        315, // Whispering Dawn
    ];
    private static bool HasHealOverTime(Actor a) => HasAnyStatus(a, HealOverTimeStatuses);

    // includes pending statuses, so we don't reapply something that was just cast
    private static bool HasAnyStatus(Actor a, uint[] ids) => a.Statuses.Any(s => ids.Contains(s.ID)) || a.PendingStatuses.Any(s => ids.Contains(s.StatusId));

    private float NextDamageIn(int slot)
        => SecondsUntilNext(Raidwides.Concat(Tankbusters.Where(t => slot < 0 || World.Party.FindSlot(t.Item1.InstanceID) == slot).Select(t => t.Item2)));

    private float SecondsUntilNext(IEnumerable<DateTime> times)
        => times.Where(t => t >= World.CurrentTime).Select(t => (float)(t - World.CurrentTime).TotalSeconds).DefaultIfEmpty(float.MaxValue).Min();

    // heal earlier before predicted damage, later when nothing is coming
    private float ThresholdShift(Actor target, int slot)
    {
        if (Bossmods.ActiveModule == null)
            return 0;
        var damageIn = NextDamageIn(slot);
        if (damageIn < 8)
            return 0.15f;
        if (damageIn < 15)
            return 0;
        var hot = HasHealOverTime(target);
        if (target.Role == Role.Tank)
            return hot ? -0.1f : 0;
        return hot ? -0.2f : -0.1f;
    }

    private float AreaShift()
    {
        if (Bossmods.ActiveModule == null)
            return 0;
        if (RaidwideIn < 8)
            return 0.1f;
        if (RaidwideIn < 15)
            return 0;
        var party = LightParty.ToList();
        return party.Count(HasHealOverTime) * 2 >= party.Count ? -0.2f : -0.1f;
    }

    // party members with a tankbuster landing within the given time
    private IEnumerable<(Actor tank, float busterIn)> TankbustersWithin(float seconds)
    {
        foreach (var (tank, at) in Tankbusters)
        {
            var busterIn = (float)(at - World.CurrentTime).TotalSeconds;
            if (busterIn >= 0 && busterIn <= seconds && !tank.IsDead && World.Party.FindSlot(tank.InstanceID) >= 0)
                yield return (tank, busterIn);
        }
    }

    private bool QuietPeriod => Bossmods.ActiveModule != null && NextDamageIn(-1) >= 15;
    // nothing is coming for a while: top up a chipped target with a regen instead of a direct heal
    private bool PreferRegen(Actor target) => QuietPeriod && PredictedRatio(target) is > 0.55f and < 0.85f && !HasHealOverTime(target);
    private float PredictedRatio(Actor a) => World.Party.FindSlot(a.InstanceID) is var slot && slot >= 0 ? HealRatio(slot) : a.HPRatio;
    // predicted HP without the flat 30% per upcoming hit: a heal before the hit can't go above max HP
    private float HealRatio(int slot) => Health.PartyMemberStates[slot].PredictedHPRatio + 0.3f * Hints.PredictedDamage.Count(d => d.Players[slot]);
    private int MissingWithoutRegen(float radius, float below = 0.85f) => LightParty.Count(p => p.Position.InCircle(Player.Position, radius) && PredictedRatio(p) < below && !HasHealOverTime(p));

    private bool PartyLow(in Strategy strategy, float radius, float ratio) => ShouldHealInAreaNow(strategy, Player.Position, radius, ratio + AreaShift());

    /// <summary>
    /// Run the given Action if the party has exactly one tank, otherwise do nothing
    /// </summary>
    /// <param name="tankFun"></param>
    private void RunForTank(Action<Actor, PartyMemberState> tankFun)
    {
        var tankSlot = -1;
        foreach (var (slot, actor) in World.Party.WithSlot(excludeAlliance: true))
            if (actor.ClassCategory == ClassCategory.Tank)
            {
                if (tankSlot >= 0)
                    return;
                else
                    tankSlot = slot;
            }

        if (tankSlot >= 0)
            tankFun(World.Party[tankSlot]!, Health.PartyMemberStates[tankSlot]!);
    }

    // dungeon trash pulls: no boss module, exactly one tank, holding at least 2 enemies
    private void RunForTrashPull(in Strategy strategy, Action<Actor, PartyMemberState, int> pullFun)
    {
        if (strategy.Heal != HealMode.Enabled || Bossmods.ActiveModule != null)
            return;
        RunForTank((tank, tankState) =>
        {
            var pulled = Hints.PotentialTargets.Count(e => e.Actor.InCombat && e.Actor.TargetID == tank.InstanceID);
            if (tank.InCombat && pulled >= 2)
                pullFun(tank, tankState, pulled);
        });
    }

    // big pull and the tank stopped moving: worth placing a ground heal on them
    private static bool StationaryBigPull(PartyMemberState tankState, int pulled) => pulled >= 3 && tankState.MoveDelta < 0.75f;

    private IEnumerable<Actor> LightParty => Health.TrackedMembers.Select(x => x.Item2);

    public override void Execute(in Strategy strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        Health.Update(Hints);
        AutoMit = strategy.Mitigation.Value == MitigationMode.Automatic;
        RaidwideIn = SecondsUntilNext(Raidwides);
        FullRaidwideIn = SecondsUntilNext(StateTimeline.Raidwides(Bossmods.ActiveModule, World, Hints, includeShared: false));
        var multiHit = Hints.PredictedDamage.Where(d => d.Hits > 1 && d.Type is AIHints.PredictedDamageType.Raidwide or AIHints.PredictedDamageType.Shared).ToList();
        MultiHitIn = SecondsUntilNext(multiHit.Select(d => d.Activation));
        MultiHitLong = multiHit.Any(d => d.Hits >= 8 || (d.LastHit - d.Activation).TotalSeconds >= 10);

        // module goals (towers, stacks, baits) come first; goals are summed, so keep this one at most 1
        if (strategy.StayNearParty.IsEnabled() && Player.InCombat && Hints.GoalZones.Count == 0)
        {
            List<(WPos pos, float radius)> allies = [.. LightParty.Exclude(Player).Select(e => (e.Position, e.HitboxRadius))];
            var currentCoverage = allies.Count(a => a.pos.InCircle(Player.Position, a.radius + 0.5f + 15));
            var scale = 1f / Math.Max(1, allies.Count);
            // require a margin to improve coverage, so allies moving near the edge don't make us shuffle
            Hints.GoalZones.Add(p => scale * Math.Max(currentCoverage, allies.Count(a => a.pos.InCircle(p, a.radius + 0.5f + 13))));
        }

        AutoRaise(strategy);

        var esuna = strategy.Esuna.Value;

        if (esuna.IsEnabled())
            foreach (var st in Health.PartyMemberStates)
                if (st.EsunableStatusRemaining > GCD + 1.14f && esuna.Check(Hints.ShouldCleanse[st.Slot]))
                    UseGCD(BossMod.WHM.AID.Esuna, World.Party[st.Slot]);

        switch (Player.Class)
        {
            case Class.CNJ or Class.WHM:
                AutoWHM(strategy);
                break;
            case Class.AST:
                AutoAST(strategy);
                break;
            case Class.SCH:
                AutoSCH(strategy);
                break;
            case Class.SGE:
                AutoSGE(strategy, primaryTarget);
                break;
        }
    }

    private void UseGCD<AID>(AID action, Actor? target, int extraPriority = 0, bool instant = false) where AID : Enum
        => UseGCD(ActionID.MakeSpell(action), target, extraPriority, instant);
    private void UseGCD(ActionID action, Actor? target, int extraPriority = 0, bool instant = false)
    {
        var def = ActionDefinitions.Instance[action];
        if (def == null)
            return;

        var castTime = instant ? 0 : Math.Max(0, def.CastTime - 0.5f);
        if (castTime > 0)
        {
            if (StatusDetails(Player, (uint)BossMod.WHM.SID.Swiftcast, Player.InstanceID).Left > GCD || StatusDetails(Player, (uint)ClassShared.SID.LostChainspell, Player.InstanceID).Left > GCD)
                castTime = 0;
        }

        Hints.ActionsToExecute.Push(action, target, ActionQueue.Priority.High + 500 + extraPriority, castTime: castTime);
    }

    private void UseOGCD<AID>(AID action, Actor? target, int extraPriority = 0) where AID : Enum
        => UseOGCD(ActionID.MakeSpell(action), target, extraPriority);
    private void UseOGCD(ActionID action, Actor? target, int extraPriority = 0)
        => Hints.ActionsToExecute.Push(action, target, ActionQueue.Priority.Medium + extraPriority);
    private void UseOGCDAt<AID>(AID action, Vector3 location, int extraPriority = 0) where AID : Enum
        => Hints.ActionsToExecute.Push(ActionID.MakeSpell(action), null, ActionQueue.Priority.Medium + extraPriority, targetPos: location);

    private void AutoRaise(in Strategy strategy)
    {
        // set of all statuses called "Resurrection Restricted"
        // TODO maybe this is a flag in sheets somewhere
        if (Player.Statuses.Any(s => s.ID is 1755 or 2449 or 3380 or 4262))
            return;

        var swiftcast = StatusDetails(Player, (uint)BossMod.WHM.SID.Swiftcast, Player.InstanceID, 15).Left;
        var thinair = StatusDetails(Player, (uint)BossMod.WHM.SID.ThinAir, Player.InstanceID, 12).Left;
        var swiftcastCD = NextChargeIn(BossMod.WHM.AID.Swiftcast);
        var raise = strategy.Raise.Value;
        // must outrank every heal GCD, otherwise a heal consumes the swiftcast meant for the raise
        const int swiftRaisePriority = 10;

        void UseThinAir()
        {
            if (thinair == 0 && Player.Class == Class.WHM)
                UseGCD(BossMod.WHM.AID.ThinAir, Player, extraPriority: 3);
        }

        switch (raise)
        {
            case RaiseStrategy.None:
                break;
            case RaiseStrategy.Hardcast:
                if (swiftcast == 0 && GetRaiseTarget(strategy) is Actor tar)
                {
                    UseThinAir();
                    UseGCD(RaiseAction, tar);
                }
                break;
            case RaiseStrategy.Swiftcast:
                if (GetRaiseTarget(strategy) is Actor tar2)
                {
                    if (swiftcast > GCD)
                    {
                        UseThinAir();
                        UseGCD(RaiseAction, tar2, extraPriority: swiftRaisePriority);
                    }
                    else
                        UseGCD(BossMod.WHM.AID.Swiftcast, Player);
                }
                break;
            case RaiseStrategy.Slowcast:
                if (GetRaiseTarget(strategy) is Actor tar3)
                {
                    UseThinAir();
                    UseGCD(BossMod.WHM.AID.Swiftcast, Player, extraPriority: 2);
                    if (swiftcastCD > 8 || !Unlocked(BossMod.WHM.AID.Swiftcast))
                        UseGCD(RaiseAction, tar3, extraPriority: swiftcast > GCD ? swiftRaisePriority : 1);
                }
                break;
        }
    }

    private Actor? GetRaiseTarget(in Strategy strategy) => RaiseUtil.FindRaiseTargets(World, strategy.RaiseTargets.Value).FirstOrDefault();

    private bool ShouldHealInAreaNow(in Strategy strategy, WPos pos, float radius, float ratio) => strategy.Heal.Value == HealMode.Enabled && Health.ShouldHealInArea(pos, radius, ratio);

    private void AutoWHM(in Strategy strategy)
    {
        var gauge = World.Client.GetGauge<WhiteMageGauge>();
        var canLily = gauge.Lily > 0 && gauge.BloodLily < 3;
        var hasDivineGrace = Player.FindStatus(BossMod.WHM.SID.DivineGrace) != null;
        var thinAirUp = Player.FindStatus(BossMod.WHM.SID.ThinAir, World.FutureTime(12)) != null;

        var bestC2 = BestActionUnlocked(BossMod.WHM.AID.CureII, BossMod.WHM.AID.Cure);
        var bestM2 = BestActionUnlocked(BossMod.WHM.AID.MedicaIII, BossMod.WHM.AID.MedicaII);
        var canApplyMedicaRegen = Unlocked(BossMod.WHM.AID.MedicaII) && CanApplyPartyRegen(bestM2 == BossMod.WHM.AID.MedicaIII ? BossMod.WHM.SID.MedicaIII : BossMod.WHM.SID.MedicaII, bestM2);

        if (strategy.Heal == HealMode.Enabled && AutoMit)
        {
            var rwBudget = RaidwideBudget(BossMod.WHM.AID.Temperance, BossMod.WHM.AID.DivineCaress, BossMod.WHM.AID.PlenaryIndulgence, BossMod.WHM.AID.Asylum);
            if (WantMultiHitTool && ReadySoon(BossMod.WHM.AID.LiturgyOfTheBell))
                UseOGCDAt(BossMod.WHM.AID.LiturgyOfTheBell, Player.PosRot.XYZ(), 21);
            if (RaidwideIn < 5)
            {
                PushMitOGCD(ref rwBudget, BossMod.WHM.AID.Temperance, Player, 20);
                if (hasDivineGrace)
                    PushMitOGCD(ref rwBudget, BossMod.WHM.AID.DivineCaress, Player, 19);
                PushMitOGCD(ref rwBudget, BossMod.WHM.AID.PlenaryIndulgence, Player, 18);
            }
            if (RaidwideIn < 3 && Health.PartyHealth.AvgCurrent <= 0.9f)
                PushMitOGCDAt(ref rwBudget, BossMod.WHM.AID.Asylum, GetBestPartyCoverage(10), 17);

            foreach (var (tank, _) in TankbustersWithin(4))
            {
                var tbBudget = 2;
                if (tank.FindStatus(BossMod.WHM.SID.DivineBenison, World.FutureTime(15)) == null)
                    PushMitOGCD(ref tbBudget, BossMod.WHM.AID.DivineBenison, tank, 25);
                PushMitOGCD(ref tbBudget, BossMod.WHM.AID.Aquaveil, tank, 24);
            }
        }

        HealLowest(strategy, false, (target, ratio) =>
        {
            var tank = target.Role == Role.Tank;
            if (PreferRegen(target))
            {
                if (AutoMit && MissingWithoutRegen(10) >= 3 && ReadySoon(BossMod.WHM.AID.Asylum))
                    UseOGCDAt(BossMod.WHM.AID.Asylum, GetBestPartyCoverage(10, injuredOnly: true), 12);
                else if (MissingWithoutRegen(20, 0.8f) >= 3 && canApplyMedicaRegen)
                    UseGCD(bestM2, Player, 2);
                else if (PredictedRatio(target) < 0.7f)
                    UseGCD(BossMod.WHM.AID.Regen, target, 2);
                return;
            }
            if (ratio <= 0.2f)
                UseOGCD(BossMod.WHM.AID.Benediction, target, 12);
            if (ratio <= 0.55f)
                UseOGCD(BossMod.WHM.AID.Tetragrammaton, target, 11);
            if (AutoMit && tank && ratio <= 0.7f && Bossmods.ActiveModule == null)
                UseOGCD(BossMod.WHM.AID.Aquaveil, target, 10);

            var ogcdCovers = Unlocked(BossMod.WHM.AID.Tetragrammaton) && NextChargeIn(BossMod.WHM.AID.Tetragrammaton) < 1;
            if (!SingledOut(ratio))
                return;
            if (ratio <= 0.55f && canLily && Unlocked(BossMod.WHM.AID.AfflatusSolace))
                UseGCD(BossMod.WHM.AID.AfflatusSolace, target, 2);
            else if (ratio <= 0.3f || ratio <= 0.5f && !ogcdCovers)
            {
                if (!thinAirUp && Player.HPMP.CurMP < 8000)
                    UseOGCD(BossMod.WHM.AID.ThinAir, Player, 1);
                UseGCD(bestC2, target, 1);
            }
            else if (ratio <= 0.8f && tank && target.FindStatus(BossMod.WHM.SID.Regen, World.FutureTime(18)) == null)
                UseGCD(BossMod.WHM.AID.Regen, target);
        });

        HealLowest(strategy, true, (target, ratio) =>
        {
            if (AutoMit && ratio < 0.75f && target.FindStatus(BossMod.WHM.SID.DivineBenison, World.FutureTime(15)) == null)
                UseOGCD(BossMod.WHM.AID.DivineBenison, target, 9);
        });

        RunForTrashPull(strategy, (tank, tankState, pulled) =>
        {
            if (tank.FindStatus(BossMod.WHM.SID.Regen, Player.InstanceID, World.FutureTime(18)) == null && tank.HPRatio < 0.95f)
                UseGCD(BossMod.WHM.AID.Regen, tank);
            if (tank.FindStatus(BossMod.WHM.SID.DivineBenison, World.FutureTime(15)) == null)
                UseOGCD(BossMod.WHM.AID.DivineBenison, tank, 8);
            if (StationaryBigPull(tankState, pulled))
                UseOGCDAt(BossMod.WHM.AID.Asylum, tank.PosRot.XYZ(), 7);
        });

        if (PartyLow(strategy, 20, 0.8f))
        {
            UseOGCD(BossMod.WHM.AID.Assize, Player, 16);
            if (AutoMit)
                UseOGCD(BossMod.WHM.AID.PlenaryIndulgence, Player, 15);
            if (AutoMit && hasDivineGrace)
                UseOGCD(BossMod.WHM.AID.DivineCaress, Player, 14);
            if (canApplyMedicaRegen)
                UseGCD(bestM2, Player);
        }
        if (AutoMit && PartyLow(strategy, 30, 0.55f))
            UseOGCD(BossMod.WHM.AID.Temperance, Player, 13);
        if (AutoMit && PartyLow(strategy, 20, 0.5f) && Player.FindStatus(BossMod.WHM.SID.LiturgyOfTheBell, World.FutureTime(20)) == null)
            UseOGCDAt(BossMod.WHM.AID.LiturgyOfTheBell, Player.PosRot.XYZ(), 12);
        var useRapture = canLily && Unlocked(BossMod.WHM.AID.AfflatusRapture) && PartyLow(strategy, 20, 0.7f);
        if (useRapture)
            UseGCD(BossMod.WHM.AID.AfflatusRapture, Player, 3);
        if (PartyLow(strategy, 10, 0.55f) && Unlocked(BossMod.WHM.AID.CureIII) && LightParty.Count(p => p.Position.InCircle(Player.Position, 10)) >= 4)
        {
            if (!thinAirUp)
                UseOGCD(BossMod.WHM.AID.ThinAir, Player, 1);
            UseGCD(BossMod.WHM.AID.CureIII, Player, 2);
        }
        // no cure III or rapture: refresh the medica regen if possible, otherwise plain medica
        else if (PartyLow(strategy, 15, 0.55f) && !useRapture)
            UseGCD(canApplyMedicaRegen ? bestM2 : BossMod.WHM.AID.Medica, Player);
    }

    private static readonly (AstrologianCard, BossMod.AST.AID)[] SupportCards = [
        (AstrologianCard.Arrow, BossMod.AST.AID.TheArrow),
        (AstrologianCard.Spire, BossMod.AST.AID.TheSpire),
        (AstrologianCard.Bole, BossMod.AST.AID.TheBole),
        (AstrologianCard.Ewer, BossMod.AST.AID.TheEwer)
    ];

    private void AutoAST(in Strategy strategy)
    {
        var gauge = World.Client.GetGauge<AstrologianGauge>();
        AstrologianCard[] cards = [gauge.Card1, gauge.Card2, gauge.Card3];

        var heliosHoT = Unlocked(BossMod.AST.AID.HeliosConjunction) ? BossMod.AST.SID.HeliosConjunction : BossMod.AST.SID.AspectedHelios;
        var canApplyHeliosHoT = Unlocked(BossMod.AST.AID.AspectedHelios) && CanApplyPartyRegen(heliosHoT, BossMod.AST.AID.AspectedHelios, BossMod.AST.AID.HeliosConjunction);

        if (strategy.Heal == HealMode.Enabled && AutoMit)
        {
            var rwBudget = RaidwideBudget(BossMod.AST.AID.CollectiveUnconscious);
            if (RaidwideIn < 5)
                PushMitOGCD(ref rwBudget, BossMod.AST.AID.CollectiveUnconscious, Player, 20);
            if (WantMultiHitTool)
                UseGCD(BossMod.AST.AID.Macrocosmos, Player, 6);
            if (Player.InCombat)
                UseOGCDAt(BossMod.AST.AID.EarthlyStar, GetBestPartyCoverage(20), 5);

            foreach (var (tank, _) in TankbustersWithin(4))
            {
                UseOGCD(BossMod.AST.AID.Exaltation, tank, 25);
                UseOGCD(BossMod.AST.AID.CelestialIntersection, tank, 24);
            }
        }

        HealLowest(strategy, false, (target, ratio) =>
        {
            var tank = target.Role == Role.Tank;
            if (PreferRegen(target))
            {
                if (MissingWithoutRegen(15, 0.8f) >= 3 && canApplyHeliosHoT)
                    UseGCD(BossMod.AST.AID.AspectedHelios, Player, 2);
                else if (PredictedRatio(target) < 0.7f && Unlocked(BossMod.AST.AID.AspectedBenefic))
                    UseGCD(BossMod.AST.AID.AspectedBenefic, target, 2);
                return;
            }
            if (ratio <= 0.3f)
                UseOGCD(BossMod.AST.AID.EssentialDignity, target, 12);
            if (ratio <= 0.5f)
                UseOGCD(BossMod.AST.AID.CelestialIntersection, target, 11);
            if (ratio <= 0.55f)
                foreach (var (card, action) in SupportCards)
                    if (cards.Contains(card))
                        UseOGCD(action, target, 10);
            if (AutoMit && tank && ratio <= 0.6f)
                UseOGCD(BossMod.AST.AID.Exaltation, target, 9);

            var ogcdCovers = ReadySoon(BossMod.AST.AID.EssentialDignity) || ReadySoon(BossMod.AST.AID.CelestialIntersection);
            if (!SingledOut(ratio))
                return;
            if (ratio <= 0.3f || ratio <= 0.5f && !ogcdCovers)
                UseGCD(BestActionUnlocked(BossMod.AST.AID.BeneficII, BossMod.AST.AID.Benefic), target, 1);
            else if (ratio <= 0.8f && tank && Unlocked(BossMod.AST.AID.AspectedBenefic) && target.FindStatus(BossMod.AST.SID.AspectedBenefic, World.FutureTime(15)) == null)
                UseGCD(BossMod.AST.AID.AspectedBenefic, target);
        });

        RunForTrashPull(strategy, (tank, tankState, pulled) =>
        {
            if (Unlocked(BossMod.AST.AID.AspectedBenefic) && tank.FindStatus(BossMod.AST.SID.AspectedBenefic, Player.InstanceID, World.FutureTime(15)) == null && tank.HPRatio < 0.95f)
                UseGCD(BossMod.AST.AID.AspectedBenefic, tank);
            UseOGCD(BossMod.AST.AID.Exaltation, tank, 8);
            if (StationaryBigPull(tankState, pulled))
                UseOGCDAt(BossMod.AST.AID.EarthlyStar, tank.PosRot.XYZ(), 7);
        });

        if (PartyLow(strategy, 20, 0.8f))
        {
            if (gauge.CurrentArcana == AstrologianCard.Lady)
                UseOGCD(BossMod.AST.AID.LadyOfCrowns, Player, 16);
            if (AutoMit)
                UseOGCD(BossMod.AST.AID.CelestialOpposition, Player, 15);
            if (canApplyHeliosHoT)
                UseGCD(BossMod.AST.AID.AspectedHelios, Player);
        }
        if (PartyLow(strategy, 15, 0.6f) && !canApplyHeliosHoT)
            UseGCD(BossMod.AST.AID.Helios, Player, 2);
    }

    private void AutoSCH(in Strategy strategy)
    {
        var gauge = World.Client.GetGauge<ScholarGauge>();
        var aetherflow = gauge.Aetherflow > 0;
        var pet = World.Client.ActivePet.InstanceID == 0xE0000000 ? null : World.Actors.Find(World.Client.ActivePet.InstanceID);
        var haveSeraph = gauge.SeraphTimer > 0; // aetherpact and fey blessing are refused while seraph is out
        var succor = BestActionUnlocked(BossMod.SCH.AID.Concitation, BossMod.SCH.AID.Succor);
        var useOutOfCombat = strategy.OutOfCombat.IsEnabled();

        void UseSoil(Vector3 location, int prio)
        {
            if (aetherflow)
                UseOGCDAt(BossMod.SCH.AID.SacredSoil, location, prio);
        }

        // fairy abilities are centered on the pet
        bool PetLow(in Strategy s, float radius, float ratio) => pet != null && ShouldHealInAreaNow(s, pet.Position, radius, ratio + AreaShift());

        if (strategy.Heal == HealMode.Enabled && AutoMit)
        {
            var rwBudget = RaidwideBudget(BossMod.SCH.AID.SacredSoil, BossMod.SCH.AID.Expedient);
            if (WantMultiHitTool && !haveSeraph && ReadySoon(BossMod.SCH.AID.Seraphism))
                UseOGCD(BossMod.SCH.AID.Seraphism, Player, 21);
            if (RaidwideIn < 5 && rwBudget > 0 && NextChargeIn(BossMod.SCH.AID.SacredSoil) == 0)
            {
                UseSoil(GetBestPartyCoverage(15), 20);
                --rwBudget;
            }
            if (RaidwideIn < 8 && pet != null)
                UseOGCD(BossMod.SCH.AID.FeyIllumination, Player, 19);
            if (RaidwideIn < 5)
                PushMitOGCD(ref rwBudget, BossMod.SCH.AID.Expedient, Player, 18);
            if (FullRaidwideIn < GCD + 2.5f && UnshieldedShare(15) >= 0.5f)
                UseGCD(succor, Player, 5);

            foreach (var (tank, busterIn) in TankbustersWithin(5))
            {
                UseOGCD(BossMod.SCH.AID.Protraction, tank, 25);
                if (aetherflow)
                    UseOGCD(BossMod.SCH.AID.Excogitation, tank, 24);
                if (busterIn < GCD + 2.5f && !HasGCDShield(tank))
                    UseGCD(BossMod.SCH.AID.Adloquium, tank, 4);
            }
        }

        HealLowest(strategy, false, (target, ratio) =>
        {
            var tank = target.Role == Role.Tank;
            if (PreferRegen(target) && MissingWithoutRegen(15) >= 3 && pet != null && ReadySoon(BossMod.SCH.AID.WhisperingDawn))
            {
                UseOGCD(BossMod.SCH.AID.WhisperingDawn, Player, 12);
                return;
            }
            if (tank && ratio <= 0.6f && aetherflow)
                UseOGCD(BossMod.SCH.AID.Excogitation, target, 12);
            if (ratio <= 0.5f && gauge.FairyGauge >= 10 && pet != null && !haveSeraph && target.FindStatus(BossMod.SCH.SID.FeyUnion, World.FutureTime(1000)) == null)
                UseOGCD(BossMod.SCH.AID.Aetherpact, target, 11);
            if (AutoMit && tank && ratio <= 0.5f)
                UseOGCD(BossMod.SCH.AID.Protraction, target, 10);
            // aetherflow is too valuable to spend on lustrate unless it's an emergency
            if (ratio <= 0.25f && aetherflow)
                UseOGCD(BossMod.SCH.AID.Lustrate, target, 9);

            var ogcdCovers = tank && aetherflow && ReadySoon(BossMod.SCH.AID.Excogitation);
            if (SingledOut(ratio) && (ratio <= 0.3f || ratio <= 0.5f && !ogcdCovers))
                UseGCD(BestActionUnlocked(BossMod.SCH.AID.Adloquium, BossMod.SCH.AID.Physick), target, 3);
        });

        HealLowest(strategy, true, (target, ratio) =>
        {
            if (ratio < 0.5f && aetherflow)
                UseOGCD(BossMod.SCH.AID.Excogitation, target, 8);
        });

        // stop draining the fairy gauge once the union target is healthy again; only our own union can be dissolved
        if (pet != null && LightParty.FirstOrDefault(p => p.FindStatus(BossMod.SCH.SID.FeyUnion, pet.InstanceID) != null || p.FindStatus(BossMod.SCH.SID.FeyUnion, Player.InstanceID) != null) is { } unionTarget && PredictedRatio(unionTarget) >= 0.95f)
            UseOGCD(BossMod.SCH.AID.DissolveUnion, Player, 5);

        if (strategy.Heal == HealMode.Enabled && useOutOfCombat)
        {
            RunForTank((tank, tankState) =>
            {
                if (!Player.InCombat && (World.CurrentTime - tankState.LastCombat).TotalSeconds > 1)
                {
                    // excog's AllowExecute needs aetherflow even under recitation
                    if (aetherflow && NextChargeIn(BossMod.SCH.AID.Excogitation) == 0)
                        UseOGCD(BossMod.SCH.AID.Recitation, Player, 5);
                    UseOGCD(BossMod.SCH.AID.Excogitation, tank);
                }
            });
        }

        RunForTrashPull(strategy, (tank, tankState, pulled) =>
        {
            if (StationaryBigPull(tankState, pulled))
                UseSoil(tank.PosRot.XYZ(), 5);
        });

        if (PetLow(strategy, 30, 0.5f))
        {
            if (haveSeraph)
                UseOGCD(BossMod.SCH.AID.Consolation, Player, 17);
            else if (AutoMit)
                UseOGCD(BossMod.SCH.AID.SummonSeraph, Player, 17);
        }
        if (AutoMit && !haveSeraph && PetLow(strategy, 20, 0.6f))
            UseOGCD(BossMod.SCH.AID.FeyBlessing, Player, 16);
        if (PetLow(strategy, 15, 0.8f))
            UseOGCD(BossMod.SCH.AID.WhisperingDawn, Player, 15);
        if (aetherflow && PartyLow(strategy, 15, 0.6f))
            UseOGCD(BossMod.SCH.AID.Indomitability, Player, 14);
        if (PartyLow(strategy, 15, 0.5f) && UnshieldedShare(15) >= 0.5f && !(aetherflow && ReadySoon(BossMod.SCH.AID.Indomitability)))
            UseGCD(succor, Player, 1);
    }

    // O(n³) :3
    private Vector3 GetBestPartyCoverage(float radius, bool injuredOnly = false)
    {
        var allies = LightParty.Where(p => !injuredOnly || PredictedRatio(p) < 0.85f && !HasHealOverTime(p)).Select(p => p.Position).ToList();
        if (allies.Count < 2)
            return Player.PosRot.XYZ();

        var rsq = radius * radius;
        var bestCount = 0;
        var bestCenter = allies[0];
        for (var i = 0; i < allies.Count; i++)
        {
            for (var j = i; j < allies.Count; j++)
            {
                var center = WPos.Lerp(allies[i], allies[j], 0.5f);
                var thisCount = allies.Count(pos => (pos - center).LengthSq() <= rsq);
                if (thisCount > bestCount)
                {
                    bestCount = thisCount;
                    bestCenter = center;
                }
            }
        }

        return new Vector3(bestCenter.X, Player.PosRot.Y, bestCenter.Z);
    }

    // galvanize and the eukrasian shields don't stack with each other
    private static readonly uint[] GCDShields = [(uint)BossMod.SGE.SID.EukrasianDiagnosis, (uint)BossMod.SGE.SID.EukrasianPrognosis, (uint)BossMod.SCH.SID.Galvanize];
    private static bool HasGCDShield(Actor a) => HasAnyStatus(a, GCDShields);

    // share of party members in range without a GCD shield
    private float UnshieldedShare(float radius)
    {
        var inRange = LightParty.Where(p => p.Position.InCircle(Player.Position, radius)).ToList();
        return inRange.Count == 0 ? 0 : (float)inRange.Count(p => !HasGCDShield(p)) / inRange.Count;
    }

    private bool IsCasting<AID>(params AID[] aids) where AID : Enum => Player.CastInfo?.Action is { } cast && aids.Any(a => cast == ActionID.MakeSpell(a));

    // party regen on ourselves is (nearly) gone, including pending statuses, and we aren't already casting it
    private bool CanApplyPartyRegen<SID, AID>(SID regen, params AID[] casts) where SID : Enum where AID : Enum
        => StatusDetails(Player, regen, Player.InstanceID).Left < 3 && !IsCasting(casts);

    private bool ReadySoon<AID>(AID aid) where AID : Enum => Unlocked(aid) && NextChargeIn(aid) < 0.6f;

    // predictive raidwide/tankbuster mits: at most 2 ready big oGCDs per event (GCDs/shields don't count)
    private bool PushMitOGCD<AID>(ref int budget, AID aid, Actor? target, int prio) where AID : Enum
    {
        if (budget <= 0 || !ReadySoon(aid))
            return false;
        UseOGCD(aid, target, prio);
        --budget;
        return true;
    }

    private bool PushMitOGCDAt<AID>(ref int budget, AID aid, Vector3 location, int prio) where AID : Enum
    {
        if (budget <= 0 || !ReadySoon(aid))
            return false;
        UseOGCDAt(aid, location, prio);
        --budget;
        return true;
    }

    // party mitigation; another healer's on most of the light party means the hit is already partly covered
    private static readonly uint[] PartyMitStatuses = [
        (uint)BossMod.SGE.SID.Kerachole, (uint)BossMod.SGE.SID.Panhaima, (uint)BossMod.SGE.SID.Holosakos, (uint)BossMod.AST.SID.Macrocosmos,
        (uint)BossMod.SCH.SID.SacredSoil, (uint)BossMod.SCH.SID.DesperateMeasures, (uint)BossMod.AST.SID.CollectiveUnconscious,
        (uint)BossMod.WHM.SID.TemperanceParty, (uint)BossMod.AST.SID.SunSign,
    ];
    // another healer's multi-hit tool: on the party, or on the caster (Bell, Seraphism)
    private static readonly uint[] MultiHitPartyStatuses = [(uint)BossMod.SGE.SID.Panhaima, (uint)BossMod.AST.SID.Macrocosmos, (uint)BossMod.SCH.SID.SeraphismRegen];
    private static readonly uint[] MultiHitSelfStatuses = [(uint)BossMod.WHM.SID.LiturgyOfTheBell, (uint)BossMod.SCH.SID.Seraphism];

    private bool CoveredByOthers(uint[] ids)
    {
        var party = LightParty.ToList();
        return party.Count > 0 && party.Count(p => p.Statuses.Any(s => ids.Contains(s.ID) && s.SourceID != Player.InstanceID)) * 2 >= party.Count;
    }

    private bool OtherMultiHitToolUp => CoveredByOthers(MultiHitPartyStatuses) || LightParty.Any(p => p != Player && p.Statuses.Any(s => MultiHitSelfStatuses.Contains(s.ID)));

    // a multi-hit sequence starts soon and no healer has a multi-hit tool out yet
    private bool WantMultiHitTool => AutoMit && MultiHitIn < 5 && !OtherMultiHitToolUp;

    // big party oGCDs for one raidwide or sequence: 2, one less if the co-healer already covers it, one more for long sequences
    private int RaidwideBudget<AID>(params AID[] tools) where AID : Enum
        => 2 - (CoveredByOthers(PartyMitStatuses) ? 1 : 0) + (MultiHitLong ? 1 : 0) - tools.Count(t => UsedWithin(ActionID.MakeSpell(t), 10));

    private bool UsedWithin(ActionID action, float seconds)
        => ActionDefinitions.Instance[action] is { MainCooldownGroup: >= 0 } def && World.Client.Cooldowns[def.MainCooldownGroup] is { Total: > 0 } cd && cd.Elapsed < seconds;

    private bool SingledOut(float ratio) => Health.PartyHealth.Count <= 1 || ratio < Health.PartyHealth.AvgCurrent - 0.2f;

    private void AutoSGE(in Strategy strategy, Actor? primaryTarget)
    {
        var gauge = World.Client.GetGauge<SageGauge>();
        var gall = gauge.Addersgall;
        var eukrasia = gauge.EukrasiaActive;
        var physis = Unlocked(BossMod.SGE.AID.PhysisII) ? BossMod.SGE.AID.PhysisII : BossMod.SGE.AID.Physis;
        var keracholeRegen = Player.Level >= 78; // enhanced kerachole trait

        // never toggle eukrasia for a target out of range or line of sight: the dps rotation would burn it on a dosis and we'd toggle it again every gcd
        void UseEukrasian(BossMod.SGE.AID heal, Actor target, int prio)
        {
            if (eukrasia)
                UseGCD(heal, target, prio, instant: true);
            else if (Player.DistanceToHitbox(target) <= 30 && target.Visibility != Visibility.Blocked)
                UseGCD(BossMod.SGE.AID.Eukrasia, Player, prio);
        }

        if (strategy.Heal == HealMode.Enabled)
        {
            var rwBudget = RaidwideBudget(BossMod.SGE.AID.Kerachole, BossMod.SGE.AID.Holos);
            if (WantMultiHitTool && ReadySoon(BossMod.SGE.AID.Panhaima))
                UseOGCD(BossMod.SGE.AID.Panhaima, Player, 21);
            if (AutoMit && RaidwideIn < 8 && gall > 0)
                PushMitOGCD(ref rwBudget, BossMod.SGE.AID.Kerachole, Player, 20);
            if (AutoMit && RaidwideIn < 5 && Health.PartyHealth.AvgCurrent <= 0.8f)
                PushMitOGCD(ref rwBudget, BossMod.SGE.AID.Holos, Player, 15);
            if (FullRaidwideIn < GCD + 2.5f && UnshieldedShare(15) >= 0.5f)
                UseEukrasian(BossMod.SGE.AID.Prognosis, Player, 5);

            foreach (var (tank, busterIn) in TankbustersWithin(5))
            {
                if (AutoMit && tank.FindStatus(BossMod.SGE.SID.Haima, World.FutureTime(15)) == null)
                    UseOGCD(BossMod.SGE.AID.Haima, tank, 25);
                if (AutoMit && busterIn < 3 && gall > 0)
                    UseOGCD(BossMod.SGE.AID.Taurochole, tank, 24);
                if (busterIn < GCD + 2.5f && !HasGCDShield(tank))
                    UseEukrasian(BossMod.SGE.AID.Diagnosis, tank, 4);
            }
        }

        HealLowest(strategy, false, (target, ratio) =>
        {
            var tank = target.Role == Role.Tank;
            var raw = PredictedRatio(target);
            if (AutoMit && QuietPeriod && raw > 0.3f && (raw < 0.75f || MissingWithoutRegen(30) >= 2) && !HasHealOverTime(target))
            {
                if (ReadySoon(physis))
                {
                    UseOGCD(physis, Player, 12);
                    return;
                }
                if (gall >= 3 && keracholeRegen && ReadySoon(BossMod.SGE.AID.Kerachole))
                {
                    UseOGCD(BossMod.SGE.AID.Kerachole, Player, 12);
                    return;
                }
            }
            if (tank && ratio <= 0.7f)
                UseOGCD(BossMod.SGE.AID.Krasis, target, 12);
            if (AutoMit && tank && ratio <= 0.5f && gall > 0)
                UseOGCD(BossMod.SGE.AID.Taurochole, target, 11);
            if (AutoMit && tank && ratio <= 0.45f)
                UseOGCD(BossMod.SGE.AID.Haima, target, 10);
            if (ratio <= 0.7f && target.FindStatus(BossMod.SGE.SID.Kardion, Player.InstanceID) != null)
                UseOGCD(BossMod.SGE.AID.Soteria, Player, 9);
            if (ratio <= 0.55f && gall > 0)
                UseOGCD(BossMod.SGE.AID.Druochole, target, 8);
            if (AutoMit && ratio <= 0.4f)
                UseOGCD(BossMod.SGE.AID.Zoe, Player, 7);

            var ogcdCovers = gall > 0 && Unlocked(BossMod.SGE.AID.Druochole) || AutoMit && tank && (gall > 0 && ReadySoon(BossMod.SGE.AID.Taurochole) || ReadySoon(BossMod.SGE.AID.Haima));
            if (SingledOut(ratio) && (ratio <= 0.3f || ratio <= 0.5f && !ogcdCovers))
            {
                if (Unlocked(BossMod.SGE.AID.Eukrasia) && !HasGCDShield(target))
                    UseEukrasian(BossMod.SGE.AID.Diagnosis, target, 3);
                else
                    UseGCD(BossMod.SGE.AID.Diagnosis, target, 2);
            }
        });

        HealLowest(strategy, true, (target, ratio) =>
        {
            if (AutoMit && ratio < 0.5f && target.Role == Role.Tank)
                UseOGCD(BossMod.SGE.AID.Haima, target, 10);
        });

        RunForTrashPull(strategy, (tank, tankState, pulled) =>
        {
            if (AutoMit && tank.HPRatio < 0.8f && tank.FindStatus(BossMod.SGE.SID.Haima, World.FutureTime(15)) == null)
                UseOGCD(BossMod.SGE.AID.Haima, tank, 8);
            if (gall > 0 && keracholeRegen && StationaryBigPull(tankState, pulled) && tank.Position.InCircle(Player.Position, 30))
                UseOGCD(BossMod.SGE.AID.Kerachole, Player, 7);
        });

        if (AutoMit && PartyLow(strategy, 30, 0.8f))
        {
            UseOGCD(physis, Player, 19);
            if (gall > 0 && keracholeRegen)
                UseOGCD(BossMod.SGE.AID.Kerachole, Player, 18);
        }
        if (AutoMit && PartyLow(strategy, 30, 0.65f))
            UseOGCD(BossMod.SGE.AID.Holos, Player, 17);
        if (AutoMit && PartyLow(strategy, 20, 0.6f))
            UseOGCD(BossMod.SGE.AID.Philosophia, Player, 16);
        if (AutoMit && PartyLow(strategy, 30, 0.55f) && Player.FindStatus(BossMod.SGE.SID.Panhaima, World.FutureTime(15)) == null)
            UseOGCD(BossMod.SGE.AID.Panhaima, Player, 15);
        if (gall > 0 && PartyLow(strategy, 15, 0.7f))
            UseOGCD(BossMod.SGE.AID.Ixochole, Player, 14);

        if (AutoMit && PartyLow(strategy, 20, 0.45f) && ReadySoon(BossMod.SGE.AID.Pneuma) && primaryTarget is { IsAlly: false } enemy && Player.DistanceToHitbox(enemy) <= 25)
        {
            UseOGCD(BossMod.SGE.AID.Zoe, Player, 13);
            UseGCD(BossMod.SGE.AID.Pneuma, enemy, 1);
        }

        if (PartyLow(strategy, 15, 0.55f))
        {
            if (!Unlocked(BossMod.SGE.AID.Eukrasia))
                UseGCD(BossMod.SGE.AID.Prognosis, Player, 1);
            else if (UnshieldedShare(15) >= 0.5f && !(gall > 0 && ReadySoon(BossMod.SGE.AID.Ixochole))
                && !(AutoMit && (ReadySoon(BossMod.SGE.AID.PhysisII) || ReadySoon(BossMod.SGE.AID.Holos))))
                UseEukrasian(BossMod.SGE.AID.Prognosis, Player, 1);
        }
    }
}
