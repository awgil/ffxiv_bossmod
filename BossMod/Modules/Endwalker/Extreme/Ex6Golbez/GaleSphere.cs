namespace BossMod.Endwalker.Extreme.Ex6Golbez;

class GaleSphere(BossModule module) : Components.GenericAOEs(module)
{
    public enum Side { S, E, N, W } // direction = value * 90deg

    private readonly List<Side> _sides = [];
    private readonly List<Actor>[] _spheres = [[], [], [], []];

    private static readonly AOEShapeRect _shape = new(30, 2.5f);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (NumCasts < _spheres.Length)
            foreach (var s in _spheres[NumCasts])
                yield return new(_shape, s.Position, s.CastInfo?.Rotation ?? s.Rotation, s.CastInfo?.NPCFinishAt ?? WorldState.CurrentTime);
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (_sides.Count > NumCasts)
            hints.Add($"Order: {string.Join(" -> ", _sides.Skip(NumCasts))}");
    }

    // note: PATE 11D5 happens 1.5s before all casts start, but we don't really care
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var order = CastOrder(spell.Action);
        if (order >= 0)
            _spheres[order].Add(caster);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        var order = CastOrder(spell.Action);
        if (order >= 0)
            _spheres[order].Remove(caster);
        if (order >= NumCasts)
            NumCasts = order + 1;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.GaleSpherePrepareN:
                _sides.Add(Side.N);
                break;
            case AID.GaleSpherePrepareE:
                _sides.Add(Side.E);
                break;
            case AID.GaleSpherePrepareW:
                _sides.Add(Side.W);
                break;
            case AID.GaleSpherePrepareS:
                _sides.Add(Side.S);
                break;
        }
    }

    private int CastOrder(ActionID aid) => (AID)aid.ID switch
    {
        AID.GaleSphereAOE1 => 0,
        AID.GaleSphereAOE2 => 1,
        AID.GaleSphereAOE3 => 2,
        AID.GaleSphereAOE4 => 3,
        _ => -1
    };
}
