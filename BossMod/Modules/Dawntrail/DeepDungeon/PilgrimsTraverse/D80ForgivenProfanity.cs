namespace BossMod.Dawntrail.DeepDungeon.PilgrimsTraverse.D80ForgivenProfanity;

public enum OID : uint
{
    Boss = 0x485D, // R5.600, x1
    Helper = 0x233C, // R0.500, x8, Helper type
    BallOfLevin = 0x485E, // R1.300, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 45130, // Boss->player, no cast, single-target
    RoaringRingCast1 = 43465, // Boss->self, 5.2+0.8s cast, single-target
    RoaringRingCast2 = 43467, // Boss->self, 5.2+0.8s cast, single-target
    RoaringRing = 43468, // Helper->self, 6.0s cast, range 8-40 donut
    PerilousLairCast1 = 43469, // Boss->self, 5.2+0.8s cast, single-target
    PerilousLairCast2 = 43471, // Boss->self, 5.2+0.8s cast, single-target
    PerilousLair = 43472, // Helper->self, 6.0s cast, range 12 circle
    ProfaneWaul = 43473, // Helper->self, 6.0s cast, range 40 180-degree cone
    StalkingStaticCast = 43476, // Boss->self, 3.0s cast, range 40 circle
    StalkingStaticVisual = 43477, // BallOfLevin->location, 0.7s cast, width 4 rect charge
    StalkingStatic = 43795, // Helper->location, 1.0s cast, width 4 rect charge
    StaticShockVisual = 43478, // BallOfLevin->self, 1.5s cast, range 30 circle
    StaticShock = 44060, // Helper->self, 2.0s cast, range 30 circle
    ProwlingDeath = 43475, // Boss->self, 3.0s cast, range 40 circle
}

public enum TetherID : uint
{
    Lightning = 21, // BallOfLevin->BallOfLevin
}

public enum SID : uint
{
    ShadowOfDeath = 4518, // none->player, extra=0x0, doom, removed by Profane Waul
    NowhereToRun = 4519, // none->player, extra=0x1-0x8, stacking pyretic
}

class RoaringRing(BossModule module) : Components.StandardAOEs(module, AID.RoaringRing, new AOEShapeDonut(8, 40));
class PerilousLair(BossModule module) : Components.StandardAOEs(module, AID.PerilousLair, 12);

class ProfaneWaul(BossModule module) : Components.StandardAOEs(module, AID.ProfaneWaul, new AOEShapeCone(40, 90.Degrees()))
{
    private BitMask _shadow;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => base.ActiveAOEs(slot, actor).Select(a => a with { Inverted = _shadow[slot] });

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.ShadowOfDeath && Raid.TryFindSlot(actor, out var slot))
            _shadow.Set(slot);
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.ShadowOfDeath && Raid.TryFindSlot(actor, out var slot))
            _shadow.Clear(slot);
    }
}

class StaticShock(BossModule module) : Components.GenericAOEs(module)
{
    private readonly Graph<ulong> _tethers = new();
    private DateTime _first;

    private readonly List<AOEInstance> _predicted = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _predicted;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.StalkingStaticCast)
            _first = Module.CastFinishAt(spell, 11.7f);
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID == TetherID.Lightning)
        {
            _tethers.Add(source.InstanceID, tether.Target);
            if (_tethers.Edges.Count == 8 && _tethers.Topo(out var sorted))
            {
                for (var i = 0; i + 1 < sorted.Count; i++)
                {
                    var b1 = WorldState.Actors.Find(sorted[i])!;
                    var b2 = WorldState.Actors.Find(sorted[i + 1])!;
                    var dir = b2.Position - b1.Position;

                    _predicted.Add(new AOEInstance(new AOEShapeRect(dir.Length(), 2), b1.Position, dir.ToAngle(), _first.AddSeconds(i * 0.5f)));

                    if (i + 2 == sorted.Count)
                        _predicted.Add(new(new AOEShapeCircle(30), b2.Position, default, _first.AddSeconds(4.2f)));
                }

                _tethers.Clear();
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.StalkingStatic or AID.StaticShock)
        {
            NumCasts++;
            _first = default;
            if (_predicted.Count > 0)
                _predicted.RemoveAt(0);
        }
    }
}

class D80ForgivenProfanityStates : StateMachineBuilder
{
    public D80ForgivenProfanityStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<RoaringRing>()
            .ActivateOnEnter<PerilousLair>()
            .ActivateOnEnter<ProfaneWaul>()
            .ActivateOnEnter<StaticShock>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1039, NameID = 13968)]
public class D80ForgivenProfanity(WorldState ws, Actor primary) : BossModule(ws, primary, new(-600, -300), new ArenaBoundsCircle(19.5f));

