namespace BossMod.Endwalker.Alliance.A31Thaliak
{   
    [ModuleInfo(CFCID = 962, PrimaryActorOID = 0x404C)]
    public class A31Thaliak : BossModule
    {
      public static ArenaBoundsSquare BoundsSquare = new ArenaBoundsSquare(new(-945.006f, 944.976f), 24f);
      public static ArenaBoundsTri BoundsTri = new ArenaBoundsTri(new(-945.006f, 948.500f), 41f);
      public A31Thaliak(WorldState ws, Actor primary) : base(ws, primary, BoundsSquare) { }
      protected override void DrawEnemies(int pcSlot, Actor pc)
        {
              Arena.Actor(PrimaryActor, ArenaColor.Enemy, true);
              foreach (var s in Enemies(OID.ThaliakHelper))
                Arena.Actor(s, ArenaColor.Object, true);
        }
    }
}
