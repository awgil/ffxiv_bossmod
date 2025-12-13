using BossMod.BLU;
using static BossMod.AIHints;

namespace BossMod.Autorotation.xan;

public sealed class BLU(RotationModuleManager manager, Actor player) : Castxan<AID, TraitID, BLU.Strategy>(manager, player, PotionType.Intelligence)
{
    public struct Strategy : IStrategyCommon
    {
        public Track<Targeting> Targeting;
        public Track<AOEStrategy> AOE;
        [Track(Actions = [AID.Nightbloom, AID.BeingMortal, AID.BothEnds, AID.Apokalypsis, AID.MatraMagic])]
        public Track<OffensiveStrategy> Buffs;

        readonly Targeting IStrategyCommon.Targeting => Targeting.Value;
        readonly AOEStrategy IStrategyCommon.AOE => AOE.Value;
    }

    public static RotationModuleDefinition Definition()
    {
        var def = new RotationModuleDefinition("xan BLU", "Blue Mage", "Standard rotation (xan)", "xan", RotationModuleQuality.WIP, BitMask.Build(Class.BLU), 80);

        def.DefineShared("Burst actions").AddAssociatedActions(AID.Nightbloom, AID.BeingMortal, AID.BothEnds, AID.Apokalypsis, AID.MatraMagic);

        return def;
    }

    public enum Mimicry
    {
        None,
        Tank,
        DPS,
        Healer
    }

    public enum GCDPriority : int
    {
        None = 0,
        FillerST = 100,
        FillerAOE = 150,
        GCDWithCooldown = 200,
        Ultravibe = 250,
        Poop = 300,
        Scoop = 301,
        BuffRefresh = 600,
        SurpanakhaRepeat = 900,
    }

    private Mimicry Mimic;

    protected override bool CanUse(AID action) => action switch
    {
        // transformed blu spells
        AID.DivineCataract or AID.PhantomFlurryEnd or AID.AethericMimicryReleaseTank or AID.AethericMimicryReleaseDPS or AID.AethericMimicryReleaseHealer => true,

        // magic role actions
        AID.Addle or AID.Sleep or AID.LucidDreaming or AID.Swiftcast or AID.Surecast => true,

        // regular blu spells
        _ => World.Client.BlueMageSpells.Contains((uint)action),
    };

