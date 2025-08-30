using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using static BossMod.Autorotation.xan.TrackPartyHealth;

namespace BossMod.Autorotation.xan;

public class HealerAI(RotationModuleManager manager, Actor player) : AIBase(manager, player)
{
    private readonly TrackPartyHealth Health = new(manager.WorldState);

    public enum Track { Raise, RaiseTarget, Heal, Esuna, StayNearParty, OutOfCombat }
    public enum RaiseStrategy
    {
        None,
        Swiftcast,
        Slowcast,
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
        var def = new RotationModuleDefinition("Healer AI", "Auto-healer", "AI (xan)", "xan", RotationModuleQuality.WIP, BitMask.Build(Class.CNJ, Class.WHM, Class.SCH, Class.SGE, Class.AST), 100);

        def.Define(Track.Raise).As<RaiseStrategy>("Raise")
            .AddOption(RaiseStrategy.None, "Don't automatically raise")
            .AddOption(RaiseStrategy.Swiftcast, "Raise using Swiftcast only")
            .AddOption(RaiseStrategy.Slowcast, "Raise without requiring Swiftcast to be available")
            .AddOption(RaiseStrategy.Hardcast, "Never use Swiftcast to raise");

        def.Define(Track.RaiseTarget).As<RaiseUtil.Targets>("RaiseTargets", "Raise targets")
            .AddOption(RaiseUtil.Targets.Party, "Party members")
            .AddOption(RaiseUtil.Targets.Alliance, "Alliance raid members")
            .AddOption(RaiseUtil.Targets.Everyone, "Any dead player");

        def.AbilityTrack(Track.Heal, "Heal");

        def.Define(Track.Esuna).As<HintedStrategy>("Esuna2", "Esuna")
            .AddOption(HintedStrategy.Disabled, "Disabled", "Don't use")
            .AddOption(HintedStrategy.HintOnly, "HintOnly", "Cleanse targets suggested by active module")
            .AddOption(HintedStrategy.Enabled, "Enabled", "Cleanse all eligible party members")
            .AddAssociatedActions(ClassShared.AID.Esuna);

        def.AbilityTrack(Track.StayNearParty, "Stay near party");
        def.AbilityTrack(Track.OutOfCombat, "OutOfCombat", "Allow generic out-of-combat predictive heals on tank (Excogitation, Divine Benison, etc)");

        return def;
    }

    private void HealSingleSoon(Action<Actor, float> healFun)
    {
        if (Health.BestSTHealTargetPredicted is (var a, var b))
            healFun(a, b.PredictedHPRatio);
    }

