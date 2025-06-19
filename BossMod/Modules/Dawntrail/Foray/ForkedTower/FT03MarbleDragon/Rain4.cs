using Lumina.Extensions;

namespace BossMod.Dawntrail.Foray.ForkedTower.FT03MarbleDragon;

class ImitationBlizzard4(BossModule module) : ImitationBlizzard(module, 10, imminentOnly: true)
{
    private bool _sorted;
    public bool Enabled;

    private Actor? _twister;

    private readonly List<(Actor Actor, DateTime Activation)> _aoes = [];

    protected override IEnumerable<(Actor Actor, DateTime Activation, bool Safe)> Puddles() => _aoes.Select(a => (a.Actor, a.Activation, false));

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.Icewind)
            _twister ??= actor;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.ImitationBlizzardCircle or AID.ImitationBlizzardCross)
        {
            NumCasts++;
            _aoes.RemoveAll(l => l.Actor.Position.AlmostEqual(caster.Position, 0.5f));
        }
    }

    public override void Update()
    {
        if (Enabled && !_sorted && _twister is { } t)
        {
            if (t.LastFrameMovement != default)
            {
                var puddle = WorldState.Actors.Where(a => (OID)a.OID is OID.CrossPuddle or OID.IcePuddle).FirstOrDefault(a => a.Position.InCone(t.Position, t.LastFrameMovement.Normalized(), 45.Degrees()));
                if (puddle == null)
                {
                    ReportError($"no puddle found near twister {t} {t.LastFrameMovement}");
                    return;
                }

                var (p1, p2) = puddle.OID == (uint)OID.CrossPuddle ? (OID.CrossPuddle, OID.IcePuddle) : (OID.IcePuddle, OID.CrossPuddle);
                foreach (var p in Module.Enemies(p1))
                    _aoes.Add((p, WorldState.FutureTime(5.7f)));
                foreach (var p in Module.Enemies(p2))
                    _aoes.Add((p, WorldState.FutureTime(15.7f)));

                _sorted = true;
            }
        }
    }
}

class B4TowerFreeze(BossModule module) : Components.GenericAOEs(module, AID.BallOfIceSmall)
{
    private readonly ImitationBlizzard4 _b4 = module.FindComponent<ImitationBlizzard4>()!;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var active = _b4.ActiveAOEs(slot, actor).FirstOrNull();
        if (active is { } t)
        {
            foreach (var p in Module.Enemies(OID.IceTower))
                yield return new AOEInstance(new AOEShapeCircle(4), p.Position, default, Activation: t.Activation);
        }
    }
}

class B4Tower(BossModule module) : Components.CastTowers(module, AID.ImitationBlizzardTower, 4, minSoakers: 4, maxSoakers: int.MaxValue);
