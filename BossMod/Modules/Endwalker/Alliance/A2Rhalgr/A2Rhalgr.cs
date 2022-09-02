namespace BossMod.Endwalker.Alliance.A2Rhalgr
{
    class DestructiveBolt : Components.SpreadFromCastTargets
    {
        public DestructiveBolt() : base(ActionID.MakeSpell(AID.DestructiveBoltAOE), 3) { }
    }

    class StrikingMeteor : Components.LocationTargetedAOEs
    {
        public StrikingMeteor() : base(ActionID.MakeSpell(AID.StrikingMeteor), 6) { }
    }

    class LightningStorm : Components.SpreadFromCastTargets
    {
        public LightningStorm() : base(ActionID.MakeSpell(AID.LightningStorm), 5) { }
    }

    class HandOfTheDestroyerWrath : Components.SelfTargetedLegacyRotationAOEs
    {
        public HandOfTheDestroyerWrath() : base(ActionID.MakeSpell(AID.HandOfTheDestroyerWrathAOE), new AOEShapeRect(90, 20)) { }
    }

    class HandOfTheDestroyerJudgment : Components.SelfTargetedLegacyRotationAOEs
    {
        public HandOfTheDestroyerJudgment() : base(ActionID.MakeSpell(AID.HandOfTheDestroyerJudgmentAOE), new AOEShapeRect(90, 20)) { }
    }

    class BrokenWorld : Components.SelfTargetedAOEs
    {
        public BrokenWorld() : base(ActionID.MakeSpell(AID.BrokenWorldAOE), new AOEShapeCircle(30)) { } // TODO: determine falloff
    }

    class BronzeLightning : Components.SelfTargetedLegacyRotationAOEs
    {
        public BronzeLightning() : base(ActionID.MakeSpell(AID.BronzeLightning), new AOEShapeCone(50, 22.5f.Degrees()), 4) { }
    }

    // TODO: show knockback hints, lightning hints, etc... need to draw complex arena shape though
    class RhalgrBeacon : Components.SelfTargetedAOEs
    {
        public RhalgrBeacon() : base(ActionID.MakeSpell(AID.RhalgrsBeaconAOE), new AOEShapeCircle(10)) { }
    }

    public class A2RhalgrStates : StateMachineBuilder
    {
        public A2RhalgrStates(BossModule module) : base(module)
        {
            // TODO: reconsider
            TrivialPhase()
                .ActivateOnEnter<DestructiveBolt>()
                .ActivateOnEnter<StrikingMeteor>()
                .ActivateOnEnter<LightningStorm>()
                .ActivateOnEnter<HandOfTheDestroyerWrath>()
                .ActivateOnEnter<HandOfTheDestroyerJudgment>()
                .ActivateOnEnter<BrokenWorld>()
                .ActivateOnEnter<BrokenShards>()
                .ActivateOnEnter<BronzeLightning>()
                .ActivateOnEnter<RhalgrBeacon>();
        }
    }

    public class A2Rhalgr : BossModule
    {
        public A2Rhalgr(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsSquare(new(-15, 275), 30)) // note: arena has a really complex shape...
        {
        }

        protected override void DrawArenaForeground(int pcSlot, Actor pc)
        {
            Arena.PathLineTo(new(2.5f, 245));
            Arena.PathLineTo(new(2.6f, 275));
            Arena.PathLineTo(new(7.3f, 295));
            Arena.PathLineTo(new(3, 297));
            Arena.PathLineTo(new(-1, 286));
            Arena.PathLineTo(new(-6.5f, 288));
            Arena.PathLineTo(new(-6.5f, 305));
            Arena.PathLineTo(new(-13, 305));
            Arena.PathLineTo(new(-13, 288));
            Arena.PathLineTo(new(-21.5f, 288));
            Arena.PathLineTo(new(-21.5f, 305));
            Arena.PathLineTo(new(-28, 305));
            Arena.PathLineTo(new(-28, 283));
            Arena.PathLineTo(new(-42, 300));
            Arena.PathLineTo(new(-45.5f, 297));
            Arena.PathLineTo(new(-34, 271.5f));
            Arena.PathLineTo(new(-37, 245));
            Arena.PathStroke(true, ArenaColor.Border);
        }
    }
}
