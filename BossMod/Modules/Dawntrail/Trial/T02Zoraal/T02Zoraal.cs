namespace BossMod.Dawntrail.Trial.T02Zoraal;

class T02ZoraalStates : StateMachineBuilder
{
    public T02ZoraalStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SoulOverflow1>()
            .ActivateOnEnter<SoulOverflow2>()
            .ActivateOnEnter<DoubleEdgedSwords2>()
            .ActivateOnEnter<PatricidalPique>()
            .ActivateOnEnter<CalamitysEdge>()
            .ActivateOnEnter<Burst>()
            .ActivateOnEnter<VorpalTrail3>()
            .ActivateOnEnter<VorpalTrail4>()
            .ActivateOnEnter<VorpalTrail5>();
    }
}
class SoulOverflow1(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.SoulOverflow1));
class SoulOverflow2(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.SoulOverflow2));
class DoubleEdgedSwords2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.DoubleEdgedSwords2), new AOEShapeCone(30, 90.Degrees()));
class PatricidalPique(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.PatricidalPique));
class CalamitysEdge(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.CalamitysEdge));
class Burst(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Burst), new AOEShapeCircle(8));
class VorpalTrail3(BossModule module) : Components.ChargeAOEs(module, ActionID.MakeSpell(AID.VorpalTrail3), 2f);
class VorpalTrail4(BossModule module) : Components.ChargeAOEs(module, ActionID.MakeSpell(AID.VorpalTrail4), 2f);
class VorpalTrail5(BossModule module) : Components.ChargeAOEs(module, ActionID.MakeSpell(AID.VorpalTrail5), 2f);

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 995, NameID = 12881)]
public class T02Zoraal(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsSquare(20, Rotation: 45.Degrees()));
