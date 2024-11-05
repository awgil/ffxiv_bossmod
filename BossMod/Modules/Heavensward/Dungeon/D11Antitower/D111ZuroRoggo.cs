namespace BossMod.Modules.Heavensward.Dungeon.D07Antitower.D071ZuroRoggo;

public enum OID : uint
{
    Boss = 0x14FC, // R3.000
    Helper = 0x233C,
    Chirp = 0x14FE, // R2.000, x0 (spawn during fight)
    PoroggoChoirtoad = 0x14FD, // R2.100, x0 (spawn during fight)
}

public enum AID : uint
{
    _AutoAttack_Attack = 872, // Boss->player, no cast, single-target
    _Weaponskill_WaterBomb = 5538, // Helper->location, 3.0s cast, range 6 circle
    _Weaponskill_WaterBomb1 = 5537, // Boss->self, 3.0s cast, single-target
    _Weaponskill_ = 5542, // Boss->self, no cast, single-target
    _Weaponskill_OdiousCroak = 5540, // Helper->self, no cast, range 11+R ?-degree cone
    _Weaponskill_1 = 32370, // Helper->self, 4.0s cast, range 40+R ?-degree cone
    _Weaponskill_DiscordantHarmony = 5543, // Chirp->self, no cast, range 6 circle
    _Weaponskill_FrogSong = 5541, // Helper->self, no cast, range 40 circle
    _Weaponskill_WaterBomb2 = 5979, // Helper->location, 3.0s cast, range 6 circle
    _Weaponskill_WaterBomb3 = 5977, // Helper->location, 3.0s cast, range 6 circle
}

class Choirtoad(BossModule module) : Components.Adds(module, (uint)OID.PoroggoChoirtoad)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) => hints.PrioritizeTargetsByOID(OID.PoroggoChoirtoad, 1);
}

class Chirp(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<(Actor, DateTime)> Sources = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Sources.Select(src => new AOEInstance(new AOEShapeCircle(6), src.Item1.Position, src.Item1.Rotation, src.Item2));

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.Chirp)
            Sources.Add((actor, WorldState.FutureTime(8.95f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID._Weaponskill_DiscordantHarmony)
            Sources.RemoveAll(x => x.Item1 == caster);
    }
}
class OdiousCroak(BossModule module) : Components.GenericAOEs(module)
{
    private record struct PersistentAOE(WPos Source, Angle Rotation, DateTime Activation, int NumCastsRemaining);

    private PersistentAOE? AOE;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(AOE).Select(p => new AOEInstance(new AOEShapeCone(14, 60.Degrees()), p.Source, p.Rotation, p.Activation));

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID._Weaponskill_OdiousCroak)
        {
            if (AOE is PersistentAOE p)
            {
                AOE = p with
                {
                    Source = caster.Position,
                    Rotation = caster.Rotation,
                    NumCastsRemaining = p.NumCastsRemaining - 1
                };
                if (AOE?.NumCastsRemaining == 0)
                    AOE = null;
            }
            else
            {
                AOE = new(caster.Position, caster.Rotation, default, 11);
            }
        }
    }
}

class WaterBomb(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<(WPos, DateTime)> aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => aoes.Select(a => new AOEInstance(new AOEShapeCircle(6), a.Item1, default, a.Item2));

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID._Weaponskill_WaterBomb or AID._Weaponskill_WaterBomb2 or AID._Weaponskill_WaterBomb3)
            aoes.Add((spell.LocXZ, Module.CastFinishAt(spell)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID._Weaponskill_WaterBomb or AID._Weaponskill_WaterBomb2 or AID._Weaponskill_WaterBomb3)
            aoes.RemoveAll(a => a.Item1.AlmostEqual(spell.LocXZ, 1));
    }
}

class ZuroRoggoStates : StateMachineBuilder
{
    public ZuroRoggoStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Chirp>()
            .ActivateOnEnter<WaterBomb>()
            .ActivateOnEnter<OdiousCroak>()
            .ActivateOnEnter<Choirtoad>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 141, NameID = 4805)]
public class ZuroRoggo(WorldState ws, Actor primary) : BossModule(ws, primary, new(-365, -250), new ArenaBoundsCircle(20));

