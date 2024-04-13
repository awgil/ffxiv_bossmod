namespace BossMod.Endwalker.Alliance.A21Nophica;

class MatronsBreath(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<(Actor actor, DateTime activation)> _flowers = [];

    private static readonly AOEShapeDonut _shape = new(8, 50);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var _blueFlowers = Module.Enemies(OID.BlueSafeZone).FirstOrDefault();
        var _goldFlowers = Module.Enemies(OID.GoldSafeZone).FirstOrDefault();
        if (_flowers.Count > 0)
            yield return new(_shape, _flowers[0].actor.OID == (uint)OID.BlueFlowers ? _blueFlowers!.Position : _goldFlowers!.Position, default, _flowers[0].activation);
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID is OID.BlueFlowers or OID.GoldFlowers)
            _flowers.Add((actor, WorldState.FutureTime(11.1f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_flowers.Count > 0 && (AID)spell.Action.ID is AID.Blueblossoms or AID.Giltblossoms)
        {
            ++NumCasts;
            _flowers.RemoveAt(0);
        }
    }
}
