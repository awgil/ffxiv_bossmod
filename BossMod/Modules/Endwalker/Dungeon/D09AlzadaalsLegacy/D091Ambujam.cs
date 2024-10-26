namespace BossMod.Endwalker.Dungeon.D09AlzadaalsLegacy.D091Ambujam;

public enum OID : uint
{
    Boss = 0x3879, // R=9.0
    CyanTentacle = 0x387B, // R2.400, x1
    ScarletTentacle = 0x387A, // R2.400, x1
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target

    BigWave = 28512, // Boss->self, 5.0s cast, range 40 circle

    CorrosiveFountain = 29556, // Helper->self, 7.0s cast, range 8 circle, knockback 10, away from source
    ToxicFountainVisual = 29466, // Boss->self, 4.0s cast, single-target
    ToxicFountain = 29467, // Helper->self, 7.0s cast, range 8 circle

    TentacleDig1 = 28501, // Boss->self, 3.0s cast, single-target
    TentacleDig2 = 28505, // Boss->self, 3.0s cast, single-target

    CorrosiveVenomVisual = 29157, // CyanTentacle->self, no cast, single-target
    CorrosiveVenom = 29158, // Helper->self, 2.5s cast, range 21 circle, knockback 10, away from source
    ToxinShowerVisual = 28507, // ScarletTentacle->self, no cast, single-target
    ToxinShower = 28508, // Helper->self, 2.5s cast, range 21 circle

    ModelStateChange1 = 28502, // Boss->self, no cast, single-target
    ModelStateChange2 = 28506 // Boss->self, no cast, single-target
}

class ToxinShowerCorrosiveVenom(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(21);
    private readonly List<AOEInstance> _aoes = [];
    private readonly Dictionary<byte, Dictionary<uint, WPos>> _statePositions = new()
    {
        {0x11, new Dictionary<uint, WPos>
            {{ 0x00080004, new(117, -97) }, { 0x00800004, new(131, -83) },
            { 0x00200004, new(131, -97) }, { 0x02000004, new(117, -83) }}},
        {0x10, new Dictionary<uint, WPos>
            {{ 0x00200004, new(109, -90) }, { 0x02000004, new(139, -90) },
            { 0x00080004, new(124, -75) }, { 0x00800004, new(124, -105) }}}
    };

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (_statePositions.TryGetValue(index, out var statePosition) && statePosition.TryGetValue(state, out var position))
        {
            var activation = WorldState.FutureTime(10.5f);
            _aoes.Add(new(circle, position, default, activation));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count > 0 && (AID)spell.Action.ID is AID.CorrosiveVenom or AID.ToxinShower)
            _aoes.RemoveAt(0);
    }
}

class ToxicCorrosiveFountain(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(8);
    private readonly List<AOEInstance> _aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Take(10);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.ToxicFountain or AID.CorrosiveFountain)
            _aoes.Add(new(circle, caster.Position, default, Module.CastFinishAt(spell)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count > 0 && (AID)spell.Action.ID is AID.ToxicFountain or AID.CorrosiveFountain)
            _aoes.RemoveAt(0);
    }
}

class BigWave(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.BigWave));

class D091AmbujamStates : StateMachineBuilder
{
    public D091AmbujamStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ToxicCorrosiveFountain>()
            .ActivateOnEnter<ToxinShowerCorrosiveVenom>()
            .ActivateOnEnter<BigWave>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 844, NameID = 11241)]
public class D091Ambujam(WorldState ws, Actor primary) : BossModule(ws, primary, new(124, -90), new ArenaBoundsCircle(19.5f));
