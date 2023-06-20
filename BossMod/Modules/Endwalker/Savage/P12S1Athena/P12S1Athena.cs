namespace BossMod.Endwalker.Savage.P12S1Athena
{
    class RayOfLight : Components.SelfTargetedAOEs
    {
        public RayOfLight() : base(ActionID.MakeSpell(AID.RayOfLight), new AOEShapeRect(60, 5)) { }
    }

    class UltimaBlade : Components.CastCounter
    {
        public UltimaBlade() : base(ActionID.MakeSpell(AID.UltimaBladeAOE)) { }
    }

    class Parthenos : Components.SelfTargetedAOEs
    {
        public Parthenos() : base(ActionID.MakeSpell(AID.Parthenos), new AOEShapeRect(60, 8, 60)) { }
    }

    public class P12S1Athena : BossModule
    {
        public P12S1Athena(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsSquare(new(100, 100), 20)) { }
    }
}
