using System;

namespace BossMod.Endwalker.Alliance.A2Rhalgr
{
    class DestructiveBolt : CommonComponents.SpreadFromCastTargets
    {
        public DestructiveBolt() : base(ActionID.MakeSpell(AID.DestructiveBoltAOE), 3) { }
    }

    class StrikingMeteor : CommonComponents.Puddles
    {
        public StrikingMeteor() : base(ActionID.MakeSpell(AID.StrikingMeteor), 6) { }
    }

    class LightningStorm : CommonComponents.SpreadFromCastTargets
    {
        public LightningStorm() : base(ActionID.MakeSpell(AID.LightningStorm), 5) { }
    }

    class HandOfTheDestroyerWrath : CommonComponents.SelfTargetedAOE
    {
        public HandOfTheDestroyerWrath() : base(ActionID.MakeSpell(AID.HandOfTheDestroyerWrathAOE), new AOEShapeRect(90, 20)) { }
    }

    class HandOfTheDestroyerJudgment : CommonComponents.SelfTargetedAOE
    {
        public HandOfTheDestroyerJudgment() : base(ActionID.MakeSpell(AID.HandOfTheDestroyerJudgmentAOE), new AOEShapeRect(90, 20)) { }
    }

    class BrokenWorld : CommonComponents.SelfTargetedAOE
    {
        public BrokenWorld() : base(ActionID.MakeSpell(AID.BrokenWorldAOE), new AOEShapeCircle(30)) { } // TODO: determine falloff
    }

    class BronzeLightning : CommonComponents.SelfTargetedAOE
    {
        public BronzeLightning() : base(ActionID.MakeSpell(AID.BronzeLightning), new AOEShapeCone(50, MathF.PI / 8), 4) { }
    }

    // TODO: show knockback hints, lightning hints, etc... need to draw complex arena shape though
    class RhalgrBeacon : CommonComponents.SelfTargetedAOE
    {
        public RhalgrBeacon() : base(ActionID.MakeSpell(AID.RhalgrsBeaconAOE), new AOEShapeCircle(10)) { }
    }

    public class A2Rhalgr : BossModule
    {
        public A2Rhalgr(BossModuleManager manager, Actor primary)
            : base(manager, primary, true)
        {
            Arena.WorldCenter = new(-15, 20, 275); // arena has a really complex shape...
            Arena.WorldHalfSize = 30;

            var sb = new StateMachineBuilder(this);
            var s = sb.Simple(0, 600, "Fight")
                .ActivateOnEnter<DestructiveBolt>()
                .ActivateOnEnter<StrikingMeteor>()
                .ActivateOnEnter<LightningStorm>()
                .ActivateOnEnter<HandOfTheDestroyerWrath>()
                .ActivateOnEnter<HandOfTheDestroyerJudgment>()
                .ActivateOnEnter<BrokenWorld>()
                .ActivateOnEnter<BrokenShards>()
                .ActivateOnEnter<BronzeLightning>()
                .ActivateOnEnter<RhalgrBeacon>()
                .DeactivateOnExit<DestructiveBolt>()
                .DeactivateOnExit<StrikingMeteor>()
                .DeactivateOnExit<LightningStorm>()
                .DeactivateOnExit<HandOfTheDestroyerWrath>()
                .DeactivateOnExit<HandOfTheDestroyerJudgment>()
                .DeactivateOnExit<BrokenWorld>()
                .DeactivateOnExit<BrokenShards>()
                .DeactivateOnExit<BronzeLightning>()
                .DeactivateOnExit<RhalgrBeacon>();
            s.Raw.Update = _ => PrimaryActor.IsDead ? s.Raw.Next : null;
            sb.Simple(1, 0, "???");
            InitStates(sb.Initial);
            //InitStates(new A2RhalgrStates(this).Initial);
        }

        protected override void DrawArenaForegroundPre(int pcSlot, Actor pc)
        {
            Arena.PathLineTo(new(2.5f, 0, 245));
            Arena.PathLineTo(new(2.6f, 0, 275));
            Arena.PathLineTo(new(7.3f, 0, 295));
            Arena.PathLineTo(new(3, 0, 297));
            Arena.PathLineTo(new(-1, 0, 286));
            Arena.PathLineTo(new(-6.5f, 0, 288));
            Arena.PathLineTo(new(-6.5f, 0, 305));
            Arena.PathLineTo(new(-13, 0, 305));
            Arena.PathLineTo(new(-13, 0, 288));
            Arena.PathLineTo(new(-21.5f, 0, 288));
            Arena.PathLineTo(new(-21.5f, 0, 305));
            Arena.PathLineTo(new(-28, 0, 305));
            Arena.PathLineTo(new(-28, 0, 283));
            Arena.PathLineTo(new(-42, 0, 300));
            Arena.PathLineTo(new(-45.5f, 0, 297));
            Arena.PathLineTo(new(-34, 0, 271.5f));
            Arena.PathLineTo(new(-37, 0, 245));
            Arena.PathStroke(true, Arena.ColorBorder);

            foreach (var p in WorldState.Actors)
                if (p.Type == ActorType.Player && !p.IsDead)
                    Arena.Actor(p, Arena.ColorPlayerGeneric);
        }

        protected override void DrawArenaForegroundPost(int pcSlot, Actor pc)
        {
            Arena.Actor(PrimaryActor, Arena.ColorEnemy);
            Arena.Actor(pc, Arena.ColorPC);
        }
    }
}
