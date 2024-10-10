using FFXIVClientStructs.FFXIV.Client.Game.Gauge;

namespace BossMod.Autorotation.xan;

public class HealerAI(RotationModuleManager manager, Actor player) : AIBase(manager, player)
{
    public record struct PartyMemberState
    {
        public int Slot;
        public int PredictedHP;
        public int PredictedHPMissing;
        public float AttackerStrength;
        // predicted ratio including pending HP loss and current attacker strength
        public float PredictedHPRatio;
        // *actual* ratio including pending HP loss, used mainly just for essential dignity
        public float PendingHPRatio;
        // remaining time on cleansable status, to avoid casting it on a target that will lose the status by the time we finish
        public float EsunableStatusRemaining;
        // tank invulns go here, but also statuses like Excog that give burst heal below a certain HP threshold
        // no point in spam healing a tank in an area with high mob density (like Sirensong Sea pull after second boss) until their excog falls off
        public float NoHealStatusRemaining;
        // Doom (1769 and possibly other statuses) is only removed once a player reaches full HP, must be healed asap
        public float DoomRemaining;
    }

    public record PartyHealthState
    {
        public int LowestHPSlot;
        public int Count;
        public float Avg;
        public float StdDev;
    }

    public const float AOEBreakpointHPVariance = 0.25f;

    private readonly PartyMemberState[] PartyMemberStates = new PartyMemberState[PartyState.MaxAllies];
    private PartyHealthState PartyHealth = new();

    public enum Track { Raise, RaiseTarget, Heal, Esuna }
    public enum RaiseStrategy
    {
        None,
        Swiftcast,
        Slowcast,
        Hardcast,
    }
    public enum RaiseTarget
    {
        Party,
        Alliance,
        Everyone
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
        var def = new RotationModuleDefinition("Healer AI", "Auto-healer", "xan", RotationModuleQuality.WIP, BitMask.Build(Class.CNJ, Class.WHM, Class.ACN, Class.SCH, Class.SGE, Class.AST), 100);

        def.Define(Track.Raise).As<RaiseStrategy>("Raise")
            .AddOption(RaiseStrategy.None, "Don't automatically raise")
            .AddOption(RaiseStrategy.Swiftcast, "Raise using Swiftcast only")
            .AddOption(RaiseStrategy.Slowcast, "Raise without requiring Swiftcast to be available")
            .AddOption(RaiseStrategy.Hardcast, "Never use Swiftcast to raise");

        def.Define(Track.RaiseTarget).As<RaiseTarget>("RaiseTargets")
            .AddOption(RaiseTarget.Party, "Party members")
            .AddOption(RaiseTarget.Alliance, "Alliance raid members")
            .AddOption(RaiseTarget.Everyone, "Any dead player");

        def.AbilityTrack(Track.Heal, "Heal");
        def.AbilityTrack(Track.Esuna, "Esuna");

        return def;
    }

    private (Actor Target, PartyMemberState State)? BestSTHealTarget => PartyHealth.StdDev > AOEBreakpointHPVariance ? (World.Party[PartyHealth.LowestHPSlot]!, PartyMemberStates[PartyHealth.LowestHPSlot]) : null;

    private PartyHealthState CalcPartyHealthInArea(WPos center, float radius) => CalculatePartyHealthState(act => act.Position.InCircle(center, radius));
    private bool ShouldHealInArea(WPos center, float radius, float hpThreshold)
    {
        var st = CalcPartyHealthInArea(center, radius);
        // Service.Log($"party health in radius {radius}: {st}");
        return st.Count > 1 && st.StdDev <= AOEBreakpointHPVariance && st.Avg <= hpThreshold;
    }

    private void HealSingle(Action<Actor, PartyMemberState> healFun)
    {
        if (BestSTHealTarget is (var a, var b))
            healFun(a, b);
    }

