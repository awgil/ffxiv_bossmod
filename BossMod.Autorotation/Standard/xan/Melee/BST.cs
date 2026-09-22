using BossMod.BST;
using static BossMod.AIHints;

namespace BossMod.Autorotation.xan;

public sealed class BST(RotationModuleManager manager, Actor player) : Attackxan<AID, TraitID, BST.Strategy>(manager, player, PotionType.Strength)
{
    public struct Strategy : IStrategyCommon
    {
        public Track<Targeting> Targeting;
        public Track<AOEStrategy> AOE;

        [Track("Shield Charge")]
        public Track<EnabledByDefault> ShieldCharge;

        [Track("Summon first available pet")]
        public Track<EnabledByDefault> Summon;

        [Track("Tempered Release")]
        public Track<EnabledByDefault> TemperedRelease;

        [Track("Parting Blow")]
        public Track<EnabledByDefault> PartingBlow;

        [Track("Resummon pet out of combat to refresh One With Nature")]
        public Track<EnabledByDefault> Resummon;

        [Track("Pre-Borrow")]
        public Track<EnabledByDefault> Preborrow;

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
    private Enemy? BestExplosionTarget;
    private int NumExplosionTargets;
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

        var petIsLeaving = ReadyIn(AID.PartingBlow) > 5;

        (BestJumpTarget, NumJumpTargets) = SelectTarget(strategy, primaryTarget, 25, (primary, other) => TargetInAOECircle(other, primary.Position, 6));
        (BestExplosionTarget, NumExplosionTargets) = SelectTarget(strategy, primaryTarget, 25, (primary, other) => TargetInAOECircle(other, primary.Position, 8));
        (BestLineTarget, NumLineTargets) = SelectTarget(strategy, primaryTarget, 10, (primary, other) => TargetInAOERect(other, Player.Position, Player.DirectionTo(primary), 10, 3));

        // level 50 3 chain infinitive combo
        if (Unlocked(TraitID.InstinctualMastery) && HavePet)
        {
            // infinitive combo finisher
            if (TP == 250 && ComboAffinity is BeastmasterAffinity.Sunstrider or BeastmasterAffinity.Moonstalker)
                UseAxe(Cycle(ComboAffinity), primaryTarget, 100);

            if (ComboAffinity == Cycle(TrickAffinity, Direction.CCW))
            {
                if (PetTP >= 100 && !petIsLeaving)
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

                if (PetTP >= 100 && !petIsLeaving)
                    PushOGCD(AID.Trick, primaryTarget, 60);
            }
        }

        // at level 40 we also get pet gems for use with cheer
        if (Unlocked(TraitID.WildHeartIV))
        {
            if (gauge.NaturalInstinct < 3)
            {
                if (PetTP >= 100 && ComboAffinity != BeastmasterAffinity.None && !petIsLeaving)
                    PushOGCD(AID.Trick, primaryTarget, 20);

                if (PetTP >= 100 && TP >= 100 && HavePet)
                    UseAxe(Cycle(TrickAffinity, Direction.CCW), primaryTarget, gauge.NaturalInstinct == 0 ? 10 : 1);
            }

            // cheer in opener
            // TODO is the condition right?
            if (gauge.NaturalInstinct == 0 && ComboAffinity != BeastmasterAffinity.None)
                PushOGCD(AID.RallyingCheer, Player);
        }

        // at level 30 we get player gems for use with rally
        if (Unlocked(TraitID.WildHeartIII))
        {
            if (gauge.MasteredInstinct < 3)
            {
                if (TP >= 100 && ComboAffinity != BeastmasterAffinity.None
                    && (gauge.MasteredInstinct > 0 || !CanFitGCD(ComboTimer, 1)))
                    UseAxe(Cycle(ComboAffinity), primaryTarget, 20);

                if (PetTP >= 100 && TP >= 100 && HavePet && !petIsLeaving)
                    PushOGCD(AID.Trick, primaryTarget, gauge.MasteredInstinct switch
                    {
                        0 => 30,
                        1 => 10,
                        _ => 1
                    });
            }

            if (gauge.MasteredInstinct > 0 && ComboAffinity is BeastmasterAffinity.Sunstrider or BeastmasterAffinity.Moonstalker)
                PushOGCD(AID.Rally, Player);
        }

        // TODO: pet aoe targeting
        // 10 = wespe (final sting)
        if (strategy.TemperedRelease.IsEnabled() && HavePet && OneWithNature && CurrentPet != 10)
            PushOGCD(AID.TemperedRelease, primaryTarget);

        var pbOk = CurrentPet == 10 ? OneWithNature : !OneWithNature;

