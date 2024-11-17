namespace BossMod.Heavensward.Dungeon.D11Antitower.D111ZuroRoggo;

public enum OID : uint
{
    Boss = 0x14FC, // R3.000
    Helper = 0x233C,
    Chirp = 0x14FE, // R2.000, x0 (spawn during fight)
    PoroggoChoirtoad = 0x14FD, // R2.100, x0 (spawn during fight)
}

public enum AID : uint
{
    WaterBomb1 = 5538, // Helper->location, 3.0s cast, range 6 circle
    WaterBombVisual = 5537, // Boss->self, 3.0s cast, single-target
    OdiousCroak = 5540, // Helper->self, no cast, range 11+R ?-degree cone
    DiscordantHarmony = 5543, // Chirp->self, no cast, range 6 circle
    FrogSong = 5541, // Helper->self, no cast, range 40 circle
    WaterBomb2 = 5979, // Helper->location, 3.0s cast, range 6 circle
    WaterBomb3 = 5977, // Helper->location, 3.0s cast, range 6 circle
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
        if (spell.Action.ID == (uint)AID.DiscordantHarmony)
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
        if (spell.Action.ID == (uint)AID.OdiousCroak)
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
        if ((AID)spell.Action.ID is AID.WaterBomb1 or AID.WaterBomb2 or AID.WaterBomb3)
            aoes.Add((spell.LocXZ, Module.CastFinishAt(spell)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.WaterBomb1 or AID.WaterBomb2 or AID.WaterBomb3)
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

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 141, NameID = 4805, Contributors = "xan")]
public class ZuroRoggo(WorldState ws, Actor primary) : BossModule(ws, primary, new(-365, -250), new ArenaBoundsCircle(20));

