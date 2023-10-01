namespace BossMod.Endwalker.Alliance.A14Naldthal
{
    class GoldenTenet : Components.CastSharedTankbuster
    {
        public GoldenTenet() : base(ActionID.MakeSpell(AID.GoldenTenetAOE), 6) { }
    }

    class StygianTenet : Components.SpreadFromCastTargets
    {
        public StygianTenet() : base(ActionID.MakeSpell(AID.StygianTenetAOE), 6) { }
    }

    class HellOfFireFront : Components.SelfTargetedAOEs
    {
        public HellOfFireFront() : base(ActionID.MakeSpell(AID.HellOfFireFrontAOE), new AOEShapeCone(60, 90.Degrees())) { }
    }

    class HellOfFireBack : Components.SelfTargetedAOEs
    {
        public HellOfFireBack() : base(ActionID.MakeSpell(AID.HellOfFireBackAOE), new AOEShapeCone(60, 90.Degrees())) { }
    }

    class WaywardSoul : Components.SelfTargetedAOEs
    {
        public WaywardSoul() : base(ActionID.MakeSpell(AID.WaywardSoulAOE), new AOEShapeCircle(18), 3) { }
    }

    class SoulVessel : Components.Adds
    {
        public SoulVessel() : base((uint)OID.SoulVesselReal) { }
    }

    class Twingaze : Components.SelfTargetedAOEs
    {
        public Twingaze() : base(ActionID.MakeSpell(AID.Twingaze), new AOEShapeCone(60, 15.Degrees())) { }
    }

    class MagmaticSpell : Components.StackWithCastTargets
    {
        public MagmaticSpell() : base(ActionID.MakeSpell(AID.MagmaticSpellAOE), 6, 8) { }
    }

    class TippedScales : Components.CastCounter
    {
        public TippedScales() : base(ActionID.MakeSpell(AID.TippedScalesAOE)) { }
    }

    // TODO: balancing counter
    public class A14Naldthal : BossModule
    {
        public A14Naldthal(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(750, -750), 30)) { }
    }
}
