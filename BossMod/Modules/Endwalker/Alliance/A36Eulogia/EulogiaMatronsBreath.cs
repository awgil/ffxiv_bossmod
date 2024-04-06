namespace BossMod.Endwalker.Alliance.A36Eulogia;

class MatronsBreath : Components.GenericAOEs
{
    private readonly List<(Actor actor, DateTime activation)> _flowers = [];

    private static readonly AOEShapeDonut _shape = new(8, 50);

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
    {
        var _blueFlowers = module.Enemies(OID.BlueSafeZone).FirstOrDefault();
        var _goldFlowers = module.Enemies(OID.GoldSafeZone).FirstOrDefault();
        if (_flowers.Count > 0)
            yield return new(_shape, _flowers[0].actor.OID == (uint)OID.BlueFlowers ? _blueFlowers!.Position : _goldFlowers!.Position, activation: _flowers[0].activation);
    }

    public override void OnActorCreated(BossModule module, Actor actor)
    {
        if ((OID)actor.OID is OID.BlueFlowers or OID.GoldFlowers)
            _flowers.Add((actor, module.WorldState.CurrentTime.AddSeconds(10.8f)));
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if (_flowers.Count > 0 && (AID)spell.Action.ID is AID.Blueblossoms or AID.Giltblossoms)
        {
            ++NumCasts;
            _flowers.RemoveAt(0);
        }
    }
}
