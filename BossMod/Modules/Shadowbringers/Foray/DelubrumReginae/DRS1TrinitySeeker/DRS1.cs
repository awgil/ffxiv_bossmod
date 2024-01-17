namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS1TrinitySeeker
{
    class VerdantTempest : Components.CastCounter
    {
        public VerdantTempest() : base(ActionID.MakeSpell(AID.VerdantTempestAOE)) { }
    }

    class MercifulBreeze : Components.SelfTargetedAOEs
    {
        public MercifulBreeze() : base(ActionID.MakeSpell(AID.MercifulBreeze), new AOEShapeRect(50, 2.5f)) { }
    }

    class MercifulBlooms : Components.SelfTargetedAOEs
    {
        public MercifulBlooms() : base(ActionID.MakeSpell(AID.MercifulBlooms), new AOEShapeCircle(20)) { }
    }

    // TODO: it's a cleave, target can be determined by icon
    class MercifulArc : Components.CastCounter
    {
        public MercifulArc() : base(ActionID.MakeSpell(AID.MercifulArc)) { }
    }

    // TODO: it's a line stack, but I don't think there's a way to determine cast target - so everyone should just stack?..
    class IronImpact : Components.CastCounter
    {
        public IronImpact() : base(ActionID.MakeSpell(AID.IronImpact)) { }
    }

    class IronRose : Components.SelfTargetedAOEs
    {
        public IronRose() : base(ActionID.MakeSpell(AID.IronRose), new AOEShapeRect(50, 4)) { }
    }

    [ModuleInfo(CFCID = 761, NameID = 9834)]
    public class DRS1 : BossModule
    {
        public static float BarricadeRadius = 20;

        public DRS1(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(0, 278), 25)) { }

        protected override void DrawArenaForeground(int pcSlot, Actor pc)
        {
            for (int i = 0; i < 4; ++i)
            {
                var center = (45 + i * 90).Degrees();
                Arena.PathArcTo(Bounds.Center, BarricadeRadius, (center - 22.5f.Degrees()).Rad, (center + 22.5f.Degrees()).Rad);
                Arena.PathStroke(false, ArenaColor.Border, 2);
            }
        }
    }
}
