namespace BossMod.Endwalker.Extreme.Ex5Rubicante;

class ArchInferno : Components.PersistentVoidzoneAtCastTarget
{
    public ArchInferno() : base(5, ActionID.MakeSpell(AID.ArchInferno), m => Enumerable.Repeat(m.PrimaryActor, (m.PrimaryActor.CastInfo?.IsSpell(AID.ArchInferno) ?? false) ? 0 : 1), 0) { }
}

class InfernoDevilFirst : Components.SelfTargetedAOEs
{
    public InfernoDevilFirst() : base(ActionID.MakeSpell(AID.InfernoDevilFirst), new AOEShapeCircle(10)) { }
}

class InfernoDevilRest : Components.SelfTargetedAOEs
{
    public InfernoDevilRest() : base(ActionID.MakeSpell(AID.InfernoDevilRest), new AOEShapeCircle(10)) { }
}

class Conflagration : Components.SelfTargetedAOEs
{
    public Conflagration() : base(ActionID.MakeSpell(AID.Conflagration), new AOEShapeRect(20, 5, 20)) { }
}

class RadialFlagration : Components.SimpleProtean
{
    public RadialFlagration() : base(ActionID.MakeSpell(AID.RadialFlagrationAOE), new AOEShapeCone(21, 15.Degrees())) { } // TODO: verify angle
}

class SpikeOfFlame : Components.SpreadFromCastTargets
{
    public SpikeOfFlame() : base(ActionID.MakeSpell(AID.SpikeOfFlame), 5) { }
}

class FourfoldFlame : Components.StackWithCastTargets
{
    public FourfoldFlame() : base(ActionID.MakeSpell(AID.FourfoldFlame), 6, 4, 4) { }
}

class TwinfoldFlame : Components.StackWithCastTargets
{
    public TwinfoldFlame() : base(ActionID.MakeSpell(AID.TwinfoldFlame), 4, 2, 2) { }
}
