namespace BossMod.RealmReborn.Dungeon.D26Snowcloak.D261Wandil;

public enum OID : uint
{
    Boss = 0xD07, // R3.230, x?
    Wandil = 0xD2E, // R0.500, x?
    FrostBomb = 0xD09,
    Helper = 0x233C,
}

public enum AID : uint
{
    Attack = 872, // D20/D1B/D1D/D23/D19/D24/D07/D1F/3977->player, no cast, single-target
    FoulBite = 510, // D23->player, no cast, single-target
    SnowDrift = 3080, // D07->self, 5.0s cast, single-target
    SnowDrift1 = 3079, // D2E->self, no cast, range 80+R circle
    IceGuillotine = 3084, // D07->player, no cast, range 8+R ?-degree cone
    ColdWave = 3083, // D07->self, 3.0s cast, ???
    ColdWave1 = 3111, // D2E->location, 4.0s cast, range 8 circle
    Tundra = 3082, // D07->self, 3.0s cast, single-target
    HypothermalCombustion = 3085, // D09->self, 3.0s cast, range 80+R circle

}
class SnowDrift(BossModule module) : Components.StayMove(module)
{
    private DateTime _time;

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.SnowDrift)
        {
            _time = WorldState.CurrentTime;
            Array.Fill(PlayerStates, new(Requirement.Move, Module.CastFinishAt(spell, 2.4f)));
        }
    }
    public override void Update()
    {
        if (WorldState.CurrentTime > _time.AddSeconds(3))
        {
            Array.Fill(PlayerStates, default);
        }
    }
}
class IceGuillotine(BossModule module) : Components.StandardAOEs(module, AID.IceGuillotine, new AOEShapeCone(8.5f, 45.Degrees()));
class ColdWave(BossModule module) : Components.StandardAOEs(module, AID.ColdWave1, new AOEShapeCircle(8));
class HypothermalCombustion(BossModule module) : Components.RaidwideCast(module, AID.HypothermalCombustion);
class Tundra(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.Tundra)
        {
            _aoes.Add(new AOEInstance(new AOEShapeDonut(12, 20), Arena.Center));
        }
    }
}
class Adds(BossModule module) : Components.Adds(module, (uint)OID.FrostBomb, 3);
class D261WandilStates : StateMachineBuilder
{
    public D261WandilStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SnowDrift>()
            .ActivateOnEnter<IceGuillotine>()
            .ActivateOnEnter<ColdWave>()
            .ActivateOnEnter<Tundra>()
            .ActivateOnEnter<HypothermalCombustion>()
            .ActivateOnEnter<Adds>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "VeraNala", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 27, NameID = 3038)]
public class D261Wandil(WorldState ws, Actor primary) : BossModule(ws, primary, new(56.5f, -88.8f), new ArenaBoundsCircle(18));
