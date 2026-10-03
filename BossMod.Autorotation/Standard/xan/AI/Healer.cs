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
    }

    private readonly TrackPartyHealth Health = new(manager.WorldState);

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

    private void HealSingleSoon(in Strategy strategy, Action<Actor, float> healFun)
    {
        switch (strategy.Heal.Value)
        {
            case HealMode.Enabled:
                if (Health.BestSTHealTargetPredicted is (var a, var b))
                    healFun(a, b.PredictedHPRatio);
                break;
            case HealMode.Babysit:
                var targetSlot = ResolveHealTarget(strategy);

                if (targetSlot >= 0 && Health.PartyMemberStates[targetSlot].NoHealStatusRemaining < 1.5f && !World.Party[targetSlot]!.IsDead)
                    healFun(World.Party[targetSlot]!, Health.PartyMemberStates[targetSlot].PredictedHPRatio);
                break;
        }
    }

    private void HealSingleNow(in Strategy strategy, Action<Actor, float> healFun)
    {
        switch (strategy.Heal.Value)
        {
            case HealMode.Enabled:
                if (Health.BestSTHealTarget is (var a, var b))
                    healFun(a, b.PredictedHPRatio);
                break;
            case HealMode.Babysit:
                var targetSlot = ResolveHealTarget(strategy);

                if (targetSlot >= 0 && Health.PartyMemberStates[targetSlot].NoHealStatusRemaining < 1.5f && !World.Party[targetSlot]!.IsDead)
                    healFun(World.Party[targetSlot]!, Health.PartyMemberStates[targetSlot].CurrentHPRatio);
                break;
        }
    }

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

    private IEnumerable<Actor> LightParty => Health.TrackedMembers.Select(x => x.Item2);

    private Vector3? ArenaCenter
    {
        get
        {
            if (Bossmods.ActiveModule is BossModule m)
            {
                var center = m.Arena.Center;
                return new Vector3(center.X, Player.PosRot.Y, center.Z);
            }
            return null;
        }
    }

    public override void Execute(in Strategy strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        Health.Update(Hints);

        if (strategy.StayNearParty.IsEnabled() && Player.InCombat)
        {
            List<(WPos pos, float radius)> allies = [.. LightParty.Exclude(Player).Select(e => (e.Position, e.HitboxRadius))];
            Hints.GoalZones.Add(p => allies.Count(a => a.pos.InCircle(p, a.radius + 0.5f + 15)));
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
                AutoSCH(strategy, primaryTarget);
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
                        UseGCD(RaiseAction, tar2);
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
                    if (swiftcastCD > 8)
                        UseGCD(RaiseAction, tar3, extraPriority: 1);
                }
                break;
        }
    }

    private Actor? GetRaiseTarget(in Strategy strategy) => RaiseUtil.FindRaiseTargets(World, strategy.RaiseTargets.Value).FirstOrDefault();

    private bool ShouldHealInAreaSoon(in Strategy strategy, WPos pos, float radius, float ratio) => strategy.Heal.Value == HealMode.Enabled && Health.PredictShouldHealInArea(pos, radius, ratio);
    private bool ShouldHealInAreaNow(in Strategy strategy, WPos pos, float radius, float ratio) => strategy.Heal.Value == HealMode.Enabled && Health.ShouldHealInArea(pos, radius, ratio);

    // pre-damage tools from raidwide / tankbuster predictions, then HP-threshold heals
    private void AutoWHM(in Strategy strategy)
    {
        var gauge = World.Client.GetGauge<WhiteMageGauge>();
        var canLily = gauge.Lily > 0 && gauge.BloodLily < 3;
        var inBoss = Bossmods.ActiveModule != null;

        var bestC2 = BestActionUnlocked(BossMod.WHM.AID.CureII, BossMod.WHM.AID.Cure);
        var bestM2 = BestActionUnlocked(BossMod.WHM.AID.MedicaIII, BossMod.WHM.AID.MedicaII);

        // Tetragrammaton / Divine Benison: whichever has more charges goes first
        var tetraFirst = NextChargeIn(BossMod.WHM.AID.Tetragrammaton) <= NextChargeIn(BossMod.WHM.AID.DivineBenison);

        if (strategy.Heal == HealMode.Enabled)
        {
            var raidwideIn = Raidwides.Select(r => (float)(r - World.CurrentTime).TotalSeconds).Where(t => t >= 0).DefaultIfEmpty(float.MaxValue).Min();
            if (raidwideIn < 5)
            {
                UseOGCD(BossMod.WHM.AID.Temperance, Player, 20);
                if (Player.FindStatus(BossMod.WHM.SID.DivineGrace) != null)
                    UseOGCD(BossMod.WHM.AID.DivineCaress, Player, 19);
                UseOGCD(BossMod.WHM.AID.PlenaryIndulgence, Player, 18);
            }
            // Asylum goes where it covers the most of the party
            if (raidwideIn < 3 && Health.PartyHealth.AvgCurrent <= 0.9f)
                Hints.ActionsToExecute.Push(ActionID.MakeSpell(BossMod.WHM.AID.Asylum), null, ActionQueue.Priority.Medium + 17, targetPos: GetBestPartyCoverage(10));

            foreach (var (tank, at) in Tankbusters)
            {
                var busterIn = (float)(at - World.CurrentTime).TotalSeconds;
                if (busterIn is < 0 or > 4 || tank.IsDead || World.Party.FindSlot(tank.InstanceID) < 0)
                    continue;
                if (tank.FindStatus(BossMod.WHM.SID.DivineBenison) == null)
                    UseOGCD(BossMod.WHM.AID.DivineBenison, tank, 25);
                UseOGCD(BossMod.WHM.AID.Aquaveil, tank, 24);
            }
        }

        HealSingleNow(strategy, (target, ratio) =>
        {
            var tank = target.Role == Role.Tank;
            if (ratio <= 0.2f)
                UseOGCD(BossMod.WHM.AID.Benediction, target, 12);
            if (tank && ratio <= 0.7f && !inBoss)
                UseOGCD(BossMod.WHM.AID.Aquaveil, target, 11);
            if (ratio <= 0.7f && target.FindStatus(BossMod.WHM.SID.DivineBenison) == null)
                UseOGCD(BossMod.WHM.AID.DivineBenison, target, tetraFirst ? 9 : 10);
            if (ratio <= 0.55f)
                UseOGCD(BossMod.WHM.AID.Tetragrammaton, target, tetraFirst ? 10 : 9);

            // GCD heals only when the oGCDs can't cover it
            var ogcdCovers = NextChargeIn(BossMod.WHM.AID.Tetragrammaton) < 1 || ratio <= 0.2f && NextChargeIn(BossMod.WHM.AID.Benediction) < 1;
            if (ratio <= 0.55f && canLily)
                UseGCD(BossMod.WHM.AID.AfflatusSolace, target, 2);
            else if (ratio <= 0.3f || ratio <= 0.5f && !ogcdCovers)
            {
                if (Player.FindStatus(BossMod.WHM.SID.ThinAir) == null && Player.HPMP.CurMP < 8000)
                    UseOGCD(BossMod.WHM.AID.ThinAir, Player, 1);
                UseGCD(bestC2, target, 1);
            }
            else if (ratio <= 0.8f && ratio > 0.3f && tank && target.FindStatus(BossMod.WHM.SID.Regen) == null)
                UseGCD(BossMod.WHM.AID.Regen, target);
        });

        HealSingleSoon(strategy, (target, ratio) =>
        {
            if (ratio < 0.75f && target.FindStatus(BossMod.WHM.SID.DivineBenison) == null)
                UseOGCD(BossMod.WHM.AID.DivineBenison, target);
        });

        if (ShouldHealInAreaNow(strategy, Player.Position, 20, 0.8f))
        {
            UseOGCD(BossMod.WHM.AID.Assize, Player, 16);
            UseOGCD(BossMod.WHM.AID.PlenaryIndulgence, Player, 15);
            if (Player.FindStatus(BossMod.WHM.SID.DivineGrace) != null)
                UseOGCD(BossMod.WHM.AID.DivineCaress, Player, 14);
            if (Player.FindStatus(Unlocked(BossMod.WHM.AID.MedicaIII) ? BossMod.WHM.SID.MedicaIII : BossMod.WHM.SID.MedicaII, World.FutureTime(3)) == null)
                UseGCD(bestM2, Player);
        }
        if (ShouldHealInAreaNow(strategy, Player.Position, 30, 0.55f))
            UseOGCD(BossMod.WHM.AID.Temperance, Player, 13);
        if (ShouldHealInAreaNow(strategy, Player.Position, 20, 0.5f) && Player.FindStatus(BossMod.WHM.SID.LiturgyOfTheBell) == null)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(BossMod.WHM.AID.LiturgyOfTheBell), null, ActionQueue.Priority.Medium + 12, targetPos: Player.PosRot.XYZ());
        if (ShouldHealInAreaNow(strategy, Player.Position, 20, 0.7f) && canLily)
            UseGCD(BossMod.WHM.AID.AfflatusRapture, Player, 3);
        if (ShouldHealInAreaNow(strategy, Player.Position, 10, 0.55f) && Unlocked(BossMod.WHM.AID.CureIII) && LightParty.Count(p => p.Position.InCircle(Player.Position, 10)) >= 4)
        {
            if (Player.FindStatus(BossMod.WHM.SID.ThinAir) == null)
                UseOGCD(BossMod.WHM.AID.ThinAir, Player, 1);
            UseGCD(BossMod.WHM.AID.CureIII, Player, 2);
        }
        else if (ShouldHealInAreaNow(strategy, Player.Position, 15, 0.55f) && !Unlocked(BossMod.WHM.AID.MedicaII))
            UseGCD(BossMod.WHM.AID.Medica, Player);
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

        HealSingleNow(strategy, (target, ratio) =>
        {
            if (ratio < 0.3)
                UseGCD(BossMod.AST.AID.EssentialDignity, target);
        });

        HealSingleSoon(strategy, (target, ratio) =>
        {
            if (ratio < 0.3)
                UseOGCD(BossMod.AST.AID.CelestialIntersection, target);

            if (ratio < 0.5)
            {
                foreach (var (card, action) in SupportCards)
                    if (cards.Contains(card))
                        UseOGCD(action, target);

                if (NextChargeIn(BossMod.AST.AID.CelestialIntersection) > GCD && NextChargeIn(BossMod.AST.AID.EssentialDignity) > GCD)
                    UseGCD(BestActionUnlocked(BossMod.AST.AID.BeneficII, BossMod.AST.AID.Benefic), target);
            }
        });

        if (ShouldHealInAreaNow(strategy, Player.Position, 15, 0.7f))
        {
            if (gauge.CurrentArcana == AstrologianCard.Lady)
                UseOGCD(BossMod.AST.AID.LadyOfCrowns, Player);

            UseOGCD(BossMod.AST.AID.CelestialOpposition, Player);

            if (Player.FindStatus(Unlocked(BossMod.AST.AID.HeliosConjunction) ? BossMod.AST.SID.HeliosConjunction : BossMod.AST.SID.AspectedHelios, World.FutureTime(15)) == null)
                UseGCD(BossMod.AST.AID.AspectedHelios, Player);

            if (!Unlocked(BossMod.AST.AID.HeliosConjunction))
                UseGCD(BossMod.AST.AID.Helios, Player);
        }

        if (strategy.Heal == HealMode.Enabled)
        {
            if (Player.InCombat)
                Hints.ActionsToExecute.Push(ActionID.MakeSpell(BossMod.AST.AID.EarthlyStar), Player, ActionQueue.Priority.Medium, targetPos: Player.PosRot.XYZ());

            foreach (var rw in Raidwides)
                if (World.FutureTime(5) > rw)
                    UseOGCD(BossMod.AST.AID.CollectiveUnconscious, Player);
        }
    }

    private void AutoSCH(in Strategy strategy, Actor? primaryTarget)
    {
        var useOutOfCombat = strategy.OutOfCombat.IsEnabled();

        void UseSoil(Vector3? location = null)
        {
            if (World.Client.GetGauge<ScholarGauge>().Aetherflow == 0)
                return;
            location ??= ArenaCenter ?? Player.PosRot.XYZ();
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(BossMod.SCH.AID.SacredSoil), null, ActionQueue.Priority.Medium + 5, targetPos: location.Value);
        }

        var gauge = World.Client.GetGauge<ScholarGauge>();

        var pet = World.Client.ActivePet.InstanceID == 0xE0000000 ? null : World.Actors.Find(World.Client.ActivePet.InstanceID);
        var haveSeraph = gauge.SeraphTimer > 0;
        var haveEos = !haveSeraph;

        var aetherflow = gauge.Aetherflow > 0;

        if (aetherflow && ShouldHealInAreaNow(strategy, Player.Position, 15, 0.5f))
            UseOGCD(BossMod.SCH.AID.Indomitability, Player);

        if (pet != null)
        {
            if (ShouldHealInAreaSoon(strategy, pet.Position, 30, 0.5f))
            {
                if (haveSeraph)
                    UseOGCD(BossMod.SCH.AID.Consolation, Player);
                else if (NextChargeIn(BossMod.SCH.AID.SummonSeraph) == 0)
                    UseOGCD(BossMod.SCH.AID.SummonSeraph, Player);
            }

            if (ShouldHealInAreaNow(strategy, pet.Position, 20, 0.5f))
                UseOGCD(BossMod.SCH.AID.FeyBlessing, Player);

            if (ShouldHealInAreaSoon(strategy, pet.Position, 15, 0.8f))
                UseOGCD(BossMod.SCH.AID.WhisperingDawn, Player);
        }

        HealSingleNow(strategy, (target, ratio) =>
        {
            if (ratio < 0.5)
            {
                // aetherflow is too valuable
                var canLustrate = false; // gauge.Aetherflow > 0 && Unlocked(BossMod.SCH.AID.Lustrate);
                if (canLustrate)
                    UseOGCD(BossMod.SCH.AID.Lustrate, target);
                else
                    UseGCD(BossMod.SCH.AID.Adloquium, target);
            }
        });

        HealSingleSoon(strategy, (target, ratio) =>
        {
            if (ratio < 0.5)
            {
                if (gauge.Aetherflow > 0)
                    UseOGCD(BossMod.SCH.AID.Excogitation, target);
                if (gauge.FairyGauge > 0 && target.FindStatus(BossMod.SCH.SID.FeyUnion) == null)
                    UseOGCD(BossMod.SCH.AID.Aetherpact, target);
            }
        });

        if (strategy.Heal == HealMode.Enabled)
        {
            RunForTank((tank, tankState) =>
            {
                if (!Player.InCombat && (World.CurrentTime - tankState.LastCombat).TotalSeconds > 1 && useOutOfCombat)
                {
                    if (NextChargeIn(BossMod.SCH.AID.Excogitation) == 0)
                        UseOGCD(BossMod.SCH.AID.Recitation, Player, 5);
                    UseOGCD(BossMod.SCH.AID.Excogitation, tank);
                }

                if (tank.InCombat && Bossmods.ActiveModule is null && tankState.MoveDelta < 0.75f)
                    UseSoil(tank.PosRot.XYZ());
            });

            foreach (var rw in Raidwides)
                if (World.FutureTime(5) > rw && NextChargeIn(BossMod.SCH.AID.SacredSoil) == 0)
                    UseSoil(GetBestPartyCoverage(15));
        }
    }

    // O(n³) :3
    private Vector3 GetBestPartyCoverage(float radius)
    {
        var allies = LightParty.Select(p => p.Position).ToList();
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

    private static readonly uint[] EukrasianShields = [(uint)BossMod.SGE.SID.EukrasianDiagnosis, (uint)BossMod.SGE.SID.EukrasianPrognosis, (uint)BossMod.SCH.SID.Galvanize];
    private static bool HasEukrasianShield(Actor a) => a.Statuses.Any(s => EukrasianShields.Contains(s.ID));

    private bool ReadySoon<AID>(AID aid) where AID : Enum => Unlocked(aid) && NextChargeIn(aid) < 0.6f;

    // pre-damage tools from raidwide / tankbuster predictions, then HP-threshold heals
    private void AutoSGE(in Strategy strategy, Actor? primaryTarget)
    {
        var gauge = World.Client.GetGauge<SageGauge>();
        var gall = gauge.Addersgall;
        var eukrasia = gauge.EukrasiaActive;

        // Eukrasia first, then the heal (instant while Eukrasia is up)
        void UseEukrasian(BossMod.SGE.AID heal, Actor target, int prio)
        {
            if (eukrasia)
                UseGCD(heal, target, prio, instant: true);
            else
                UseGCD(BossMod.SGE.AID.Eukrasia, Player, prio);
        }

        var party = LightParty.Where(p => !p.IsDead).ToList();
        float UnshieldedShare(float radius)
        {
            var inRange = party.Where(p => p.Position.InCircle(Player.Position, radius)).ToList();
            return inRange.Count == 0 ? 0 : (float)inRange.Count(p => !HasEukrasianShield(p)) / inRange.Count;
        }

        if (strategy.Heal == HealMode.Enabled)
        {
            var raidwideIn = Raidwides.Select(r => (float)(r - World.CurrentTime).TotalSeconds).Where(t => t >= 0).DefaultIfEmpty(float.MaxValue).Min();

            if (raidwideIn < 8 && gall > 0)
                UseOGCD(BossMod.SGE.AID.Kerachole, Player, 20);
            if (raidwideIn < 5 && Health.PartyHealth.AvgCurrent <= 0.8f)
                UseOGCD(BossMod.SGE.AID.Holos, Player, 15);
            if (raidwideIn < GCD + 2.5f && UnshieldedShare(15) >= 0.5f)
                UseEukrasian(BossMod.SGE.AID.Prognosis, Player, 5);

            foreach (var (tank, at) in Tankbusters)
            {
                var busterIn = (float)(at - World.CurrentTime).TotalSeconds;
                if (busterIn is < 0 or > 5 || tank.IsDead || World.Party.FindSlot(tank.InstanceID) < 0)
                    continue;
                if (tank.FindStatus(BossMod.SGE.SID.Haima) == null)
                    UseOGCD(BossMod.SGE.AID.Haima, tank, 25);
                if (busterIn < 3 && gall > 0)
                    UseOGCD(BossMod.SGE.AID.Taurochole, tank, 24);
                if (busterIn < GCD + 2.5f && !HasEukrasianShield(tank))
                    UseEukrasian(BossMod.SGE.AID.Diagnosis, tank, 4);
            }
        }

        HealSingleNow(strategy, (target, ratio) =>
        {
            var tank = target.Role == Role.Tank;
            if (tank && ratio <= 0.7f)
                UseOGCD(BossMod.SGE.AID.Krasis, target, 12);
            if (tank && ratio <= 0.5f && gall > 0)
                UseOGCD(BossMod.SGE.AID.Taurochole, target, 11);
            if (tank && ratio <= 0.45f)
                UseOGCD(BossMod.SGE.AID.Haima, target, 10);
            if (ratio <= 0.7f && target.FindStatus(BossMod.SGE.SID.Kardion, Player.InstanceID) != null)
                UseOGCD(BossMod.SGE.AID.Soteria, Player, 9);
            if (ratio <= 0.55f && gall > 0)
                UseOGCD(BossMod.SGE.AID.Druochole, target, 8);
            if (ratio <= 0.4f)
                UseOGCD(BossMod.SGE.AID.Zoe, Player, 7);
            if (ratio <= 0.55f && target.FindStatus(BossMod.SGE.SID.EukrasianDiagnosis, Player.InstanceID) != null)
                UseOGCD(BossMod.SGE.AID.Pepsis, Player, 6);

            // GCD heals only when the oGCDs can't cover it
            var ogcdCovers = gall > 0 || tank && (ReadySoon(BossMod.SGE.AID.Taurochole) || ReadySoon(BossMod.SGE.AID.Haima));
            if (ratio <= 0.3f || ratio <= 0.5f && !ogcdCovers)
            {
                if (!HasEukrasianShield(target))
                    UseEukrasian(BossMod.SGE.AID.Diagnosis, target, 3);
                else
                    UseGCD(BossMod.SGE.AID.Diagnosis, target, 2);
            }
        });

        HealSingleSoon(strategy, (target, ratio) =>
        {
            if (ratio < 0.5f && target.Role == Role.Tank)
                UseOGCD(BossMod.SGE.AID.Haima, target, 10);
        });

        if (ShouldHealInAreaNow(strategy, Player.Position, 30, 0.8f))
        {
            UseOGCD(Unlocked(BossMod.SGE.AID.PhysisII) ? BossMod.SGE.AID.PhysisII : BossMod.SGE.AID.Physis, Player, 19);
            if (gall > 0 && Player.Level >= 78)
                UseOGCD(BossMod.SGE.AID.Kerachole, Player, 18);
        }
        if (ShouldHealInAreaNow(strategy, Player.Position, 30, 0.65f))
            UseOGCD(BossMod.SGE.AID.Holos, Player, 17);
        if (ShouldHealInAreaNow(strategy, Player.Position, 20, 0.6f))
            UseOGCD(BossMod.SGE.AID.Philosophia, Player, 16);
        if (ShouldHealInAreaNow(strategy, Player.Position, 30, 0.55f) && Player.FindStatus(BossMod.SGE.SID.Panhaima) == null)
            UseOGCD(BossMod.SGE.AID.Panhaima, Player, 15);
        if (gall > 0 && ShouldHealInAreaNow(strategy, Player.Position, 15, 0.7f))
            UseOGCD(BossMod.SGE.AID.Ixochole, Player, 14);
        if (ShouldHealInAreaNow(strategy, Player.Position, 15, 0.6f) && Player.FindStatus(BossMod.SGE.SID.EukrasianPrognosis, Player.InstanceID) != null)
            UseOGCD(BossMod.SGE.AID.Pepsis, Player, 13);

        // big party heal: Zoe into Pneuma
        if (ShouldHealInAreaNow(strategy, Player.Position, 20, 0.45f) && ReadySoon(BossMod.SGE.AID.Pneuma) && primaryTarget is { IsAlly: false } enemy && Player.DistanceToHitbox(enemy) <= 25)
        {
            UseOGCD(BossMod.SGE.AID.Zoe, Player, 13);
            UseGCD(BossMod.SGE.AID.Pneuma, enemy, 1);
        }

        // E.Prognosis when the oGCDs are spent and shields are missing
        if (ShouldHealInAreaNow(strategy, Player.Position, 15, 0.55f) && UnshieldedShare(15) >= 0.5f
            && !(gall > 0 && ReadySoon(BossMod.SGE.AID.Ixochole)) && !ReadySoon(BossMod.SGE.AID.PhysisII) && !ReadySoon(BossMod.SGE.AID.Holos))
            UseEukrasian(BossMod.SGE.AID.Prognosis, Player, 1);
    }
}
