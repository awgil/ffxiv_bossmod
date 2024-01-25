namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS2Dahu
{
    class FallingRock : Components.LocationTargetedAOEs
    {
        public FallingRock() : base(ActionID.MakeSpell(AID.FallingRock), 4) { }
    }

    class HotCharge : Components.ChargeAOEs
    {
        public HotCharge() : base(ActionID.MakeSpell(AID.HotCharge), 4) { }
    }

    class Firebreathe : Components.SelfTargetedAOEs
    {
        public Firebreathe() : base(ActionID.MakeSpell(AID.Firebreathe), new AOEShapeCone(60, 45.Degrees())) { }
    }

    class HeadDown : Components.ChargeAOEs
    {
        public HeadDown() : base(ActionID.MakeSpell(AID.HeadDown), 2) { }
    }

    class HuntersClaw : Components.SelfTargetedAOEs
    {
        public HuntersClaw() : base(ActionID.MakeSpell(AID.HuntersClaw), new AOEShapeCircle(8)) { }
    }

    [ModuleInfo(CFCID = 761, NameID = 9751)]
    public class DRS2 : BossModule
    {
        public DRS2(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(82, 138), 30)) { }

        protected override void DrawEnemies(int pcSlot, Actor pc)
        {
            base.DrawEnemies(pcSlot, pc);
            Arena.Actors(Enemies(OID.CrownedMarchosias), ArenaColor.Enemy);
        }
    }
}
