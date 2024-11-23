namespace BossMod.Dawntrail.Alliance.A12Fafnir;

class SpikeFlail(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.SpikeFlail), new AOEShapeCone(80, 135.Degrees()));
class Touchdown(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Touchdown), new AOEShapeCircle(24));

// TODO: implement gradual deactivation, it's probably done in 60-degree segments...
class DragonBreath(BossModule module) : Components.GenericAOEs(module, ActionID.MakeSpell(AID.DragonBreath))
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeDonut _shape = new(16, 30); // TODO: verify inner radius

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.OffensivePostureDragonBreath)
        {
            NumCasts = 0;
            _aoes.Add(new(_shape, caster.Position, default, Module.CastFinishAt(spell, 1.2f)));
        }
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if ((OID)actor.OID == OID.DragonBreath && state == 0x00040008)
            _aoes.Clear();
    }
}