    private void HealSingleNow(Action<Actor, float> healFun)
    {
        if (Health.BestSTHealTarget is (var a, var b))
            healFun(a, b.CurrentHPRatio);
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

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        Health.Update(Hints);

        if (strategy.Enabled(Track.StayNearParty) && Player.InCombat)
        {
            List<(WPos pos, float radius)> allies = [.. LightParty.Exclude(Player).Select(e => (e.Position, e.HitboxRadius))];
            Hints.GoalZones.Add(p => allies.Count(a => a.pos.InCircle(p, a.radius + Player.HitboxRadius + 15)));
        }

        AutoRaise(strategy);

        var esuna = strategy.Option(Track.Esuna).As<HintedStrategy>();

        if (esuna.IsEnabled())
            foreach (var st in Health.PartyMemberStates)
                if (st.EsunableStatusRemaining > GCD + 1.14f && esuna.Check(Hints.ShouldCleanse[st.Slot]))
                    UseGCD(BossMod.WHM.AID.Esuna, World.Party[st.Slot]);

        if (strategy.Enabled(Track.Heal))
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

    private void UseGCD<AID>(AID action, Actor? target, int extraPriority = 0) where AID : Enum
        => UseGCD(ActionID.MakeSpell(action), target, extraPriority);
    private void UseGCD(ActionID action, Actor? target, int extraPriority = 0)
    {
        var def = ActionDefinitions.Instance[action];
        if (def == null)
            return;

        var castTime = Math.Max(0, def.CastTime - 0.5f);
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

    private void AutoRaise(StrategyValues strategy)
    {
        // set of all statuses called "Resurrection Restricted"
        // TODO maybe this is a flag in sheets somewhere
        if (Player.Statuses.Any(s => s.ID is 1755 or 2449 or 3380 or 4262))
            return;

        var swiftcast = StatusDetails(Player, (uint)BossMod.WHM.SID.Swiftcast, Player.InstanceID, 15).Left;
        var thinair = StatusDetails(Player, (uint)BossMod.WHM.SID.ThinAir, Player.InstanceID, 12).Left;
        var swiftcastCD = NextChargeIn(BossMod.WHM.AID.Swiftcast);
        var raise = strategy.Option(Track.Raise).As<RaiseStrategy>();

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

    private Actor? GetRaiseTarget(StrategyValues strategy) => RaiseUtil.FindRaiseTargets(World, strategy.Option(Track.RaiseTarget).As<RaiseUtil.Targets>()).FirstOrDefault();

    private bool ShouldHealInAreaSoon(WPos pos, float radius, float ratio) => Health.PredictShouldHealInArea(pos, radius, ratio);
    private bool ShouldHealInAreaNow(WPos pos, float radius, float ratio) => Health.ShouldHealInArea(pos, radius, ratio);

    private void AutoWHM(StrategyValues strategy)
    {
        var gauge = World.Client.GetGauge<WhiteMageGauge>();

        var bestC2 = BestActionUnlocked(BossMod.WHM.AID.CureII, BossMod.WHM.AID.Cure);
        var bestM2 = BestActionUnlocked(BossMod.WHM.AID.MedicaIII, BossMod.WHM.AID.MedicaII);

        HealSingleNow((target, ratio) =>
        {
            // TODO add a track for this kind of stuff
            //if (state.PredictedHPRatio < 1 && target.FindStatus(BossMod.WHM.SID.Regen) == null)
            //    UseGCD(BossMod.WHM.AID.Regen, target);

            if (ratio < 0.5)
            {
                if (gauge.Lily > 0)
                    UseGCD(BossMod.WHM.AID.AfflatusSolace, target);
                else
                    UseGCD(bestC2, target);

                UseOGCD(BossMod.WHM.AID.Tetragrammaton, target);
            }
        });

        HealSingleSoon((target, ratio) =>
        {
            if (ratio < 0.75f && target.FindStatus(BossMod.WHM.SID.DivineBenison) == null)
                UseOGCD(BossMod.WHM.AID.DivineBenison, target);
        });

        if (ShouldHealInAreaNow(Player.Position, 15, 0.75f))
        {
            // do actual heals
            if (gauge.Lily > 0)
                UseGCD(BossMod.WHM.AID.AfflatusRapture, Player);
            else if (Unlocked(BossMod.WHM.AID.CureIII))
            {
                if (Player.FindStatus(BossMod.WHM.SID.ThinAir) == null)
                    UseGCD(BossMod.WHM.AID.ThinAir, Player, 1);
                UseGCD(BossMod.WHM.AID.CureIII, Player);
            }
            else
                UseGCD(BossMod.WHM.AID.Medica, Player);

            // apply regens
            if (Player.FindStatus(Unlocked(BossMod.WHM.AID.MedicaIII) ? BossMod.WHM.SID.MedicaIII : BossMod.WHM.SID.MedicaII, World.FutureTime(15)) == null)
                UseGCD(bestM2, Player);
        }
    }

    private static readonly (AstrologianCard, BossMod.AST.AID)[] SupportCards = [
        (AstrologianCard.Arrow, BossMod.AST.AID.TheArrow),
        (AstrologianCard.Spire, BossMod.AST.AID.TheSpire),
        (AstrologianCard.Bole, BossMod.AST.AID.TheBole),
        (AstrologianCard.Ewer, BossMod.AST.AID.TheEwer)
    ];

    private void AutoAST(StrategyValues strategy)
    {
        var gauge = World.Client.GetGauge<AstrologianGauge>();

        HealSingleNow((target, ratio) =>
        {
            if (ratio < 0.3)
                UseGCD(BossMod.AST.AID.EssentialDignity, target);
        });

        HealSingleSoon((target, ratio) =>
        {
            if (ratio < 0.3)
                UseOGCD(BossMod.AST.AID.CelestialIntersection, target);

            if (ratio < 0.5)
            {
                foreach (var (card, action) in SupportCards)
                    if (gauge.CurrentCards.Contains(card))
                        UseOGCD(action, target);

                if (!Unlocked(BossMod.AST.AID.CelestialIntersection) && NextChargeIn(BossMod.AST.AID.EssentialDignity) > 2.5f)
                    UseGCD(BossMod.AST.AID.Benefic, target);
            }
        });

        if (ShouldHealInAreaNow(Player.Position, 15, 0.7f))
        {
            if (gauge.CurrentArcana == AstrologianCard.Lady)
                UseOGCD(BossMod.AST.AID.LadyOfCrowns, Player);

            if (Player.FindStatus(Unlocked(BossMod.AST.AID.HeliosConjunction) ? BossMod.AST.SID.HeliosConjunction : BossMod.AST.SID.AspectedHelios, World.FutureTime(15)) == null)
                UseGCD(BossMod.AST.AID.AspectedHelios, Player);

            if (!Unlocked(BossMod.AST.AID.HeliosConjunction))
                UseGCD(BossMod.AST.AID.Helios, Player);
        }

        if (Player.InCombat)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(BossMod.AST.AID.EarthlyStar), Player, ActionQueue.Priority.Medium, targetPos: Player.PosRot.XYZ());
    }

    private void AutoSCH(StrategyValues strategy, Actor? primaryTarget)
    {
        var useOutOfCombat = strategy.Enabled(Track.OutOfCombat);

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

        if (aetherflow && ShouldHealInAreaNow(Player.Position, 15, 0.5f))
            UseOGCD(BossMod.SCH.AID.Indomitability, Player);

        if (pet != null)
        {
            if (ShouldHealInAreaSoon(pet.Position, 30, 0.5f))
            {
                if (haveSeraph)
                    UseOGCD(BossMod.SCH.AID.Consolation, Player);
                else if (NextChargeIn(BossMod.SCH.AID.SummonSeraph) == 0)
                    UseOGCD(BossMod.SCH.AID.SummonSeraph, Player);
            }

            if (ShouldHealInAreaNow(pet.Position, 20, 0.5f))
                UseOGCD(BossMod.SCH.AID.FeyBlessing, Player);

            if (ShouldHealInAreaSoon(pet.Position, 15, 0.8f))
                UseOGCD(BossMod.SCH.AID.WhisperingDawn, Player);
        }

        HealSingleNow((target, ratio) =>
        {
            if (ratio < 0.5)
            {
                var canLustrate = gauge.Aetherflow > 0 && Unlocked(BossMod.SCH.AID.Lustrate);
                if (canLustrate)
                    UseOGCD(BossMod.SCH.AID.Lustrate, target);
                else
                    UseGCD(BossMod.SCH.AID.Adloquium, target);
            }
        });

        HealSingleSoon((target, ratio) =>
        {
            if (ratio < 0.5)
            {
                if (gauge.Aetherflow > 0)
                    UseOGCD(BossMod.SCH.AID.Excogitation, target);
                if (gauge.FairyGauge > 0 && target.FindStatus(BossMod.SCH.SID.FeyUnion) == null)
                    UseOGCD(BossMod.SCH.AID.Aetherpact, target);
            }
        });

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
            if ((rw - World.CurrentTime).TotalSeconds < 5 && NextChargeIn(BossMod.SCH.AID.SacredSoil) == 0)
                UseSoil(GetBestPartyCoverage(15));
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

    private void AutoSGE(StrategyValues strategy, Actor? primaryTarget)
    {
        var gauge = World.Client.GetGauge<SageGauge>();

        var haveBalls = gauge.Addersgall > 0;

        if (haveBalls && ShouldHealInAreaNow(Player.Position, 15, 0.5f))
            UseOGCD(BossMod.SGE.AID.Ixochole, Player);

        if (ShouldHealInAreaSoon(Player.Position, 30, 0.8f))
        {
            UseOGCD(Unlocked(BossMod.SGE.AID.PhysisII) ? BossMod.SGE.AID.PhysisII : BossMod.SGE.AID.Physis, Player);
        }

        HealSingleNow((target, ratio) =>
        {
            if (ratio < 0.5)
            {
                UseOGCD(BossMod.SGE.AID.Taurochole, target);
                UseOGCD(BossMod.SGE.AID.Druochole, target);
            }
        });

        HealSingleSoon((target, ratio) =>
        {
            if (ratio < 0.5)
                UseOGCD(BossMod.SGE.AID.Haima, target);
        });

        foreach (var rw in Raidwides)
            if ((rw - World.CurrentTime).TotalSeconds < 15 && haveBalls)
                UseOGCD(BossMod.SGE.AID.Kerachole, Player);
    }
}
