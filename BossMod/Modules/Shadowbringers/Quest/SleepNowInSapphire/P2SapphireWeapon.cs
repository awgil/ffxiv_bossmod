using BossMod.Shadowbringers.Quest.SleepNowInSapphire.P1GuidanceSystem;

namespace BossMod.Shadowbringers.Quest.SleepNowInSapphire.P2SapphireWeapon;

public enum OID : uint
{
    Boss = 0x2DFA,
    Helper = 0x233C,
}

public enum AID : uint
{
    TailSwing = 20326, // Boss->self, 4.0s cast, range 46 circle
    OptimizedJudgment = 20325, // Boss->self, 4.0s cast, range -60 donut
    MagitekSpread = 20336, // RegulasImage->self, 5.0s cast, range 43 ?-degree cone
    SideraysRight = 20329, // Helper->self, 8.0s cast, range 128 ?-degree cone
    SideraysLeft = 21021, // Helper->self, 8.0s cast, range 128 ?-degree cone
    SapphireRay = 20327, // Boss->self, 8.0s cast, range 120 width 40 rect
    MagitekRay = 20332, // 2DFC->self, 3.0s cast, range 100 width 6 rect
    ServantRoar = 20339, // 2DFD->self, 2.5s cast, range 100 width 8 rect
}

public enum SID : uint
{
    Invincibility = 775, // none->Boss, extra=0x0
}

class MagitekRay(BossModule module) : Components.StandardAOEs(module, AID.MagitekRay, new AOEShapeRect(100, 3));
class ServantRoar(BossModule module) : Components.StandardAOEs(module, AID.ServantRoar, new AOEShapeRect(100, 4));
class TailSwing(BossModule module) : Components.StandardAOEs(module, AID.TailSwing, new AOEShapeCircle(46));
class OptimizedJudgment(BossModule module) : Components.StandardAOEs(module, AID.OptimizedJudgment, new AOEShapeDonut(21, 60));
class MagitekSpread(BossModule module) : Components.StandardAOEs(module, AID.MagitekSpread, new AOEShapeCone(43, 120.Degrees()));
class SapphireRay(BossModule module) : Components.StandardAOEs(module, AID.SapphireRay, new AOEShapeRect(120, 20));
class Siderays(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<(Actor, WPos)> Casters = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Casters.Select(c => new AOEInstance(new AOEShapeCone(128, 45.Degrees()), c.Item2, c.Item1.CastInfo!.Rotation, Module.CastFinishAt(c.Item1.CastInfo)));

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.SideraysLeft:
                Casters.Add((caster, caster.Position + caster.Rotation.ToDirection().OrthoL() * 15));
                break;
            case AID.SideraysRight:
                Casters.Add((caster, caster.Position + caster.Rotation.ToDirection().OrthoR() * 15));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        Casters.RemoveAll(c => c.Item1 == caster);
    }
}

class TheSapphireWeaponStates : StateMachineBuilder
{
    public TheSapphireWeaponStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<TailSwing>()
            .ActivateOnEnter<OptimizedJudgment>()
            .ActivateOnEnter<MagitekSpread>()
            .ActivateOnEnter<Siderays>()
            .ActivateOnEnter<SapphireRay>()
            .ActivateOnEnter<MagitekRay>()
            .ActivateOnEnter<ServantRoar>()
            .ActivateOnEnter<GWarrior>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 69431, NameID = 9458)]
public class TheSapphireWeapon(WorldState ws, Actor primary) : BossModule(ws, primary, new(-15, 610), new ArenaBoundsSquare(60, 1))
{
    protected override void DrawEnemies(int pcSlot, Actor pc) => Arena.Actors(WorldState.Actors.Where(x => !x.IsAlly), ArenaColor.Enemy);

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var h in hints.PotentialTargets)
            h.Priority = h.Actor.FindStatus(SID.Invincibility) == null ? 1 : 0;
    }
}