    public override void Exec(in Strategy strategy, Enemy? primaryTarget)
    {
        SelectPrimaryTarget(strategy, ref primaryTarget, 25);

        var currentHP = Player.PendingHPRaw;

        Mimic = CurrentMimic();

        if (World.CurrentCFCID > 0 && World.Party.WithoutSlot().Count(p => p.Type == ActorType.Player) == 1)
        {
            if (CanUse(AID.BasicInstinct) && Player.FindStatus(SID.MightyGuard) == null)
                PushGCD(AID.MightyGuard, Player);

            if (Player.FindStatus(SID.BasicInstinct) == null)
                PushGCD(AID.BasicInstinct, Player);
        }

        if (Player.FindStatus(SID.AuspiciousTrance) != null)
            PushGCD(AID.DivineCataract, Player, GCDPriority.SurpanakhaRepeat);
        if (Player.FindStatus(SID.ChelonianGate) != null)
            return;

        if (Mimic == Mimicry.Tank)
            TankSpecific(primaryTarget);

        if (Mimic == Mimicry.DPS)
            DpsSpecific(primaryTarget);

        var haveModule = Bossmods.ActiveModule?.StateMachine.ActiveState != null;

        // mortal flame
        if (primaryTarget is { } p && StatusDetails(p.Actor, SID.MortalFlame, Player.InstanceID).Left == 0 && Hints.PriorityTargets.Count() == 1 && haveModule)
            PushGCD(AID.MortalFlame, p, GCDPriority.GCDWithCooldown);

        if (haveModule && currentHP * 2 < Player.HPMP.MaxHP)
        {
            PushGCD(AID.Swiftcast, Player, GCDPriority.SurpanakhaRepeat);
            PushGCD(AID.Rehydration, Player, GCDPriority.SurpanakhaRepeat);
        }

        // bom
        var numBomTargets = Hints.NumPriorityTargetsInAOE(e => StatusDetails(e.Actor, SID.BreathOfMagic, Player.InstanceID).Left < 5 && e.Actor.Position.InCircleCone(Player.Position, 10 + Player.HitboxRadius + e.Actor.HitboxRadius, Player.Rotation.ToDirection(), 60.Degrees()));
        if (numBomTargets > (haveModule ? 0 : 2))
            PushGCD(AID.BreathOfMagic, Player, GCDPriority.BuffRefresh);

        // if channeling surpanakha, don't use anything else
        var numSurpTargets = AdjustNumTargets(strategy.AOE, Hints.NumPriorityTargetsInAOECone(Player.Position, 16, Player.Rotation.ToDirection(), 60.Degrees()));
        var surp = StatusLeft(SID.SurpanakhasFury);
        if (numSurpTargets > 0 && (MaxChargesIn(AID.Surpanakha) == 0 || surp > 0 && ReadyIn(AID.Surpanakha) <= 1))
        {
            PushGCD(AID.Surpanakha, Player, GCDPriority.SurpanakhaRepeat);
            return;
        }

        if (ReadyIn(AID.TheRoseOfDestruction) <= GCD)
            PushGCD(AID.TheRoseOfDestruction, primaryTarget, GCDPriority.GCDWithCooldown);

        // standard filler spells
        if (primaryTarget != null && GetCurrentPositional(primaryTarget.Actor) is Positional.Front or Positional.Any)
            PushGCD(AID.GoblinPunch, primaryTarget, GCDPriority.FillerST);

        PushGCD(AID.SonicBoom, primaryTarget, GCDPriority.FillerST);

        if (World.Actors.Any(p => p.Type == ActorType.Chocobo && p.OwnerID == Player.InstanceID))
            PushGCD(AID.ChocoMeteor, primaryTarget, GCDPriority.FillerST + 1);

        if (CanUse(AID.PeatPelt) && CanUse(AID.DeepClean) && StatusLeft(SID.SpickAndSpan) < GCDLength)
        {
            var (poopTarget, poopNum) = SelectTarget(strategy, primaryTarget, 25, (primary, other) => Hints.TargetInAOECircle(other, primary.Position, 6));
            if (poopTarget != null && poopNum > 2)
            {
                var scoopNum = Hints.NumPriorityTargetsInAOE(act => StatusDetails(act.Actor, SID.Begrimed, Player.InstanceID).Left > SpellGCDLength && Hints.TargetInAOECircle(act.Actor, poopTarget.Actor.Position, 6));
                if (scoopNum > 2)
                    PushGCD(AID.DeepClean, poopTarget, GCDPriority.Scoop);
                PushGCD(AID.PeatPelt, poopTarget, GCDPriority.Poop);
            }
        }

        if (CanUse(AID.TheRamsVoice) && CanUse(AID.Ultravibration))
        {
            Hints.GoalZones.Add(Hints.GoalAOECircle(6));
            var priorityTotal = 0;
            var nearbyTotal = 0;
            var nearbyFrozen = 0;

            foreach (var target in Hints.PriorityTargets)
            {
                priorityTotal++;
                if (target.Actor.Position.InCircle(Player.Position, 6 + Player.HitboxRadius + target.Actor.HitboxRadius))
                {
                    nearbyTotal++;
                    if (StatusDetails(target.Actor, SID.DeepFreeze, Player.InstanceID).Left > 3)
                        nearbyFrozen++;
                }
            }
            if (nearbyTotal == priorityTotal && nearbyTotal > 2)
            {
                if (nearbyFrozen == nearbyTotal)
                    PushGCD(AID.Ultravibration, Player, GCDPriority.Ultravibe + 1);

                PushGCD(AID.TheRamsVoice, Player, GCDPriority.Ultravibe);
            }
        }

        if (NumNearbyTargets(strategy, 10) > 0)
        {
            PushOGCD(AID.BeingMortal, Player);
            PushOGCD(AID.Nightbloom, Player);
        }

        PushOGCD(AID.FeatherRain, primaryTarget);
        PushOGCD(AID.ShockStrike, primaryTarget);
        PushOGCD(AID.JKick, primaryTarget);

        if (Player.HPMP.CurMP < Player.HPMP.MaxMP * 0.7f)
            PushOGCD(AID.LucidDreaming, Player);

        if (NextGCD is AID.GoblinPunch or AID.Devour && primaryTarget is { } t)
            Hints.GoalZones.Add(Hints.GoalSingleTarget(t.Actor, Positional.Front, 3));
    }

    private void TankSpecific(Enemy? primaryTarget)
    {
        if (CanUse(AID.Devour) && !CanFitGCD(StatusLeft(SID.HPBoost), 1))
            PushGCD(AID.Devour, primaryTarget, GCDPriority.BuffRefresh);

        var d = Hints.PredictedDamage.Count(p => p.Players[0] && p.Activation >= World.FutureTime(GCD + GetCastTime(AID.ChelonianGate)) && p.Type is PredictedDamageType.Tankbuster or PredictedDamageType.Shared);
        if (d > 0)
        {
            PushGCD(AID.ChelonianGate, Player, GCDPriority.BuffRefresh);
            PushGCD(AID.DragonForce, Player, GCDPriority.BuffRefresh);
        }
    }

    private void DpsSpecific(Enemy? primaryTarget)
    {
        PushGCD(AID.MatraMagic, primaryTarget, GCDPriority.GCDWithCooldown);
    }

    public Mimicry CurrentMimic()
    {
        foreach (var st in Player.Statuses)
        {
            switch ((SID)st.ID)
            {
                case SID.AethericMimicryTank:
                    return Mimicry.Tank;
                case SID.AethericMimicryDPS:
                    return Mimicry.DPS;
                case SID.AethericMimicryHealer:
                    return Mimicry.Healer;
            }
        }

        return Mimicry.None;
    }
}
