namespace BossMod.Autorotation.xan;

public class DeepDungeonAI(RotationModuleManager manager, Actor player) : AIBase(manager, player)
{
    public enum Track { Potion, Kite }

    public static RotationModuleDefinition Definition()
    {
        var def = new RotationModuleDefinition("Deep Dungeon AI", "Utilities for deep dungeon - potion/pomander user", "AI (xan)", "xan", RotationModuleQuality.Basic, new BitMask(~0ul), 100, CanUseWhileRoleplaying: true);

        def.AbilityTrack(Track.Potion, "Potion");
        def.AbilityTrack(Track.Kite, "Kite enemies");

        return def;
    }

    enum OID : uint
    {
        Unei = 0x3E1A,
    }

    enum Transformation : uint
    {
        None,
        Manticore,
        Succubus,
        Kuribu,
        Dreadnaught,
        Bomb,
        Mudball
    }

    enum SID : uint
    {
        Transfiguration = 565,
        ItemPenalty = 1094,
        Transfiguration2 = 4708,
    }

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        if (World.DeepDungeon.DungeonId == 0)
            return;

        var transformation = Transformation.None;
        var stat = Player.FindStatus(SID.Transfiguration) ?? Player.FindStatus(SID.Transfiguration2);
        if (stat is { } status)
        {
            transformation = (status.Extra & 0xFF) switch
            {
                42 => Transformation.Manticore,
                43 => Transformation.Succubus,
                49 => Transformation.Kuribu,
                54 => Transformation.Mudball,
                55 => Transformation.Bomb,
                244 => Transformation.Dreadnaught,
                _ => Transformation.None
            };
        }

        if (transformation != Transformation.None)
        {
            DoTransformActions(strategy, primaryTarget, transformation);
            return;
        }

        if (IsRanged && !Player.InCombat && primaryTarget is Actor target && !target.InCombat && !target.IsAlly)
            // bandaid fix to help deal with constant LOS issues
            Hints.GoalZones.Add(Hints.GoalSingleTarget(target, 3, 0.1f));

        SetupKiteZone(strategy, primaryTarget);

        if (Player.FindStatus(SID.ItemPenalty) != null)
            return;

        var (regenAction, potAction) = World.DeepDungeon.DungeonId switch
        {
            DeepDungeonState.DungeonType.POTD => (ActionDefinitions.IDPotionSustaining, ActionDefinitions.IDPotionMax),
            DeepDungeonState.DungeonType.HOH => (ActionDefinitions.IDPotionEmpyrean, ActionDefinitions.IDPotionSuper),
            DeepDungeonState.DungeonType.EO => (ActionDefinitions.IDPotionOrthos, ActionDefinitions.IDPotionHyper),
            DeepDungeonState.DungeonType.PT => (ActionDefinitions.IDPotionPilgrim, ActionDefinitions.IDPotionUltra),
            _ => (default, default)
        };

        if (regenAction != default && ShouldPotion(strategy))
            Hints.ActionsToExecute.Push(regenAction, Player, ActionQueue.Priority.ManualGCD - 1);

