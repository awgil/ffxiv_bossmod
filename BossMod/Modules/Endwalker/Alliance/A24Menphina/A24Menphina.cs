namespace BossMod.Endwalker.Alliance.A24Menphina
{
    class BlueMoon : Components.CastCounter
    {
        public BlueMoon() : base(ActionID.MakeSpell(AID.BlueMoonAOE)) { }
    }

    class FirstBlush : Components.SelfTargetedAOEs
    {
        public FirstBlush() : base(ActionID.MakeSpell(AID.FirstBlush), new AOEShapeRect(80, 12.5f)) { }
    }

    class SilverMirror : Components.SelfTargetedAOEs
    {
        public SilverMirror() : base(ActionID.MakeSpell(AID.SilverMirrorAOE), new AOEShapeCircle(7)) { }
    }

    class Moonset : Components.SelfTargetedAOEs
    {
        public Moonset() : base(ActionID.MakeSpell(AID.MoonsetAOE), new AOEShapeCircle(12)) { }
    }

    class LoversBridgeShort : Components.SelfTargetedAOEs
    {
        public LoversBridgeShort() : base(ActionID.MakeSpell(AID.LoversBridgeShort), new AOEShapeCircle(19)) { }
    }

    class LoversBridgeLong : Components.SelfTargetedAOEs
    {
        public LoversBridgeLong() : base(ActionID.MakeSpell(AID.LoversBridgeLong), new AOEShapeCircle(19)) { }
    }

    class CeremonialPillar : Components.Adds
    {
        public CeremonialPillar() : base((uint)OID.CeremonialPillar) { }
    }

    class AncientBlizzard : Components.SelfTargetedAOEs
    {
        public AncientBlizzard() : base(ActionID.MakeSpell(AID.AncientBlizzard), new AOEShapeCone(45, 22.5f.Degrees())) { }
    }

    class KeenMoonbeam : Components.SpreadFromCastTargets
    {
        public KeenMoonbeam() : base(ActionID.MakeSpell(AID.KeenMoonbeamAOE), 6) { }
    }

    class RiseOfTheTwinMoons : Components.CastCounter
    {
        public RiseOfTheTwinMoons() : base(ActionID.MakeSpell(AID.RiseOfTheTwinMoons)) { }
    }

    class CrateringChill : Components.SelfTargetedAOEs
    {
        public CrateringChill() : base(ActionID.MakeSpell(AID.CrateringChillAOE), new AOEShapeCircle(20)) { }
    }

    class MoonsetRays : Components.StackWithCastTargets
    {
        public MoonsetRays() : base(ActionID.MakeSpell(AID.MoonsetRaysAOE), 6, 4) { }
    }

    [ModuleInfo(CFCID = 911, NameID = 12063)]
    public class A24Menphina : BossModule
    {
        public A24Menphina(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(800, 750), 30)) { }
    }
}
