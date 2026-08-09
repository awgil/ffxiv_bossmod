namespace BossMod.Dawntrail.Dungeon.D10Underkeep.D101Gargant;

public enum OID : uint
{
    Boss = 0x4791, // R4.2
    SandSphere = 0x4792, // R1.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target

    ChillingChirp = 42547, // Boss->self, 5.0s cast, range 30 circle
    AlmightyRacket = 42546, // Boss->self, 4.0s cast, range 30 width 30 rect
    AerialAmbushVisual = 42542, // Boss->location, 3.0s cast, single-target
    AerialAmbush = 42543, // Helper->self, 3.5s cast, range 30 width 15 rect
    FoundationalDebris = 43161, // Helper->location, 6.0s cast, range 10 circle
    SedimentaryDebris = 43160, // Helper->players, 5.0s cast, range 5 circle, spread
    Earthsong = 42544, // Boss->self, 5.0s cast, range 30 circle
    SphereShatter1 = 42545, // SandSphere->self, 2.0s cast, range 6 circle
    SphereShatter2 = 43135, // SandSphere->self, 2.0s cast, range 6 circle
    TrapJaws = 42548 // Boss->player, 5.0s cast, single-target
}

sealed class AerialAmbush(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AerialAmbush, new AOEShapeRect(30f, 7.5f));
sealed class AlmightyRacket(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AlmightyRacket, new AOEShapeRect(30f, 15f));
sealed class FoundationalDebris(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FoundationalDebris, 10f);
sealed class SedimentaryDebris(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.SedimentaryDebris, 5f);
sealed class EarthsongChillingChirp(BossModule module) : Components.RaidwideCasts(module, [(uint)AID.Earthsong, (uint)AID.ChillingChirp]);
sealed class TrapJaws(BossModule module) : Components.SingleTargetDelayableCast(module, (uint)AID.TrapJaws);

sealed class SphereShatter(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(6f);
    private readonly List<AOEInstance> _aoes = [with(13)];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
        {
            return [];
        }
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        var deadline = aoes[0].Activation.AddSeconds(1d);

        var index = 0;
        while (index < count)
        {
            ref var aoe = ref aoes[index];
            if (aoe.Activation >= deadline)
            {
                break;
            }
            ++index;
        }

        return aoes[..index];
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.SandSphere)
        {
            _aoes.Add(new(circle, actor.Position.Quantized(), default, WorldState.FutureTime(7.9d)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID is (uint)AID.SphereShatter1 or (uint)AID.SphereShatter2)
        {
            _aoes.RemoveAt(0);
        }
    }
}

sealed class D101GargantStates : StateMachineBuilder
{
    public D101GargantStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AerialAmbush>()
            .ActivateOnEnter<AlmightyRacket>()
            .ActivateOnEnter<FoundationalDebris>()
            .ActivateOnEnter<SedimentaryDebris>()
            .ActivateOnEnter<EarthsongChillingChirp>()
            .ActivateOnEnter<TrapJaws>()
            .ActivateOnEnter<SphereShatter>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.AISupport, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1027u, NameID = 13753u)]
public sealed class D101Gargant : BossModule
{
    public D101Gargant(WorldState ws, Actor primary) : this(ws, primary, BuildArena()) { }

    private D101Gargant(WorldState ws, Actor primary, (WPos center, ArenaBoundsCustom arena) a) : base(ws, primary, a.center, a.arena) { }

    private static (WPos center, ArenaBoundsCustom arena) BuildArena()
    {
        var arena = new ArenaBoundsCustom([new Polygon(new(-248f, 122f), 14.5f, 72)]);
        return (arena.Center, arena);
    }
}
