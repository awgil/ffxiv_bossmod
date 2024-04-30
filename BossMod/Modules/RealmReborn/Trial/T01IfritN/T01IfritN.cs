namespace BossMod.RealmReborn.Trial.T01IfritN;

public enum OID : uint
{
    Boss = 0xCF, // x1
    InfernalNail = 0xD0, // spawn during fight
    Helper = 0x191, // x19
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast
    Incinerate = 453, // Boss->self, no cast, range 10+R ?-degree cone cleave
    VulcanBurst = 454, // Boss->self, no cast, range 16+R circle unavoidable aoe with knockback ?
    Eruption = 455, // Boss->self, 2.2s cast, visual
    EruptionAOE = 733, // Helper->location, 3.0s cast, range 8 aoe
    Hellfire = 458, // Boss->self, 2.0s cast, infernal nail 'enrage'
    RadiantPlume = 456, // Boss->self, 2.2s cast, visual
    RadiantPlumeAOE = 734, // Helper->location, 3.0s cast, range 8 aoe
}

class Hints(BossModule module) : BossComponent(module)
{
    private DateTime _nailSpawn;

    public override void AddGlobalHints(GlobalHints hints)
    {
        var nail = Module.Enemies(OID.InfernalNail).FirstOrDefault();
        if (_nailSpawn == default && nail != null && nail.IsTargetable)
        {
            _nailSpawn = WorldState.CurrentTime;
        }
        if (_nailSpawn != default && nail != null && nail.IsTargetable && !nail.IsDead)
        {
            hints.Add($"Nail enrage in: {Math.Max(35 - (WorldState.CurrentTime - _nailSpawn).TotalSeconds, 0.0f):f1}s");
        }
    }
}

class Incinerate(BossModule module) : Components.Cleave(module, ActionID.MakeSpell(AID.Incinerate), new AOEShapeCone(16, 60.Degrees())); // TODO: verify angle
class Eruption(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.EruptionAOE), 8);
class RadiantPlume(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.RadiantPlumeAOE), 8);

class T01IfritNStates : StateMachineBuilder
{
    public T01IfritNStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Hints>()
            .ActivateOnEnter<Incinerate>()
            .ActivateOnEnter<Eruption>()
            .ActivateOnEnter<RadiantPlume>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 56, NameID = 1185)]
public class T01IfritN(WorldState ws, Actor primary) : BossModule(ws, primary, new(-0, 0), new ArenaBoundsCircle(20));