    private static readonly Dictionary<uint, bool> _esunaCache = [];
    private static bool StatusIsRemovable(uint statusID)
    {
        if (_esunaCache.TryGetValue(statusID, out var value))
            return value;
        var check = Utils.StatusIsRemovable(statusID);
        _esunaCache[statusID] = check;
        return check;
    }

    private static readonly uint[] NoHealStatuses = [
        82, // Hallowed Ground
        409, // Holmgang
        810, // Living Dead
        811, // Walking Dead
        1220, // Excogitation
        1836, // Superbolide
        2685, // Catharsis of Corundum
        (uint)WAR.SID.BloodwhettingDefenseLong
    ];

    public override void Execute(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        if (Player.MountId > 0)
            return;

        // copied from veyn's HealerActions in EW bossmod - i am a thief
        BitMask esunas = new();
        foreach (var caster in World.Party.WithoutSlot(excludeAlliance: true).Where(a => a.CastInfo?.IsSpell(BossMod.WHM.AID.Esuna) ?? false))
            esunas.Set(World.Party.FindSlot(caster.CastInfo!.TargetID));

        for (var i = 0; i < PartyState.MaxAllies; i++)
        {
            var actor = World.Party[i];
            ref var state = ref PartyMemberStates[i];
            state.Slot = i;
            if (actor == null || actor.IsDead || actor.HPMP.MaxHP == 0)
            {
                state.PredictedHP = state.PredictedHPMissing = 0;
                state.PredictedHPRatio = state.PendingHPRatio = 1;
            }
            else
            {
                state.PredictedHP = (int)actor.HPMP.CurHP + World.PendingEffects.PendingHPDifference(actor.InstanceID);
                state.PredictedHPMissing = (int)actor.HPMP.MaxHP - state.PredictedHP;
                state.PredictedHPRatio = state.PendingHPRatio = (float)state.PredictedHP / actor.HPMP.MaxHP;
                state.AttackerStrength = 0;
                state.EsunableStatusRemaining = 0;
                state.DoomRemaining = 0;
                state.NoHealStatusRemaining = 0;
                var canEsuna = actor.IsTargetable && !esunas[i];
                foreach (var s in actor.Statuses)
                {
                    if (canEsuna && StatusIsRemovable(s.ID))
                        state.EsunableStatusRemaining = Math.Max(StatusDuration(s.ExpireAt), state.EsunableStatusRemaining);

                    if (NoHealStatuses.Contains(s.ID))
                        state.NoHealStatusRemaining = StatusDuration(s.ExpireAt);

                    if (s.ID == 1769)
                        state.DoomRemaining = StatusDuration(s.ExpireAt);
                }
            }
        }

        foreach (var enemy in Hints.PotentialTargets)
        {
            var targetSlot = World.Party.FindSlot(enemy.Actor.TargetID);
            if (targetSlot >= 0)
            {
                ref var state = ref PartyMemberStates[targetSlot];
                state.AttackerStrength += enemy.AttackStrength;
                if (state.PredictedHPRatio < 0.99f)
                    state.PredictedHPRatio -= enemy.AttackStrength;
            }
        }
        PartyHealth = CalculatePartyHealthState(_ => true);

        AutoRaise(strategy);

        if (strategy.Enabled(Track.Esuna))
        {
            foreach (var st in PartyMemberStates)
            {
                if (st.EsunableStatusRemaining > GCD + 2f)
                {
                    UseGCD(BossMod.WHM.AID.Esuna, World.Party[st.Slot]);
                    break;
                }
            }
        }

        if (strategy.Enabled(Track.Heal))
            switch (Player.Class)
            {
                case Class.WHM:
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

    private PartyHealthState CalculatePartyHealthState(Func<Actor, bool> filter)
    {
        int count = 0;
        float mean = 0;
        float m2 = 0;
        float min = float.MaxValue;
        int minSlot = -1;

        foreach (var p in PartyMemberStates)
        {
            var act = World.Party[p.Slot];
            if (act == null || !filter(act))
                continue;

            if (p.NoHealStatusRemaining > 1.5f && p.DoomRemaining == 0)
                continue;

            var pred = p.DoomRemaining > 0 ? 0 : p.PredictedHPRatio;
            if (pred < min)
            {
                min = pred;
                minSlot = p.Slot;
            }
            count++;
            var delta = pred - mean;
            mean += delta / count;
            var delta2 = pred - mean;
            m2 += delta * delta2;
        }

        var variance = m2 / count;
        return new PartyHealthState()
        {
            LowestHPSlot = minSlot,
            Avg = mean,
            StdDev = MathF.Sqrt(variance),
            Count = count
        };
    }

    private void UseGCD<AID>(AID action, Actor? target, int extraPriority = 0) where AID : Enum
        => UseGCD(ActionID.MakeSpell(action), target, extraPriority);
    private void UseGCD(ActionID action, Actor? target, int extraPriority = 0)
        => Hints.ActionsToExecute.Push(action, target, ActionQueue.Priority.High + 500 + extraPriority);

    private void UseOGCD<AID>(AID action, Actor? target, int extraPriority = 0) where AID : Enum
        => UseOGCD(ActionID.MakeSpell(action), target, extraPriority);
    private void UseOGCD(ActionID action, Actor? target, int extraPriority = 0)
        => Hints.ActionsToExecute.Push(action, target, ActionQueue.Priority.Medium + extraPriority);

    private void AutoRaise(StrategyValues strategy)
    {
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

    private Actor? GetRaiseTarget(StrategyValues strategy)
    {
        var candidates = strategy.Option(Track.RaiseTarget).As<RaiseTarget>() switch
        {
            RaiseTarget.Everyone => World.Actors.Where(x => x.Type is ActorType.Player or ActorType.DutySupport && x.IsAlly),
            RaiseTarget.Alliance => World.Party.WithoutSlot(true, false),
            _ => World.Party.WithoutSlot(true, true)
        };

        return candidates.Where(x => x.IsDead && Player.DistanceToHitbox(x) <= 30 && !BeingRaised(x)).MaxBy(actor => actor.Class.GetRole() switch
        {
            Role.Healer => 5,
            Role.Tank => 4,
            _ => actor.Class is Class.RDM or Class.SMN or Class.ACN ? 3 : 2
        });
    }

    private static bool BeingRaised(Actor actor) => actor.Statuses.Any(s => s.ID is 148 or 1140 or 2648);

    private void AutoWHM(StrategyValues strategy)
    {
        var gauge = GetGauge<WhiteMageGauge>();

        HealSingle((target, state) =>
        {
            if (state.PredictedHPRatio < 0.5 && gauge.Lily > 0)
                UseGCD(BossMod.WHM.AID.AfflatusSolace, target);

            if (state.PredictedHPRatio < 0.25)
                UseOGCD(BossMod.WHM.AID.Tetragrammaton, target);
        });

        if (ShouldHealInArea(Player.Position, 15, 0.75f) && gauge.Lily > 0)
            UseGCD(BossMod.WHM.AID.AfflatusRapture, Player);

        if (ShouldHealInArea(Player.Position, 10, 0.5f))
            UseGCD(BossMod.WHM.AID.Cure3, Player);
    }

    private static readonly (AstrologianCard, BossMod.AST.AID)[] SupportCards = [
        (AstrologianCard.Arrow, BossMod.AST.AID.TheArrow),
        (AstrologianCard.Spire, BossMod.AST.AID.TheSpire),
        (AstrologianCard.Bole, BossMod.AST.AID.TheBole),
        (AstrologianCard.Ewer, BossMod.AST.AID.TheEwer)
    ];

    private void AutoAST(StrategyValues strategy)
    {
        var gauge = GetGauge<AstrologianGauge>();

        HealSingle((target, state) =>
        {
            if (state.PendingHPRatio < 0.3)
                UseOGCD(BossMod.AST.AID.EssentialDignity, target);

            if (state.PredictedHPRatio < 0.3)
            {
                UseOGCD(BossMod.AST.AID.CelestialIntersection, target);

                if (gauge.CurrentArcana == AstrologianCard.Lady)
                    UseOGCD(BossMod.AST.AID.LadyOfCrowns, Player);

                foreach (var (card, action) in SupportCards)
                    if (gauge.CurrentCards.Contains(card))
                        UseOGCD(action, target);
            }

            if (state.PredictedHPRatio < 0.5 && !Unlocked(BossMod.AST.AID.CelestialIntersection) && NextChargeIn(BossMod.AST.AID.EssentialDignity) > 2.5f)
                UseGCD(BossMod.AST.AID.Benefic, target);
        });

        if (ShouldHealInArea(Player.Position, 15, 0.7f))
            UseGCD(BossMod.AST.AID.AspectedHelios, Player);

        if (Player.InCombat)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(BossMod.AST.AID.EarthlyStar), Player, ActionQueue.Priority.Medium, targetPos: Player.PosRot.XYZ());
    }

    private void AutoSCH(StrategyValues strategy, Actor? primaryTarget)
    {
        var gauge = GetGauge<ScholarGauge>();

        var pet = World.Client.ActivePet.InstanceID == 0xE0000000 ? null : World.Actors.Find(World.Client.ActivePet.InstanceID);
        var haveSeraph = gauge.SeraphTimer > 0;
        var haveEos = !haveSeraph;

        var aetherflow = gauge.Aetherflow > 0;

        if (aetherflow && ShouldHealInArea(Player.Position, 15, 0.5f))
            UseOGCD(BossMod.SCH.AID.Indomitability, Player);

        if (pet != null)
        {
            if (haveEos && ShouldHealInArea(pet.Position, 20, 0.5f))
                UseOGCD(BossMod.SCH.AID.FeyBlessing, Player);

            if (ShouldHealInArea(pet.Position, 15, 0.8f))
                UseOGCD(BossMod.SCH.AID.WhisperingDawn, Player);
        }

        HealSingle((target, state) =>
        {
            if (state.PredictedHPRatio < 0.3)
            {
                var canLustrate = gauge.Aetherflow > 0 && Unlocked(BossMod.SCH.AID.Lustrate);
                if (canLustrate)
                {
                    UseOGCD(BossMod.SCH.AID.Excogitation, target);
                    UseOGCD(BossMod.SCH.AID.Lustrate, target);
                }
                else
                    UseGCD(BossMod.SCH.AID.Physick, target);
            }
        });
    }

    private void AutoSGE(StrategyValues strategy, Actor? primaryTarget)
    {
        var gauge = GetGauge<SageGauge>();

        var haveBalls = gauge.Addersgall > 0;

        if (haveBalls && ShouldHealInArea(Player.Position, 15, 0.5f))
            UseOGCD(BossMod.SGE.AID.Ixochole, Player);

        if (ShouldHealInArea(Player.Position, 30, 0.8f))
        {
            UseOGCD(Unlocked(BossMod.SGE.AID.PhysisII) ? BossMod.SGE.AID.PhysisII : BossMod.SGE.AID.Physis, Player);
        }

        HealSingle((target, state) =>
        {
            if (haveBalls && state.PredictedHPRatio < 0.5)
            {
                UseOGCD(BossMod.SGE.AID.Taurochole, target);
                UseOGCD(BossMod.SGE.AID.Druochole, target);
            }

            if (state.PredictedHPRatio < 0.3)
            {
                UseOGCD(BossMod.SGE.AID.Haima, target);
            }
        });

        foreach (var rw in Raidwides)
        {
            if ((rw - World.CurrentTime).TotalSeconds < 15 && haveBalls)
                UseOGCD(BossMod.SGE.AID.Kerachole, Player);
        }
    }
}
