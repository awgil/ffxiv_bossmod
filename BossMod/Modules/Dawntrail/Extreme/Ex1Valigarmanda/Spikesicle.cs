namespace BossMod.Dawntrail.Extreme.Ex1Valigarmanda;

class Spikesicle(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShape[] _shapes = [new AOEShapeDonut(20, 25), new AOEShapeDonut(25, 30), new AOEShapeDonut(30, 35), new AOEShapeDonut(35, 40), new AOEShapeRect(40, 2.5f)]; // TODO: verify inner radius

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Skip(NumCasts).Take(2).Select((a, b) => a with { Color = b == 0 ? ArenaColor.Danger : ArenaColor.AOE }).Reverse();

    public override void OnMapEffect(byte index, uint state)
    {
        if (state == 0x00020004 && index is >= 4 and <= 13)
        {
            // 4  - 53 +20
            // 5  - 53 -20
            // 6  - 54 +20
            // 7  - 54 -20
            // 8  - 55 +20
            // 9  - 55 -20
            // 10 - 56 +20
            // 11 - 56 -20
            // 12 - 57 -17
            // 13 - 57 +17
            var shape = _shapes[(index - 4) >> 1];
            var odd = (index & 1) != 0;
            var x = index < 12 ? (odd ? -20 : +20) : (odd ? +17 : -17);
            var activationDelay = 11.3f + 0.2f * _aoes.Count;
            _aoes.Add(new(shape, Module.PrimaryActor.Position + new WDir(x, 0), default, WorldState.FutureTime(activationDelay)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.SpikesicleAOE1 or AID.SpikesicleAOE2 or AID.SpikesicleAOE3 or AID.SpikesicleAOE4 or AID.SpikesicleAOE5)
        {
            ++NumCasts;
        }
    }
}

class SphereShatter(BossModule module) : Components.GenericAOEs(module, AID.SphereShatter)
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeCircle _shape = new(13);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Skip(NumCasts).Take(2).Select((a, b) => a with { Color = b == 0 ? ArenaColor.Danger : ArenaColor.AOE }).Reverse();

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.IceBoulder)
            _aoes.Add(new(_shape, actor.Position, default, WorldState.FutureTime(6.5f)));
    }
}
