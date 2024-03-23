namespace BossMod.Endwalker.Alliance.A30OpeningMobs
{
    class WaterIII : Components.SelfTargetedAOEs
    {
        public WaterIII() : base(ActionID.MakeSpell(AID.WaterIII), new AOEShapeCircle(8)) { }
    }
    
    class PelagicCleaver1 : Components.SelfTargetedAOEs
    {
        public PelagicCleaver1() : base(ActionID.MakeSpell(AID.PelagicCleaver1), new AOEShapeCone(40, 30.Degrees())) { }
    }

    class PelagicCleaver2 : Components.SelfTargetedAOEs
    {
        public PelagicCleaver2() : base(ActionID.MakeSpell(AID.PelagicCleaver2), new AOEShapeCone(40, 30.Degrees())) { }
    }

    class WaterFlood : Components.SelfTargetedAOEs
    {
        public WaterFlood() : base(ActionID.MakeSpell(AID.WaterFlood), new AOEShapeCircle(6)) { }
    }
    
    class WaterBurst : Components.SelfTargetedAOEs
    {
        public WaterBurst() : base(ActionID.MakeSpell(AID.WaterBurst), new AOEShapeCircle(40)) { }
    }

    class DivineFlood : Components.SelfTargetedAOEs
    {
        public DivineFlood() : base(ActionID.MakeSpell(AID.DivineFlood), new AOEShapeCircle(6)) { }
    }

    class DivineBurst : Components.SelfTargetedAOEs
    {
        public DivineBurst() : base(ActionID.MakeSpell(AID.DivineBurst), new AOEShapeCircle(40)) { }
    }

    //[ModuleInfo(CFCID = 962, PrimaryActorOID = 0x4010)]
    public class A30OpeningMobs : BossModule
    {
        public A30OpeningMobs(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(-800, -800), 20)) { }
        
        protected override void DrawEnemies(int pcSlot, Actor pc)
        {
            Arena.Actor(PrimaryActor, ArenaColor.Enemy, true);
            foreach (var e in Enemies(OID.Triton))
                Arena.Actor(e, ArenaColor.Enemy);
            foreach (var e in Enemies(OID.DivineSprite))
                Arena.Actor(e, ArenaColor.Enemy);
            foreach (var e in Enemies(OID.WaterSprite))
                Arena.Actor(e, ArenaColor.Enemy);
        }
    }
}
