namespace BossMod.Shadowbringers.Alliance.A25Compound2P;

class EnergyCompression1(BossModule module) : Components.GenericTowers(module)
{
    private readonly List<(Actor from, Actor to)> _transfers = [];

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.EnergyCompressionTower1)
            Towers.Add(new(actor.Position, 5, maxSoakers: int.MaxValue));
    }

    public override void Update()
    {
        for (var i = _transfers.Count - 1; i >= 0; i--)
        {
            var ix = Towers.FindIndex(t => t.Position.AlmostEqual(_transfers[i].to.Position, 1));
            if (ix >= 0)
            {
                Towers.Ref(ix).Position = _transfers[i].from.Position;
                _transfers.RemoveAt(i);
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.Explosion or AID.BigExplosion)
            Towers.RemoveAll(t => t.Position.AlmostEqual(caster.Position, 1));
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID == TetherID.Transfer && WorldState.Actors.Find(tether.Target) is { } tar)
        {
            _transfers.Add((source, tar));
        }
    }
}
