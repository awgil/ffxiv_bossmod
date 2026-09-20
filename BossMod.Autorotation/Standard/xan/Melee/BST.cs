using BossMod.BST;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using static BossMod.AIHints;

namespace BossMod.Autorotation.xan;

public sealed class BST(RotationModuleManager manager, Actor player) : Attackxan<AID, TraitID, BST.Strategy>(manager, player, PotionType.Strength)
{
    public struct Strategy : IStrategyCommon
    {
        public Track<Targeting> Targeting;
        public Track<AOEStrategy> AOE;

        public Track<EnabledByDefault> ShieldCharge;

        public Track<EnabledByDefault> TemperedRelease;

        [Track("Refresh One With Nature out of combat")]
        public Track<EnabledByDefault> Resummon;

        readonly Targeting IStrategyCommon.Targeting => Targeting.Value;
        readonly AOEStrategy IStrategyCommon.AOE => AOE.Value;
    }

    public static RotationModuleDefinition Definition()
    {
        return new RotationModuleDefinition("xan BST", "Beastmaster", "Standard rotation (xan)|Melee", "xan", RotationModuleQuality.WIP, BitMask.Build(Class.BST), 100).WithStrategies<Strategy>();
    }

    protected override float GetCastTime(AID aid) => aid switch
    {
        // cast time is fixed
        AID.FirstBattlehorn or AID.SecondBattlehorn or AID.ThirdBattlehorn => 1,
        _ => 0
    };

    public bool HavePet => CurrentPet > 0;
    public byte CurrentPet;
    public BeastmasterAffinity TrickAffinity;
    public Kinship Kinship;
    public float KinshipLeft;
    public int LastUsedHorn;
    public bool OneWithNature;

    public byte TP;
    public byte PetTP;
    public BeastmasterAffinity ComboAffinity;

    private Enemy? BestJumpTarget;
    private int NumJumpTargets;
    private Enemy? BestLineTarget;
    private int NumLineTargets;

    // TODO: 4-chain opener
    // TODO: parting blow
    // TODO: pet AOE target selection (probably only for targeted circles)
    public override void Exec(in Strategy strategy, Enemy? primaryTarget)
    {
        SelectPrimaryTarget(strategy, ref primaryTarget, 3);

        var gauge = World.Client.GetGauge<BeastmasterGauge>();

        CurrentPet = 0;
        TP = gauge.TPGauge;
        PetTP = gauge.FamiliarTPGauge;
        ComboAffinity = gauge.CurrentAffinity;
        if (gauge.ActiveBattlehornIndex > 0)
        {
            LastUsedHorn = gauge.ActiveBattlehornIndex;
            CurrentPet = World.Client.BeastmasterBeasts[gauge.ActiveBattlehornIndex - 1];
        }
        OneWithNature = Player.Statuses.Any(s => (SID)s.ID == SID.OneWithNature);
        TrickAffinity = ActionDefinitions.TrickAffinity[CurrentPet];
        (Kinship, KinshipLeft) = CurrentKinship;

        (BestJumpTarget, NumJumpTargets) = SelectTarget(strategy, primaryTarget, 25, (primary, other) => TargetInAOECircle(other, primary.Position, 6));
        (BestLineTarget, NumLineTargets) = SelectTarget(strategy, primaryTarget, 10, (primary, other) => TargetInAOERect(other, Player.Position, Player.DirectionTo(primary), 10, 3));

        // level 50 3 chain infinitive combo
        if (Unlocked(TraitID.InstinctualMastery) && HavePet)
        {
            // infinitive combo finisher
            if (TP == 250 && ComboAffinity is BeastmasterAffinity.Sunstrider or BeastmasterAffinity.Moonstalker)
                UseAxe(Cycle(ComboAffinity), primaryTarget, 100);

            if (ComboAffinity == Cycle(TrickAffinity, Direction.CCW))
            {
                if (PetTP >= 100)
                    PushOGCD(AID.Trick, primaryTarget, 90);

                if (gauge.MasteredInstinct > 1)
                    PushOGCD(AID.Rally, Player, 80);
                if (gauge.NaturalInstinct > 0)
                    PushOGCD(AID.RallyingCheer, Player, 80);
            }

            if (gauge.MasteredInstinct > 1 && gauge.NaturalInstinct > 0 && TP >= 100 && CanWeave(AID.Rally, 1) && CanWeave(AID.RallyingCheer, 1))
            {
                if (ComboAffinity == TrickAffinity)
                    UseAxe(Cycle(ComboAffinity, Direction.CCW), primaryTarget, 70);

                if (PetTP >= 100)
                    PushOGCD(AID.Trick, primaryTarget, 60);
            }
        }

        // player -> pet combo for gems for cheer
        // TODO: i don't know if building pet gems is worth anything before 50
        if (Unlocked(TraitID.WildHeartIV) && gauge.NaturalInstinct < 3)
        {
            if (PetTP >= 100 && ComboAffinity != BeastmasterAffinity.None)
                PushOGCD(AID.Trick, primaryTarget, 20);

            if (PetTP >= 100 && TP >= 100 && HavePet)
                UseAxe(Cycle(TrickAffinity, Direction.CCW), primaryTarget, gauge.NaturalInstinct == 0 ? 10 : 1);
        }

        // pet -> player combo for gems for rally
        if (Unlocked(TraitID.WildHeartIII) && gauge.MasteredInstinct < 3)
        {
            if (TP >= 100 && ComboAffinity != BeastmasterAffinity.None)
                UseAxe(Cycle(ComboAffinity), primaryTarget, 20);

            if (PetTP >= 100 && TP >= 100 && HavePet)
                PushOGCD(AID.Trick, primaryTarget, gauge.MasteredInstinct < 2 ? 10 : 1);
        }

        // TODO: pet aoe targeting
        // 10 = wespe (final sting)
        if (strategy.TemperedRelease.IsEnabled() && HavePet && OneWithNature && CurrentPet != 10)
            PushOGCD(AID.TemperedRelease, primaryTarget);

        if (strategy.ShieldCharge.IsEnabled() && MaxChargesIn(AID.ShieldCharge) < 60)
            PushOGCD(AID.ShieldCharge, BestJumpTarget);

        if (ComboLastMove == AID.AxebladeBite)
            PushGCD(AID.Shieldsplitter, primaryTarget);

        if (ComboLastMove == AID.SmashAxe)
            PushGCD(AID.AxebladeBite, primaryTarget);

        PushGCD(AID.SmashAxe, primaryTarget);

        if (PlayerTarget != null)
            Hints.GoalZones.Add(Hints.GoalSingleTarget(PlayerTarget.Actor, Player, World.Actors, 3));

        if (strategy.Resummon.IsEnabled() && !Player.InCombat && Unlocked(TraitID.BattlehornMastery) && !OneWithNature && Kinship == Kinship.None)
        {
            if (HavePet)
                Hints.ActionsToExecute.Push(new ActionID(ActionType.PetAction, 1), Player, ActionQueue.Priority.Low);
            else if (LastUsedHorn > 0)
            {
                var horn = LastUsedHorn switch
                {
                    1 => AID.FirstBattlehorn,
                    2 => AID.SecondBattlehorn,
                    3 => AID.ThirdBattlehorn,
                    _ => AID.None
                };

                PushOGCD(horn, Player, 10);
            }
        }

        if (!HavePet)
        {
            AID[] horns = [AID.FirstBattlehorn, AID.SecondBattlehorn, AID.ThirdBattlehorn];
            foreach (var (slot, h) in World.Client.BeastmasterBeasts.Zip(horns))
                if (slot > 0 && ReadyIn(h) == 0)
                    PushOGCD(h, Player);
        }
    }

