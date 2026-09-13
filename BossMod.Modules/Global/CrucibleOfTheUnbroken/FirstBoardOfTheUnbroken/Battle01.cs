namespace BossMod.Global.CrucibleOfTheUnbroken.FirstBoardOfTheUnbroken.Battle01;

public enum OID : uint
{
    Boss = 0x4B86,
    BoneBishop = 0x4B87,
    Helper = 0x233C,
}
public enum AID : uint
{
    AutoAttack  = 50784, // Boss->player, no cast, single-target
    Blizzard = 50788, // BoneBishop->player, no cast, single-target
    DeathSpiral = 46867, // BoneBishop->self, 5.0s cast, single-target
    DeathSpiralDonut = 46868, // 233C->self, 6.0s cast, range 4-40 donut
    Tumulus = 46866, // Boss->self, 5.0s cast, range 6 circle
    BlackEruption = 46873, // BoneBishop->self, 5.0+1.0s cast, single-target
    BlackEruption1 = 46874, // 233C->location, 6.0s cast, range 5 circle
    BlackEruption2 = 46900, // 233C->location, 1.5s cast, range 5 circle
    Ossify = 46871, // Boss->self, 8.0s cast, single-target
    ForwardGuard = 46864, // Boss->self, 5.0s cast, single-target
}
class DeathSpiralDonut(BossModule module) : Components.StandardAOEs(module, AID.DeathSpiralDonut, new AOEShapeDonut(4, 40));
class Tumulus(BossModule module) : Components.StandardAOEs(module, AID.Tumulus, 6f);
class BlackEruption(BossModule module) : Components.StandardAOEs(module, AID.BlackEruption1, 5f, highlightImminent: true);
class BlackEruptionExa(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private readonly List<AOEInstance> _aoesNext = [];
    private static readonly AOEShapeCircle circ = new(5f);
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoes.Count > 0)
        {
            foreach (var e in _aoes)
            {
                yield return e with { Color = ArenaColor.AOE };
            }
        }
        if (_aoesNext.Count > 0)
        {
            foreach (var e in _aoes)
            {
                yield return e with { Color = ArenaColor.Danger };
            }
        }

    }
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.BlackEruption1)
        {
            var origin = caster.Position;
            var rot = caster.Rotation;

            for (var i = 0; i < 4; ++i)
            {
                var dir = rot + (90 * i).Degrees();
                var pos = origin + dir.ToDirection() * 3f;
                _aoes.Add(new(circ, pos));
            }
        }
        if ((AID)spell.Action.ID is AID.BlackEruption2)
        {
            var origin = caster.Position;
            var rot = caster.Rotation;

            if (_aoesNext.Count >= 4)
            {
                _aoesNext.Clear();
            }
            _aoes.Add(new(circ, origin, caster.Rotation));
            var pos = origin + rot.ToDirection() * 3f;
            _aoesNext.Add(new(circ, pos));
        }
    }
    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.BlackEruption2)
        {
            NumCasts++;
        }
        if ((AID)spell.Action.ID is AID.BlackEruption2 && NumCasts >= 4)
        {
            _aoes.Clear();
            _aoes.AddRange(_aoesNext);
            NumCasts = 0;
        }
    }
    public override void OnActorDestroyed(Actor actor)
    {
        if ((OID)actor.OID is OID.BoneBishop)
        {
            _aoesNext.Clear();
            _aoes.Clear();
        }
    }
}
class ForwardGuard(BossModule module) : Components.DirectionalParry(module, (uint)OID.Boss);
class Ossify(BossModule module) : Components.CastInterruptHint(module, AID.Ossify);
class Adds(BossModule module) : Components.Adds(module, (uint)OID.BoneBishop);

class Battle01States : StateMachineBuilder
{
    public Battle01States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<DeathSpiralDonut>()
            .ActivateOnEnter<Tumulus>()
            .ActivateOnEnter<BlackEruptionExa>()
            .ActivateOnEnter<BlackEruption>()
            .ActivateOnEnter<Ossify>()
            .ActivateOnEnter<ForwardGuard>()
            .ActivateOnEnter<Adds>();
    }
}

[ModuleInfo(Contributors = "VeraNala", GroupType = BossModuleInfo.GroupType.CrucibleOfTheUnbroken, GroupID = 1088, NameID = 14531)]
public class Battle01(WorldState ws, Actor primary) : BossModule(ws, primary, new(120, -420), new ArenaBoundsCircle(20));
