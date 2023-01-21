namespace BossMod.Endwalker.Extreme.Ex5Rubicante
{
    class SweepingImmolationSpread : Components.SelfTargetedAOEs
    {
        public SweepingImmolationSpread() : base(ActionID.MakeSpell(AID.SweepingImmolationSpread), new AOEShapeCone(20, 90.Degrees())) { }
    }

    class SweepingImmolationStack : Components.SelfTargetedAOEs
    {
        public SweepingImmolationStack() : base(ActionID.MakeSpell(AID.SweepingImmolationStack), new AOEShapeCone(20, 90.Degrees())) { }
    }

    class PartialTotalImmolation : Components.CastStackSpread
    {
        public PartialTotalImmolation() : base(ActionID.MakeSpell(AID.TotalImmolation), ActionID.MakeSpell(AID.PartialImmolation), 6, 5, 8, 8, true) { }
    }

    class ScaldingSignal : Components.SelfTargetedAOEs
    {
        public ScaldingSignal() : base(ActionID.MakeSpell(AID.ScaldingSignal), new AOEShapeCircle(10)) { }
    }

    class ScaldingRing : Components.SelfTargetedAOEs
    {
        public ScaldingRing() : base(ActionID.MakeSpell(AID.ScaldingRing), new AOEShapeDonut(10, 20)) { }
    }

    class ScaldingFleetFirst : Components.BaitAwayEveryone
    {
        public ScaldingFleetFirst() : base(new AOEShapeRect(40, 3), ActionID.MakeSpell(AID.ScaldingFleetFirst)) { }
    }

    // note: it seems to have incorrect target, but acts like self-targeted
    class ScaldingFleetSecond : Components.SelfTargetedAOEs
    {
        public ScaldingFleetSecond() : base(ActionID.MakeSpell(AID.ScaldingFleetSecond), new AOEShapeRect(60, 3)) { }
    }
}
