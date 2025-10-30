namespace BossMod.Dawntrail.Foray.ForkedTower.FT03MarbleDragon;

class IceTwister(BossModule module) : Components.PersistentVoidzone(module, 5, m => m.Enemies(OID.Icewind));

// 3 puddles north, 3 puddles south, 1 center, activated in an arbitrary illogical order
class ImitationBlizzard2(BossModule module) : ImitationBlizzard(module, 4)
{
    // initially unsorted; sorted in activation order once twisters start moving
    private readonly List<Actor> _puddlesNorth = [];
    private readonly List<Actor> _puddlesSouth = [];

    private Actor? _puddleCenter;
    private Actor? _twister;

    private bool _sorted;
    public bool Enabled; // twisters move a miniscule amount when spawning, so we don't check their positions until the component is "enabled" (when draconiform is cast)

    private readonly List<(Actor Actor, DateTime Activation)> _aoes = [];

    protected override IEnumerable<(Actor Actor, DateTime Activation, bool Safe)> Puddles() => _aoes.Select(a => (a.Actor, a.Activation, false));

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.ImitationBlizzardCircle or AID.ImitationBlizzardCross)
        {
            NumCasts++;
            _aoes.RemoveAll(l => l.Actor.Position.AlmostEqual(caster.Position, 0.5f));
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID is OID.IcePuddle or OID.CrossPuddle)
        {
            if (actor.Position.AlmostEqual(Arena.Center, 1))
                _puddleCenter = actor;
            else if (actor.Position.Z < Arena.Center.Z)
                _puddlesNorth.Add(actor);
            else
                _puddlesSouth.Add(actor);
        }

        if ((OID)actor.OID == OID.Icewind)
            _twister ??= actor;
    }

    public override void Update()
    {
        if (Enabled && !_sorted && _twister is { } t)
        {
            if (t.LastFrameMovement != default)
            {
                Service.Log(t.LastFrameMovement.ToString());
                var dirCenter = t.Position - Arena.Center;
                var cw = dirCenter.OrthoR().Dot(t.LastFrameMovement) > 0;

                _puddlesNorth.SortBy(p => Angle.FromDirection(Arena.Center - p.Position).Rad);
                _puddlesSouth.SortBy(p => Angle.FromDirection(p.Position - Arena.Center).Rad);

                if (cw)
                {
                    _puddlesNorth.Reverse();
                    _puddlesSouth.Reverse();
                }

                PredictAOEs();

                _sorted = true;
            }
        }
    }

    private void PredictAOEs()
    {
        if (_puddlesNorth[0].OID == (uint)OID.CrossPuddle)
        {
            // cross pattern: first puddles, then second and third simultaneously, then center
            var next = WorldState.FutureTime(5.9f);
            _aoes.Add((_puddlesNorth[0], next));
            _aoes.Add((_puddlesSouth[0], next));
            next = next.AddSeconds(4);
            _aoes.Add((_puddlesNorth[1], next));
            _aoes.Add((_puddlesNorth[2], next));
            _aoes.Add((_puddlesSouth[1], next));
            _aoes.Add((_puddlesSouth[2], next));
            next = next.AddSeconds(4);
            _aoes.Add((_puddleCenter!, next));
        }
        else
        {
            // circle pattern: first puddles, then second + center, then third
            var next = WorldState.FutureTime(5.9f);
            _aoes.Add((_puddlesNorth[0], next));
            _aoes.Add((_puddlesSouth[0], next));
            next = next.AddSeconds(4);
            _aoes.Add((_puddleCenter!, next));
            _aoes.Add((_puddlesNorth[1], next));
            _aoes.Add((_puddlesSouth[1], next));
            next = next.AddSeconds(4);
            _aoes.Add((_puddlesNorth[2], next));
            _aoes.Add((_puddlesSouth[2], next));
        }
    }
}
