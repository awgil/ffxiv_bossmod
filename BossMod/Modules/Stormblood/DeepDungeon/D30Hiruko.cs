namespace BossMod.Stormblood.DeepDungeon.HeavenOnHigh.D30Hiruko;

public enum OID : uint
{
    Boss = 0x2251, // R5.250, x1
    RaiunClouds = 0x2252, // R1.000, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 6499, // Boss->player, no cast, single-target
    CloudCall = 11290, // Boss->self, 3.0s cast, single-target // spawns the clouds with this cast
    LightningBolt = 11294, // RaiunClouds->self, 8.0s cast, range 8 circle
    LightningStrike = 11292, // Boss->self, 2.5s cast, range 50+R width 6 rect
    Shiko = 11291, // Boss->self, 5.0s cast, range 100 circle // 6 yalms is the damage falloff, Knockup into the cloud to make a safe spot
    Supercell = 11293, // Boss->self, 7.0s cast, range 50+R width 100 rect
}

class LightningStrike(BossModule module) : Components.StandardAOEs(module, AID.LightningStrike, new AOEShapeRect(55.25f, 3));
class Shiko(BossModule module) : Components.StandardAOEs(module, AID.Shiko, new AOEShapeCircle(10));
class LightningBolt(BossModule module) : Components.StandardAOEs(module, AID.LightningBolt, new AOEShapeCircle(8))
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count == 20)
        {
            var closest = Casters
                .Where(c => !c.Position.InCircle(Arena.Center, 10) && !c.Position.InCircle(Module.PrimaryActor.Position, 10))
                .MinBy(c => c.DistanceToHitbox(actor))!;
            hints.AddForbiddenZone(new AOEShapeDonut(2, 100), closest.Position, default, Module.CastFinishAt(Module.PrimaryActor.CastInfo));
        }
        else
        {
            base.AddAIHints(slot, actor, assignment, hints);
        }
    }
}
class Supercell(BossModule module) : Components.StandardAOEs(module, AID.Supercell, new AOEShapeRect(50, 50));

class D30HirukoStates : StateMachineBuilder
{
    public D30HirukoStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<LightningStrike>()
            .ActivateOnEnter<Shiko>()
            .ActivateOnEnter<Supercell>()
            .ActivateOnEnter<LightningBolt>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "LegendofIceman", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 542, NameID = 7482)]
public class D30Hiruko(WorldState ws, Actor primary) : BossModule(ws, primary, new(-300, -300), new ArenaBoundsCircle(25f));
