namespace BossMod.Endwalker.Criterion.C03AAI.C033Statice;

class RingARingOExplosions(BossModule module) : Components.GenericAOEs(module)
{
    public List<Actor> ActiveBombs = [];
    private readonly List<Actor> _bombs = [];
    private DateTime _activation;

    private static readonly AOEShapeCircle _shape = new(12);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => ActiveBombs.Select(b => new AOEInstance(_shape, b.Position, default, _activation));

    public override void Update()
    {
        if (_bombs.Count == 6 && ActiveBombs.Count == 0)
        {
            var glowingBomb = _bombs.FirstOrDefault(b => b.ModelState.AnimState1 == 1);
            if (glowingBomb != null)
            {
                var cur = glowingBomb;
                do
                {
                    ActiveBombs.Add(cur);
                    cur = WorldState.Actors.Find(cur.Tether.Target);
                } while (cur != null && cur != glowingBomb);
                _activation = WorldState.FutureTime(17.4f);
            }
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID is OID.NBomb or OID.SBomb)
            _bombs.Add(actor);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.NBombBurst or AID.SBombBurst)
        {
            ++NumCasts;
            ActiveBombs.Remove(caster);
        }
    }
}
