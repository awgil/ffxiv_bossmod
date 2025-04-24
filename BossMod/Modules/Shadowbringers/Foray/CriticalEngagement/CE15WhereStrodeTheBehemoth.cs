namespace BossMod.Shadowbringers.Foray.CriticalEngagement.CE15WhereStrodeTheBehemoth;

public enum OID : uint
{
    Boss = 0x2DCB, // R8.700, x1
    Helper = 0x233C, // R0.500, x15, Helper type
    BestialCorpse = 0x2DCC, // R4.350, x5
}

public enum AID : uint
{
    WildHorn = 20193, // Boss->self/players, 6.0s cast, range 18 ?-degree cone
    ZombieBile = 20195, // Boss->self, 4.0s cast, single-target
    ZombieJuice = 20196, // Helper->location, 6.0s cast, range 6 circle
    ZombieJuiceVoidzone = 20199, // Helper->location, no cast, range 6 circle
    Thunderbolt = 20197, // 2DCC->self, 6.0s cast, range 45 90-degree cone
    WildBolt = 20200, // Boss->self, 4.0s cast, single-target
    WildBoltInstant = 20201, // Helper->self, no cast, range 30 circle
    BlazingMeteor = 20194, // Boss->location, 6.0s cast, range 30 circle
    ZombieBreath = 20198, // Boss->self, 3.0s cast, range 60 width 6 rect
}

class WildHorn(BossModule module) : Components.StandardAOEs(module, AID.WildHorn, new AOEShapeCone(18, 45.Degrees()));
class ZombieBreath(BossModule module) : Components.StandardAOEs(module, AID.ZombieBreath, new AOEShapeRect(60, 3));
class ZombieJuice(BossModule module) : Components.PersistentVoidzoneAtCastTarget(module, 6, AID.ZombieJuice, m => m.Enemies(0x1E972C).Where(e => e.EventState != 7), 1);
class Thunderbolt(BossModule module) : Components.GenericAOEs(module, AID.Thunderbolt)
{
    private readonly List<(Actor Caster, DateTime Activation)> Casters = [];

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.ZombieJuice)
            Casters.AddRange(Module.Enemies(OID.BestialCorpse).Where(b => b.Position.InCircle(spell.TargetXZ, 6)).Select(b => (b, WorldState.FutureTime(12.7f))));

        if (spell.Action == WatchedAction)
            Casters.RemoveAll(c => c.Caster == caster);
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Casters.Select(c => new AOEInstance(new AOEShapeCone(45, 45.Degrees()), c.Caster.Position, c.Caster.Rotation, c.Caster.CastInfo == null ? c.Activation : Module.CastFinishAt(c.Caster.CastInfo)));
}
class WildBolt(BossModule module) : Components.RaidwideCastDelay(module, AID.WildBolt, AID.WildBoltInstant, 0.8f);
class BlazingMeteor(BossModule module) : Components.GenericLineOfSightAOE(module, AID.BlazingMeteor, 60, false)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            Modify(spell.LocXZ, Module.Enemies(OID.BestialCorpse).Select(c => (c.Position, c.HitboxRadius)), Module.CastFinishAt(spell));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            Modify(null, []);
        }
    }
}

class ChlevnikStates : StateMachineBuilder
{
    public ChlevnikStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<WildHorn>()
            .ActivateOnEnter<ZombieBreath>()
            .ActivateOnEnter<ZombieJuice>()
            .ActivateOnEnter<Thunderbolt>()
            .ActivateOnEnter<WildBolt>()
            .ActivateOnEnter<BlazingMeteor>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.BozjaCE, GroupID = 735, NameID = 15)] // bnpcname=9427
public class Chlevnik(WorldState ws, Actor primary) : BossModule(ws, primary, new(231, 95), new ArenaBoundsCircle(25));

