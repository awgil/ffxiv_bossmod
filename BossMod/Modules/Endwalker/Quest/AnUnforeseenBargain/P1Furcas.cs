using BossMod.QuestBattle.Endwalker.MSQ;

namespace BossMod.Endwalker.Quest.AnUnforeseenBargain.P1Furcas;

public enum OID : uint
{
    Boss = 0x3D71,
    Helper = 0x233C,
}

public enum AID : uint
{
    VoidSlash = 33027, // _Gen_VisitantBlackguard->self, 4.0s cast, range 8+R 90-degree cone
    Explosion = 33004, // Helper->self, 10.0s cast, range 5 circle
    JongleursX = 31802, // Boss->player, 4.0s cast, single-target
    StraightSpindle = 31796, // _Gen_VisitantVoidskipper->self, 4.0s cast, range 50+R width 5 rect
    VoidTorch = 33007, // Helper->location, 3.0s cast, range 6 circle
    HellishScythe = 31800, // Boss->self, 5.0s cast, range 10 circle
    FlameBlast = 33008, // 3ED4->self, 4.0s cast, range 80+R width 4 rect
    Blackout = 31798, // _Gen_VisitantVoidskipper->self, 13.0s cast, range 60 circle
    JestersReward = 33031, // Boss->self, 6.0s cast, range 28 180-degree cone
}

class AutoZero(BossModule module) : QuestBattle.RotationModule<ZeroAI>(module);
class Explosion(BossModule module) : Components.CastTowers(module, AID.Explosion, 5);
class VoidSlash(BossModule module) : Components.StandardAOEs(module, AID.VoidSlash, new AOEShapeCone(9.7f, 45.Degrees()));
class JongleursX(BossModule module) : Components.SingleTargetCast(module, AID.JongleursX);
class StraightSpindle(BossModule module) : Components.StandardAOEs(module, AID.StraightSpindle, new AOEShapeRect(50, 2.5f));
class VoidTorch(BossModule module) : Components.StandardAOEs(module, AID.VoidTorch, 6);
class HellishScythe(BossModule module) : Components.StandardAOEs(module, AID.HellishScythe, new AOEShapeCircle(10));
class FlameBlast(BossModule module) : Components.StandardAOEs(module, AID.FlameBlast, new AOEShapeRect(80, 2));
class JestersReward(BossModule module) : Components.StandardAOEs(module, AID.JestersReward, new AOEShapeCone(28, 90.Degrees()));
class Blackout(BossModule module) : Components.CastHint(module, AID.Blackout, "Kill wasp before enrage!", true)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var h in hints.PriorityTargets)
            if (h.Actor.CastInfo?.Action == WatchedAction)
                h.Priority = 5;
    }
}

class FurcasStates : StateMachineBuilder
{
    public FurcasStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AutoZero>()
            .ActivateOnEnter<Explosion>()
            .ActivateOnEnter<VoidSlash>()
            .ActivateOnEnter<JongleursX>()
            .ActivateOnEnter<StraightSpindle>()
            .ActivateOnEnter<VoidTorch>()
            .ActivateOnEnter<HellishScythe>()
            .ActivateOnEnter<FlameBlast>()
            .ActivateOnEnter<Blackout>()
            .ActivateOnEnter<JestersReward>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 70209, NameID = 12066)]
public class Furcas(WorldState ws, Actor primary) : BossModule(ws, primary, new(97.85f, 286), new ArenaBoundsCircle(19.5f))
{
    protected override void DrawEnemies(int pcSlot, Actor pc) => Arena.Actors(WorldState.Actors.Where(x => !x.IsAlly), ArenaColor.Enemy);
}