        if (strategy.PartingBlow.IsEnabled() && HavePet && pbOk && CanWeave(GetNextHorn(gauge), 1.1f, 1) && NumExplosionTargets > 0)
        {
            // wespe
            if (CurrentPet == 10)
                PushOGCD(AID.TemperedRelease, primaryTarget);
            else
                PushOGCD(AID.PartingBlow, BestExplosionTarget);
        }

        if (strategy.ShieldCharge.IsEnabled() && MaxChargesIn(AID.ShieldCharge) < 60)
            PushOGCD(AID.ShieldCharge, BestJumpTarget);

        if (ComboLastMove == AID.AxebladeBite)
            PushGCD(AID.Shieldsplitter, primaryTarget);

        if (ComboLastMove == AID.SmashAxe)
            PushGCD(AID.AxebladeBite, primaryTarget);

        PushGCD(AID.SmashAxe, primaryTarget);

        if (PlayerTarget != null)
            Hints.GoalZones.Add(Hints.GoalSingleTarget(PlayerTarget.Actor, Player, World.Actors, 3));

        // summon first available pet
        // TOOD: this should give the preborrowed pet lowest priority, so we should do 1 -> 3 -> 2 if 2 is borrowed
        if (!HavePet && strategy.Summon.IsEnabled())
        {
            AID[] horns = [AID.FirstBattlehorn, AID.SecondBattlehorn, AID.ThirdBattlehorn];
            foreach (var (slot, h) in World.Client.BeastmasterBeasts.Zip(horns))
                if (slot > 0 && ReadyIn(h) == 0)
                    PushOGCD(h, Player);
        }

        if (!Player.InCombat)
            Prep(strategy, gauge);
    }

    void Prep(in Strategy strategy, in BeastmasterGauge gauge)
    {
        // resummon
        if (strategy.Resummon.IsEnabled() && Unlocked(TraitID.BattlehornMastery) && !OneWithNature && gauge.KinshipBattlehornIndex != gauge.ActiveBattlehornIndex)
        {
            if (HavePet)
                Hints.ActionsToExecute.Push(new ActionID(ActionType.PetAction, 1), Player, ActionQueue.Priority.Low);
            else if (LastUsedHorn > 0 && LastUsedHorn != gauge.KinshipBattlehornIndex)
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

        // preborrow. note that preborrow must be performed with a different pet than the one we plan to use, because resummoning that pet will remove the buff; this is what gauge.KinshipBattlehornIndex tracks
        // presumably designed to prevent players from using all available TRs PLUS a borrowed skill within the 90s duration of borrow (in standard rotation you use each beast for 30 seconds or so due to Parting Blow cooldown)
        // TODO this should not be hardcoded to horn 3
        if (strategy.Preborrow.IsEnabled() && World.Client.BeastmasterBeasts[2] > 0)
        {
            if (gauge.Classification == 0)
            {
                if (gauge.ActiveBattlehornIndex > 1)
                    PushOGCD(AID.Borrow, Player, 10);
                else
                    PushOGCD(AID.ThirdBattlehorn, Player, 10);
            }
            else if (gauge.KinshipBattlehornIndex > 1 && gauge.ActiveBattlehornIndex == gauge.KinshipBattlehornIndex)
                Hints.ActionsToExecute.Push(new ActionID(ActionType.PetAction, 1), Player, ActionQueue.Priority.Low);
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

    float GetNextHorn(in BeastmasterGauge gauge)
    {
        var h1 = gauge.ActiveBattlehornIndex == 1 ? 90 : ReadyIn(AID.FirstBattlehorn);
        var h2 = gauge.ActiveBattlehornIndex == 2 ? 90 : ReadyIn(AID.SecondBattlehorn);
        var h3 = gauge.ActiveBattlehornIndex == 3 ? 90 : ReadyIn(AID.ThirdBattlehorn);

        return MathF.Min(h1, MathF.Min(h2, h3));
    }

    float ComboTimer
    {
        get
        {
            foreach (var s in Player.Statuses)
            {
                var a = (SID)s.ID switch
                {
                    SID.VolantHeart => BeastmasterAffinity.Volant,
                    SID.RampantHeart => BeastmasterAffinity.Rampant,
                    SID.DurantHeart => BeastmasterAffinity.Durant,
                    SID.EldritchHeart => BeastmasterAffinity.Eldritch,
                    SID.Sunstrider => BeastmasterAffinity.Sunstrider,
                    SID.Moonstalker => BeastmasterAffinity.Moonstalker,
                    _ => BeastmasterAffinity.None
                };
                if (a != BeastmasterAffinity.None)
                    return (float)(s.ExpireAt - World.CurrentTime).TotalSeconds;
            }
            return 0;
        }
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