        if (potAction != default && Player.HPRatio <= 0.3f)
            Hints.ActionsToExecute.Push(potAction, Player, ActionQueue.Priority.ManualGCD - 1);
    }

    private bool IsRanged => Player.Class.GetRole() is Role.Ranged or Role.Healer;

    private static readonly HashSet<uint> NoMeleeAutos = [
        // hoh
        0x22C3, // heavenly onibi
        0x22C5, // heavenly dhruva
        0x22C6, // heavenly sai taisui
        0x22DC, // heavenly dogu
        0x22DE, // heavenly ganseki
        0x22ED, // heavenly kongorei
        0x22EF, // heavenly maruishi
        0x22F3, // heavenly rachimonai
        0x22FC, // heavenly doguzeri
        0x2320, // heavenly nuppeppo (WHM) (uses stone)

        // orthos
        0x3DCC, // orthos imp
        0x3DCE, // orthos fachan
        0x3DD2, // orthos water sprite
        0x3DD4, // orthos microsystem
        0x3DD5, // orthosystem β
        0x3DE0, // orthodemolisher
        0x3DE2, // orthodroid
        0x3DFD, // orthos apa
        0x3E10, // orthos ice sprite
        0x3E5C, // orthos ahriman
        0x3E62, // orthos abyss
        0x3E63, // orthodrone
        0x3E64, // orthosystem γ
        0x3E66, // orthosystem α
    ];

    private void SetupKiteZone(StrategyValues strategy, Actor? primaryTarget)
    {
        if (!IsRanged || primaryTarget == null || !Player.InCombat || !strategy.Enabled(Track.Kite) || World.DeepDungeon.IsBossFloor)
            return;

        // wew
        if (NoMeleeAutos.Contains(primaryTarget.OID))
            return;

        // assume we don't need to kite if mob is busy casting (TODO: some mob spells can be cast while moving, maybe there's a column in sheets for it)
        if (primaryTarget.CastInfo != null)
            return;

        float maxRange = 25;
        float maxKite = 9;

        var primaryPos = primaryTarget.Position;
        var total = maxRange + Player.HitboxRadius + primaryTarget.HitboxRadius;
        var totalKite = maxKite + Player.HitboxRadius + primaryTarget.HitboxRadius;
        float goalFactor = 0.05f;
        Hints.GoalZones.Add(pos =>
        {
            var dist = (pos - primaryPos).Length();
            return dist <= total && dist >= totalKite ? goalFactor : 0;
        });
    }

    private void DoTransformActions(StrategyValues strategy, Actor? primaryTarget, Transformation t)
    {
        if (primaryTarget == null)
            return;

        Func<WPos, float> goal;
        ActionID attack;
        int numTargets;
        var castTime = 0f;

        switch (t)
        {
            case Transformation.Manticore:
                goal = Hints.GoalSingleTarget(primaryTarget, 3);
                numTargets = 1;
                attack = ActionID.MakeSpell(Roleplay.AID.Pummel);
                break;
            case Transformation.Succubus:
                goal = Hints.GoalSingleTarget(primaryTarget, 25);
                numTargets = Hints.NumPriorityTargetsInAOECircle(primaryTarget.Position, 5);
                attack = ActionID.MakeSpell(Roleplay.AID.VoidFireII);
                castTime = 2.5f;
                break;
            case Transformation.Kuribu:
                // heavenly judge is ground targeted
                goal = Hints.GoalSingleTarget(primaryTarget.Position, 25);
                numTargets = Hints.NumPriorityTargetsInAOECircle(primaryTarget.Position, 6);
                attack = ActionID.MakeSpell(Roleplay.AID.HeavenlyJudge);
                castTime = 2.5f;
                break;
            case Transformation.Dreadnaught:
                goal = Hints.GoalSingleTarget(primaryTarget, 3);
                numTargets = 1;
                attack = ActionID.MakeSpell(Roleplay.AID.Rotosmash);
                break;
            case Transformation.Bomb:
                numTargets = 1;
                goal = Hints.GoalSingleTarget(primaryTarget, 14);
                attack = ActionID.MakeSpell(Roleplay.AID.BigBurst);
                break;
            case Transformation.Mudball:
                numTargets = 1;
                goal = Hints.GoalSingleTarget(primaryTarget.Position, 25);
                attack = ActionID.MakeSpell(Roleplay.AID.RockyRoll);
                break;
            default:
                return;
        }

        if (numTargets == 0)
            return;

        Hints.GoalZones.Add(goal);
        if (t == Transformation.Mudball)
        {
            if (primaryTarget.Position.InCircle(Player.Position, 25))
                Hints.ActionsToExecute.Push(attack, Player, ActionQueue.Priority.High, facingAngle: Player.AngleTo(primaryTarget));
        }
        else if (t == Transformation.Bomb)
        {
            if (primaryTarget.Position.InCircle(Player.Position, 15 + primaryTarget.HitboxRadius))
                Hints.ActionsToExecute.Push(attack, Player, ActionQueue.Priority.High, castTime: 1);
        }
        else
            Hints.ActionsToExecute.Push(attack, primaryTarget, ActionQueue.Priority.High, targetPos: primaryTarget.PosRot.XYZ(), castTime: castTime - 0.5f);
    }

    private bool ShouldPotion(StrategyValues strategy)
    {
        if (!strategy.Enabled(Track.Potion) || World.Actors.Any(w => w.OID == (uint)OID.Unei))
            return false;

        var ratio = Player.ClassCategory is ClassCategory.Tank ? 0.4f : 0.6f;
        return Player.PendingHPRatio < ratio && Player.FindStatus(648) == null && Player.InCombat;
    }
}
