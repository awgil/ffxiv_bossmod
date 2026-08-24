namespace BossMod.Dawntrail.Alliance.A12Fafnir;

class SpikeFlail(BossModule module) : Components.StandardAOEs(module, AID.SpikeFlail, new AOEShapeCone(80, 135.Degrees()));
class Touchdown(BossModule module) : Components.StandardAOEs(module, AID.Touchdown, new AOEShapeCircle(24));

class DragonBreath(BossModule module) : Components.GenericAOEs(module, AID.DragonBreath)
{
    private readonly List<AOEInstance> _aoes = [];
    private DateTime _removeNextAOE = DateTime.MaxValue;

    private static readonly AOEShapeDonutSector _shape = new(16, 30, 30.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void Update()
    {
        if (WorldState.CurrentTime >= _removeNextAOE && _aoes.Count > 0)
        {
            _aoes.RemoveAt(0);
            _removeNextAOE = WorldState.FutureTime(2); // TODO: find out the real disappearance speed...
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.OffensivePostureDragonBreath)
        {
            NumCasts = 0;
            _removeNextAOE = DateTime.MaxValue;
            for (int i = 0; i < 6; ++i)
                _aoes.Add(new(_shape, caster.Position, spell.Rotation - i * 60.Degrees(), Module.CastFinishAt(spell, 1.2f)));
        }
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if ((OID)actor.OID == OID.DragonBreath && state == 0x00040008)
        {
            _removeNextAOE = WorldState.CurrentTime;
        }
    }
}
