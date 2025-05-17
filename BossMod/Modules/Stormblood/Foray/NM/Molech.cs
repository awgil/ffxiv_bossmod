namespace BossMod.Stormblood.Foray.NM.Molech;

public enum OID : uint
{
    Boss = 0x275D, // R6.000, x1
    Adulator = 0x275E, // R2.800, x3
}

public enum AID : uint
{
    W11TonzeSwipeAdds = 14978, // Adulator->player, no cast, single-target
    W11TonzeSwipe = 14972, // Boss->self, 3.0s cast, range 9 ?-degree cone
    W111TonzeSwing = 14973, // Boss->self, 4.0s cast, range 13 circle
    OrderToStandFast = 14976, // Boss->self, 3.0s cast, range 100 circle
    W111TonzeSwingAdds = 14979, // Adulator->self, 3.0s cast, range 13 circle
    W111TonzeSwingBig = 14974, // Boss->self, 4.0s cast, range 20 circle
    OrderToAssault = 14975, // Boss->self, 3.0s cast, range 100 circle
    ZoomIn = 14980, // Adulator->location, 3.0s cast, width 8 rect charge
}

class Adds(BossModule module) : Components.AddsPointless(module, (uint)OID.Adulator);
class W11TonzeSwipe(BossModule module) : Components.StandardAOEs(module, AID.W11TonzeSwipe, new AOEShapeCone(9, 75.Degrees()));
class W111TonzeSwing(BossModule module) : Components.StandardAOEs(module, AID.W111TonzeSwing, new AOEShapeCircle(13));
class W111TonzeSwingAdds(BossModule module) : Components.StandardAOEs(module, AID.W111TonzeSwingAdds, new AOEShapeCircle(13));
class W111TonzeSwingBig(BossModule module) : Components.StandardAOEs(module, AID.W111TonzeSwingBig, new AOEShapeCircle(20));
class ZoomIn(BossModule module) : Components.ChargeAOEs(module, AID.ZoomIn, 4);

class MolechStates : StateMachineBuilder
{
    public MolechStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Adds>()
            .ActivateOnEnter<W11TonzeSwipe>()
            .ActivateOnEnter<W111TonzeSwing>()
            .ActivateOnEnter<W111TonzeSwingAdds>()
            .ActivateOnEnter<W111TonzeSwingBig>()
            .ActivateOnEnter<ZoomIn>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.EurekaNM, GroupID = 639, NameID = 1414, Contributors = "xan", SortOrder = 3)]
public class Molech(WorldState ws, Actor primary) : BossModule(ws, primary, new(-676.8632f, -441.8009f), new ArenaBoundsCircle(80, MapResolution: 1));