    enum Direction
    {
        CW,
        CCW
    }

    static BeastmasterAffinity Cycle(BeastmasterAffinity b, Direction dir = Direction.CW) => b switch
    {
        BeastmasterAffinity.Volant => dir == Direction.CW ? BeastmasterAffinity.Rampant : BeastmasterAffinity.Eldritch,
        BeastmasterAffinity.Rampant => dir == Direction.CW ? BeastmasterAffinity.Durant : BeastmasterAffinity.Volant,
        BeastmasterAffinity.Durant => dir == Direction.CW ? BeastmasterAffinity.Eldritch : BeastmasterAffinity.Rampant,
        BeastmasterAffinity.Eldritch => dir == Direction.CW ? BeastmasterAffinity.Volant : BeastmasterAffinity.Durant,
        BeastmasterAffinity.Sunstrider => BeastmasterAffinity.Moonstalker,
        BeastmasterAffinity.Moonstalker => BeastmasterAffinity.Sunstrider,
        _ => BeastmasterAffinity.None
    };

    (AID, Enemy?) GetAxe(BeastmasterAffinity b) => b switch
    {
        BeastmasterAffinity.Rampant => (AID.AvalancheAxe, null),
        BeastmasterAffinity.Durant => (AID.MistralAxe, null),
        BeastmasterAffinity.Eldritch => (AID.SpinningAxe, null),
        BeastmasterAffinity.Volant => (AID.GaleAxe, null),
        BeastmasterAffinity.Sunstrider => (AID.BrutalRage, BestJumpTarget),
        BeastmasterAffinity.Moonstalker => NumLineTargets > NumJumpTargets ? (AID.Calamity, BestLineTarget) : (AID.HawkishTalons, BestJumpTarget),
        _ => (AID.None, null)
    };

    void UseAxe(BeastmasterAffinity b, Enemy? primaryTarget, int priority = 1)
    {
        if (b is BeastmasterAffinity.Sunstrider or BeastmasterAffinity.Moonstalker && TP < 250)
            b = Cycle(TrickAffinity, Direction.CCW); // TODO: specify in args

        if (b is BeastmasterAffinity.Rampant or BeastmasterAffinity.Durant or BeastmasterAffinity.Eldritch or BeastmasterAffinity.Volant && TP == 250)
            b = BeastmasterAffinity.Sunstrider;

        var (a, t) = GetAxe(b);
        PushOGCD(a, t ?? primaryTarget, priority);
    }

    (Kinship, float) CurrentKinship
    {
        get
        {
            foreach (var s in Player.Statuses)
            {
                var k = (SID)s.ID switch
                {
                    SID.BeastKinship => Kinship.Beast,
                    SID.VileKinship => Kinship.Vile,
                    SID.CloudKinship => Kinship.Cloud,
                    SID.SeedKinship => Kinship.Seed,
                    SID.WaveKinship => Kinship.Wave,
                    SID.ScaleKinship => Kinship.Scale,
                    SID.SoulKinship => Kinship.Soul,
                    SID.AshKinship => Kinship.Ash,
                    _ => Kinship.None
                };
                if (k != default)
                {
                    var duration = s.ExpireAt > World.CurrentTime ? (float)(s.ExpireAt - World.CurrentTime).TotalSeconds : float.MaxValue;
                    return (k, duration);
                }
            }
            return (Kinship.None, 0);
        }
    }
}
